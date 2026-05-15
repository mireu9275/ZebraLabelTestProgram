using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using ZebraLabelPrinter.Core.Models;
using ZebraLabelPrinter.Core.Preview;
using ZebraLabelPrinter.Core.Printing;

namespace ZebraLabelPrinter.UI.Forms
{
    public partial class SheetPrintForm : Form
    {
        private readonly LabelTemplate _template;
        private readonly KoreanFontProfile _profile;
        private readonly LabelPreviewService _renderer;
        private readonly IDictionary<string, string> _bindingValues;

        public SheetPrintForm(LabelTemplate template, KoreanFontProfile profile,
            LabelPreviewService renderer, IDictionary<string, string> bindingValues)
        {
            InitializeComponent();
            _template = template;
            _profile = profile;
            _renderer = renderer;
            _bindingValues = bindingValues;

            LoadPrinters();
            cmbPageType.SelectedIndex = 0;
            UpdateGridInfo();
        }

        private void LoadPrinters()
        {
            cmbPrinter.Items.Clear();
            foreach (string p in PrinterSettings.InstalledPrinters) cmbPrinter.Items.Add(p);
            var def = new PrinterSettings().PrinterName;
            if (cmbPrinter.Items.Contains(def)) cmbPrinter.SelectedItem = def;
            else if (cmbPrinter.Items.Count > 0) cmbPrinter.SelectedIndex = 0;
        }

        private SheetLayout BuildLayout()
        {
            var layout = new SheetLayout
            {
                PageType = cmbPageType.SelectedIndex == 1 ? SheetPageType.Letter : SheetPageType.A4,
                MarginLeftMm = (double)numMarginL.Value,
                MarginTopMm = (double)numMarginT.Value,
                MarginRightMm = (double)numMarginR.Value,
                MarginBottomMm = (double)numMarginB.Value,
                GapXMm = (double)numGapX.Value,
                GapYMm = (double)numGapY.Value
            };
            layout.ApplyPageType();
            return layout;
        }

        private void UpdateGridInfo()
        {
            try
            {
                var labelWidthMm = _template.WidthDots * 25.4 / Math.Max(1, _template.SourceDpi);
                var labelHeightMm = _template.HeightDots * 25.4 / Math.Max(1, _template.SourceDpi);
                var layout = BuildLayout();
                var (rows, cols, perPage) = layout.ComputeGrid(labelWidthMm, labelHeightMm);
                var count = (int)numCount.Value;
                var pages = perPage > 0 ? (int)Math.Ceiling((double)count / perPage) : 0;
                lblGridInfo.Text = string.Format(
                    "라벨 {0:F1}×{1:F1}mm  |  배치 {2}행 × {3}열 = 페이지당 {4}장  |  총 {5}장 → {6}페이지",
                    labelWidthMm, labelHeightMm, rows, cols, perPage, count, pages);
                btnPrint.Enabled = perPage > 0 && count > 0;
                if (perPage <= 0) lblGridInfo.Text += "  ⚠ 라벨이 페이지 안에 안 들어감";
            }
            catch (Exception ex)
            {
                lblGridInfo.Text = "오류: " + ex.Message;
            }
        }

        private void OnSettingsChanged(object sender, EventArgs e) => UpdateGridInfo();

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var count = (int)numCount.Value;
                var data = new List<IDictionary<string, string>>();
                for (int i = 0; i < count; i++)
                {
                    data.Add(_bindingValues ?? new Dictionary<string, string>());
                }

                var req = new SheetPrintRequest
                {
                    WindowsPrinterName = cmbPrinter.SelectedItem as string,
                    Template = _template,
                    Profile = _profile,
                    Sheet = BuildLayout(),
                    Renderer = _renderer,
                    DataPerLabel = data,
                    Count = count
                };

                // 미리보기 다이얼로그 — WinForms 내장. 페이지 넘김/줌/인쇄 버튼이 모두 들어있음.
                using (var doc = new SheetPrintService().BuildPrintDocument(req))
                using (var preview = new PrintPreviewDialog())
                {
                    preview.Document = doc;
                    preview.WindowState = FormWindowState.Maximized;
                    preview.UseAntiAlias = true;
                    preview.ShowDialog(this);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "출력 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
