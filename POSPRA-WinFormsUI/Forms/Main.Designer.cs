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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            panel1 = new Panel();
            rightMenuPanel = new Panel();
            btnDropDown = new Button();
            btnNotification = new Button();
            panExportInvoice = new Panel();
            label1 = new Label();
            btnDashboard = new Button();
            btnItemEntry = new Button();
            panItemEntry = new Panel();
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
            panel1.Controls.Add(rightMenuPanel);
            panel1.Controls.Add(panExportInvoice);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(btnDashboard);
            panel1.Controls.Add(btnItemEntry);
            panel1.Controls.Add(panItemEntry);
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
            // rightMenuPanel
            // 
            rightMenuPanel.Controls.Add(btnDropDown);
            rightMenuPanel.Controls.Add(btnNotification);
            rightMenuPanel.Dock = DockStyle.Right;
            rightMenuPanel.Location = new Point(1329, 0);
            rightMenuPanel.Margin = new Padding(3, 4, 3, 4);
            rightMenuPanel.Name = "rightMenuPanel";
            rightMenuPanel.Size = new Size(178, 85);
            rightMenuPanel.TabIndex = 2;
            // 
            // btnDropDown
            // 
            btnDropDown.Dock = DockStyle.Right;
            btnDropDown.FlatAppearance.BorderSize = 0;
            btnDropDown.FlatStyle = FlatStyle.Flat;
            btnDropDown.Image = (Image)resources.GetObject("btnDropDown.Image");
            btnDropDown.ImageAlign = ContentAlignment.MiddleLeft;
            btnDropDown.Location = new Point(92, 0);
            btnDropDown.Margin = new Padding(3, 4, 3, 4);
            btnDropDown.Name = "btnDropDown";
            btnDropDown.Size = new Size(86, 85);
            btnDropDown.TabIndex = 2;
            btnDropDown.Text = "▼";
            btnDropDown.TextAlign = ContentAlignment.MiddleRight;
            btnDropDown.UseVisualStyleBackColor = true;
            // 
            // btnNotification
            // 
            btnNotification.Dock = DockStyle.Left;
            btnNotification.FlatAppearance.BorderSize = 0;
            btnNotification.FlatStyle = FlatStyle.Flat;
            //btnNotification.Image = Properties.Resources.notification_filled;
            btnNotification.Location = new Point(0, 0);
            btnNotification.Margin = new Padding(3, 4, 3, 4);
            btnNotification.Name = "btnNotification";
            btnNotification.Size = new Size(87, 85);
            btnNotification.TabIndex = 2;
            btnNotification.UseVisualStyleBackColor = true;
            // 
            // panExportInvoice
            // 
            panExportInvoice.BackColor = Color.FromArgb(104, 109, 244);
            panExportInvoice.Location = new Point(647, 79);
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
            // btnItemEntry
            // 
            btnItemEntry.FlatAppearance.BorderSize = 0;
            btnItemEntry.FlatStyle = FlatStyle.Flat;
            btnItemEntry.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            btnItemEntry.Location = new Point(538, 4);
            btnItemEntry.Margin = new Padding(3, 4, 3, 4);
            btnItemEntry.Name = "btnItemEntry";
            btnItemEntry.Size = new Size(102, 67);
            btnItemEntry.TabIndex = 1;
            btnItemEntry.Text = "Item Entry";
            btnItemEntry.UseVisualStyleBackColor = true;
            btnItemEntry.Click += btnItemEntry_Click;
            // 
            // panItemEntry
            // 
            panItemEntry.BackColor = Color.FromArgb(104, 109, 244);
            panItemEntry.Location = new Point(538, 79);
            panItemEntry.Margin = new Padding(3, 4, 3, 4);
            panItemEntry.Name = "panItemEntry";
            panItemEntry.Size = new Size(102, 7);
            panItemEntry.TabIndex = 2;
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
            btnExportInvoice.Location = new Point(647, 4);
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
            Load += Main_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            rightMenuPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel rightMenuPanel;
        private Button btnDropDown;
        private Button btnNotification;
        private Panel panExportInvoice;
        private Label label1;
        private Button btnDashboard;
        private Button btnItemEntry;
        private Panel panItemEntry;
        private Button btnInvoiceSelection;
        private Button btnExportInvoice;
        private Panel panDashboard;
        private Panel panInvoiceSelection;
    }
}