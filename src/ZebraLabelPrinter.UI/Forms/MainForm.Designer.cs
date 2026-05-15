namespace ZebraLabelPrinter.UI.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.grpData = new System.Windows.Forms.GroupBox();
            this.pnlDataBindings = new System.Windows.Forms.Panel();
            this.grpLabelSize = new System.Windows.Forms.GroupBox();
            this.numLabelWidth = new System.Windows.Forms.NumericUpDown();
            this.lblLabelWidth = new System.Windows.Forms.Label();
            this.lblLabelTimes = new System.Windows.Forms.Label();
            this.numLabelHeight = new System.Windows.Forms.NumericUpDown();
            this.lblLabelHeight = new System.Windows.Forms.Label();
            this.cmbLabelUnit = new System.Windows.Forms.ComboBox();
            this.grpPrinter = new System.Windows.Forms.GroupBox();
            this.numCopies = new System.Windows.Forms.NumericUpDown();
            this.lblCopies = new System.Windows.Forms.Label();
            this.btnRefreshPrinters = new System.Windows.Forms.Button();
            this.cmbPrinter = new System.Windows.Forms.ComboBox();
            this.lblPrinter = new System.Windows.Forms.Label();
            this.tabRight = new System.Windows.Forms.TabControl();
            this.tabPreview = new System.Windows.Forms.TabPage();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.tabZpl = new System.Windows.Forms.TabPage();
            this.txtZpl = new System.Windows.Forms.TextBox();
            this.tabDesigner = new System.Windows.Forms.TabPage();
            this.splitDesigner = new System.Windows.Forms.SplitContainer();
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.pgFieldProps = new System.Windows.Forms.PropertyGrid();
            this.toolStripDesigner = new System.Windows.Forms.ToolStrip();
            this.btnAddText = new System.Windows.Forms.ToolStripButton();
            this.btnAddBarcode = new System.Windows.Forms.ToolStripButton();
            this.btnAddQr = new System.Windows.Forms.ToolStripButton();
            this.btnAddBox = new System.Windows.Forms.ToolStripButton();
            this.btnAddHLine = new System.Windows.Forms.ToolStripButton();
            this.btnAddVLine = new System.Windows.Forms.ToolStripButton();
            this.toolSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnBringToFront = new System.Windows.Forms.ToolStripButton();
            this.btnSendToBack = new System.Windows.Forms.ToolStripButton();
            this.btnRotate = new System.Windows.Forms.ToolStripButton();
            this.toolSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDeleteField = new System.Windows.Forms.ToolStripButton();
            this.btnClearAll = new System.Windows.Forms.ToolStripButton();
            this.toolSep3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSaveTemplate = new System.Windows.Forms.ToolStripButton();
            this.btnLoadTemplate = new System.Windows.Forms.ToolStripButton();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.grpData.SuspendLayout();
            this.grpLabelSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelHeight)).BeginInit();
            this.grpPrinter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCopies)).BeginInit();
            this.tabRight.SuspendLayout();
            this.tabPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.tabZpl.SuspendLayout();
            this.tabDesigner.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDesigner)).BeginInit();
            this.splitDesigner.Panel1.SuspendLayout();
            this.splitDesigner.Panel2.SuspendLayout();
            this.splitDesigner.SuspendLayout();
            this.toolStripDesigner.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Panel1.Controls.Add(this.panelLeft);
            this.splitContainer.Panel2.Controls.Add(this.tabRight);
            this.splitContainer.Size = new System.Drawing.Size(1100, 678);
            this.splitContainer.SplitterDistance = 360;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.IsSplitterFixed = true;
            this.splitContainer.SplitterWidth = 1;
            this.splitContainer.TabIndex = 0;
            //
            // panelLeft
            //
            this.panelLeft.Controls.Add(this.btnPrint);
            this.panelLeft.Controls.Add(this.btnPreview);
            this.panelLeft.Controls.Add(this.btnGenerate);
            this.panelLeft.Controls.Add(this.grpData);
            this.panelLeft.Controls.Add(this.grpLabelSize);
            this.panelLeft.Controls.Add(this.grpPrinter);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(12);
            this.panelLeft.Size = new System.Drawing.Size(360, 678);
            this.panelLeft.TabIndex = 0;
            //
            // btnPrint
            //
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPrint.Location = new System.Drawing.Point(12, 615);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(330, 44);
            this.btnPrint.TabIndex = 4;
            this.btnPrint.Text = "프린터로 전송";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            //
            // btnPreview
            //
            this.btnPreview.Location = new System.Drawing.Point(180, 565);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(162, 40);
            this.btnPreview.TabIndex = 3;
            this.btnPreview.Text = "미리보기";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            //
            // btnGenerate
            //
            this.btnGenerate.Location = new System.Drawing.Point(12, 565);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(162, 40);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "ZPL 생성";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            //
            // grpData (동적 데이터 바인딩 입력 — MainForm.RebuildDataBindings에서 컨트롤 생성)
            //
            this.grpData.Controls.Add(this.pnlDataBindings);
            this.grpData.Location = new System.Drawing.Point(12, 305);
            this.grpData.Name = "grpData";
            this.grpData.Size = new System.Drawing.Size(330, 245);
            this.grpData.TabIndex = 1;
            this.grpData.TabStop = false;
            this.grpData.Text = "데이터 바인딩";
            //
            // pnlDataBindings — 동적 입력 컨테이너 (스크롤 가능)
            //
            this.pnlDataBindings.AutoScroll = true;
            this.pnlDataBindings.Location = new System.Drawing.Point(5, 20);
            this.pnlDataBindings.Name = "pnlDataBindings";
            this.pnlDataBindings.Size = new System.Drawing.Size(320, 220);
            this.pnlDataBindings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlDataBindings.TabIndex = 0;
            //
            // grpPrinter
            //
            this.grpPrinter.Controls.Add(this.numCopies);
            this.grpPrinter.Controls.Add(this.lblCopies);
            this.grpPrinter.Controls.Add(this.btnRefreshPrinters);
            this.grpPrinter.Controls.Add(this.cmbPrinter);
            this.grpPrinter.Controls.Add(this.lblPrinter);
            this.grpPrinter.Location = new System.Drawing.Point(12, 15);
            this.grpPrinter.Name = "grpPrinter";
            this.grpPrinter.Size = new System.Drawing.Size(330, 200);
            this.grpPrinter.TabIndex = 0;
            this.grpPrinter.TabStop = false;
            this.grpPrinter.Text = "프린터 설정";
            //
            // grpLabelSize
            //
            this.grpLabelSize.Controls.Add(this.lblLabelWidth);
            this.grpLabelSize.Controls.Add(this.numLabelWidth);
            this.grpLabelSize.Controls.Add(this.lblLabelTimes);
            this.grpLabelSize.Controls.Add(this.lblLabelHeight);
            this.grpLabelSize.Controls.Add(this.numLabelHeight);
            this.grpLabelSize.Controls.Add(this.cmbLabelUnit);
            this.grpLabelSize.Location = new System.Drawing.Point(12, 225);
            this.grpLabelSize.Name = "grpLabelSize";
            this.grpLabelSize.Size = new System.Drawing.Size(330, 70);
            this.grpLabelSize.TabIndex = 5;
            this.grpLabelSize.TabStop = false;
            this.grpLabelSize.Text = "라벨 크기";
            //
            // lblLabelWidth
            //
            this.lblLabelWidth.AutoSize = true;
            this.lblLabelWidth.Location = new System.Drawing.Point(15, 32);
            this.lblLabelWidth.Name = "lblLabelWidth";
            this.lblLabelWidth.Size = new System.Drawing.Size(28, 15);
            this.lblLabelWidth.TabIndex = 0;
            this.lblLabelWidth.Text = "폭:";
            //
            // numLabelWidth
            //
            this.numLabelWidth.DecimalPlaces = 1;
            this.numLabelWidth.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            this.numLabelWidth.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this.numLabelWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numLabelWidth.Location = new System.Drawing.Point(50, 30);
            this.numLabelWidth.Name = "numLabelWidth";
            this.numLabelWidth.Size = new System.Drawing.Size(75, 23);
            this.numLabelWidth.TabIndex = 1;
            this.numLabelWidth.Value = new decimal(new int[] { 100, 0, 0, 0 });
            this.numLabelWidth.ValueChanged += new System.EventHandler(this.OnLabelSizeChanged);
            //
            // lblLabelTimes
            //
            this.lblLabelTimes.AutoSize = true;
            this.lblLabelTimes.Location = new System.Drawing.Point(130, 32);
            this.lblLabelTimes.Name = "lblLabelTimes";
            this.lblLabelTimes.Size = new System.Drawing.Size(12, 15);
            this.lblLabelTimes.TabIndex = 2;
            this.lblLabelTimes.Text = "×";
            //
            // lblLabelHeight
            //
            this.lblLabelHeight.AutoSize = true;
            this.lblLabelHeight.Location = new System.Drawing.Point(150, 32);
            this.lblLabelHeight.Name = "lblLabelHeight";
            this.lblLabelHeight.Size = new System.Drawing.Size(40, 15);
            this.lblLabelHeight.TabIndex = 3;
            this.lblLabelHeight.Text = "높이:";
            //
            // numLabelHeight
            //
            this.numLabelHeight.DecimalPlaces = 1;
            this.numLabelHeight.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            this.numLabelHeight.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this.numLabelHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numLabelHeight.Location = new System.Drawing.Point(195, 30);
            this.numLabelHeight.Name = "numLabelHeight";
            this.numLabelHeight.Size = new System.Drawing.Size(75, 23);
            this.numLabelHeight.TabIndex = 4;
            this.numLabelHeight.Value = new decimal(new int[] { 50, 0, 0, 0 });
            this.numLabelHeight.ValueChanged += new System.EventHandler(this.OnLabelSizeChanged);
            //
            // cmbLabelUnit
            //
            this.cmbLabelUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLabelUnit.Items.AddRange(new object[] { "mm", "cm" });
            this.cmbLabelUnit.Location = new System.Drawing.Point(275, 30);
            this.cmbLabelUnit.Name = "cmbLabelUnit";
            this.cmbLabelUnit.Size = new System.Drawing.Size(50, 23);
            this.cmbLabelUnit.TabIndex = 5;
            this.cmbLabelUnit.SelectedIndexChanged += new System.EventHandler(this.OnLabelUnitChanged);
            //
            // numCopies
            //
            this.numCopies.Location = new System.Drawing.Point(110, 150);
            this.numCopies.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numCopies.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numCopies.Name = "numCopies";
            this.numCopies.Size = new System.Drawing.Size(80, 23);
            this.numCopies.TabIndex = 4;
            this.numCopies.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // lblCopies
            //
            this.lblCopies.AutoSize = true;
            this.lblCopies.Location = new System.Drawing.Point(15, 153);
            this.lblCopies.Name = "lblCopies";
            this.lblCopies.Size = new System.Drawing.Size(40, 15);
            this.lblCopies.TabIndex = 3;
            this.lblCopies.Text = "매수:";
            //
            // btnRefreshPrinters
            //
            this.btnRefreshPrinters.Location = new System.Drawing.Point(15, 100);
            this.btnRefreshPrinters.Name = "btnRefreshPrinters";
            this.btnRefreshPrinters.Size = new System.Drawing.Size(120, 30);
            this.btnRefreshPrinters.TabIndex = 2;
            this.btnRefreshPrinters.Text = "프린터 새로고침";
            this.btnRefreshPrinters.UseVisualStyleBackColor = true;
            this.btnRefreshPrinters.Click += new System.EventHandler(this.btnRefreshPrinters_Click);
            //
            // cmbPrinter
            //
            this.cmbPrinter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrinter.DropDownWidth = 330;
            this.cmbPrinter.FormattingEnabled = true;
            this.cmbPrinter.Location = new System.Drawing.Point(15, 60);
            this.cmbPrinter.Name = "cmbPrinter";
            this.cmbPrinter.Size = new System.Drawing.Size(300, 23);
            this.cmbPrinter.TabIndex = 1;
            //
            // lblPrinter
            //
            this.lblPrinter.AutoSize = true;
            this.lblPrinter.Location = new System.Drawing.Point(15, 35);
            this.lblPrinter.Name = "lblPrinter";
            this.lblPrinter.Size = new System.Drawing.Size(80, 15);
            this.lblPrinter.TabIndex = 0;
            this.lblPrinter.Text = "프린터:";
            //
            // tabRight
            //
            this.tabRight.Controls.Add(this.tabPreview);
            this.tabRight.Controls.Add(this.tabZpl);
            this.tabRight.Controls.Add(this.tabDesigner);
            this.tabRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabRight.Location = new System.Drawing.Point(0, 0);
            this.tabRight.Name = "tabRight";
            this.tabRight.SelectedIndex = 0;
            this.tabRight.Size = new System.Drawing.Size(736, 678);
            this.tabRight.TabIndex = 0;
            //
            // tabPreview
            //
            this.tabPreview.AutoScroll = true;
            this.tabPreview.BackColor = System.Drawing.Color.LightGray;
            this.tabPreview.Controls.Add(this.picPreview);
            this.tabPreview.Location = new System.Drawing.Point(4, 24);
            this.tabPreview.Name = "tabPreview";
            this.tabPreview.Padding = new System.Windows.Forms.Padding(10);
            this.tabPreview.Size = new System.Drawing.Size(728, 650);
            this.tabPreview.TabIndex = 0;
            this.tabPreview.Text = "미리보기";
            //
            // picPreview — SizeMode=Zoom + 수동 Size로 _previewZoom 따라 PictureBox 크기 조정
            //
            this.picPreview.BackColor = System.Drawing.Color.White;
            this.picPreview.Location = new System.Drawing.Point(10, 10);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(1, 1);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            //
            // tabZpl
            //
            this.tabZpl.Controls.Add(this.txtZpl);
            this.tabZpl.Location = new System.Drawing.Point(4, 24);
            this.tabZpl.Name = "tabZpl";
            this.tabZpl.Padding = new System.Windows.Forms.Padding(3);
            this.tabZpl.Size = new System.Drawing.Size(728, 650);
            this.tabZpl.TabIndex = 1;
            this.tabZpl.Text = "ZPL 코드";
            this.tabZpl.UseVisualStyleBackColor = true;
            //
            // txtZpl
            //
            this.txtZpl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtZpl.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtZpl.Location = new System.Drawing.Point(3, 3);
            this.txtZpl.Multiline = true;
            this.txtZpl.Name = "txtZpl";
            this.txtZpl.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtZpl.Size = new System.Drawing.Size(722, 644);
            this.txtZpl.TabIndex = 0;
            this.txtZpl.WordWrap = false;
            //
            // tabDesigner
            //
            this.tabDesigner.Controls.Add(this.splitDesigner);
            this.tabDesigner.Controls.Add(this.toolStripDesigner);
            this.tabDesigner.Location = new System.Drawing.Point(4, 24);
            this.tabDesigner.Name = "tabDesigner";
            this.tabDesigner.Padding = new System.Windows.Forms.Padding(3);
            this.tabDesigner.Size = new System.Drawing.Size(728, 650);
            this.tabDesigner.TabIndex = 2;
            this.tabDesigner.Text = "디자이너";
            this.tabDesigner.UseVisualStyleBackColor = true;
            //
            // toolStripDesigner
            //
            this.toolStripDesigner.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.btnAddText, this.btnAddBarcode, this.btnAddQr, this.btnAddBox,
                this.btnAddHLine, this.btnAddVLine,
                this.toolSep1, this.btnBringToFront, this.btnSendToBack, this.btnRotate,
                this.toolSep2, this.btnDeleteField, this.btnClearAll,
                this.toolSep3, this.btnSaveTemplate, this.btnLoadTemplate });
            this.toolStripDesigner.Location = new System.Drawing.Point(3, 3);
            this.toolStripDesigner.Name = "toolStripDesigner";
            this.toolStripDesigner.Size = new System.Drawing.Size(722, 25);
            this.toolStripDesigner.TabIndex = 0;
            //
            // btnAddText
            //
            this.btnAddText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddText.Name = "btnAddText";
            this.btnAddText.Size = new System.Drawing.Size(75, 22);
            this.btnAddText.Text = "+ 텍스트";
            this.btnAddText.Click += new System.EventHandler(this.btnAddText_Click);
            //
            // btnAddBarcode
            //
            this.btnAddBarcode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddBarcode.Name = "btnAddBarcode";
            this.btnAddBarcode.Size = new System.Drawing.Size(75, 22);
            this.btnAddBarcode.Text = "+ 바코드";
            this.btnAddBarcode.Click += new System.EventHandler(this.btnAddBarcode_Click);
            //
            // btnAddQr
            //
            this.btnAddQr.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddQr.Name = "btnAddQr";
            this.btnAddQr.Size = new System.Drawing.Size(45, 22);
            this.btnAddQr.Text = "+ QR";
            this.btnAddQr.Click += new System.EventHandler(this.btnAddQr_Click);
            //
            // btnAddBox
            //
            this.btnAddBox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddBox.Name = "btnAddBox";
            this.btnAddBox.Size = new System.Drawing.Size(60, 22);
            this.btnAddBox.Text = "+ 박스";
            this.btnAddBox.Click += new System.EventHandler(this.btnAddBox_Click);
            //
            // btnAddHLine
            //
            this.btnAddHLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddHLine.Name = "btnAddHLine";
            this.btnAddHLine.Size = new System.Drawing.Size(75, 22);
            this.btnAddHLine.Text = "+ 가로선";
            this.btnAddHLine.Click += new System.EventHandler(this.btnAddHLine_Click);
            //
            // btnAddVLine
            //
            this.btnAddVLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddVLine.Name = "btnAddVLine";
            this.btnAddVLine.Size = new System.Drawing.Size(75, 22);
            this.btnAddVLine.Text = "+ 세로선";
            this.btnAddVLine.Click += new System.EventHandler(this.btnAddVLine_Click);
            //
            // toolSep1
            //
            this.toolSep1.Name = "toolSep1";
            this.toolSep1.Size = new System.Drawing.Size(6, 25);
            //
            // btnBringToFront
            //
            this.btnBringToFront.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnBringToFront.Name = "btnBringToFront";
            this.btnBringToFront.Size = new System.Drawing.Size(75, 22);
            this.btnBringToFront.Text = "맨 위로";
            this.btnBringToFront.ToolTipText = "선택 필드를 그리는 순서 맨 뒤로 이동 (시각적으로 맨 위)";
            this.btnBringToFront.Click += new System.EventHandler(this.btnBringToFront_Click);
            //
            // btnSendToBack
            //
            this.btnSendToBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSendToBack.Name = "btnSendToBack";
            this.btnSendToBack.Size = new System.Drawing.Size(75, 22);
            this.btnSendToBack.Text = "맨 뒤로";
            this.btnSendToBack.ToolTipText = "선택 필드를 그리는 순서 맨 앞으로 이동 (시각적으로 맨 뒤)";
            this.btnSendToBack.Click += new System.EventHandler(this.btnSendToBack_Click);
            //
            // btnRotate
            //
            this.btnRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRotate.Name = "btnRotate";
            this.btnRotate.Size = new System.Drawing.Size(65, 22);
            this.btnRotate.Text = "회전 ↻";
            this.btnRotate.ToolTipText = "선택 필드를 90도씩 회전 (Normal → 90 → 180 → 270 → ...)";
            this.btnRotate.Click += new System.EventHandler(this.btnRotate_Click);
            //
            // toolSep2
            //
            this.toolSep2.Name = "toolSep2";
            this.toolSep2.Size = new System.Drawing.Size(6, 25);
            //
            // btnDeleteField
            //
            this.btnDeleteField.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDeleteField.Name = "btnDeleteField";
            this.btnDeleteField.Size = new System.Drawing.Size(60, 22);
            this.btnDeleteField.Text = "삭제";
            this.btnDeleteField.ToolTipText = "선택 필드 삭제 (Delete 키)";
            this.btnDeleteField.Click += new System.EventHandler(this.btnDeleteField_Click);
            //
            // btnClearAll
            //
            this.btnClearAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(75, 22);
            this.btnClearAll.Text = "전체 삭제";
            this.btnClearAll.ToolTipText = "모든 필드를 한 번에 삭제 (확인 다이얼로그 뜸)";
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            //
            // toolSep3
            //
            this.toolSep3.Name = "toolSep3";
            this.toolSep3.Size = new System.Drawing.Size(6, 25);
            //
            // btnSaveTemplate
            //
            this.btnSaveTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSaveTemplate.Name = "btnSaveTemplate";
            this.btnSaveTemplate.Size = new System.Drawing.Size(60, 22);
            this.btnSaveTemplate.Text = "💾 저장";
            this.btnSaveTemplate.ToolTipText = "현재 디자이너 템플릿을 .zlbl 파일로 저장";
            this.btnSaveTemplate.Click += new System.EventHandler(this.btnSaveTemplate_Click);
            //
            // btnLoadTemplate
            //
            this.btnLoadTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLoadTemplate.Name = "btnLoadTemplate";
            this.btnLoadTemplate.Size = new System.Drawing.Size(75, 22);
            this.btnLoadTemplate.Text = "📂 불러오기";
            this.btnLoadTemplate.ToolTipText = ".zlbl 파일에서 디자이너 템플릿 복원";
            this.btnLoadTemplate.Click += new System.EventHandler(this.btnLoadTemplate_Click);
            //
            // splitDesigner
            //
            this.splitDesigner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDesigner.Location = new System.Drawing.Point(3, 28);
            this.splitDesigner.Name = "splitDesigner";
            this.splitDesigner.Panel1.AutoScroll = true;
            this.splitDesigner.Panel1.Controls.Add(this.pnlCanvas);
            this.splitDesigner.Panel2.Controls.Add(this.pgFieldProps);
            this.splitDesigner.Size = new System.Drawing.Size(722, 619);
            this.splitDesigner.SplitterDistance = 460;
            this.splitDesigner.TabIndex = 1;
            //
            // pnlCanvas
            //
            this.pnlCanvas.BackColor = System.Drawing.Color.LightGray;
            this.pnlCanvas.Location = new System.Drawing.Point(0, 0);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(840, 440);
            this.pnlCanvas.TabIndex = 0;
            this.pnlCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlCanvas_Paint);
            this.pnlCanvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlCanvas_MouseDown);
            this.pnlCanvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlCanvas_MouseMove);
            this.pnlCanvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlCanvas_MouseUp);
            //
            // pgFieldProps
            //
            this.pgFieldProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgFieldProps.Location = new System.Drawing.Point(0, 0);
            this.pgFieldProps.Name = "pgFieldProps";
            this.pgFieldProps.Size = new System.Drawing.Size(258, 619);
            this.pgFieldProps.TabIndex = 0;
            this.pgFieldProps.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgFieldProps_PropertyValueChanged);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.lblStatus });
            this.statusStrip.Location = new System.Drawing.Point(0, 678);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1100, 22);
            this.statusStrip.TabIndex = 1;
            //
            // lblStatus
            //
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(35, 17);
            this.lblStatus.Text = "준비";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "MainForm";
            this.Text = "Zebra Label Printer";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.grpData.ResumeLayout(false);
            this.grpData.PerformLayout();
            this.grpLabelSize.ResumeLayout(false);
            this.grpLabelSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelHeight)).EndInit();
            this.grpPrinter.ResumeLayout(false);
            this.grpPrinter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCopies)).EndInit();
            this.tabRight.ResumeLayout(false);
            this.tabPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.tabZpl.ResumeLayout(false);
            this.tabZpl.PerformLayout();
            this.tabDesigner.ResumeLayout(false);
            this.tabDesigner.PerformLayout();
            this.splitDesigner.Panel1.ResumeLayout(false);
            this.splitDesigner.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDesigner)).EndInit();
            this.splitDesigner.ResumeLayout(false);
            this.toolStripDesigner.ResumeLayout(false);
            this.toolStripDesigner.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.GroupBox grpPrinter;
        private System.Windows.Forms.Label lblPrinter;
        private System.Windows.Forms.ComboBox cmbPrinter;
        private System.Windows.Forms.Button btnRefreshPrinters;
        private System.Windows.Forms.Label lblCopies;
        private System.Windows.Forms.NumericUpDown numCopies;
        private System.Windows.Forms.GroupBox grpLabelSize;
        private System.Windows.Forms.Label lblLabelWidth;
        private System.Windows.Forms.NumericUpDown numLabelWidth;
        private System.Windows.Forms.Label lblLabelTimes;
        private System.Windows.Forms.Label lblLabelHeight;
        private System.Windows.Forms.NumericUpDown numLabelHeight;
        private System.Windows.Forms.ComboBox cmbLabelUnit;
        private System.Windows.Forms.GroupBox grpData;
        private System.Windows.Forms.Panel pnlDataBindings;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.TabControl tabRight;
        private System.Windows.Forms.TabPage tabPreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.TabPage tabZpl;
        private System.Windows.Forms.TextBox txtZpl;
        private System.Windows.Forms.TabPage tabDesigner;
        private System.Windows.Forms.ToolStrip toolStripDesigner;
        private System.Windows.Forms.ToolStripButton btnAddText;
        private System.Windows.Forms.ToolStripButton btnAddBarcode;
        private System.Windows.Forms.ToolStripButton btnAddQr;
        private System.Windows.Forms.ToolStripButton btnAddBox;
        private System.Windows.Forms.ToolStripButton btnAddHLine;
        private System.Windows.Forms.ToolStripButton btnAddVLine;
        private System.Windows.Forms.ToolStripSeparator toolSep1;
        private System.Windows.Forms.ToolStripButton btnBringToFront;
        private System.Windows.Forms.ToolStripButton btnSendToBack;
        private System.Windows.Forms.ToolStripButton btnRotate;
        private System.Windows.Forms.ToolStripSeparator toolSep2;
        private System.Windows.Forms.ToolStripButton btnDeleteField;
        private System.Windows.Forms.ToolStripButton btnClearAll;
        private System.Windows.Forms.ToolStripSeparator toolSep3;
        private System.Windows.Forms.ToolStripButton btnSaveTemplate;
        private System.Windows.Forms.ToolStripButton btnLoadTemplate;
        private System.Windows.Forms.SplitContainer splitDesigner;
        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.PropertyGrid pgFieldProps;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
