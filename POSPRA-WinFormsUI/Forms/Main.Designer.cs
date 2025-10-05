namespace POSPRA_WinFormsUI.Forms
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            posStatus = new Label();
            internetStatus = new Label();
            pictureBox2 = new PictureBox();
            label2 = new Label();
            panCatalogView = new Panel();
            btnCatalogView = new Button();
            lblWorkerService = new Label();
            lblNetworkStatus = new Label();
            panExportInvoice = new Panel();
            label1 = new Label();
            btnDashboard = new Button();
            btnInvoiceSelection = new Button();
            btnExportInvoice = new Button();
            panDashboard = new Panel();
            panInvoiceSelection = new Panel();
            sqliteCommand1 = new Microsoft.Data.Sqlite.SqliteCommand();
            pictureBox1 = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(224, 242, 254);
            panel1.Controls.Add(posStatus);
            panel1.Controls.Add(internetStatus);
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(panCatalogView);
            panel1.Controls.Add(btnCatalogView);
            panel1.Controls.Add(lblWorkerService);
            panel1.Controls.Add(lblNetworkStatus);
            panel1.Controls.Add(panExportInvoice);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(btnDashboard);
            panel1.Controls.Add(btnInvoiceSelection);
            panel1.Controls.Add(btnExportInvoice);
            panel1.Controls.Add(panDashboard);
            panel1.Controls.Add(panInvoiceSelection);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(1507, 60);
            panel1.TabIndex = 4;
            // 
            // posStatus
            // 
            posStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            posStatus.AutoSize = true;
            posStatus.Location = new Point(1422, 3);
            posStatus.Name = "posStatus";
            posStatus.Size = new Size(72, 20);
            posStatus.TabIndex = 9;
            posStatus.Text = "INACTIVE";
            // 
            // internetStatus
            // 
            internetStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            internetStatus.AutoSize = true;
            internetStatus.Location = new Point(1243, 0);
            internetStatus.Name = "internetStatus";
            internetStatus.Size = new Size(72, 20);
            internetStatus.TabIndex = 8;
            internetStatus.Text = "INACTIVE";
            // 
            // pictureBox2
            // 
            pictureBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pictureBox2.Image = Resources.pos;
            pictureBox2.Location = new Point(26, 12);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(34, 38);
            pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label2.Font = new Font("Microsoft Sans Serif", 10.2F);
            label2.ForeColor = Color.FromArgb(72, 167, 135);
            label2.Location = new Point(80, 33);
            label2.Name = "label2";
            label2.Size = new Size(124, 20);
            label2.TabIndex = 7;
            label2.Text = "COMPONENT";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panCatalogView
            // 
            panCatalogView.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panCatalogView.BackColor = Color.FromArgb(72, 167, 135);
            panCatalogView.Location = new Point(995, 46);
            panCatalogView.Margin = new Padding(3, 4, 3, 4);
            panCatalogView.Name = "panCatalogView";
            panCatalogView.Size = new Size(33, 10);
            panCatalogView.TabIndex = 6;
            // 
            // btnCatalogView
            // 
            btnCatalogView.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCatalogView.FlatAppearance.BorderSize = 0;
            btnCatalogView.FlatStyle = FlatStyle.Flat;
            btnCatalogView.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnCatalogView.ForeColor = Color.Black;
            btnCatalogView.Location = new Point(909, 8);
            btnCatalogView.Margin = new Padding(3, 4, 3, 4);
            btnCatalogView.Name = "btnCatalogView";
            btnCatalogView.Size = new Size(206, 35);
            btnCatalogView.TabIndex = 5;
            btnCatalogView.Text = "PRODUCT CATALOGUE";
            btnCatalogView.TextAlign = ContentAlignment.BottomCenter;
            btnCatalogView.UseVisualStyleBackColor = true;
            btnCatalogView.Click += btnCatalogView_Click;
            // 
            // lblWorkerService
            // 
            lblWorkerService.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lblWorkerService.AutoSize = true;
            lblWorkerService.Font = new Font("Segoe UI", 12F);
            lblWorkerService.ForeColor = Color.Black;
            lblWorkerService.Location = new Point(1378, 16);
            lblWorkerService.Name = "lblWorkerService";
            lblWorkerService.Size = new Size(116, 28);
            lblWorkerService.TabIndex = 4;
            lblWorkerService.Text = "POS Service";
            // 
            // lblNetworkStatus
            // 
            lblNetworkStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lblNetworkStatus.AutoSize = true;
            lblNetworkStatus.Font = new Font("Segoe UI", 12F);
            lblNetworkStatus.ForeColor = Color.Black;
            lblNetworkStatus.Location = new Point(1177, 16);
            lblNetworkStatus.Name = "lblNetworkStatus";
            lblNetworkStatus.Size = new Size(138, 28);
            lblNetworkStatus.TabIndex = 3;
            lblNetworkStatus.Text = "Internet Status";
            // 
            // panExportInvoice
            // 
            panExportInvoice.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panExportInvoice.BackColor = Color.FromArgb(72, 167, 135);
            panExportInvoice.Location = new Point(818, 46);
            panExportInvoice.Margin = new Padding(3, 4, 3, 4);
            panExportInvoice.Name = "panExportInvoice";
            panExportInvoice.Size = new Size(33, 10);
            panExportInvoice.TabIndex = 2;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label1.Font = new Font("Microsoft Sans Serif", 10.2F);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(80, 9);
            label1.Name = "label1";
            label1.Size = new Size(54, 23);
            label1.TabIndex = 0;
            label1.Text = "POS";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnDashboard
            // 
            btnDashboard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnDashboard.ForeColor = Color.Black;
            btnDashboard.Location = new Point(458, 9);
            btnDashboard.Margin = new Padding(3, 4, 3, 4);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(126, 36);
            btnDashboard.TabIndex = 1;
            btnDashboard.Text = "DASHBOARD";
            btnDashboard.TextAlign = ContentAlignment.BottomCenter;
            btnDashboard.UseVisualStyleBackColor = true;
            btnDashboard.Click += btnDashboard_Click;
            // 
            // btnInvoiceSelection
            // 
            btnInvoiceSelection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnInvoiceSelection.FlatAppearance.BorderSize = 0;
            btnInvoiceSelection.FlatStyle = FlatStyle.Flat;
            btnInvoiceSelection.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnInvoiceSelection.ForeColor = Color.Black;
            btnInvoiceSelection.Location = new Point(595, 9);
            btnInvoiceSelection.Margin = new Padding(3, 4, 3, 4);
            btnInvoiceSelection.Name = "btnInvoiceSelection";
            btnInvoiceSelection.Size = new Size(151, 36);
            btnInvoiceSelection.TabIndex = 1;
            btnInvoiceSelection.Text = "INVOICE ENTRY";
            btnInvoiceSelection.TextAlign = ContentAlignment.BottomCenter;
            btnInvoiceSelection.UseVisualStyleBackColor = true;
            btnInvoiceSelection.Click += btnInvoiceSelection_Click;
            // 
            // btnExportInvoice
            // 
            btnExportInvoice.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExportInvoice.FlatAppearance.BorderSize = 0;
            btnExportInvoice.FlatStyle = FlatStyle.Flat;
            btnExportInvoice.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnExportInvoice.ForeColor = Color.Black;
            btnExportInvoice.Location = new Point(752, 2);
            btnExportInvoice.Margin = new Padding(3, 4, 3, 4);
            btnExportInvoice.Name = "btnExportInvoice";
            btnExportInvoice.Size = new Size(163, 41);
            btnExportInvoice.TabIndex = 1;
            btnExportInvoice.Text = "EXPORT INVOICE";
            btnExportInvoice.TextAlign = ContentAlignment.BottomCenter;
            btnExportInvoice.UseVisualStyleBackColor = true;
            btnExportInvoice.Click += btnExportInvoice_Click;
            // 
            // panDashboard
            // 
            panDashboard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panDashboard.BackColor = Color.FromArgb(72, 167, 135);
            panDashboard.Location = new Point(505, 46);
            panDashboard.Margin = new Padding(3, 4, 3, 4);
            panDashboard.Name = "panDashboard";
            panDashboard.Size = new Size(33, 10);
            panDashboard.TabIndex = 2;
            // 
            // panInvoiceSelection
            // 
            panInvoiceSelection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panInvoiceSelection.BackColor = Color.FromArgb(72, 167, 135);
            panInvoiceSelection.Location = new Point(656, 46);
            panInvoiceSelection.Margin = new Padding(3, 4, 3, 4);
            panInvoiceSelection.Name = "panInvoiceSelection";
            panInvoiceSelection.Size = new Size(33, 10);
            panInvoiceSelection.TabIndex = 2;
            // 
            // sqliteCommand1
            // 
            sqliteCommand1.CommandTimeout = 30;
            sqliteCommand1.Connection = null;
            sqliteCommand1.Transaction = null;
            sqliteCommand1.UpdatedRowSource = System.Data.UpdateRowSource.None;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 50);
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1507, 849);
            Controls.Add(panel1);
            Controls.Add(pictureBox1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Main";
            Text = "Main";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panExportInvoice;
        private Label label1;
        private Button btnDashboard;
        private Button btnInvoiceSelection;
        private Button btnExportInvoice;
        private Panel panDashboard;
        private Panel panInvoiceSelection;
        private Label lblNetworkStatus;
        private Label lblWorkerService;
        private Panel panCatalogView;
        private Button btnCatalogView;
        private Label label2;
        private Microsoft.Data.Sqlite.SqliteCommand sqliteCommand1;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Label posStatus;
        private Label lblInternetStatusDot;
        private Label lblPosStatusDot;
        private Label internetStatus;
    }
}