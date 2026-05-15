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
        // 미리보기/프린터 둘 다 같은 ZPL 사용 (^CI28 + ^A1 + KFONT3, raw UTF-8 한글)
        private readonly KoreanFontProfile _profile = KoreanFontProfile.Kfont3();
        private bool _suppressDataBindingHandler;

        // Designer state
        private LabelField _selectedField;
        private bool _isDragging;
        private Point _dragStartCanvas;
        private Point _fieldStartLabel;
        private const int CanvasPadding = 10;

        public MainForm()
        {
            InitializeComponent();
            _template = SampleTemplates.PartLabel100x50();
            LoadDefaults();
            WireDataBindingEvents();
            InitializeDesigner();
            RegenerateZplFromTemplate();
        }

        private void InitializeDesigner()
        {
            // 캔버스 크기 = 라벨 dots + padding (1:1 스케일)
            pnlCanvas.Size = new System.Drawing.Size(
                _template.WidthDots + CanvasPadding * 2,
                _template.HeightDots + CanvasPadding * 2);
            pgFieldProps.SelectedObject = null;
        }

        private void LoadDefaults()
        {
            LoadInstalledPrinters();
            EnsureKoreanFont();

            _suppressDataBindingHandler = true;
            try
            {
                txtPartNo.Text = "PART-12345";
                txtLotNo.Text = "2605150099";
                txtQr.Text = "PART-12345|2605150099";
                numCopies.Value = 1;
            }
            finally
            {
                _suppressDataBindingHandler = false;
            }
        }

        private void WireDataBindingEvents()
        {
            txtPartNo.TextChanged += OnDataBindingChanged;
            txtLotNo.TextChanged += OnDataBindingChanged;
            txtQr.TextChanged += OnDataBindingChanged;
            numCopies.ValueChanged += OnDataBindingChanged;
        }

        private void OnDataBindingChanged(object sender, EventArgs e)
        {
            if (_suppressDataBindingHandler) return;
            RegenerateZplFromTemplate();
        }

        private void RegenerateZplFromTemplate()
        {
            try
            {
                var zpl = BuildZpl(_profile);
                txtZpl.Text = ZplLabelBuilder.Format(zpl);
                pnlCanvas?.Invalidate();
            }
            catch (Exception ex)
            {
                SetStatus("ZPL 자동 갱신 실패: " + ex.Message);
            }
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
                RegenerateZplFromTemplate();
                tabRight.SelectedTab = tabZpl;
                SetStatus("ZPL 생성 완료 — " + _profile.DisplayName + " (" + txtZpl.Text.Length + " bytes). 데이터 필드 변경 시 자동 재생성됨");
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
                var zplToRender = CurrentZplFromTextBox();
                if (zplToRender == null)
                {
                    zplToRender = BuildZpl(_profile);
                    txtZpl.Text = ZplLabelBuilder.Format(zplToRender);
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
                SetStatus("미리보기 렌더 완료 (" + zplToRender.Length + " bytes)");
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

                var zplToPrint = CurrentZplFromTextBox() ?? BuildZpl(_profile);

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

        // ========== Designer ==========

        private Rectangle FieldRect(LabelField f)
        {
            int w, h;
            switch (f.FieldType)
            {
                case LabelFieldType.Text:
                    w = Math.Max(20, (f.Value?.Length ?? f.Name?.Length ?? 6) * Math.Max(f.FontWidth, 10));
                    h = Math.Max(f.FontHeight, 20);
                    break;
                case LabelFieldType.Barcode128:
                case LabelFieldType.BarcodeEan13:
                    w = f.Width > 0 ? f.Width : 200;
                    h = f.Height > 0 ? f.Height : 80;
                    break;
                case LabelFieldType.QrCode:
                    w = h = Math.Max(f.FontWidth * 21, 80);
                    break;
                case LabelFieldType.Box:
                case LabelFieldType.Line:
                    w = Math.Max(f.Width, 10);
                    h = Math.Max(f.Height, 10);
                    break;
                default:
                    w = h = 40; break;
            }
            return new Rectangle(f.X + CanvasPadding, f.Y + CanvasPadding, w, h);
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var labelRect = new Rectangle(CanvasPadding, CanvasPadding, _template.WidthDots, _template.HeightDots);

            using (var bg = new SolidBrush(Color.White))
                g.FillRectangle(bg, labelRect);
            using (var border = new Pen(Color.Black, 1))
                g.DrawRectangle(border, labelRect);

            if (_template.Fields == null) return;
            foreach (var field in _template.Fields)
            {
                DrawField(g, field, ReferenceEquals(field, _selectedField));
            }
        }

        private void DrawField(Graphics g, LabelField field, bool selected)
        {
            var r = FieldRect(field);
            using (var fill = new SolidBrush(Color.FromArgb(40, selected ? Color.DodgerBlue : Color.Gray)))
                g.FillRectangle(fill, r);
            using (var pen = new Pen(selected ? Color.DodgerBlue : Color.DimGray, selected ? 2f : 1f))
            {
                if (!selected) pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, r);
            }
            var label = "[" + field.FieldType + "] " + (field.Name ?? "");
            if (!string.IsNullOrEmpty(field.DataBindingKey)) label += " {" + field.DataBindingKey + "}";
            using (var fontBrush = new SolidBrush(Color.Black))
            using (var labelFont = new Font("Segoe UI", 8f))
                g.DrawString(label, labelFont, fontBrush, r.X + 2, r.Y + 2);
        }

        private LabelField HitTest(Point canvasPoint)
        {
            // 위에 그려진 필드(나중에 추가된) 우선
            for (int i = _template.Fields.Count - 1; i >= 0; i--)
            {
                if (FieldRect(_template.Fields[i]).Contains(canvasPoint))
                    return _template.Fields[i];
            }
            return null;
        }

        private void pnlCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var hit = HitTest(e.Location);
            SelectField(hit);
            if (hit != null)
            {
                _isDragging = true;
                _dragStartCanvas = e.Location;
                _fieldStartLabel = new Point(hit.X, hit.Y);
            }
        }

        private void pnlCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _selectedField == null) return;
            var dx = e.X - _dragStartCanvas.X;
            var dy = e.Y - _dragStartCanvas.Y;
            _selectedField.X = Math.Max(0, _fieldStartLabel.X + dx);
            _selectedField.Y = Math.Max(0, _fieldStartLabel.Y + dy);
            pnlCanvas.Invalidate();
            pgFieldProps.Refresh();
        }

        private void pnlCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            RegenerateZplFromTemplate();
        }

        private void SelectField(LabelField field)
        {
            _selectedField = field;
            pgFieldProps.SelectedObject = field;
            pnlCanvas.Invalidate();
            SetStatus(field != null
                ? "선택: [" + field.FieldType + "] " + (field.Name ?? "")
                : "선택 해제");
        }

        private void btnAddText_Click(object sender, EventArgs e) { AddField(LabelFieldType.Text); }
        private void btnAddBarcode_Click(object sender, EventArgs e) { AddField(LabelFieldType.Barcode128); }
        private void btnAddQr_Click(object sender, EventArgs e) { AddField(LabelFieldType.QrCode); }
        private void btnAddBox_Click(object sender, EventArgs e) { AddField(LabelFieldType.Box); }

        private void AddField(LabelFieldType type)
        {
            var f = new LabelField
            {
                Name = type.ToString() + (_template.Fields.Count + 1),
                FieldType = type,
                X = 50,
                Y = 50
            };
            switch (type)
            {
                case LabelFieldType.Text:
                    f.FontWidth = 24; f.FontHeight = 24; f.Value = "Sample"; break;
                case LabelFieldType.Barcode128:
                case LabelFieldType.BarcodeEan13:
                    f.Height = 80; f.Value = "12345"; break;
                case LabelFieldType.QrCode:
                    f.FontWidth = 5; f.Value = "QR"; break;
                case LabelFieldType.Box:
                    f.Width = 200; f.Height = 100; f.Thickness = 2; break;
            }
            _template.Fields.Add(f);
            SelectField(f);
            RegenerateZplFromTemplate();
        }

        private void btnDeleteField_Click(object sender, EventArgs e)
        {
            if (_selectedField == null) { SetStatus("삭제할 필드를 먼저 선택하세요"); return; }
            _template.Fields.Remove(_selectedField);
            SelectField(null);
            RegenerateZplFromTemplate();
        }

        private void pgFieldProps_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            pnlCanvas.Invalidate();
            RegenerateZplFromTemplate();
        }
    }
}
