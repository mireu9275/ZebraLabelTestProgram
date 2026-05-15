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
            this.txtQr = new System.Windows.Forms.TextBox();
            this.lblQr = new System.Windows.Forms.Label();
            this.txtLotNo = new System.Windows.Forms.TextBox();
            this.lblLotNo = new System.Windows.Forms.Label();
            this.txtPartNo = new System.Windows.Forms.TextBox();
            this.lblPartNo = new System.Windows.Forms.Label();
            this.grpPrinter = new System.Windows.Forms.GroupBox();
            this.numCopies = new System.Windows.Forms.NumericUpDown();
            this.lblCopies = new System.Windows.Forms.Label();
            this.cmbDpi = new System.Windows.Forms.ComboBox();
            this.lblDpi = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.lblIp = new System.Windows.Forms.Label();
            this.tabRight = new System.Windows.Forms.TabControl();
            this.tabPreview = new System.Windows.Forms.TabPage();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.tabZpl = new System.Windows.Forms.TabPage();
            this.txtZpl = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.grpData.SuspendLayout();
            this.grpPrinter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCopies)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.tabRight.SuspendLayout();
            this.tabPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.tabZpl.SuspendLayout();
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
            this.splitContainer.SplitterDistance = 340;
            this.splitContainer.TabIndex = 0;
            //
            // panelLeft
            //
            this.panelLeft.Controls.Add(this.btnPrint);
            this.panelLeft.Controls.Add(this.btnPreview);
            this.panelLeft.Controls.Add(this.btnGenerate);
            this.panelLeft.Controls.Add(this.grpData);
            this.panelLeft.Controls.Add(this.grpPrinter);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(12);
            this.panelLeft.Size = new System.Drawing.Size(340, 678);
            this.panelLeft.TabIndex = 0;
            //
            // btnPrint
            //
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPrint.Location = new System.Drawing.Point(12, 540);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(310, 44);
            this.btnPrint.TabIndex = 4;
            this.btnPrint.Text = "프린터로 전송 (TCP 9100)";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            //
            // btnPreview
            //
            this.btnPreview.Location = new System.Drawing.Point(170, 490);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(152, 40);
            this.btnPreview.TabIndex = 3;
            this.btnPreview.Text = "미리보기";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            //
            // btnGenerate
            //
            this.btnGenerate.Location = new System.Drawing.Point(12, 490);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(152, 40);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "ZPL 생성";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            //
            // grpData
            //
            this.grpData.Controls.Add(this.txtQr);
            this.grpData.Controls.Add(this.lblQr);
            this.grpData.Controls.Add(this.txtLotNo);
            this.grpData.Controls.Add(this.lblLotNo);
            this.grpData.Controls.Add(this.txtPartNo);
            this.grpData.Controls.Add(this.lblPartNo);
            this.grpData.Location = new System.Drawing.Point(12, 280);
            this.grpData.Name = "grpData";
            this.grpData.Size = new System.Drawing.Size(310, 195);
            this.grpData.TabIndex = 1;
            this.grpData.TabStop = false;
            this.grpData.Text = "데이터 바인딩";
            //
            // txtQr
            //
            this.txtQr.Location = new System.Drawing.Point(110, 130);
            this.txtQr.Name = "txtQr";
            this.txtQr.Size = new System.Drawing.Size(190, 23);
            this.txtQr.TabIndex = 5;
            //
            // lblQr
            //
            this.lblQr.AutoSize = true;
            this.lblQr.Location = new System.Drawing.Point(15, 133);
            this.lblQr.Name = "lblQr";
            this.lblQr.Size = new System.Drawing.Size(50, 15);
            this.lblQr.TabIndex = 4;
            this.lblQr.Text = "QR:";
            //
            // txtLotNo
            //
            this.txtLotNo.Location = new System.Drawing.Point(110, 85);
            this.txtLotNo.Name = "txtLotNo";
            this.txtLotNo.Size = new System.Drawing.Size(190, 23);
            this.txtLotNo.TabIndex = 3;
            //
            // lblLotNo
            //
            this.lblLotNo.AutoSize = true;
            this.lblLotNo.Location = new System.Drawing.Point(15, 88);
            this.lblLotNo.Name = "lblLotNo";
            this.lblLotNo.Size = new System.Drawing.Size(70, 15);
            this.lblLotNo.TabIndex = 2;
            this.lblLotNo.Text = "LOT_NO:";
            //
            // txtPartNo
            //
            this.txtPartNo.Location = new System.Drawing.Point(110, 40);
            this.txtPartNo.Name = "txtPartNo";
            this.txtPartNo.Size = new System.Drawing.Size(190, 23);
            this.txtPartNo.TabIndex = 1;
            //
            // lblPartNo
            //
            this.lblPartNo.AutoSize = true;
            this.lblPartNo.Location = new System.Drawing.Point(15, 43);
            this.lblPartNo.Name = "lblPartNo";
            this.lblPartNo.Size = new System.Drawing.Size(70, 15);
            this.lblPartNo.TabIndex = 0;
            this.lblPartNo.Text = "PART_NO:";
            //
            // grpPrinter
            //
            this.grpPrinter.Controls.Add(this.numCopies);
            this.grpPrinter.Controls.Add(this.lblCopies);
            this.grpPrinter.Controls.Add(this.cmbDpi);
            this.grpPrinter.Controls.Add(this.lblDpi);
            this.grpPrinter.Controls.Add(this.numPort);
            this.grpPrinter.Controls.Add(this.lblPort);
            this.grpPrinter.Controls.Add(this.txtIp);
            this.grpPrinter.Controls.Add(this.lblIp);
            this.grpPrinter.Location = new System.Drawing.Point(12, 15);
            this.grpPrinter.Name = "grpPrinter";
            this.grpPrinter.Size = new System.Drawing.Size(310, 250);
            this.grpPrinter.TabIndex = 0;
            this.grpPrinter.TabStop = false;
            this.grpPrinter.Text = "프린터 설정";
            //
            // numCopies
            //
            this.numCopies.Location = new System.Drawing.Point(110, 180);
            this.numCopies.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numCopies.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numCopies.Name = "numCopies";
            this.numCopies.Size = new System.Drawing.Size(80, 23);
            this.numCopies.TabIndex = 7;
            this.numCopies.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // lblCopies
            //
            this.lblCopies.AutoSize = true;
            this.lblCopies.Location = new System.Drawing.Point(15, 183);
            this.lblCopies.Name = "lblCopies";
            this.lblCopies.Size = new System.Drawing.Size(40, 15);
            this.lblCopies.TabIndex = 6;
            this.lblCopies.Text = "매수:";
            //
            // cmbDpi
            //
            this.cmbDpi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDpi.FormattingEnabled = true;
            this.cmbDpi.Items.AddRange(new object[] { "203", "300", "600" });
            this.cmbDpi.Location = new System.Drawing.Point(110, 135);
            this.cmbDpi.Name = "cmbDpi";
            this.cmbDpi.Size = new System.Drawing.Size(80, 23);
            this.cmbDpi.TabIndex = 5;
            //
            // lblDpi
            //
            this.lblDpi.AutoSize = true;
            this.lblDpi.Location = new System.Drawing.Point(15, 138);
            this.lblDpi.Name = "lblDpi";
            this.lblDpi.Size = new System.Drawing.Size(40, 15);
            this.lblDpi.TabIndex = 4;
            this.lblDpi.Text = "DPI:";
            //
            // numPort
            //
            this.numPort.Location = new System.Drawing.Point(110, 90);
            this.numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(120, 23);
            this.numPort.TabIndex = 3;
            this.numPort.Value = new decimal(new int[] { 9100, 0, 0, 0 });
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(15, 93);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(40, 15);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "포트:";
            //
            // txtIp
            //
            this.txtIp.Location = new System.Drawing.Point(110, 45);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(190, 23);
            this.txtIp.TabIndex = 1;
            //
            // lblIp
            //
            this.lblIp.AutoSize = true;
            this.lblIp.Location = new System.Drawing.Point(15, 48);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(40, 15);
            this.lblIp.TabIndex = 0;
            this.lblIp.Text = "IP:";
            //
            // tabRight
            //
            this.tabRight.Controls.Add(this.tabPreview);
            this.tabRight.Controls.Add(this.tabZpl);
            this.tabRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabRight.Location = new System.Drawing.Point(0, 0);
            this.tabRight.Name = "tabRight";
            this.tabRight.SelectedIndex = 0;
            this.tabRight.Size = new System.Drawing.Size(756, 678);
            this.tabRight.TabIndex = 0;
            //
            // tabPreview
            //
            this.tabPreview.Controls.Add(this.picPreview);
            this.tabPreview.Location = new System.Drawing.Point(4, 24);
            this.tabPreview.Name = "tabPreview";
            this.tabPreview.Padding = new System.Windows.Forms.Padding(3);
            this.tabPreview.Size = new System.Drawing.Size(748, 650);
            this.tabPreview.TabIndex = 0;
            this.tabPreview.Text = "미리보기";
            this.tabPreview.UseVisualStyleBackColor = true;
            //
            // picPreview
            //
            this.picPreview.BackColor = System.Drawing.Color.White;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(3, 3);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(742, 644);
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
            this.tabZpl.Size = new System.Drawing.Size(748, 650);
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
            this.txtZpl.Size = new System.Drawing.Size(742, 644);
            this.txtZpl.TabIndex = 0;
            this.txtZpl.WordWrap = false;
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
            this.grpPrinter.ResumeLayout(false);
            this.grpPrinter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCopies)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.tabRight.ResumeLayout(false);
            this.tabPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.tabZpl.ResumeLayout(false);
            this.tabZpl.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.GroupBox grpPrinter;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label lblIp;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ComboBox cmbDpi;
        private System.Windows.Forms.Label lblDpi;
        private System.Windows.Forms.NumericUpDown numCopies;
        private System.Windows.Forms.Label lblCopies;
        private System.Windows.Forms.GroupBox grpData;
        private System.Windows.Forms.TextBox txtPartNo;
        private System.Windows.Forms.Label lblPartNo;
        private System.Windows.Forms.TextBox txtLotNo;
        private System.Windows.Forms.Label lblLotNo;
        private System.Windows.Forms.TextBox txtQr;
        private System.Windows.Forms.Label lblQr;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.TabControl tabRight;
        private System.Windows.Forms.TabPage tabPreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.TabPage tabZpl;
        private System.Windows.Forms.TextBox txtZpl;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
