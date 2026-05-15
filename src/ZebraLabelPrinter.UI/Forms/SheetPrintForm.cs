using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using ZebraLabelPrinter.Core.Builders;
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

        private Bitmap _labelBitmap;          // 캔버스에서 N번 재사용하는 라벨 PNG (모두 같은 데이터라 1번만 렌더)
        private bool _labelDirty = true;      // 라벨 PNG 재렌더 필요 여부

        public SheetPrintForm(LabelTemplate template, KoreanFontProfile profile,
            LabelPreviewService renderer, IDictionary<string, string> bindingValues)
        {
            InitializeComponent();
            _template = template;
            _profile = profile;
            _renderer = renderer;
            _bindingValues = bindingValues;

            // 캔버스 더블 버퍼링 (디자이너 캔버스랑 같은 트릭)
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(pnlA4Canvas, true);

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
                GapYMm = (double)numGapY.Value,
                DrawCutLines = chkCutLines.Checked
            };
            layout.ApplyPageType();
            return layout;
        }

        private double LabelWidthMm => _template.WidthDots * 25.4 / Math.Max(1, _template.SourceDpi);
        private double LabelHeightMm => _template.HeightDots * 25.4 / Math.Max(1, _template.SourceDpi);

        private void UpdateGridInfo()
        {
            try
            {
                var layout = BuildLayout();
                var (rows, cols, perPage) = layout.ComputeGrid(LabelWidthMm, LabelHeightMm);
                var count = (int)numCount.Value;
                var pages = perPage > 0 ? (int)Math.Ceiling((double)count / perPage) : 0;
                lblGridInfo.Text = string.Format(
                    "라벨 {0:F1}×{1:F1}mm\n배치 {2}행 × {3}열 = 페이지당 {4}장\n총 {5}장 → {6}페이지",
                    LabelWidthMm, LabelHeightMm, rows, cols, perPage, count, pages);
                btnPrint.Enabled = perPage > 0 && count > 0;
                if (perPage <= 0) lblGridInfo.Text += "\n⚠ 라벨이 페이지 안에 안 들어감";
                pnlA4Canvas.Invalidate();
            }
            catch (Exception ex)
            {
                lblGridInfo.Text = "오류: " + ex.Message;
            }
        }

        private void OnSettingsChanged(object sender, EventArgs e) => UpdateGridInfo();

        private Bitmap GetOrRenderLabel()
        {
            if (_labelBitmap != null && !_labelDirty) return _labelBitmap;
            try
            {
                if (_labelBitmap != null) { _labelBitmap.Dispose(); _labelBitmap = null; }
                var builder = new ZplLabelBuilder(_template);
                var zpl = builder.Build(_bindingValues ?? new Dictionary<string, string>(), _template.SourceDpi, _profile);
                var png = _renderer.RenderPng(zpl, _template.WidthDots, _template.HeightDots, 8);
                _labelBitmap = new Bitmap(new MemoryStream(png));
                _labelDirty = false;
                return _labelBitmap;
            }
            catch
            {
                _labelDirty = false;
                return null;
            }
        }

        private void pnlA4Canvas_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                DrawA4Canvas(e.Graphics);
            }
            catch (Exception ex)
            {
                using (var brush = new SolidBrush(Color.Red))
                using (var font = new Font("Segoe UI", 10f))
                    e.Graphics.DrawString("렌더 오류: " + ex.Message, font, brush, 10, 10);
            }
        }

        private void DrawA4Canvas(Graphics g)
        {
            var layout = BuildLayout();
            var labelImage = GetOrRenderLabel();

            // 캔버스 픽셀당 mm 비율 — A4 297mm가 캔버스 높이에 맞도록 자동 스케일.
            // 단, 패널이 작으면 최소 스케일 보장.
            const int marginPx = 30;
            var availPxH = Math.Max(200, pnlA4Canvas.ClientSize.Height - marginPx * 2);
            var availPxW = Math.Max(200, pnlA4Canvas.ClientSize.Width - marginPx * 2);
            var scaleByHeight = availPxH / layout.PageHeightMm;
            var scaleByWidth = availPxW / layout.PageWidthMm;
            var pxPerMm = Math.Min(scaleByHeight, scaleByWidth);
            // 최소/최대 줌
            pxPerMm = Math.Max(0.5, Math.Min(8.0, pxPerMm));

            var pageWPx = (float)(layout.PageWidthMm * pxPerMm);
            var pageHPx = (float)(layout.PageHeightMm * pxPerMm);
            var offsetX = marginPx;
            var offsetY = marginPx;

            // 페이지 흰 배경 + 그림자
            using (var shadow = new SolidBrush(Color.FromArgb(40, Color.Black)))
                g.FillRectangle(shadow, offsetX + 3, offsetY + 3, pageWPx, pageHPx);
            using (var white = new SolidBrush(Color.White))
                g.FillRectangle(white, offsetX, offsetY, pageWPx, pageHPx);
            using (var border = new Pen(Color.DarkGray, 1f))
                g.DrawRectangle(border, offsetX, offsetY, pageWPx, pageHPx);

            // 여백 점선
            var marginRect = new RectangleF(
                offsetX + (float)(layout.MarginLeftMm * pxPerMm),
                offsetY + (float)(layout.MarginTopMm * pxPerMm),
                (float)((layout.PageWidthMm - layout.MarginLeftMm - layout.MarginRightMm) * pxPerMm),
                (float)((layout.PageHeightMm - layout.MarginTopMm - layout.MarginBottomMm) * pxPerMm));
            using (var marginPen = new Pen(Color.FromArgb(120, Color.SteelBlue), 1f))
            {
                marginPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(marginPen, marginRect.X, marginRect.Y, marginRect.Width, marginRect.Height);
            }

            // 라벨 grid
            var (rows, cols, perPage) = layout.ComputeGrid(LabelWidthMm, LabelHeightMm);
            if (perPage <= 0) return;

            var count = (int)numCount.Value;
            var labelsOnFirstPage = Math.Min(count, perPage);
            int idx = 0;
            for (int r = 0; r < rows && idx < labelsOnFirstPage; r++)
            {
                for (int c = 0; c < cols && idx < labelsOnFirstPage; c++)
                {
                    var xMm = layout.MarginLeftMm + c * (LabelWidthMm + layout.GapXMm);
                    var yMm = layout.MarginTopMm + r * (LabelHeightMm + layout.GapYMm);
                    var slot = new RectangleF(
                        offsetX + (float)(xMm * pxPerMm),
                        offsetY + (float)(yMm * pxPerMm),
                        (float)(LabelWidthMm * pxPerMm),
                        (float)(LabelHeightMm * pxPerMm));
                    if (labelImage != null)
                    {
                        g.DrawImage(labelImage, slot);
                    }
                    else
                    {
                        using (var lbl = new SolidBrush(Color.LightYellow))
                            g.FillRectangle(lbl, slot);
                    }
                    using (var slotBorder = new Pen(Color.FromArgb(80, Color.Black), 0.5f))
                        g.DrawRectangle(slotBorder, slot.X, slot.Y, slot.Width, slot.Height);
                    idx++;
                }
            }

            // 구분선 (자르기 안내) — 캔버스 픽셀 좌표로 변환해서 그림
            if (layout.DrawCutLines && (rows > 1 || cols > 1))
            {
                using (var pen = new Pen(Color.FromArgb(150, Color.Gray), 1f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    // 가로 구분선
                    for (int r = 1; r < rows; r++)
                    {
                        var yMm = layout.MarginTopMm + r * LabelHeightMm + (r - 0.5) * layout.GapYMm;
                        var yPx = offsetY + (float)(yMm * pxPerMm);
                        g.DrawLine(pen,
                            offsetX + (float)(layout.MarginLeftMm * pxPerMm), yPx,
                            offsetX + (float)((layout.PageWidthMm - layout.MarginRightMm) * pxPerMm), yPx);
                    }
                    // 세로 구분선
                    for (int c = 1; c < cols; c++)
                    {
                        var xMm = layout.MarginLeftMm + c * LabelWidthMm + (c - 0.5) * layout.GapXMm;
                        var xPx = offsetX + (float)(xMm * pxPerMm);
                        g.DrawLine(pen,
                            xPx, offsetY + (float)(layout.MarginTopMm * pxPerMm),
                            xPx, offsetY + (float)((layout.PageHeightMm - layout.MarginBottomMm) * pxPerMm));
                    }
                }
            }

            // 페이지 정보 (좌상단)
            var totalPages = (int)Math.Ceiling((double)count / perPage);
            using (var infoFont = new Font("Segoe UI", 9f))
            using (var infoBrush = new SolidBrush(Color.DimGray))
            {
                var text = totalPages > 1 ? "1 / " + totalPages + " 페이지" : "1 페이지";
                g.DrawString(text, infoFont, infoBrush, offsetX, offsetY - 22);
            }
        }

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

                using (var doc = new SheetPrintService().BuildPrintDocument(req))
                using (var preview = new PrintPreviewDialog())
                {
                    preview.Document = doc;
                    preview.WindowState = FormWindowState.Maximized;
                    preview.UseAntiAlias = true;
                    preview.ShowDialog(this);
                }
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
