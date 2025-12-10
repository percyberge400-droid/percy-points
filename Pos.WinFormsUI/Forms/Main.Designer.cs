namespace Pos.WinFormsUI.Forms
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
            components = new System.ComponentModel.Container();
            panel1 = new Panel();
            panel2 = new Panel();
            lblNetworkStatus = new Label();
            lblEnvironment = new Label();
            posStatus = new Label();
            lblWorkerService = new Label();
            internetStatus = new Label();
            pictureBox2 = new PictureBox();
            panCatalogView = new Panel();
            btnCatalogView = new Button();
            panExportInvoice = new Panel();
            btnDashboard = new Button();
            btnInvoiceSelection = new Button();
            btnExportInvoice = new Button();
            panDashboard = new Panel();
            panInvoiceSelection = new Panel();
            sqliteCommand1 = new Microsoft.Data.Sqlite.SqliteCommand();
            pictureBox1 = new PictureBox();
            contextMenuCatalog = new ContextMenuStrip(components);
            productCatalogToolStripMenuItem = new ToolStripMenuItem();
            uploadLogoToolStripMenuItem = new ToolStripMenuItem();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuCatalog.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(panCatalogView);
            panel1.Controls.Add(btnCatalogView);
            panel1.Controls.Add(panExportInvoice);
            panel1.Controls.Add(btnDashboard);
            panel1.Controls.Add(btnInvoiceSelection);
            panel1.Controls.Add(btnExportInvoice);
            panel1.Controls.Add(panDashboard);
            panel1.Controls.Add(panInvoiceSelection);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(1285, 60);
            panel1.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel2.BackColor = Color.White;
            panel2.Controls.Add(lblNetworkStatus);
            panel2.Controls.Add(lblEnvironment);
            panel2.Controls.Add(posStatus);
            panel2.Controls.Add(lblWorkerService);
            panel2.Controls.Add(internetStatus);
            panel2.Location = new Point(711, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(570, 60);
            panel2.TabIndex = 6;
            // 
            // lblNetworkStatus
            // 
            lblNetworkStatus.Anchor = AnchorStyles.Right;
            lblNetworkStatus.AutoSize = true;
            lblNetworkStatus.Font = new Font("Segoe UI", 11F);
            lblNetworkStatus.ForeColor = Color.Black;
            lblNetworkStatus.Location = new Point(166, 15);
            lblNetworkStatus.Name = "lblNetworkStatus";
            lblNetworkStatus.Size = new Size(133, 25);
            lblNetworkStatus.TabIndex = 3;
            lblNetworkStatus.Text = "Internet Status";
            lblNetworkStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblEnvironment
            // 
            lblEnvironment.Anchor = AnchorStyles.Left;
            lblEnvironment.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblEnvironment.ForeColor = Color.Teal;
            lblEnvironment.Location = new Point(38, 33);
            lblEnvironment.Name = "lblEnvironment";
            lblEnvironment.Size = new Size(133, 23);
            lblEnvironment.TabIndex = 8;
            lblEnvironment.Text = "Sandbox";
            lblEnvironment.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // posStatus
            // 
            posStatus.Anchor = AnchorStyles.Right;
            posStatus.AutoSize = true;
            posStatus.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            posStatus.ForeColor = Color.Black;
            posStatus.Location = new Point(495, 17);
            posStatus.Name = "posStatus";
            posStatus.Size = new Size(72, 23);
            posStatus.TabIndex = 0;
            posStatus.Text = "Inactive";
            posStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblWorkerService
            // 
            lblWorkerService.Anchor = AnchorStyles.Right;
            lblWorkerService.AutoSize = true;
            lblWorkerService.Font = new Font("Segoe UI", 11F);
            lblWorkerService.ForeColor = Color.Black;
            lblWorkerService.Location = new Point(378, 15);
            lblWorkerService.Name = "lblWorkerService";
            lblWorkerService.Size = new Size(112, 25);
            lblWorkerService.TabIndex = 1;
            lblWorkerService.Text = "POS Service";
            lblWorkerService.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // internetStatus
            // 
            internetStatus.Anchor = AnchorStyles.Right;
            internetStatus.AutoSize = true;
            internetStatus.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            internetStatus.ForeColor = Color.Black;
            internetStatus.Location = new Point(304, 17);
            internetStatus.Name = "internetStatus";
            internetStatus.Size = new Size(66, 23);
            internetStatus.TabIndex = 2;
            internetStatus.Text = "Offline";
            internetStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBox2
            // 
            pictureBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pictureBox2.Location = new Point(2, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(182, 53);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            // 
            // panCatalogView
            // 
            panCatalogView.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panCatalogView.BackColor = Color.FromArgb(72, 167, 135);
            panCatalogView.Location = new Point(731, 45);
            panCatalogView.Margin = new Padding(3, 4, 3, 4);
            panCatalogView.Name = "panCatalogView";
            panCatalogView.Size = new Size(33, 5);
            panCatalogView.TabIndex = 6;
            // 
            // btnCatalogView
            // 
            btnCatalogView.AutoSize = true;
            btnCatalogView.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCatalogView.FlatAppearance.BorderSize = 0;
            btnCatalogView.FlatStyle = FlatStyle.Flat;
            btnCatalogView.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnCatalogView.ForeColor = Color.Black;
            btnCatalogView.Location = new Point(638, 13);
            btnCatalogView.Margin = new Padding(3, 4, 3, 4);
            btnCatalogView.Name = "btnCatalogView";
            btnCatalogView.Size = new Size(228, 33);
            btnCatalogView.TabIndex = 5;
            btnCatalogView.Text = "SYSTEM CONFIGURATION";
            btnCatalogView.TextAlign = ContentAlignment.BottomCenter;
            btnCatalogView.UseVisualStyleBackColor = true;
            btnCatalogView.Click += btnCatalogView_Click;
            // 
            // panExportInvoice
            // 
            panExportInvoice.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panExportInvoice.BackColor = Color.FromArgb(72, 167, 135);
            panExportInvoice.Location = new Point(538, 45);
            panExportInvoice.Margin = new Padding(3, 4, 3, 4);
            panExportInvoice.Name = "panExportInvoice";
            panExportInvoice.Size = new Size(33, 5);
            panExportInvoice.TabIndex = 2;
            // 
            // btnDashboard
            // 
            btnDashboard.AutoSize = true;
            btnDashboard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnDashboard.ForeColor = Color.Black;
            btnDashboard.Location = new Point(194, 12);
            btnDashboard.Margin = new Padding(3, 4, 3, 4);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(128, 33);
            btnDashboard.TabIndex = 1;
            btnDashboard.Text = "DASHBOARD";
            btnDashboard.TextAlign = ContentAlignment.BottomCenter;
            btnDashboard.UseVisualStyleBackColor = true;
            btnDashboard.Click += btnDashboard_Click;
            // 
            // btnInvoiceSelection
            // 
            btnInvoiceSelection.AutoSize = true;
            btnInvoiceSelection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnInvoiceSelection.FlatAppearance.BorderSize = 0;
            btnInvoiceSelection.FlatStyle = FlatStyle.Flat;
            btnInvoiceSelection.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnInvoiceSelection.ForeColor = Color.Black;
            btnInvoiceSelection.Location = new Point(325, 13);
            btnInvoiceSelection.Margin = new Padding(3, 4, 3, 4);
            btnInvoiceSelection.Name = "btnInvoiceSelection";
            btnInvoiceSelection.Size = new Size(145, 33);
            btnInvoiceSelection.TabIndex = 1;
            btnInvoiceSelection.Text = "INVOICE ENTRY";
            btnInvoiceSelection.TextAlign = ContentAlignment.BottomCenter;
            btnInvoiceSelection.UseVisualStyleBackColor = true;
            btnInvoiceSelection.Click += btnInvoiceSelection_Click;
            // 
            // btnExportInvoice
            // 
            btnExportInvoice.AutoSize = true;
            btnExportInvoice.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExportInvoice.FlatAppearance.BorderSize = 0;
            btnExportInvoice.FlatStyle = FlatStyle.Flat;
            btnExportInvoice.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnExportInvoice.ForeColor = Color.Black;
            btnExportInvoice.Location = new Point(475, 13);
            btnExportInvoice.Margin = new Padding(3, 4, 3, 4);
            btnExportInvoice.Name = "btnExportInvoice";
            btnExportInvoice.Size = new Size(156, 33);
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
            panDashboard.Location = new Point(239, 45);
            panDashboard.Margin = new Padding(3, 4, 3, 4);
            panDashboard.Name = "panDashboard";
            panDashboard.Size = new Size(33, 5);
            panDashboard.TabIndex = 2;
            // 
            // panInvoiceSelection
            // 
            panInvoiceSelection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panInvoiceSelection.BackColor = Color.FromArgb(72, 167, 135);
            panInvoiceSelection.Location = new Point(379, 45);
            panInvoiceSelection.Margin = new Padding(3, 4, 3, 4);
            panInvoiceSelection.Name = "panInvoiceSelection";
            panInvoiceSelection.Size = new Size(33, 5);
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
            pictureBox1.Size = new Size(101, 51);
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // contextMenuCatalog
            // 
            contextMenuCatalog.ImageScalingSize = new Size(20, 20);
            contextMenuCatalog.Items.AddRange(new ToolStripItem[] { productCatalogToolStripMenuItem, uploadLogoToolStripMenuItem });
            contextMenuCatalog.Name = "contextMenuCatalog";
            contextMenuCatalog.Size = new Size(186, 52);
            // 
            // productCatalogToolStripMenuItem
            // 
            productCatalogToolStripMenuItem.Name = "productCatalogToolStripMenuItem";
            productCatalogToolStripMenuItem.Size = new Size(185, 24);
            productCatalogToolStripMenuItem.Text = "Product Catalog";
            // 
            // uploadLogoToolStripMenuItem
            // 
            uploadLogoToolStripMenuItem.Name = "uploadLogoToolStripMenuItem";
            uploadLogoToolStripMenuItem.Size = new Size(185, 24);
            uploadLogoToolStripMenuItem.Text = "Upload Logo";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1285, 749);
            Controls.Add(panel1);
            Controls.Add(pictureBox1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Main";
            Text = "Main";
            Load += Main_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuCatalog.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panExportInvoice;
        private Button btnDashboard;
        private Button btnInvoiceSelection;
        private Button btnExportInvoice;
        private Panel panDashboard;
        private Panel panInvoiceSelection;
        private Panel panCatalogView;
        private Button btnCatalogView;
        private Microsoft.Data.Sqlite.SqliteCommand sqliteCommand1;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Label lblInternetStatusDot;
        private Label lblPosStatusDot;
        private Panel panel2;
        private Label posStatus;
        private Label lblWorkerService;
        private Label internetStatus;
        private Label lblNetworkStatus;
        private ContextMenuStrip contextMenuCatalog;
        private ToolStripMenuItem productCatalogToolStripMenuItem;
        private ToolStripMenuItem uploadLogoToolStripMenuItem;
        private Label lblEnvironment;
    }
}