namespace ZebraLabelPrinter.UI.Forms
{
    partial class SheetPrintForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (_labelBitmap != null) { _labelBitmap.Dispose(); _labelBitmap = null; }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlA4Canvas = new System.Windows.Forms.Panel();
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.lblPrinter = new System.Windows.Forms.Label();
            this.cmbPrinter = new System.Windows.Forms.ComboBox();
            this.lblPageType = new System.Windows.Forms.Label();
            this.cmbPageType = new System.Windows.Forms.ComboBox();
            this.grpMargin = new System.Windows.Forms.GroupBox();
            this.lblML = new System.Windows.Forms.Label();
            this.numMarginL = new System.Windows.Forms.NumericUpDown();
            this.lblMT = new System.Windows.Forms.Label();
            this.numMarginT = new System.Windows.Forms.NumericUpDown();
            this.lblMR = new System.Windows.Forms.Label();
            this.numMarginR = new System.Windows.Forms.NumericUpDown();
            this.lblMB = new System.Windows.Forms.Label();
            this.numMarginB = new System.Windows.Forms.NumericUpDown();
            this.grpGap = new System.Windows.Forms.GroupBox();
            this.lblGX = new System.Windows.Forms.Label();
            this.numGapX = new System.Windows.Forms.NumericUpDown();
            this.lblGY = new System.Windows.Forms.Label();
            this.numGapY = new System.Windows.Forms.NumericUpDown();
            this.lblCount = new System.Windows.Forms.Label();
            this.numCount = new System.Windows.Forms.NumericUpDown();
            this.lblGridInfo = new System.Windows.Forms.Label();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlSettings.SuspendLayout();
            this.grpMargin.SuspendLayout();
            this.grpGap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGapX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGapY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCount)).BeginInit();
            this.SuspendLayout();

            // pnlA4Canvas
            this.pnlA4Canvas.AutoScroll = true;
            this.pnlA4Canvas.BackColor = System.Drawing.Color.LightGray;
            this.pnlA4Canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlA4Canvas.Location = new System.Drawing.Point(0, 0);
            this.pnlA4Canvas.Name = "pnlA4Canvas";
            this.pnlA4Canvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlA4Canvas_Paint);

            // pnlSettings
            this.pnlSettings.Controls.Add(this.lblPrinter);
            this.pnlSettings.Controls.Add(this.cmbPrinter);
            this.pnlSettings.Controls.Add(this.lblPageType);
            this.pnlSettings.Controls.Add(this.cmbPageType);
            this.pnlSettings.Controls.Add(this.grpMargin);
            this.pnlSettings.Controls.Add(this.grpGap);
            this.pnlSettings.Controls.Add(this.lblCount);
            this.pnlSettings.Controls.Add(this.numCount);
            this.pnlSettings.Controls.Add(this.lblGridInfo);
            this.pnlSettings.Controls.Add(this.btnPrint);
            this.pnlSettings.Controls.Add(this.btnCancel);
            this.pnlSettings.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlSettings.Width = 320;

            // lblPrinter / cmbPrinter
            this.lblPrinter.AutoSize = true;
            this.lblPrinter.Location = new System.Drawing.Point(15, 18);
            this.lblPrinter.Text = "프린터:";
            this.cmbPrinter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrinter.Location = new System.Drawing.Point(15, 38);
            this.cmbPrinter.Size = new System.Drawing.Size(285, 23);

            // lblPageType / cmbPageType
            this.lblPageType.AutoSize = true;
            this.lblPageType.Location = new System.Drawing.Point(15, 72);
            this.lblPageType.Text = "용지:";
            this.cmbPageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPageType.Items.AddRange(new object[] { "A4 (210×297mm)", "Letter (215.9×279.4mm)" });
            this.cmbPageType.Location = new System.Drawing.Point(15, 92);
            this.cmbPageType.Size = new System.Drawing.Size(285, 23);
            this.cmbPageType.SelectedIndexChanged += new System.EventHandler(this.OnSettingsChanged);

            // grpMargin
            this.grpMargin.Controls.Add(this.lblML); this.grpMargin.Controls.Add(this.numMarginL);
            this.grpMargin.Controls.Add(this.lblMT); this.grpMargin.Controls.Add(this.numMarginT);
            this.grpMargin.Controls.Add(this.lblMR); this.grpMargin.Controls.Add(this.numMarginR);
            this.grpMargin.Controls.Add(this.lblMB); this.grpMargin.Controls.Add(this.numMarginB);
            this.grpMargin.Location = new System.Drawing.Point(15, 130);
            this.grpMargin.Size = new System.Drawing.Size(285, 90);
            this.grpMargin.Text = "여백 (mm)";

            this.lblML.AutoSize = true; this.lblML.Location = new System.Drawing.Point(15, 28); this.lblML.Text = "좌:";
            this.numMarginL.DecimalPlaces = 1; this.numMarginL.Location = new System.Drawing.Point(50, 25);
            this.numMarginL.Maximum = 200; this.numMarginL.Size = new System.Drawing.Size(60, 23);
            this.numMarginL.Value = 10; this.numMarginL.ValueChanged += new System.EventHandler(this.OnSettingsChanged);
            this.lblMT.AutoSize = true; this.lblMT.Location = new System.Drawing.Point(135, 28); this.lblMT.Text = "상:";
            this.numMarginT.DecimalPlaces = 1; this.numMarginT.Location = new System.Drawing.Point(170, 25);
            this.numMarginT.Maximum = 200; this.numMarginT.Size = new System.Drawing.Size(60, 23);
            this.numMarginT.Value = 10; this.numMarginT.ValueChanged += new System.EventHandler(this.OnSettingsChanged);
            this.lblMR.AutoSize = true; this.lblMR.Location = new System.Drawing.Point(15, 60); this.lblMR.Text = "우:";
            this.numMarginR.DecimalPlaces = 1; this.numMarginR.Location = new System.Drawing.Point(50, 57);
            this.numMarginR.Maximum = 200; this.numMarginR.Size = new System.Drawing.Size(60, 23);
            this.numMarginR.Value = 10; this.numMarginR.ValueChanged += new System.EventHandler(this.OnSettingsChanged);
            this.lblMB.AutoSize = true; this.lblMB.Location = new System.Drawing.Point(135, 60); this.lblMB.Text = "하:";
            this.numMarginB.DecimalPlaces = 1; this.numMarginB.Location = new System.Drawing.Point(170, 57);
            this.numMarginB.Maximum = 200; this.numMarginB.Size = new System.Drawing.Size(60, 23);
            this.numMarginB.Value = 10; this.numMarginB.ValueChanged += new System.EventHandler(this.OnSettingsChanged);

            // grpGap
            this.grpGap.Controls.Add(this.lblGX); this.grpGap.Controls.Add(this.numGapX);
            this.grpGap.Controls.Add(this.lblGY); this.grpGap.Controls.Add(this.numGapY);
            this.grpGap.Location = new System.Drawing.Point(15, 230);
            this.grpGap.Size = new System.Drawing.Size(285, 90);
            this.grpGap.Text = "라벨 간격 (mm)";

            this.lblGX.AutoSize = true; this.lblGX.Location = new System.Drawing.Point(15, 28); this.lblGX.Text = "가로:";
            this.numGapX.DecimalPlaces = 1; this.numGapX.Location = new System.Drawing.Point(60, 25);
            this.numGapX.Maximum = 50; this.numGapX.Size = new System.Drawing.Size(60, 23);
            this.numGapX.Value = 2; this.numGapX.ValueChanged += new System.EventHandler(this.OnSettingsChanged);
            this.lblGY.AutoSize = true; this.lblGY.Location = new System.Drawing.Point(135, 28); this.lblGY.Text = "세로:";
            this.numGapY.DecimalPlaces = 1; this.numGapY.Location = new System.Drawing.Point(180, 25);
            this.numGapY.Maximum = 50; this.numGapY.Size = new System.Drawing.Size(60, 23);
            this.numGapY.Value = 2; this.numGapY.ValueChanged += new System.EventHandler(this.OnSettingsChanged);

            // lblCount + numCount
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(15, 335);
            this.lblCount.Text = "출력 장수:";
            this.numCount.Location = new System.Drawing.Point(110, 332);
            this.numCount.Minimum = 1; this.numCount.Maximum = 10000;
            this.numCount.Size = new System.Drawing.Size(80, 23);
            this.numCount.Value = 1;
            this.numCount.ValueChanged += new System.EventHandler(this.OnSettingsChanged);

            // lblGridInfo
            this.lblGridInfo.Location = new System.Drawing.Point(15, 370);
            this.lblGridInfo.Size = new System.Drawing.Size(285, 70);
            this.lblGridInfo.ForeColor = System.Drawing.Color.DarkBlue;

            // 버튼
            this.btnPrint.Location = new System.Drawing.Point(15, 460);
            this.btnPrint.Size = new System.Drawing.Size(180, 40);
            this.btnPrint.Text = "미리보기 ▶ 출력";
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            this.btnCancel.Location = new System.Drawing.Point(210, 460);
            this.btnCancel.Size = new System.Drawing.Size(90, 40);
            this.btnCancel.Text = "닫기";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // 폼
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 700);
            this.Controls.Add(this.pnlA4Canvas);
            this.Controls.Add(this.pnlSettings);
            this.MinimumSize = new System.Drawing.Size(800, 550);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "A4 시트 디자이너";
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            this.grpMargin.ResumeLayout(false);
            this.grpMargin.PerformLayout();
            this.grpGap.ResumeLayout(false);
            this.grpGap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarginB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGapX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGapY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCount)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlA4Canvas;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Label lblPrinter;
        private System.Windows.Forms.ComboBox cmbPrinter;
        private System.Windows.Forms.Label lblPageType;
        private System.Windows.Forms.ComboBox cmbPageType;
        private System.Windows.Forms.GroupBox grpMargin;
        private System.Windows.Forms.Label lblML;
        private System.Windows.Forms.NumericUpDown numMarginL;
        private System.Windows.Forms.Label lblMT;
        private System.Windows.Forms.NumericUpDown numMarginT;
        private System.Windows.Forms.Label lblMR;
        private System.Windows.Forms.NumericUpDown numMarginR;
        private System.Windows.Forms.Label lblMB;
        private System.Windows.Forms.NumericUpDown numMarginB;
        private System.Windows.Forms.GroupBox grpGap;
        private System.Windows.Forms.Label lblGX;
        private System.Windows.Forms.NumericUpDown numGapX;
        private System.Windows.Forms.Label lblGY;
        private System.Windows.Forms.NumericUpDown numGapY;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.NumericUpDown numCount;
        private System.Windows.Forms.Label lblGridInfo;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnCancel;
    }
}
