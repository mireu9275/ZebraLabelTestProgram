using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
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
        private string _lastZpl;

        public MainForm()
        {
            InitializeComponent();
            _template = SampleTemplates.PartLabel100x50();
            LoadDefaults();
        }

        private void LoadDefaults()
        {
            txtIp.Text = ConfigurationManager.AppSettings["DefaultPrinterIp"] ?? "192.168.1.100";
            numPort.Value = SafeInt(ConfigurationManager.AppSettings["DefaultPrinterPort"], 9100);
            cmbDpi.SelectedItem = ConfigurationManager.AppSettings["DefaultTargetDpi"] ?? "203";
            if (cmbDpi.SelectedIndex < 0) cmbDpi.SelectedIndex = 0;

            txtPartNo.Text = "PART-12345";
            txtLotNo.Text = "2605150099";
            txtQr.Text = "PART-12345|2605150099";
            numCopies.Value = 1;
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
            return SafeInt(cmbDpi.SelectedItem as string, 203);
        }

        private string GenerateZpl()
        {
            _template.Copies = (int)numCopies.Value;
            var builder = new ZplLabelBuilder(_template);
            return builder.Build(CollectData(), CurrentTargetDpi());
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                _lastZpl = GenerateZpl();
                txtZpl.Text = _lastZpl;
                tabRight.SelectedTab = tabZpl;
                SetStatus("ZPL 생성 완료 (" + _lastZpl.Length + " bytes)");
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
                if (string.IsNullOrEmpty(_lastZpl)) _lastZpl = GenerateZpl();
                txtZpl.Text = _lastZpl;

                var dpi = CurrentTargetDpi();
                var dpmm = Math.Max(1, dpi / 25);
                var png = _previewService.RenderPng(_lastZpl, _template.WidthDots, _template.HeightDots, dpmm);

                using (var ms = new MemoryStream(png))
                {
                    if (picPreview.Image != null) picPreview.Image.Dispose();
                    picPreview.Image = Image.FromStream(ms);
                }
                tabRight.SelectedTab = tabPreview;
                SetStatus("미리보기 렌더 완료");
            }
            catch (Exception ex)
            {
                ShowError("미리보기 실패 — BinaryKits.Zpl.Viewer 패키지 복원 필요할 수 있음", ex);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_lastZpl)) _lastZpl = GenerateZpl();

                var config = new PrinterConfig
                {
                    ConnectionType = PrinterConnectionType.Tcp,
                    IpAddress = txtIp.Text,
                    Port = (int)numPort.Value,
                    TargetDpi = CurrentTargetDpi()
                };

                IPrinterClient client = new TcpPrinterClient();
                var result = client.Send(config, _lastZpl);
                if (result.Success)
                {
                    SetStatus("출력 완료 — " + result.BytesSent + " bytes 전송");
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
