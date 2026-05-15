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
        private double _canvasZoom = 1.0;
        private const double MinZoom = 0.25;
        private const double MaxZoom = 4.0;
        private const int SnapGrid = 10;    // 라벨 dots 기준 스냅 단위
        private bool _suppressLabelSizeHandler;

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
            ResizeCanvasToLabel();
            pgFieldProps.SelectedObject = null;

            // 키보드 처리 (Delete 등) + 휠 줌
            pnlCanvas.TabStop = true;
            pnlCanvas.MouseWheel += new MouseEventHandler(pnlCanvas_MouseWheel);
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);

            // 라벨 크기 입력 초기화 (mm 기본)
            _suppressLabelSizeHandler = true;
            try
            {
                cmbLabelUnit.SelectedItem = "mm";
                if (cmbLabelUnit.SelectedIndex < 0) cmbLabelUnit.SelectedIndex = 0;
                numLabelWidth.Value = (decimal)DotsToUnit(_template.WidthDots);
                numLabelHeight.Value = (decimal)DotsToUnit(_template.HeightDots);
            }
            finally { _suppressLabelSizeHandler = false; }
        }

        private void ResizeCanvasToLabel()
        {
            var w = (int)Math.Round(_template.WidthDots * _canvasZoom) + CanvasPadding * 2;
            var h = (int)Math.Round(_template.HeightDots * _canvasZoom) + CanvasPadding * 2;
            pnlCanvas.Size = new Size(w, h);
        }

        private double MmPerDot()
        {
            // dpi = dots per inch, 1 inch = 25.4 mm → mm/dot = 25.4 / dpi
            return 25.4 / Math.Max(1, _template.SourceDpi);
        }

        private double DotsToUnit(int dots)
        {
            var mm = dots * MmPerDot();
            return (cmbLabelUnit?.SelectedItem as string) == "cm" ? mm / 10.0 : mm;
        }

        private int UnitToDots(decimal value)
        {
            var mm = (double)value;
            if ((cmbLabelUnit?.SelectedItem as string) == "cm") mm *= 10.0;
            return (int)Math.Round(mm / MmPerDot());
        }

        private void OnLabelSizeChanged(object sender, EventArgs e)
        {
            if (_suppressLabelSizeHandler) return;
            _template.WidthDots = UnitToDots(numLabelWidth.Value);
            _template.HeightDots = UnitToDots(numLabelHeight.Value);
            ResizeCanvasToLabel();
            pnlCanvas.Invalidate();
            RegenerateZplFromTemplate();
        }

        private void OnLabelUnitChanged(object sender, EventArgs e)
        {
            if (_suppressLabelSizeHandler) return;
            // 단위 변경 시 dots는 그대로, NumericUpDown 표시값만 단위에 맞춰 다시 채움
            _suppressLabelSizeHandler = true;
            try
            {
                numLabelWidth.Value = (decimal)DotsToUnit(_template.WidthDots);
                numLabelHeight.Value = (decimal)DotsToUnit(_template.HeightDots);
            }
            finally { _suppressLabelSizeHandler = false; }
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

        private const int BarcodeModuleWidth = 2;       // ^BY2,...  (빌더와 동기화)
        private const int BarcodeHumanReadableHeight = 28; // ^BC...,Y 일 때 하단 인쇄 텍스트 높이 추정

        private Rectangle FieldRect(LabelField f)
        {
            // 라벨 dots 좌표계 기준 (transform이 줌/패딩 처리)
            int w, h;
            switch (f.FieldType)
            {
                case LabelFieldType.Text:
                    w = Math.Max(20, (f.Value?.Length ?? f.Name?.Length ?? 6) * Math.Max(f.FontWidth, 10));
                    h = Math.Max(f.FontHeight, 20);
                    break;

                case LabelFieldType.Barcode128:
                    {
                        // Code128 폭 = (시작 + 데이터 + 체크섬 + 정지) × 11 + 정지 13 → 대략 (n+3)×11 + 2
                        var n = (f.Value?.Length ?? 8);
                        var modules = n * 11 + 35;
                        w = modules * BarcodeModuleWidth;
                        h = (f.Height > 0 ? f.Height : 80) + BarcodeHumanReadableHeight;
                    }
                    break;

                case LabelFieldType.BarcodeEan13:
                    {
                        // EAN13는 항상 95 modules
                        w = 95 * BarcodeModuleWidth;
                        h = (f.Height > 0 ? f.Height : 80) + BarcodeHumanReadableHeight;
                    }
                    break;

                case LabelFieldType.QrCode:
                    {
                        // QR 버전(데이터 길이 기준 대략): v1=21, v2=25, v3=29, v4=33, v5=37...
                        var len = f.Value?.Length ?? 10;
                        var version = len <= 14 ? 1 : len <= 26 ? 2 : len <= 42 ? 3 : len <= 62 ? 4 : 5;
                        var modules = 17 + version * 4;
                        var mag = Math.Max(1, f.FontWidth > 0 ? f.FontWidth : 5);
                        w = h = modules * mag;
                    }
                    break;

                case LabelFieldType.Box:
                case LabelFieldType.Line:
                    w = Math.Max(f.Width, 10);
                    h = Math.Max(f.Height, 10);
                    break;

                default:
                    w = h = 40; break;
            }
            return new Rectangle(f.X, f.Y, w, h);
        }

        private Point CanvasToLabel(Point canvasPt)
        {
            return new Point(
                (int)Math.Round((canvasPt.X - CanvasPadding) / _canvasZoom),
                (int)Math.Round((canvasPt.Y - CanvasPadding) / _canvasZoom));
        }

        private static int SnapToGrid(int value, bool snap)
        {
            if (!snap) return value;
            return ((value + SnapGrid / 2) / SnapGrid) * SnapGrid;
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            // 그래픽스에 줌 + 패딩 transform 적용 → 이후 좌표는 라벨 dots 기준
            g.TranslateTransform(CanvasPadding, CanvasPadding);
            g.ScaleTransform((float)_canvasZoom, (float)_canvasZoom);

            var labelRect = new Rectangle(0, 0, _template.WidthDots, _template.HeightDots);

            using (var bg = new SolidBrush(Color.White))
                g.FillRectangle(bg, labelRect);

            // 스냅 그리드 표시 (옅게)
            using (var grid = new Pen(Color.FromArgb(40, Color.Gray), 1f))
            {
                for (int x = SnapGrid; x < _template.WidthDots; x += SnapGrid)
                    g.DrawLine(grid, x, 0, x, _template.HeightDots);
                for (int y = SnapGrid; y < _template.HeightDots; y += SnapGrid)
                    g.DrawLine(grid, 0, y, _template.WidthDots, y);
            }

            using (var border = new Pen(Color.Black, 1f / (float)_canvasZoom))
                g.DrawRectangle(border, labelRect);

            if (_template.Fields == null) return;
            foreach (var field in _template.Fields)
            {
                DrawField(g, field, ReferenceEquals(field, _selectedField));
            }

            // 드래그 중이면 선택 필드 위에 좌표/크기 라벨을 floating으로 표시
            if (_isDragging && _selectedField != null)
            {
                var r = FieldRect(_selectedField);
                var mmPerDot = MmPerDot();
                var info = string.Format("X={0} ({1:F1}mm)  Y={2} ({3:F1}mm)",
                    _selectedField.X, _selectedField.X * mmPerDot,
                    _selectedField.Y, _selectedField.Y * mmPerDot);
                using (var font = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var back = new SolidBrush(Color.FromArgb(220, Color.Black)))
                using (var fore = new SolidBrush(Color.White))
                {
                    var size = g.MeasureString(info, font);
                    var pad = 4;
                    var rectAbove = new RectangleF(r.X, Math.Max(0, r.Y - size.Height - pad * 2), size.Width + pad * 2, size.Height + pad);
                    g.FillRectangle(back, rectAbove);
                    g.DrawString(info, font, fore, rectAbove.X + pad, rectAbove.Y + pad / 2);
                }
            }
        }

        private void DrawField(Graphics g, LabelField field, bool selected)
        {
            var r = FieldRect(field);
            using (var fill = new SolidBrush(Color.FromArgb(40, selected ? Color.DodgerBlue : Color.Gray)))
                g.FillRectangle(fill, r);
            // 펜 굵기는 줌과 무관하게 화면 픽셀로 일정
            var penWidth = (selected ? 2f : 1f) / (float)_canvasZoom;
            using (var pen = new Pen(selected ? Color.DodgerBlue : Color.DimGray, penWidth))
            {
                if (!selected) pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, r);
            }
            var label = "[" + field.FieldType + "] " + (field.Name ?? "");
            if (!string.IsNullOrEmpty(field.DataBindingKey)) label += " {" + field.DataBindingKey + "}";
            // 라벨 폰트는 라벨 dots 좌표계 — 줌이 알아서 스케일
            var fontSizeDots = 12f;
            using (var fontBrush = new SolidBrush(Color.Black))
            using (var labelFont = new Font("Segoe UI", fontSizeDots, GraphicsUnit.Pixel))
                g.DrawString(label, labelFont, fontBrush, r.X + 2, r.Y + 2);
        }

        private LabelField HitTest(Point canvasPoint)
        {
            var labelPt = CanvasToLabel(canvasPoint);
            // 위에 그려진 필드(나중에 추가된) 우선
            for (int i = _template.Fields.Count - 1; i >= 0; i--)
            {
                if (FieldRect(_template.Fields[i]).Contains(labelPt))
                    return _template.Fields[i];
            }
            return null;
        }

        private void pnlCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            pnlCanvas.Focus();
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
            // 항상 status bar에 마우스 위치 (라벨 dots/mm) 표시
            var labelPt = CanvasToLabel(e.Location);
            var mmX = labelPt.X * MmPerDot();
            var mmY = labelPt.Y * MmPerDot();
            SetStatus(string.Format("커서: X={0}dot ({1:F1}mm), Y={2}dot ({3:F1}mm)", labelPt.X, mmX, labelPt.Y, mmY)
                     + (_selectedField != null ? "  |  선택: " + _selectedField.Name : ""));

            if (!_isDragging || _selectedField == null) return;

            // 화면 픽셀 이동량을 라벨 dots로 환산
            var dxDots = (int)Math.Round((e.X - _dragStartCanvas.X) / _canvasZoom);
            var dyDots = (int)Math.Round((e.Y - _dragStartCanvas.Y) / _canvasZoom);
            var newX = Math.Max(0, _fieldStartLabel.X + dxDots);
            var newY = Math.Max(0, _fieldStartLabel.Y + dyDots);
            // Shift 누르면 스냅 해제, 기본은 스냅 ON
            var snap = (Control.ModifierKeys & Keys.Shift) != Keys.Shift;
            _selectedField.X = SnapToGrid(newX, snap);
            _selectedField.Y = SnapToGrid(newY, snap);
            pnlCanvas.Invalidate();
            pgFieldProps.Refresh();
        }

        private void pnlCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            RegenerateZplFromTemplate();
        }

        private void pnlCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control) return;
            var factor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            var newZoom = Math.Min(MaxZoom, Math.Max(MinZoom, _canvasZoom * factor));
            if (Math.Abs(newZoom - _canvasZoom) < 1e-6) return;
            _canvasZoom = newZoom;
            ResizeCanvasToLabel();
            pnlCanvas.Invalidate();
            SetStatus("캔버스 줌: " + (_canvasZoom * 100).ToString("0") + "%");
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 디자이너 탭에서만 키 처리
            if (tabRight.SelectedTab != tabDesigner) return;

            if (e.KeyCode == Keys.Delete && _selectedField != null)
            {
                btnDeleteField_Click(sender, EventArgs.Empty);
                e.Handled = true;
                return;
            }

            // 화살표로 1 dot씩 미세 조정 (Shift+화살표는 10 dots)
            if (_selectedField != null && (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                var step = (e.Modifiers & Keys.Shift) == Keys.Shift ? SnapGrid : 1;
                if (e.KeyCode == Keys.Left) _selectedField.X = Math.Max(0, _selectedField.X - step);
                if (e.KeyCode == Keys.Right) _selectedField.X += step;
                if (e.KeyCode == Keys.Up) _selectedField.Y = Math.Max(0, _selectedField.Y - step);
                if (e.KeyCode == Keys.Down) _selectedField.Y += step;
                pnlCanvas.Invalidate();
                pgFieldProps.Refresh();
                RegenerateZplFromTemplate();
                e.Handled = true;
            }
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
