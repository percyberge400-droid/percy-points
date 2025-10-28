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
            lblMessage = new Label();
            btnBrowseOLD = new Button();
            txtOldDB = new TextBox();
            label4 = new Label();
            rdoProduction = new RadioButton();
            rdoSandbox = new RadioButton();
            label1 = new Label();
            btnCancel = new Button();
            progressBar = new ProgressBar();
            headerPanel = new Panel();
            lblSubtitle = new Label();
            lblWelcome = new Label();
            pictureBox1 = new PictureBox();
            LOGO_img = new PictureBox();
            btnBrowse = new Button();
            txtFilePath = new TextBox();
            label3 = new Label();
            txtPassword = new TextBox();
            label2 = new Label();
            txtUsername = new TextBox();
            btnOk = new Button();
            mainPanel.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)LOGO_img).BeginInit();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.Controls.Add(lblMessage);
            mainPanel.Controls.Add(btnBrowseOLD);
            mainPanel.Controls.Add(txtOldDB);
            mainPanel.Controls.Add(label4);
            mainPanel.Controls.Add(rdoProduction);
            mainPanel.Controls.Add(rdoSandbox);
            mainPanel.Controls.Add(label1);
            mainPanel.Controls.Add(btnCancel);
            mainPanel.Controls.Add(progressBar);
            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(btnBrowse);
            mainPanel.Controls.Add(txtFilePath);
            mainPanel.Controls.Add(label3);
            mainPanel.Controls.Add(txtPassword);
            mainPanel.Controls.Add(label2);
            mainPanel.Controls.Add(txtUsername);
            mainPanel.Controls.Add(btnOk);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(727, 628);
            mainPanel.TabIndex = 0;
            // 
            // lblMessage
            // 
            lblMessage.AutoEllipsis = true;
            lblMessage.AutoSize = true;
            lblMessage.BackColor = Color.White;
            lblMessage.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMessage.ForeColor = Color.FromArgb(76, 175, 80);
            lblMessage.Location = new Point(132, 484);
            lblMessage.MaximumSize = new Size(600, 0);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(0, 25);
            lblMessage.TabIndex = 20;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Visible = false;
            // 
            // btnBrowseOLD
            // 
            btnBrowseOLD.BackColor = Color.FromArgb(52, 168, 164);
            btnBrowseOLD.Cursor = Cursors.Hand;
            btnBrowseOLD.FlatAppearance.BorderSize = 0;
            btnBrowseOLD.FlatStyle = FlatStyle.Flat;
            btnBrowseOLD.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnBrowseOLD.ForeColor = Color.White;
            btnBrowseOLD.Location = new Point(460, 389);
            btnBrowseOLD.Name = "btnBrowseOLD";
            btnBrowseOLD.Size = new Size(50, 32);
            btnBrowseOLD.TabIndex = 17;
            btnBrowseOLD.Text = "...";
            btnBrowseOLD.UseVisualStyleBackColor = false;
            // 
            // txtOldDB
            // 
            txtOldDB.BackColor = Color.White;
            txtOldDB.BorderStyle = BorderStyle.FixedSingle;
            txtOldDB.Font = new Font("Segoe UI", 11F);
            txtOldDB.Location = new Point(220, 389);
            txtOldDB.Name = "txtOldDB";
            txtOldDB.Size = new Size(230, 32);
            txtOldDB.TabIndex = 19;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label4.ForeColor = Color.FromArgb(64, 64, 64);
            label4.Location = new Point(220, 359);
            label4.Name = "label4";
            label4.Size = new Size(135, 23);
            label4.TabIndex = 18;
            label4.Text = "Old DB File Path";
            // 
            // rdoProduction
            // 
            rdoProduction.AutoSize = true;
            rdoProduction.Font = new Font("Segoe UI", 10.2F);
            rdoProduction.Location = new Point(395, 113);
            rdoProduction.Name = "rdoProduction";
            rdoProduction.Size = new Size(115, 27);
            rdoProduction.TabIndex = 16;
            rdoProduction.Text = "Production";
            rdoProduction.UseVisualStyleBackColor = true;
            // 
            // rdoSandbox
            // 
            rdoSandbox.AutoSize = true;
            rdoSandbox.Checked = true;
            rdoSandbox.Font = new Font("Segoe UI", 10.2F);
            rdoSandbox.Location = new Point(294, 113);
            rdoSandbox.Name = "rdoSandbox";
            rdoSandbox.Size = new Size(97, 27);
            rdoSandbox.TabIndex = 15;
            rdoSandbox.TabStop = true;
            rdoSandbox.Text = "SandBox";
            rdoSandbox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(64, 64, 64);
            label1.Location = new Point(220, 117);
            label1.Name = "label1";
            label1.Size = new Size(64, 23);
            label1.TabIndex = 12;
            label1.Text = "POS ID";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.BackColor = Color.Red;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(400, 436);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 45);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Close";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(220, 530);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(290, 32);
            progressBar.TabIndex = 9;
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(52, 168, 164);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblWelcome);
            headerPanel.Controls.Add(pictureBox1);
            headerPanel.Controls.Add(LOGO_img);
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
            // LOGO_img
            // 
            LOGO_img.BackColor = Color.Transparent;
            LOGO_img.Location = new Point(12, 0);
            LOGO_img.Name = "LOGO_img";
            LOGO_img.Size = new Size(130, 102);
            LOGO_img.SizeMode = PictureBoxSizeMode.Zoom;
            LOGO_img.TabIndex = 2;
            LOGO_img.TabStop = false;
            // 
            // btnBrowse
            // 
            btnBrowse.BackColor = Color.FromArgb(52, 168, 164);
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.Location = new Point(460, 308);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(50, 32);
            btnBrowse.TabIndex = 3;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = false;
            // 
            // txtFilePath
            // 
            txtFilePath.BackColor = Color.White;
            txtFilePath.BorderStyle = BorderStyle.FixedSingle;
            txtFilePath.Font = new Font("Segoe UI", 11F);
            txtFilePath.Location = new Point(220, 308);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(230, 32);
            txtFilePath.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(64, 64, 64);
            label3.Location = new Point(220, 278);
            label3.Name = "label3";
            label3.Size = new Size(147, 23);
            label3.TabIndex = 7;
            label3.Text = "Local DB File Path";
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.Location = new Point(220, 228);
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
            label2.Location = new Point(220, 198);
            label2.Name = "label2";
            label2.Size = new Size(55, 23);
            label2.TabIndex = 5;
            label2.Text = "Token";
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.White;
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.Location = new Point(220, 148);
            txtUsername.MaxLength = 120;
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(290, 32);
            txtUsername.TabIndex = 1;
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.RoyalBlue;
            btnOk.Cursor = Cursors.Hand;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnOk.ForeColor = Color.White;
            btnOk.Location = new Point(220, 436);
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
            ClientSize = new Size(727, 628);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            Name = "ConfigForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)LOGO_img).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private Panel mainPanel;
        private Panel headerPanel;
        private Label lblWelcome;
        private Label lblSubtitle;
        private PictureBox pictureBox1;
        private PictureBox LOGO_img;
        private TextBox txtUsername;
        private Label label2;
        private TextBox txtPassword;
        private Label label3;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnOk;
        private ProgressBar progressBar;
        private Button btnCancel;
        private Label label1;
        private RadioButton rdoProduction;
        private RadioButton rdoSandbox;
        private Label lblMessage;
        private Button btnBrowseOLD;
        private TextBox txtOldDB;
        private Label label4;

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