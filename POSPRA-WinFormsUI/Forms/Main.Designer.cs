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
            lblWorkerService = new Label();
            lblNetworkStatus = new Label();
            rightMenuPanel = new Panel();
            btnDropDown = new Button();
            panExportInvoice = new Panel();
            label1 = new Label();
            btnDashboard = new Button();
            btnInvoiceSelection = new Button();
            btnExportInvoice = new Button();
            panDashboard = new Panel();
            panInvoiceSelection = new Panel();
            panel1.SuspendLayout();
            rightMenuPanel.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(lblWorkerService);
            panel1.Controls.Add(lblNetworkStatus);
            panel1.Controls.Add(rightMenuPanel);
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
            panel1.Size = new Size(1507, 85);
            panel1.TabIndex = 4;
            // 
            // lblWorkerService
            // 
            lblWorkerService.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lblWorkerService.AutoSize = true;
            lblWorkerService.Font = new Font("Segoe UI", 12F);
            lblWorkerService.Location = new Point(1164, 25);
            lblWorkerService.Name = "lblWorkerService";
            lblWorkerService.Size = new Size(91, 28);
            lblWorkerService.TabIndex = 4;
            lblWorkerService.Text = "Service: -";
            // 
            // lblNetworkStatus
            // 
            lblNetworkStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lblNetworkStatus.AutoSize = true;
            lblNetworkStatus.Font = new Font("Segoe UI", 12F);
            lblNetworkStatus.Location = new Point(841, 25);
            lblNetworkStatus.Name = "lblNetworkStatus";
            lblNetworkStatus.Size = new Size(162, 28);
            lblNetworkStatus.TabIndex = 3;
            lblNetworkStatus.Text = "Network Status: -";
            // 
            // rightMenuPanel
            // 
            rightMenuPanel.Controls.Add(btnDropDown);
            rightMenuPanel.Dock = DockStyle.Right;
            rightMenuPanel.Location = new Point(1419, 0);
            rightMenuPanel.Margin = new Padding(3, 4, 3, 4);
            rightMenuPanel.Name = "rightMenuPanel";
            rightMenuPanel.Size = new Size(88, 85);
            rightMenuPanel.TabIndex = 2;
            // 
            // btnDropDown
            // 
            btnDropDown.Location = new Point(0, 0);
            btnDropDown.Name = "btnDropDown";
            btnDropDown.Size = new Size(75, 23);
            btnDropDown.TabIndex = 0;
            btnDropDown.Visible = false;
            // 
            // panExportInvoice
            // 
            panExportInvoice.BackColor = Color.FromArgb(104, 109, 244);
            panExportInvoice.Location = new Point(537, 79);
            panExportInvoice.Margin = new Padding(3, 4, 3, 4);
            panExportInvoice.Name = "panExportInvoice";
            panExportInvoice.Size = new Size(137, 7);
            panExportInvoice.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 36F, FontStyle.Regular, GraphicsUnit.Pixel);
            label1.Location = new Point(14, 17);
            label1.Name = "label1";
            label1.Size = new Size(130, 40);
            label1.TabIndex = 0;
            label1.Text = "Invoice";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnDashboard
            // 
            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            btnDashboard.Location = new Point(262, 4);
            btnDashboard.Margin = new Padding(3, 4, 3, 4);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(110, 67);
            btnDashboard.TabIndex = 1;
            btnDashboard.Text = "Dashboard";
            btnDashboard.UseVisualStyleBackColor = true;
            btnDashboard.Click += btnDashboard_Click;
            // 
            // btnInvoiceSelection
            // 
            btnInvoiceSelection.FlatAppearance.BorderColor = Color.White;
            btnInvoiceSelection.FlatAppearance.BorderSize = 0;
            btnInvoiceSelection.FlatStyle = FlatStyle.Flat;
            btnInvoiceSelection.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            btnInvoiceSelection.Location = new Point(378, 4);
            btnInvoiceSelection.Margin = new Padding(3, 4, 3, 4);
            btnInvoiceSelection.Name = "btnInvoiceSelection";
            btnInvoiceSelection.Size = new Size(153, 67);
            btnInvoiceSelection.TabIndex = 1;
            btnInvoiceSelection.Text = "Invoice Selection";
            btnInvoiceSelection.UseVisualStyleBackColor = true;
            btnInvoiceSelection.Click += btnInvoiceSelection_Click;
            // 
            // btnExportInvoice
            // 
            btnExportInvoice.FlatAppearance.BorderSize = 0;
            btnExportInvoice.FlatStyle = FlatStyle.Flat;
            btnExportInvoice.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            btnExportInvoice.Location = new Point(537, 4);
            btnExportInvoice.Margin = new Padding(3, 4, 3, 4);
            btnExportInvoice.Name = "btnExportInvoice";
            btnExportInvoice.Size = new Size(137, 67);
            btnExportInvoice.TabIndex = 1;
            btnExportInvoice.Text = "Export Invoice";
            btnExportInvoice.UseVisualStyleBackColor = true;
            btnExportInvoice.Click += btnExportInvoice_Click;
            // 
            // panDashboard
            // 
            panDashboard.BackColor = Color.FromArgb(104, 109, 244);
            panDashboard.Location = new Point(262, 79);
            panDashboard.Margin = new Padding(3, 4, 3, 4);
            panDashboard.Name = "panDashboard";
            panDashboard.Size = new Size(110, 7);
            panDashboard.TabIndex = 2;
            // 
            // panInvoiceSelection
            // 
            panInvoiceSelection.BackColor = Color.FromArgb(104, 109, 244);
            panInvoiceSelection.Location = new Point(378, 79);
            panInvoiceSelection.Margin = new Padding(3, 4, 3, 4);
            panInvoiceSelection.Name = "panInvoiceSelection";
            panInvoiceSelection.Size = new Size(153, 7);
            panInvoiceSelection.TabIndex = 2;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1507, 849);
            Controls.Add(panel1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Main";
            Text = "Main";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            rightMenuPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel rightMenuPanel;
        private Button btnDropDown;
        private Panel panExportInvoice;
        private Label label1;
        private Button btnDashboard;
        private Button btnInvoiceSelection;
        private Button btnExportInvoice;
        private Panel panDashboard;
        private Panel panInvoiceSelection;
        private Label lblNetworkStatus;
        private Label lblWorkerService;
    }
}