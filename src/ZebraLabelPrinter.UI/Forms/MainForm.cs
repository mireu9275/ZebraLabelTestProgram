using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
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
        private LabelTemplate _template;
        private readonly LabelPreviewService _previewService = new LabelPreviewService();
        // 미리보기/프린터 둘 다 같은 ZPL 사용 (^CI28 + ^A1 + KFONT3, raw UTF-8 한글)
        private readonly KoreanFontProfile _profile = KoreanFontProfile.Kfont3();
        private bool _suppressDataBindingHandler;
        private readonly Dictionary<string, TextBox> _dataBindingControls = new Dictionary<string, TextBox>();
        private bool _isDirty;
        private string _currentFilePath;

        // Designer state
        private LabelField _selectedField;                                          // primary 선택 (PropertyGrid 포커스)
        private readonly HashSet<LabelField> _selectedFields = new HashSet<LabelField>(); // 다중 선택
        private bool _isDragging;
        private Point _dragStartCanvas;
        private DragMode _dragMode = DragMode.None;
        private Rectangle _resizeStartRect; // 리사이즈 시작 시점의 필드 사각형 (X,Y,W,H 모두)
        private Rectangle _ghostRect;       // 드래그 중 ghost 사각형 (이동: X/Y 변, 리사이즈: 모두 변)
        // 다중 이동을 위한 시작 좌표 스냅샷
        private readonly Dictionary<LabelField, Point> _multiDragStart = new Dictionary<LabelField, Point>();
        // 드래그-영역-선택 (rubber band)
        private bool _isRubberBand;
        private Point _rubberStartLabel;
        private Rectangle _rubberRect;
        // 클립보드 (XML 직렬화)
        private string _clipboardXml;
        // Undo/Redo 히스토리 (XML 스냅샷)
        private readonly List<string> _historyStates = new List<string>();
        private int _historyIndex = -1;
        private const int MaxHistory = 100;
        private bool _suppressHistoryPush;

        private enum DragMode
        {
            None, Move,
            ResizeNW, ResizeN, ResizeNE,
            ResizeW,            ResizeE,
            ResizeSW, ResizeS, ResizeSE
        }
        private const int CanvasPadding = 10;
        private double _canvasZoom = 1.0;
        private const double MinZoom = 0.25;
        private const double MaxZoom = 4.0;
        private const int SnapGrid = 10;    // 라벨 dots 기준 스냅 단위
        private bool _suppressLabelSizeHandler;
        // 미리보기 탭 줌 상태
        private double _previewZoom = 1.0;
        private Size _previewNaturalSize;
        private const double MinPreviewZoom = 0.1;
        private const double MaxPreviewZoom = 5.0;

        public MainForm()
        {
            InitializeComponent();
            _template = SampleTemplates.PartLabel100x50();
            LoadDefaults();
            WireDataBindingEvents();
            InitializeDesigner();
            RebuildDataBindings();
            RegenerateZplFromTemplate();
            UpdateTitle(); // 시작 시 "(이름 없음)"으로 타이틀 설정
            PushHistory(); // 초기 상태를 히스토리에 등록 (이후 Undo의 마지막 도착지)
        }

        private void InitializeDesigner()
        {
            ResizeCanvasToLabel();
            pgFieldProps.SelectedObject = null;

            // 더블 버퍼링 활성화 — 드래그 시 깜빡임 제거.
            // Panel의 DoubleBuffered는 protected라 리플렉션으로 set.
            EnableDoubleBuffering(pnlCanvas);

            // 키보드 처리 (Delete 등) + 휠 줌
            pnlCanvas.TabStop = true;
            pnlCanvas.MouseWheel += new MouseEventHandler(pnlCanvas_MouseWheel);
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);

            // 미리보기 탭 줌: 탭과 PictureBox 둘 다에 휠 핸들러 (마우스 위치에 따라 어디로 갈지 모름)
            tabPreview.MouseWheel += new MouseEventHandler(Preview_MouseWheel);
            picPreview.MouseWheel += new MouseEventHandler(Preview_MouseWheel);

            // 라벨 크기 입력 초기화 (mm 기본).
            // ⚠️ dots×(25.4/203) 결과가 100.0985... 같이 미세 소수점 → 표시 "100.1"로 보이고
            // 사용자가 직접 입력하면 round-trip 오차로 dots가 매번 바뀜. 정수 mm로 정합 잡아 시작.
            _suppressLabelSizeHandler = true;
            try
            {
                cmbLabelUnit.SelectedItem = "mm";
                if (cmbLabelUnit.SelectedIndex < 0) cmbLabelUnit.SelectedIndex = 0;
                cmbSizePreset.SelectedIndex = 0; // 사용자 정의
                var widthMm = ClampNum(numLabelWidth, Math.Round((decimal)DotsToUnit(_template.WidthDots)));
                var heightMm = ClampNum(numLabelHeight, Math.Round((decimal)DotsToUnit(_template.HeightDots)));
                numLabelWidth.Value = widthMm;
                numLabelHeight.Value = heightMm;
                // 표시값에 맞춰 dots도 재계산 (정합 유지)
                _template.WidthDots = UnitToDots(widthMm);
                _template.HeightDots = UnitToDots(heightMm);
                ResizeCanvasToLabel();
            }
            finally { _suppressLabelSizeHandler = false; }
        }

        private static decimal ClampNum(NumericUpDown nud, decimal value)
        {
            if (value < nud.Minimum) return nud.Minimum;
            if (value > nud.Maximum) return nud.Maximum;
            return value;
        }

        private static void EnableDoubleBuffering(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(control, true);
        }

        private void ResizeCanvasToLabel()
        {
            try
            {
                // 음수/0/오버플로 방지: 최소 50 pixel, 최대 50000 pixel
                var rawW = (long)Math.Round(_template.WidthDots * _canvasZoom) + CanvasPadding * 2;
                var rawH = (long)Math.Round(_template.HeightDots * _canvasZoom) + CanvasPadding * 2;
                var w = (int)Math.Max(50, Math.Min(50000, rawW));
                var h = (int)Math.Max(50, Math.Min(50000, rawH));
                pnlCanvas.Size = new Size(w, h);
            }
            catch (Exception ex)
            {
                SetStatus("캔버스 리사이즈 실패: " + ex.Message);
            }
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
            try
            {
                var w = UnitToDots(numLabelWidth.Value);
                var h = UnitToDots(numLabelHeight.Value);
                // 비정상값 방어
                if (w < 1) w = 1;
                if (h < 1) h = 1;
                _template.WidthDots = w;
                _template.HeightDots = h;
                ResizeCanvasToLabel();
                pnlCanvas.Invalidate();
                RegenerateZplFromTemplate();
                MarkDirty();
            }
            catch (Exception ex)
            {
                SetStatus("라벨 크기 변경 실패: " + ex.Message);
            }
        }

        private void cmbSizePreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            // (폭mm, 높이mm) — index 0(사용자정의)는 변경 없음
            (double w, double h)? size = null;
            switch (cmbSizePreset.SelectedIndex)
            {
                case 1: size = (100, 50); break;
                case 2: size = (60, 40); break;
                case 3: size = (40, 20); break;
                case 4: size = (210, 297); break;   // A4
                case 5: size = (148, 210); break;   // A5
                case 6: size = (215.9, 279.4); break; // Letter
                case 7: size = (101.6, 152.4); break; // 4×6 inch
            }
            if (!size.HasValue) return;

            _suppressLabelSizeHandler = true;
            try
            {
                cmbLabelUnit.SelectedItem = "mm";
                numLabelWidth.Value = ClampNum(numLabelWidth, (decimal)size.Value.w);
                numLabelHeight.Value = ClampNum(numLabelHeight, (decimal)size.Value.h);
                _template.WidthDots = UnitToDots(numLabelWidth.Value);
                _template.HeightDots = UnitToDots(numLabelHeight.Value);
                ResizeCanvasToLabel();
                pnlCanvas.Invalidate();
            }
            finally { _suppressLabelSizeHandler = false; }
            RegenerateZplFromTemplate();
            MarkDirty();
            SetStatus("라벨 크기 프리셋: " + (cmbSizePreset.SelectedItem as string));
        }

        private void OnLabelUnitChanged(object sender, EventArgs e)
        {
            if (_suppressLabelSizeHandler) return;
            try
            {
                // 단위 변경 시 dots는 그대로, NumericUpDown 표시값만 단위에 맞춰 다시 채움
                _suppressLabelSizeHandler = true;
                try
                {
                    numLabelWidth.Value = ClampNum(numLabelWidth, Math.Round((decimal)DotsToUnit(_template.WidthDots), 1));
                    numLabelHeight.Value = ClampNum(numLabelHeight, Math.Round((decimal)DotsToUnit(_template.HeightDots), 1));
                }
                finally { _suppressLabelSizeHandler = false; }
            }
            catch (Exception ex)
            {
                SetStatus("단위 변경 실패: " + ex.Message);
            }
        }

        private void LoadDefaults()
        {
            LoadInstalledPrinters();
            EnsureKoreanFont();

            _suppressDataBindingHandler = true;
            try
            {
                numCopies.Value = 1;
            }
            finally
            {
                _suppressDataBindingHandler = false;
            }
        }

        private void WireDataBindingEvents()
        {
            // 동적 입력은 RebuildDataBindings에서 hook. numCopies만 정적이므로 여기서.
            numCopies.ValueChanged += OnDataBindingChanged;
        }

        private void RebuildDataBindings()
        {
            // DataBindingKey 있는 필드의 고유 키만 수집 (template 등장 순서 유지)
            var keys = new List<string>();
            var seen = new HashSet<string>();
            foreach (var f in _template.Fields)
            {
                if (string.IsNullOrEmpty(f.DataBindingKey)) continue;
                if (seen.Add(f.DataBindingKey)) keys.Add(f.DataBindingKey);
            }

            // 기존 입력값 보존 (재빌드 후에도 사용자가 입력한 값 유지)
            var preserved = new Dictionary<string, string>();
            foreach (var kv in _dataBindingControls) preserved[kv.Key] = kv.Value.Text;

            _suppressDataBindingHandler = true;
            try
            {
                pnlDataBindings.Controls.Clear();
                _dataBindingControls.Clear();

                if (keys.Count == 0)
                {
                    var empty = new Label
                    {
                        Text = "디자이너에서 필드를 추가하고 바인딩 키를\n지정하면 여기에 입력란이 생성됩니다.",
                        Location = new Point(10, 10),
                        AutoSize = true,
                        ForeColor = Color.Gray
                    };
                    pnlDataBindings.Controls.Add(empty);
                    return;
                }

                int y = 5;
                foreach (var key in keys)
                {
                    var lbl = new Label
                    {
                        Text = key + ":",
                        Location = new Point(5, y + 4),
                        AutoSize = true
                    };

                    var tb = new TextBox
                    {
                        Location = new Point(120, y),
                        Size = new Size(190, 23),
                        Tag = key
                    };

                    // 우선순위: 보존된 값 → 필드의 기본 Value
                    if (preserved.TryGetValue(key, out var prev))
                    {
                        tb.Text = prev;
                    }
                    else
                    {
                        var seedField = _template.Fields.FirstOrDefault(f => f.DataBindingKey == key);
                        tb.Text = seedField?.Value ?? string.Empty;
                    }

                    tb.TextChanged += OnDataBindingChanged;
                    pnlDataBindings.Controls.Add(lbl);
                    pnlDataBindings.Controls.Add(tb);
                    _dataBindingControls[key] = tb;
                    y += 32;
                }
            }
            finally
            {
                _suppressDataBindingHandler = false;
            }
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
            var data = new Dictionary<string, string>();
            foreach (var kv in _dataBindingControls)
            {
                data[kv.Key] = kv.Value.Text ?? string.Empty;
            }
            return data;
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
                    _previewNaturalSize = picPreview.Image.Size;
                }
                ApplyPreviewZoom();
                tabRight.SelectedTab = tabPreview;
                SetStatus("미리보기 렌더 완료 (" + zplToRender.Length + " bytes) — Ctrl+휠로 확대/축소");
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

        private void btnPrintSheet_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new SheetPrintForm(_template, _profile, _previewService, CollectData()))
                {
                    dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ShowError("A4 출력 다이얼로그 열기 실패", ex);
            }
        }

        private void ApplyPreviewZoom()
        {
            if (picPreview.Image == null || _previewNaturalSize.IsEmpty) return;
            var w = Math.Max(1, (int)Math.Round(_previewNaturalSize.Width * _previewZoom));
            var h = Math.Max(1, (int)Math.Round(_previewNaturalSize.Height * _previewZoom));
            picPreview.Size = new Size(w, h);
        }

        private void Preview_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control) return;
            if (picPreview.Image == null) return;
            var factor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            var newZoom = Math.Min(MaxPreviewZoom, Math.Max(MinPreviewZoom, _previewZoom * factor));
            if (Math.Abs(newZoom - _previewZoom) < 1e-6) return;
            _previewZoom = newZoom;
            ApplyPreviewZoom();
            SetStatus("미리보기 줌: " + (_previewZoom * 100).ToString("0") + "%");
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

        // ========== Dirty 추적 & 종료 확인 ==========

        private void MarkDirty()
        {
            if (!_isDirty)
            {
                _isDirty = true;
                UpdateTitle();
            }
            PushHistory();
        }

        // ========== Undo/Redo ==========

        private void PushHistory()
        {
            if (_suppressHistoryPush) return;
            try
            {
                var serialized = SerializeTemplate(_template);
                // 같은 상태 중복 추가 방지
                if (_historyIndex >= 0 && _historyIndex < _historyStates.Count && _historyStates[_historyIndex] == serialized) return;
                // 현재 위치 이후 redo 기록은 새로운 분기로 덮어쓰여짐
                if (_historyIndex < _historyStates.Count - 1)
                    _historyStates.RemoveRange(_historyIndex + 1, _historyStates.Count - _historyIndex - 1);
                _historyStates.Add(serialized);
                _historyIndex++;
                if (_historyStates.Count > MaxHistory)
                {
                    _historyStates.RemoveAt(0);
                    _historyIndex--;
                }
            }
            catch { /* 직렬화 실패는 무시 */ }
        }

        private void Undo()
        {
            if (_historyIndex <= 0) { SetStatus("실행 취소할 작업 없음"); return; }
            _historyIndex--;
            RestoreTemplateFromHistory(_historyStates[_historyIndex]);
            SetStatus("실행 취소 (" + (_historyIndex + 1) + "/" + _historyStates.Count + ")");
        }

        private void Redo()
        {
            if (_historyIndex >= _historyStates.Count - 1) { SetStatus("다시 실행할 작업 없음"); return; }
            _historyIndex++;
            RestoreTemplateFromHistory(_historyStates[_historyIndex]);
            SetStatus("다시 실행 (" + (_historyIndex + 1) + "/" + _historyStates.Count + ")");
        }

        private void RestoreTemplateFromHistory(string xml)
        {
            _suppressHistoryPush = true;
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LabelTemplate));
                using (var sr = new System.IO.StringReader(xml))
                {
                    var t = (LabelTemplate)serializer.Deserialize(sr);
                    if (t.Fields == null) t.Fields = new List<LabelField>();
                    _template = t;
                }
                // 객체 ref가 새로 만들어졌으므로 선택 초기화
                _selectedFields.Clear();
                _selectedField = null;
                pgFieldProps.SelectedObject = null;
                ResizeCanvasToLabel();
                // 라벨 크기 입력 동기화
                _suppressLabelSizeHandler = true;
                try
                {
                    numLabelWidth.Value = ClampNum(numLabelWidth, (decimal)DotsToUnit(_template.WidthDots));
                    numLabelHeight.Value = ClampNum(numLabelHeight, (decimal)DotsToUnit(_template.HeightDots));
                    numCopies.Value = Math.Max(1, _template.Copies);
                }
                finally { _suppressLabelSizeHandler = false; }
                RebuildDataBindings();
                RegenerateZplFromTemplate();
                pnlCanvas.Invalidate();
            }
            finally { _suppressHistoryPush = false; }
            if (_isDirty == false) { _isDirty = true; UpdateTitle(); }
        }

        private static string SerializeTemplate(LabelTemplate t)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LabelTemplate));
            using (var sw = new System.IO.StringWriter())
            {
                serializer.Serialize(sw, t);
                return sw.ToString();
            }
        }

        // ========== Copy/Paste ==========

        private void CopySelected()
        {
            if (_selectedFields.Count == 0) { SetStatus("복사할 필드 선택"); return; }
            try
            {
                var list = _selectedFields.ToList();
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<LabelField>));
                using (var sw = new System.IO.StringWriter())
                {
                    serializer.Serialize(sw, list);
                    _clipboardXml = sw.ToString();
                }
                SetStatus("복사: " + list.Count + "개 필드");
            }
            catch (Exception ex)
            {
                SetStatus("복사 실패: " + ex.Message);
            }
        }

        private void Paste()
        {
            if (string.IsNullOrEmpty(_clipboardXml)) { SetStatus("클립보드가 비어있음"); return; }
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<LabelField>));
                List<LabelField> pasted;
                using (var sr = new System.IO.StringReader(_clipboardXml))
                {
                    pasted = (List<LabelField>)serializer.Deserialize(sr);
                }
                if (pasted == null || pasted.Count == 0) return;

                _selectedFields.Clear();
                foreach (var f in pasted)
                {
                    f.Name = MakeUniqueName(f.Name ?? "Pasted");
                    f.X += 10;
                    f.Y += 10;
                    if (f.FieldType != LabelFieldType.Box && f.FieldType != LabelFieldType.Line)
                        f.DataBindingKey = f.Name;
                    _template.Fields.Add(f);
                    _selectedFields.Add(f);
                }
                _selectedField = pasted.Last();
                pgFieldProps.SelectedObject = _selectedField;
                RebuildDataBindings();
                RegenerateZplFromTemplate();
                MarkDirty();
                pnlCanvas.Invalidate();
                SetStatus("붙여넣기: " + pasted.Count + "개");
            }
            catch (Exception ex)
            {
                SetStatus("붙여넣기 실패: " + ex.Message);
            }
        }

        private string MakeUniqueName(string baseName)
        {
            if (!_template.Fields.Any(f => f.Name == baseName)) return baseName;
            for (int i = 2; i < 10000; i++)
            {
                var candidate = baseName + "_" + i;
                if (!_template.Fields.Any(f => f.Name == candidate)) return candidate;
            }
            return baseName + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        private void ClearDirty()
        {
            if (!_isDirty) return;
            _isDirty = false;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var fileName = string.IsNullOrEmpty(_currentFilePath)
                ? "(이름 없음)"
                : Path.GetFileName(_currentFilePath);
            this.Text = "Zebra Label Printer — " + fileName + (_isDirty ? " *" : "");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 사용자가 직접 닫은 경우(X, Alt+F4, 시스템 메뉴 더블클릭)만 확인.
            // 시스템 종료/태스크매니저 강제 종료 등은 그대로 진행.
            if (e.CloseReason == CloseReason.UserClosing && _isDirty)
            {
                var result = MessageBox.Show(this,
                    "저장하지 않은 변경사항이 있습니다.\n저장하고 종료하시겠습니까?\n\n" +
                    "예 = 저장 후 종료\n아니오 = 저장 없이 종료\n취소 = 종료 안 함",
                    "종료 확인",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button3);

                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == DialogResult.Yes)
                {
                    // SaveCurrent: 기존 파일에 직접 저장 또는 SaveAs 다이얼로그
                    if (!SaveCurrent())
                    {
                        e.Cancel = true; // 저장 취소/실패 → 종료 중단
                    }
                }
                // No: 그대로 종료
            }
            base.OnFormClosing(e);
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

                case LabelFieldType.Image:
                    w = Math.Max(f.Width, 50);
                    h = Math.Max(f.Height, 50);
                    break;

                default:
                    w = h = 40; break;
            }
            // 회전 90/270이면 가로세로 swap (시각 표시용)
            if (f.Rotation == LabelFieldRotation.Rotate90 || f.Rotation == LabelFieldRotation.Rotate270)
            {
                var tmp = w; w = h; h = tmp;
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
            try
            {
                DoPaint(e);
            }
            catch (Exception ex)
            {
                // Paint에서 예외가 폼 밖으로 propagate되면 무한 재페인트 → 앱 다운.
                // status에만 표시하고 화면은 비워둠.
                SetStatus("렌더링 오류: " + ex.Message);
            }
        }

        private void DoPaint(PaintEventArgs e)
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
                // 드래그 중이면 다중 선택된 모두 옅게 (원본 잔영), ghost는 별도
                var isInSelection = _selectedFields.Contains(field);
                var isDraggingThis = _isDragging && isInSelection;
                DrawField(g, field, isInSelection, faded: isDraggingThis);
            }

            // 선택된 박스에 리사이즈 핸들 표시 (단일 선택일 때만)
            if (!_isDragging && _selectedFields.Count == 1 && _selectedField != null && _selectedField.FieldType == LabelFieldType.Box)
            {
                DrawResizeHandles(g, FieldRect(_selectedField));
            }

            // Rubber band 사각형
            if (_isRubberBand && _rubberRect.Width > 0 && _rubberRect.Height > 0)
            {
                using (var rbFill = new SolidBrush(Color.FromArgb(40, Color.DodgerBlue)))
                    g.FillRectangle(rbFill, _rubberRect);
                using (var rbPen = new Pen(Color.DodgerBlue, 1f / (float)_canvasZoom))
                {
                    rbPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawRectangle(rbPen, _rubberRect);
                }
            }

            // 다중 이동 ghost — Move 모드이고 _multiDragStart에 여러 개일 때 각 필드별 ghost 표시
            if (_isDragging && _dragMode == DragMode.Move && _multiDragStart.Count > 1 && _selectedField != null)
            {
                var anchorStart = _multiDragStart.ContainsKey(_selectedField)
                    ? _multiDragStart[_selectedField]
                    : new Point(_selectedField.X, _selectedField.Y);
                var dx = _ghostRect.X - anchorStart.X;
                var dy = _ghostRect.Y - anchorStart.Y;
                using (var ghostFill = new SolidBrush(Color.FromArgb(30, Color.DodgerBlue)))
                using (var ghostPen = new Pen(Color.DodgerBlue, 1.5f / (float)_canvasZoom))
                {
                    ghostPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    foreach (var kv in _multiDragStart)
                    {
                        var origRect = FieldRect(kv.Key);
                        var ghostR = new Rectangle(kv.Value.X + dx, kv.Value.Y + dy, origRect.Width, origRect.Height);
                        g.FillRectangle(ghostFill, ghostR);
                        g.DrawRectangle(ghostPen, ghostR);
                    }
                }
                return;
            }

            // 드래그 중이면 ghost(목적지/새 크기 미리보기) + 좌표 floating 라벨
            if (_isDragging && _selectedField != null)
            {
                var ghost = _ghostRect;
                var fr = FieldRect(_selectedField);

                // 이동 모드일 때만 원본 → ghost 연결선
                if (_dragMode == DragMode.Move)
                {
                    using (var conn = new Pen(Color.FromArgb(120, Color.DodgerBlue), 1f / (float)_canvasZoom))
                    {
                        conn.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        var srcCenter = new Point(fr.X + fr.Width / 2, fr.Y + fr.Height / 2);
                        var dstCenter = new Point(ghost.X + ghost.Width / 2, ghost.Y + ghost.Height / 2);
                        g.DrawLine(conn, srcCenter, dstCenter);
                    }
                }

                // Ghost 사각형
                var ghostPenWidth = 2.5f / (float)_canvasZoom;
                using (var ghostFill = new SolidBrush(Color.FromArgb(30, Color.DodgerBlue)))
                    g.FillRectangle(ghostFill, ghost);
                using (var ghostPen = new Pen(Color.DodgerBlue, ghostPenWidth))
                {
                    ghostPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawRectangle(ghostPen, ghost);
                }
                // 리사이즈 중에는 ghost에 핸들도 같이
                if (_dragMode != DragMode.Move && _selectedField.FieldType == LabelFieldType.Box)
                    DrawResizeHandles(g, ghost);

                // floating 라벨: 이동이면 X/Y, 리사이즈면 W x H도 추가
                var mmPerDot = MmPerDot();
                string info;
                if (_dragMode == DragMode.Move)
                {
                    info = string.Format("X={0} ({1:F1}mm)  Y={2} ({3:F1}mm)",
                        ghost.X, ghost.X * mmPerDot, ghost.Y, ghost.Y * mmPerDot);
                }
                else
                {
                    info = string.Format("{0} × {1} dots  ({2:F1} × {3:F1}mm)",
                        ghost.Width, ghost.Height, ghost.Width * mmPerDot, ghost.Height * mmPerDot);
                }
                using (var font = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var back = new SolidBrush(Color.FromArgb(220, Color.Black)))
                using (var fore = new SolidBrush(Color.White))
                {
                    var size = g.MeasureString(info, font);
                    var pad = 4;
                    var rectAbove = new RectangleF(ghost.X, Math.Max(0, ghost.Y - size.Height - pad * 2), size.Width + pad * 2, size.Height + pad);
                    g.FillRectangle(back, rectAbove);
                    g.DrawString(info, font, fore, rectAbove.X + pad, rectAbove.Y + pad / 2);
                }
            }
        }

        private void DrawResizeHandles(Graphics g, Rectangle r)
        {
            // 핸들 크기 ~ 화면 8px (라벨 dots 기준 8/zoom)
            var handleSize = Math.Max(4, 8f / (float)_canvasZoom);
            var half = handleSize / 2f;
            var points = new[]
            {
                new PointF(r.X, r.Y),                                  // NW
                new PointF(r.X + r.Width / 2f, r.Y),                   // N
                new PointF(r.Right, r.Y),                              // NE
                new PointF(r.X, r.Y + r.Height / 2f),                  // W
                new PointF(r.Right, r.Y + r.Height / 2f),              // E
                new PointF(r.X, r.Bottom),                             // SW
                new PointF(r.X + r.Width / 2f, r.Bottom),              // S
                new PointF(r.Right, r.Bottom)                          // SE
            };
            using (var fill = new SolidBrush(Color.White))
            using (var border = new Pen(Color.DodgerBlue, 1f / (float)_canvasZoom))
            {
                foreach (var p in points)
                {
                    var rect = new RectangleF(p.X - half, p.Y - half, handleSize, handleSize);
                    g.FillRectangle(fill, rect);
                    g.DrawRectangle(border, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
        }

        private void DrawField(Graphics g, LabelField field, bool selected, bool faded = false)
        {
            var r = FieldRect(field);

            // 이미지 필드는 실제 이미지를 캔버스에 그림 (없으면 회색 박스)
            if (field.FieldType == LabelFieldType.Image && !string.IsNullOrEmpty(field.ImagePath) && File.Exists(field.ImagePath))
            {
                try
                {
                    using (var img = Image.FromFile(field.ImagePath))
                    {
                        var attrs = new System.Drawing.Imaging.ImageAttributes();
                        if (faded) attrs.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.4f });
                        g.DrawImage(img, r, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attrs);
                    }
                }
                catch { /* 손상된 이미지 — 아래 박스 그리기로 fallback */ }
            }

            var fillAlpha = faded ? 15 : 40;
            var fillColor = selected ? Color.DodgerBlue : Color.Gray;
            using (var fill = new SolidBrush(Color.FromArgb(fillAlpha, fillColor)))
                g.FillRectangle(fill, r);
            // 펜 굵기는 줌과 무관하게 화면 픽셀로 일정
            var penWidth = (selected ? 2f : 1f) / (float)_canvasZoom;
            var penColor = selected
                ? (faded ? Color.FromArgb(120, Color.DodgerBlue) : Color.DodgerBlue)
                : Color.DimGray;
            using (var pen = new Pen(penColor, penWidth))
            {
                if (!selected || faded) pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, r);
            }
            var label = "[" + field.FieldType + "] " + (field.Name ?? "");
            if (!string.IsNullOrEmpty(field.DataBindingKey)) label += " {" + field.DataBindingKey + "}";
            if (field.Rotation != LabelFieldRotation.Normal) label += " ↻" + (int)field.Rotation;
            if (field.BoldStrength > 0) label += " B" + field.BoldStrength;
            // 라벨 폰트는 라벨 dots 좌표계 — 줌이 알아서 스케일
            var fontSizeDots = 12f;
            var textAlpha = faded ? 100 : 255;
            using (var fontBrush = new SolidBrush(Color.FromArgb(textAlpha, Color.Black)))
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

            // 1) 선택된 박스의 리사이즈 핸들 hit test 먼저 (단일 선택 시만)
            if (_selectedFields.Count == 1 && _selectedField != null && _selectedField.FieldType == LabelFieldType.Box)
            {
                var handleMode = HitTestHandle(e.Location, _selectedField);
                if (handleMode != DragMode.None)
                {
                    _dragMode = handleMode;
                    _isDragging = true;
                    _dragStartCanvas = e.Location;
                    _resizeStartRect = FieldRect(_selectedField);
                    _ghostRect = _resizeStartRect;
                    return;
                }
            }

            // 2) 일반 필드 hit
            var hit = HitTest(e.Location);
            var ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            var shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            if (hit != null)
            {
                if (ctrl)
                {
                    // Ctrl+클릭: 선택 토글
                    ToggleSelectField(hit);
                    return;
                }
                // 이미 다중선택 안에 포함된 필드 클릭 → 다중 드래그
                if (_selectedFields.Contains(hit) && _selectedFields.Count > 1)
                {
                    _dragMode = DragMode.Move;
                    _isDragging = true;
                    _dragStartCanvas = e.Location;
                    _resizeStartRect = FieldRect(hit);
                    _ghostRect = _resizeStartRect;
                    _multiDragStart.Clear();
                    foreach (var f in _selectedFields) _multiDragStart[f] = new Point(f.X, f.Y);
                    return;
                }
                // 일반 클릭: 단일 선택 후 이동 준비
                SelectField(hit);
                _dragMode = DragMode.Move;
                _isDragging = true;
                _dragStartCanvas = e.Location;
                _resizeStartRect = FieldRect(hit);
                _ghostRect = _resizeStartRect;
                _multiDragStart.Clear();
                _multiDragStart[hit] = new Point(hit.X, hit.Y);
            }
            else
            {
                // 빈 영역 클릭 → rubber band 시작 (Shift/Ctrl이면 기존 선택 유지)
                if (!ctrl && !shift) SelectField(null);
                _isRubberBand = true;
                _rubberStartLabel = CanvasToLabel(e.Location);
                _rubberRect = new Rectangle(_rubberStartLabel.X, _rubberStartLabel.Y, 0, 0);
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

            // Rubber band 갱신
            if (_isRubberBand)
            {
                var cur = CanvasToLabel(e.Location);
                _rubberRect = new Rectangle(
                    Math.Min(_rubberStartLabel.X, cur.X),
                    Math.Min(_rubberStartLabel.Y, cur.Y),
                    Math.Abs(cur.X - _rubberStartLabel.X),
                    Math.Abs(cur.Y - _rubberStartLabel.Y));
                pnlCanvas.Invalidate();
                return;
            }

            if (!_isDragging)
            {
                // 드래그 안 할 때는 hover에 따라 커서 변경
                pnlCanvas.Cursor = GetHoverCursor(e.Location);
                return;
            }

            // 화면 픽셀 이동량을 라벨 dots로 환산
            var dxDots = (int)Math.Round((e.X - _dragStartCanvas.X) / _canvasZoom);
            var dyDots = (int)Math.Round((e.Y - _dragStartCanvas.Y) / _canvasZoom);
            var snap = (Control.ModifierKeys & Keys.Shift) != Keys.Shift;

            _ghostRect = ComputeGhostRect(_dragMode, _resizeStartRect, dxDots, dyDots, snap);
            pnlCanvas.Invalidate();
        }

        private void pnlCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            // Rubber band 완료
            if (_isRubberBand)
            {
                _isRubberBand = false;
                if (_rubberRect.Width >= 3 && _rubberRect.Height >= 3)
                {
                    var inRect = _template.Fields.Where(f => FieldRect(f).IntersectsWith(_rubberRect)).ToList();
                    var additive = (Control.ModifierKeys & (Keys.Control | Keys.Shift)) != Keys.None;
                    SelectMultiple(inRect, additive);
                }
                _rubberRect = Rectangle.Empty;
                pnlCanvas.Invalidate();
                return;
            }

            if (!_isDragging) return;
            _isDragging = false;
            var mode = _dragMode;
            _dragMode = DragMode.None;

            if (_selectedField == null)
            {
                pnlCanvas.Invalidate();
                return;
            }

            var changed = false;
            // 다중 이동 (Move 모드이고 _multiDragStart에 여러 개)
            if (mode == DragMode.Move && _multiDragStart.Count > 1)
            {
                var anchor = _selectedField; // 클릭한 필드 (가장 마지막에 _resizeStartRect로 잡힘)
                var anchorStart = _multiDragStart.ContainsKey(anchor) ? _multiDragStart[anchor] : new Point(anchor.X, anchor.Y);
                var dx = _ghostRect.X - anchorStart.X;
                var dy = _ghostRect.Y - anchorStart.Y;
                if (dx != 0 || dy != 0)
                {
                    foreach (var kv in _multiDragStart)
                    {
                        kv.Key.X = Math.Max(0, kv.Value.X + dx);
                        kv.Key.Y = Math.Max(0, kv.Value.Y + dy);
                    }
                    changed = true;
                }
            }
            else
            {
                // 단일 이동 또는 리사이즈
                if (_ghostRect.X != _selectedField.X) { _selectedField.X = _ghostRect.X; changed = true; }
                if (_ghostRect.Y != _selectedField.Y) { _selectedField.Y = _ghostRect.Y; changed = true; }
                if (mode != DragMode.Move && _selectedField.FieldType == LabelFieldType.Box)
                {
                    if (_ghostRect.Width != _selectedField.Width) { _selectedField.Width = _ghostRect.Width; changed = true; }
                    if (_ghostRect.Height != _selectedField.Height) { _selectedField.Height = _ghostRect.Height; changed = true; }
                }
            }
            _multiDragStart.Clear();

            if (changed)
            {
                pgFieldProps.Refresh();
                RegenerateZplFromTemplate();
                MarkDirty();
            }
            else
            {
                pnlCanvas.Invalidate();
            }
        }

        private const int MinFieldSize = 5;

        private Rectangle ComputeGhostRect(DragMode mode, Rectangle start, int dx, int dy, bool snap)
        {
            int x = start.X, y = start.Y, w = start.Width, h = start.Height;
            switch (mode)
            {
                case DragMode.Move:
                    x = Math.Max(0, start.X + dx);
                    y = Math.Max(0, start.Y + dy);
                    x = SnapToGrid(x, snap);
                    y = SnapToGrid(y, snap);
                    break;
                case DragMode.ResizeE:
                    w = Math.Max(MinFieldSize, start.Width + dx);
                    w = SnapToGrid(w, snap);
                    break;
                case DragMode.ResizeS:
                    h = Math.Max(MinFieldSize, start.Height + dy);
                    h = SnapToGrid(h, snap);
                    break;
                case DragMode.ResizeSE:
                    w = Math.Max(MinFieldSize, start.Width + dx);
                    h = Math.Max(MinFieldSize, start.Height + dy);
                    w = SnapToGrid(w, snap); h = SnapToGrid(h, snap);
                    break;
                case DragMode.ResizeW:
                    {
                        var newX = Math.Min(start.Right - MinFieldSize, Math.Max(0, start.X + dx));
                        newX = SnapToGrid(newX, snap);
                        w = start.Right - newX;
                        x = newX;
                    }
                    break;
                case DragMode.ResizeN:
                    {
                        var newY = Math.Min(start.Bottom - MinFieldSize, Math.Max(0, start.Y + dy));
                        newY = SnapToGrid(newY, snap);
                        h = start.Bottom - newY;
                        y = newY;
                    }
                    break;
                case DragMode.ResizeNW:
                    {
                        var newX = Math.Min(start.Right - MinFieldSize, Math.Max(0, start.X + dx));
                        var newY = Math.Min(start.Bottom - MinFieldSize, Math.Max(0, start.Y + dy));
                        newX = SnapToGrid(newX, snap); newY = SnapToGrid(newY, snap);
                        w = start.Right - newX; h = start.Bottom - newY;
                        x = newX; y = newY;
                    }
                    break;
                case DragMode.ResizeNE:
                    {
                        var newY = Math.Min(start.Bottom - MinFieldSize, Math.Max(0, start.Y + dy));
                        newY = SnapToGrid(newY, snap);
                        h = start.Bottom - newY;
                        y = newY;
                        w = Math.Max(MinFieldSize, start.Width + dx);
                        w = SnapToGrid(w, snap);
                    }
                    break;
                case DragMode.ResizeSW:
                    {
                        var newX = Math.Min(start.Right - MinFieldSize, Math.Max(0, start.X + dx));
                        newX = SnapToGrid(newX, snap);
                        w = start.Right - newX;
                        x = newX;
                        h = Math.Max(MinFieldSize, start.Height + dy);
                        h = SnapToGrid(h, snap);
                    }
                    break;
            }
            return new Rectangle(x, y, w, h);
        }

        private DragMode HitTestHandle(Point canvasPt, LabelField field)
        {
            var r = FieldRect(field);
            var labelPt = CanvasToLabel(canvasPt);
            var radius = Math.Max(3, (int)Math.Round(5 / _canvasZoom));

            bool Near(int x, int y) =>
                Math.Abs(labelPt.X - x) <= radius && Math.Abs(labelPt.Y - y) <= radius;

            // 모서리 4개 우선
            if (Near(r.X, r.Y)) return DragMode.ResizeNW;
            if (Near(r.Right, r.Y)) return DragMode.ResizeNE;
            if (Near(r.X, r.Bottom)) return DragMode.ResizeSW;
            if (Near(r.Right, r.Bottom)) return DragMode.ResizeSE;
            // 변 중점 4개
            if (Near(r.X + r.Width / 2, r.Y)) return DragMode.ResizeN;
            if (Near(r.X + r.Width / 2, r.Bottom)) return DragMode.ResizeS;
            if (Near(r.X, r.Y + r.Height / 2)) return DragMode.ResizeW;
            if (Near(r.Right, r.Y + r.Height / 2)) return DragMode.ResizeE;
            return DragMode.None;
        }

        private Cursor GetHoverCursor(Point canvasPt)
        {
            if (_selectedField != null && _selectedField.FieldType == LabelFieldType.Box)
            {
                switch (HitTestHandle(canvasPt, _selectedField))
                {
                    case DragMode.ResizeNW:
                    case DragMode.ResizeSE: return Cursors.SizeNWSE;
                    case DragMode.ResizeNE:
                    case DragMode.ResizeSW: return Cursors.SizeNESW;
                    case DragMode.ResizeN:
                    case DragMode.ResizeS: return Cursors.SizeNS;
                    case DragMode.ResizeE:
                    case DragMode.ResizeW: return Cursors.SizeWE;
                }
            }
            var hit = HitTest(canvasPt);
            return hit != null ? Cursors.SizeAll : Cursors.Default;
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            if (_selectedField == null) { SetStatus("회전할 필드를 선택하세요"); return; }
            RotateSelectedField();
        }

        private void RotateSelectedField()
        {
            if (_selectedField == null) return;
            // Normal → 90 → 180 → 270 → Normal 순환
            switch (_selectedField.Rotation)
            {
                case LabelFieldRotation.Normal: _selectedField.Rotation = LabelFieldRotation.Rotate90; break;
                case LabelFieldRotation.Rotate90: _selectedField.Rotation = LabelFieldRotation.Rotate180; break;
                case LabelFieldRotation.Rotate180: _selectedField.Rotation = LabelFieldRotation.Rotate270; break;
                default: _selectedField.Rotation = LabelFieldRotation.Normal; break;
            }
            pgFieldProps.Refresh();
            pnlCanvas.Invalidate();
            RegenerateZplFromTemplate();
            MarkDirty();
            SetStatus("회전: " + _selectedField.Name + " → " + _selectedField.Rotation);
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (_template.Fields.Count == 0) { SetStatus("이미 비어있음"); return; }
            var result = MessageBox.Show(this,
                "모든 필드(" + _template.Fields.Count + "개)를 삭제할까요?",
                "전체 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            _template.Fields.Clear();
            SelectField(null);
            RebuildDataBindings();
            RegenerateZplFromTemplate();
            MarkDirty();
            SetStatus("전체 삭제 완료");
        }

        private void btnSaveTemplate_Click(object sender, EventArgs e)
        {
            SaveCurrent();
        }

        // 현재 파일에 직접 저장. 처음이면 SaveAs 다이얼로그. true=저장 성공, false=취소/실패
        private bool SaveCurrent()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return SaveAs();
            }
            return SaveToPath(_currentFilePath);
        }

        private bool SaveAs()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter = "Zebra 라벨 템플릿 (*.zlbl)|*.zlbl|모든 파일 (*.*)|*.*",
                DefaultExt = "zlbl",
                FileName = string.IsNullOrEmpty(_currentFilePath)
                    ? (_template.TemplateCode ?? "label") + ".zlbl"
                    : Path.GetFileName(_currentFilePath)
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return false;
                return SaveToPath(dlg.FileName);
            }
        }

        private bool SaveToPath(string path)
        {
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LabelTemplate));
                using (var w = File.Create(path))
                {
                    serializer.Serialize(w, _template);
                }
                _currentFilePath = path;
                ClearDirty();
                SetStatus("저장 완료: " + path);
                return true;
            }
            catch (Exception ex)
            {
                ShowError("저장 실패", ex);
                return false;
            }
        }

        private void btnLoadTemplate_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Filter = "Zebra 라벨 템플릿 (*.zlbl)|*.zlbl|모든 파일 (*.*)|*.*",
                DefaultExt = "zlbl"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LabelTemplate));
                    using (var r = File.OpenRead(dlg.FileName))
                    {
                        var loaded = (LabelTemplate)serializer.Deserialize(r);
                        if (loaded == null) throw new InvalidOperationException("파일에서 템플릿을 읽지 못함");
                        if (loaded.Fields == null) loaded.Fields = new System.Collections.Generic.List<LabelField>();
                        _template = loaded;
                    }
                    // 로드 후 UI 동기화
                    SelectField(null);
                    ResizeCanvasToLabel();
                    _suppressLabelSizeHandler = true;
                    try
                    {
                        numLabelWidth.Value = (decimal)DotsToUnit(_template.WidthDots);
                        numLabelHeight.Value = (decimal)DotsToUnit(_template.HeightDots);
                        numCopies.Value = Math.Max(1, _template.Copies);
                    }
                    finally { _suppressLabelSizeHandler = false; }
                    RebuildDataBindings();
                    RegenerateZplFromTemplate();
                    pnlCanvas.Invalidate();
                    _currentFilePath = dlg.FileName;
                    ClearDirty();
                    SetStatus("불러오기 완료: " + dlg.FileName + " (필드 " + _template.Fields.Count + "개)");
                }
                catch (Exception ex)
                {
                    ShowError("불러오기 실패", ex);
                }
            }
        }

        private void btnZplToDesigner_Click(object sender, EventArgs e)
        {
            var zpl = txtZpl.Text;
            if (string.IsNullOrWhiteSpace(zpl)) { SetStatus("변환할 ZPL이 비어있음"); return; }

            if (_template.Fields.Count > 0)
            {
                var r = MessageBox.Show(this,
                    "현재 디자인을 ZPL 파싱 결과로 대체할까요?\n(실행 취소 Ctrl+Z 로 되돌릴 수 있음)",
                    "ZPL → 디자이너", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
            }

            try
            {
                var parsed = ZplParser.Parse(zpl);
                _template = parsed;
                SelectField(null);
                ResizeCanvasToLabel();
                _suppressLabelSizeHandler = true;
                try
                {
                    cmbSizePreset.SelectedIndex = 0;
                    numLabelWidth.Value = ClampNum(numLabelWidth, (decimal)DotsToUnit(_template.WidthDots));
                    numLabelHeight.Value = ClampNum(numLabelHeight, (decimal)DotsToUnit(_template.HeightDots));
                    numCopies.Value = Math.Max(1, _template.Copies);
                }
                finally { _suppressLabelSizeHandler = false; }
                RebuildDataBindings();
                RegenerateZplFromTemplate();
                pnlCanvas.Invalidate();
                MarkDirty();
                tabRight.SelectedTab = tabDesigner;
                SetStatus("ZPL 파싱 완료 — 필드 " + _template.Fields.Count + "개 생성");
            }
            catch (Exception ex)
            {
                ShowError("ZPL 파싱 실패", ex);
            }
        }

        private void btnExportCode_Click(object sender, EventArgs e)
        {
            using (var dlg = new CodeExportForm(_template))
            {
                dlg.ShowDialog(this);
            }
        }

        private void btnBringToFront_Click(object sender, EventArgs e)
        {
            if (_selectedField == null) { SetStatus("선택 후 사용"); return; }
            _template.Fields.Remove(_selectedField);
            _template.Fields.Add(_selectedField);
            pnlCanvas.Invalidate();
            RegenerateZplFromTemplate();
            MarkDirty();
            SetStatus("맨 위로 이동: " + _selectedField.Name);
        }

        private void btnSendToBack_Click(object sender, EventArgs e)
        {
            if (_selectedField == null) { SetStatus("선택 후 사용"); return; }
            _template.Fields.Remove(_selectedField);
            _template.Fields.Insert(0, _selectedField);
            pnlCanvas.Invalidate();
            RegenerateZplFromTemplate();
            MarkDirty();
            SetStatus("맨 뒤로 이동: " + _selectedField.Name);
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
            // 전역 단축키 — 어느 탭에서든 동작
            if (e.Control && !e.Alt)
            {
                if (!e.Shift && e.KeyCode == Keys.S) { SaveCurrent(); e.Handled = true; return; }
                if (!e.Shift && e.KeyCode == Keys.Z) { Undo(); e.Handled = true; return; }
                if (e.Shift && e.KeyCode == Keys.Z) { Redo(); e.Handled = true; return; }
                if (!e.Shift && e.KeyCode == Keys.Y) { Redo(); e.Handled = true; return; }
            }

            // 디자이너 탭에서만 동작
            if (tabRight.SelectedTab != tabDesigner) return;

            // Ctrl+C/V/A — 디자이너에서만
            if (e.Control && !e.Alt && !e.Shift)
            {
                if (e.KeyCode == Keys.C) { CopySelected(); e.Handled = true; return; }
                if (e.KeyCode == Keys.V) { Paste(); e.Handled = true; return; }
                if (e.KeyCode == Keys.A)
                {
                    SelectMultiple(_template.Fields, additive: false);
                    e.Handled = true;
                    return;
                }
            }

            if (e.KeyCode == Keys.Delete && _selectedFields.Count > 0)
            {
                btnDeleteField_Click(sender, EventArgs.Empty);
                e.Handled = true;
                return;
            }

            // 화살표 — 1 dot 미세 이동 (Shift는 10 dots), 다중선택이면 모두 이동
            if (_selectedFields.Count > 0 && (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                var step = (e.Modifiers & Keys.Shift) == Keys.Shift ? SnapGrid : 1;
                var dx = e.KeyCode == Keys.Left ? -step : e.KeyCode == Keys.Right ? step : 0;
                var dy = e.KeyCode == Keys.Up ? -step : e.KeyCode == Keys.Down ? step : 0;
                foreach (var f in _selectedFields)
                {
                    f.X = Math.Max(0, f.X + dx);
                    f.Y = Math.Max(0, f.Y + dy);
                }
                pnlCanvas.Invalidate();
                pgFieldProps.Refresh();
                RegenerateZplFromTemplate();
                MarkDirty();
                e.Handled = true;
            }
        }

        private void SelectField(LabelField field)
        {
            _selectedField = field;
            _selectedFields.Clear();
            if (field != null) _selectedFields.Add(field);
            pgFieldProps.SelectedObject = field;
            pnlCanvas.Invalidate();
            SetStatus(field != null
                ? "선택: [" + field.FieldType + "] " + (field.Name ?? "")
                : "선택 해제");
        }

        private void ToggleSelectField(LabelField field)
        {
            if (field == null) return;
            if (_selectedFields.Contains(field)) _selectedFields.Remove(field);
            else _selectedFields.Add(field);
            _selectedField = _selectedFields.LastOrDefault();
            pgFieldProps.SelectedObjects = _selectedFields.Cast<object>().ToArray();
            pnlCanvas.Invalidate();
            SetStatus(_selectedFields.Count + "개 선택");
        }

        private void SelectMultiple(IEnumerable<LabelField> fields, bool additive)
        {
            if (!additive) _selectedFields.Clear();
            foreach (var f in fields) _selectedFields.Add(f);
            _selectedField = _selectedFields.LastOrDefault();
            pgFieldProps.SelectedObjects = _selectedFields.Cast<object>().ToArray();
            pnlCanvas.Invalidate();
            SetStatus(_selectedFields.Count + "개 선택");
        }

        private void btnAddText_Click(object sender, EventArgs e) { AddField(LabelFieldType.Text); }
        private void btnAddBarcode_Click(object sender, EventArgs e) { AddField(LabelFieldType.Barcode128); }
        private void btnAddQr_Click(object sender, EventArgs e) { AddField(LabelFieldType.QrCode); }
        private void btnAddBox_Click(object sender, EventArgs e) { AddField(LabelFieldType.Box); }
        private void btnAddHLine_Click(object sender, EventArgs e) { AddLine(horizontal: true); }
        private void btnAddVLine_Click(object sender, EventArgs e) { AddLine(horizontal: false); }

        private void btnAddImage_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Filter = "이미지 (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|모든 파일 (*.*)|*.*",
                Title = "이미지 선택"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                int w = 200, h = 100;
                try
                {
                    using (var probe = Image.FromFile(dlg.FileName))
                    {
                        // 원본 비율 유지하며 최대 300x300에 맞춰 초기 크기 결정
                        var maxDim = 300;
                        var ratio = Math.Min((double)maxDim / probe.Width, (double)maxDim / probe.Height);
                        if (ratio > 1) ratio = 1;
                        w = Math.Max(10, (int)(probe.Width * ratio));
                        h = Math.Max(10, (int)(probe.Height * ratio));
                    }
                }
                catch { /* 비표준 이미지 — 기본 크기 사용 */ }

                var name = "Image" + (_template.Fields.Count + 1);
                var f = new LabelField
                {
                    Name = name,
                    FieldType = LabelFieldType.Image,
                    X = 50, Y = 50,
                    Width = w, Height = h,
                    ImagePath = dlg.FileName
                };
                _template.Fields.Add(f);
                SelectField(f);
                RebuildDataBindings();
                RegenerateZplFromTemplate();
                MarkDirty();
            }
        }

        private void AddLine(bool horizontal)
        {
            var name = (horizontal ? "HLine" : "VLine") + (_template.Fields.Count + 1);
            var f = new LabelField
            {
                Name = name,
                FieldType = LabelFieldType.Box, // ^GB 로 처리하기 위해 Box 타입 사용
                X = 50,
                Y = 50,
                Thickness = 3,
                Width = horizontal ? 300 : 3,
                Height = horizontal ? 3 : 200
                // 선/박스는 데이터 바인딩 없음 (DataBindingKey null)
            };
            _template.Fields.Add(f);
            SelectField(f);
            RebuildDataBindings();
            RegenerateZplFromTemplate();
            MarkDirty();
        }

        private void AddField(LabelFieldType type)
        {
            var name = type.ToString() + (_template.Fields.Count + 1);
            var f = new LabelField
            {
                Name = name,
                FieldType = type,
                X = 50,
                Y = 50,
                // 박스/라인 외에는 자동으로 데이터 바인딩 키 = Name → 좌측 입력란 자동 생성
                DataBindingKey = (type == LabelFieldType.Box || type == LabelFieldType.Line) ? null : name
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
            RebuildDataBindings();
            RegenerateZplFromTemplate();
            MarkDirty();
        }

        private void btnDeleteField_Click(object sender, EventArgs e)
        {
            if (_selectedFields.Count == 0) { SetStatus("삭제할 필드를 먼저 선택하세요"); return; }
            var toDelete = _selectedFields.ToList();
            foreach (var f in toDelete) _template.Fields.Remove(f);
            SelectField(null);
            RebuildDataBindings();
            RegenerateZplFromTemplate();
            MarkDirty();
            SetStatus("삭제: " + toDelete.Count + "개 필드");
        }

        private void pgFieldProps_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            pnlCanvas.Invalidate();
            // DisplayName 한국어로 바꿨으니 label 비교 대신 PropertyDescriptor의 진짜 Name 사용
            var propName = e.ChangedItem?.PropertyDescriptor?.Name;
            if (propName == "DataBindingKey" || propName == "Name")
            {
                RebuildDataBindings();
            }
            RegenerateZplFromTemplate();
            MarkDirty();
        }
    }
}
