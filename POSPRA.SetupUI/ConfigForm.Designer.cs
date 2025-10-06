namespace POSPRA.SetupUI
{
    partial class ConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            mainPanel = new Panel();
            btnClose = new Button();
            headerPanel = new Panel();
            lblSubtitle = new Label();
            lblWelcome = new Label();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            btnBrowse = new Button();
            txtFilePath = new TextBox();
            label3 = new Label();
            txtPassword = new TextBox();
            label2 = new Label();
            txtUsername = new TextBox();
            label1 = new Label();
            btnOk = new Button();
            mainPanel.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.Controls.Add(btnClose);
            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(btnBrowse);
            mainPanel.Controls.Add(txtFilePath);
            mainPanel.Controls.Add(label3);
            mainPanel.Controls.Add(txtPassword);
            mainPanel.Controls.Add(label2);
            mainPanel.Controls.Add(txtUsername);
            mainPanel.Controls.Add(label1);
            mainPanel.Controls.Add(btnOk);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(727, 528);
            mainPanel.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.Red;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 255, 255, 255);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(400, 380);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(110, 45);
            btnClose.TabIndex = 3;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(52, 168, 164);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblWelcome);
            headerPanel.Controls.Add(pictureBox1);
            headerPanel.Controls.Add(pictureBox2);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(727, 102);
            headerPanel.TabIndex = 0;
            headerPanel.Paint += headerPanel_Paint;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.BackColor = Color.Transparent;
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.White;
            lblSubtitle.Location = new Point(260, 66);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(206, 23);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Please login to continue...";
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.BackColor = Color.Transparent;
            lblWelcome.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Location = new Point(260, 15);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(197, 46);
            lblWelcome.TabIndex = 1;
            lblWelcome.Text = "WELCOME!";
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(594, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(121, 99);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(12, 0);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(130, 102);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // btnBrowse
            // 
            btnBrowse.BackColor = Color.FromArgb(52, 168, 164);
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.Location = new Point(460, 330);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(50, 32);
            btnBrowse.TabIndex = 3;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = false;
            btnBrowse.Click += btnBrowse_Click_1;
            // 
            // txtFilePath
            // 
            txtFilePath.BackColor = Color.White;
            txtFilePath.BorderStyle = BorderStyle.FixedSingle;
            txtFilePath.Font = new Font("Segoe UI", 11F);
            txtFilePath.Location = new Point(220, 330);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(230, 32);
            txtFilePath.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(64, 64, 64);
            label3.Location = new Point(220, 300);
            label3.Name = "label3";
            label3.Size = new Size(103, 23);
            label3.TabIndex = 7;
            label3.Text = "DB File Path";
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.Location = new Point(220, 250);
            txtPassword.MaxLength = 120;
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '•';
            txtPassword.Size = new Size(290, 32);
            txtPassword.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(64, 64, 64);
            label2.Location = new Point(220, 220);
            label2.Name = "label2";
            label2.Size = new Size(105, 23);
            label2.TabIndex = 5;
            label2.Text = "Access Code";
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.White;
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.Location = new Point(220, 170);
            txtUsername.MaxLength = 120;
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(290, 32);
            txtUsername.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(64, 64, 64);
            label1.Location = new Point(220, 140);
            label1.Name = "label1";
            label1.Size = new Size(64, 23);
            label1.TabIndex = 3;
            label1.Text = "POS ID";
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.RoyalBlue;
            btnOk.Cursor = Cursors.Hand;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnOk.ForeColor = Color.White;
            btnOk.Location = new Point(220, 380);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(110, 45);
            btnOk.TabIndex = 4;
            btnOk.Text = "Okay";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += btnOk_Click;
            // 
            // ConfigForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(727, 528);
            ControlBox = true;
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "ConfigForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "POS Configuration";
            TopMost = true;
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel mainPanel;
        private Panel headerPanel;
        private Label lblWelcome;
        private Label lblSubtitle;
        private Button btnClose;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Label label1;
        private TextBox txtUsername;
        private Label label2;
        private TextBox txtPassword;
        private Label label3;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnOk;
        private Button btnCancel;

        private void headerPanel_Paint(object sender, PaintEventArgs e)
        {
            // Create gradient from teal to blue
            using (System.Drawing.Drawing2D.LinearGradientBrush brush =
                new System.Drawing.Drawing2D.LinearGradientBrush(
                    headerPanel.ClientRectangle,
                    Color.FromArgb(52, 168, 164),  // Teal
                    Color.FromArgb(73, 160, 204),   // Blue
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
            }
        }
    }
}