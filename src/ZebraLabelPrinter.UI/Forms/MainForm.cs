using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using ZebraLabelPrinter.Core.Builders;
using ZebraLabelPrinter.Core.Models;
using ZebraLabelPrinter.Core.Preview;
using ZebraLabelPrinter.Core.Printing;
using ZebraLabelPrinter.UI.Sample;

namespace ZebraLabelPrinter.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly LabelTemplate _template;
        private readonly LabelPreviewService _previewService = new LabelPreviewService();
        private readonly KoreanFontProfile _printerProfile = KoreanFontProfile.Kfont3();
        private readonly KoreanFontProfile _previewProfile = KoreanFontProfile.Default();

        public MainForm()
        {
            InitializeComponent();
            _template = SampleTemplates.PartLabel100x50();
            LoadDefaults();
        }

        private void LoadDefaults()
        {
            LoadInstalledPrinters();
            EnsureKoreanFont();

            txtPartNo.Text = "PART-12345";
            txtLotNo.Text = "2605150099";
            txtQr.Text = "PART-12345|2605150099";
            numCopies.Value = 1;
        }

        private void EnsureKoreanFont()
        {
            // 시스템 패밀리명 기준으로 FontManager에 등록 — SkiaSharp이 OS에서 찾아 한글 glyph fallback
            var candidates = new[] { "Malgun Gothic", "NanumGothic", "Noto Sans CJK KR", "Gulim", "맑은 고딕" };
            foreach (var family in candidates)
            {
                _previewService.TryRegisterCjkFontByFamilyName(family);
            }
        }

        private void LoadInstalledPrinters()
        {
            cmbPrinter.Items.Clear();
            foreach (string name in PrinterSettings.InstalledPrinters)
            {
                cmbPrinter.Items.Add(name);
            }

            var configured = ConfigurationManager.AppSettings["DefaultPrinterName"];
            if (!string.IsNullOrEmpty(configured) && cmbPrinter.Items.Contains(configured))
            {
                cmbPrinter.SelectedItem = configured;
                return;
            }

            var defaultPrinter = new PrinterSettings().PrinterName;
            if (!string.IsNullOrEmpty(defaultPrinter) && cmbPrinter.Items.Contains(defaultPrinter))
            {
                cmbPrinter.SelectedItem = defaultPrinter;
            }
            else if (cmbPrinter.Items.Count > 0)
            {
                cmbPrinter.SelectedIndex = 0;
            }
        }

        private static int SafeInt(string s, int fallback)
        {
            int v;
            return int.TryParse(s, out v) ? v : fallback;
        }

        private IDictionary<string, string> CollectData()
        {
            return new Dictionary<string, string>
            {
                { "PART_NO", txtPartNo.Text ?? string.Empty },
                { "LOT_NO", txtLotNo.Text ?? string.Empty },
                { "QR", txtQr.Text ?? string.Empty }
            };
        }

        private int CurrentTargetDpi()
        {
            return SafeInt(ConfigurationManager.AppSettings["DefaultTargetDpi"], _template.SourceDpi);
        }

        private string BuildZpl(KoreanFontProfile profile)
        {
            _template.Copies = (int)numCopies.Value;
            var builder = new ZplLabelBuilder(_template);
            return builder.Build(CollectData(), CurrentTargetDpi(), profile);
        }

        private string CurrentZplFromTextBox()
        {
            // 사용자가 편집한 내용을 우선. Format() 줄바꿈은 ZPL에 영향 없지만 그대로 보내도 됨.
            var text = txtZpl.Text;
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                var zpl = BuildZpl(_printerProfile);
                txtZpl.Text = ZplLabelBuilder.Format(zpl);
                tabRight.SelectedTab = tabZpl;
                SetStatus("ZPL 생성 완료 — " + _printerProfile.DisplayName + " (" + zpl.Length + " bytes). 텍스트박스를 직접 편집해도 미리보기/출력에 반영됨");
            }
            catch (Exception ex)
            {
                ShowError("ZPL 생성 실패", ex);
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                // 우선순위: 1) 텍스트박스 편집 내용 2) 비어있으면 preview profile로 새로 빌드
                var zplToRender = CurrentZplFromTextBox();
                var usingPreviewProfile = false;

                if (zplToRender == null)
                {
                    zplToRender = BuildZpl(_previewProfile);
                    txtZpl.Text = ZplLabelBuilder.Format(zplToRender);
                    usingPreviewProfile = true;
                }

                var dpi = CurrentTargetDpi();
                var dpmm = Math.Max(1, dpi / 25);
                var png = _previewService.RenderPng(zplToRender, _template.WidthDots, _template.HeightDots, dpmm);

                using (var ms = new MemoryStream(png))
                {
                    if (picPreview.Image != null) picPreview.Image.Dispose();
                    picPreview.Image = Image.FromStream(ms);
                }
                tabRight.SelectedTab = tabPreview;

                var warn = !usingPreviewProfile && zplToRender.IndexOf("^CI26", StringComparison.OrdinalIgnoreCase) >= 0
                    ? " — 주의: ^CI26 ZPL은 viewer가 한글 디코딩 못함(프린터는 정상). 한글 확인하려면 ZPL 비우고 다시 미리보기."
                    : "";
                SetStatus("미리보기 렌더 완료 (" + zplToRender.Length + " bytes)" + warn);
            }
            catch (Exception ex)
            {
                ShowError("미리보기 실패", ex);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var printerName = cmbPrinter.SelectedItem as string;
                if (string.IsNullOrEmpty(printerName))
                {
                    SetStatus("프린터를 선택하세요");
                    MessageBox.Show(this, "프린터를 선택하세요", "출력 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 텍스트박스에 있으면 그것 그대로, 없으면 printer profile로 빌드
                var zplToPrint = CurrentZplFromTextBox() ?? BuildZpl(_printerProfile);

                var config = new PrinterConfig
                {
                    ConnectionType = PrinterConnectionType.WindowsSpooler,
                    SpoolerName = printerName,
                    TargetDpi = CurrentTargetDpi()
                };

                IPrinterClient client = new WindowsSpoolerClient();
                var result = client.Send(config, zplToPrint);
                if (result.Success)
                {
                    SetStatus("출력 완료 — " + printerName + " (" + result.BytesSent + " bytes)");
                }
                else
                {
                    SetStatus("출력 실패: " + result.Message);
                    MessageBox.Show(this, result.Message, "출력 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ShowError("출력 처리 오류", ex);
            }
        }

        private void btnRefreshPrinters_Click(object sender, EventArgs e)
        {
            LoadInstalledPrinters();
            SetStatus("프린터 목록 새로고침: " + cmbPrinter.Items.Count + "개");
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void ShowError(string title, Exception ex)
        {
            SetStatus(title + ": " + ex.Message);
            MessageBox.Show(this, ex.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
