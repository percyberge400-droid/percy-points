namespace POSPRA_WinFormsUI.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnClose;

        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Panel loginBox;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblLoginPortal;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.Button btnLogin;

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
            btnClose = new Button();
            leftPanel = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            rightPanel = new Panel();
            loginBox = new Panel();
            label2 = new Label();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            chkShowPassword = new CheckBox();
            btnLogin = new Button();
            txtPassword = new TextBox();
            txtUsername = new TextBox();
            lblLoginPortal = new Label();
            picLogo = new PictureBox();
            leftPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            loginBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();
            // 
            // btnClose
            // 
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClose.ForeColor = Color.Gray;
            btnClose.Location = new Point(417, 10);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(30, 30);
            btnClose.TabIndex = 1;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            btnClose.MouseEnter += btnClose_MouseEnter;
            btnClose.MouseLeave += btnClose_MouseLeave;
            // 
            // leftPanel
            // 
            leftPanel.BackColor = Color.Transparent;
            leftPanel.BackgroundImage = Resources.login_bg1;
            leftPanel.BackgroundImageLayout = ImageLayout.Stretch;
            leftPanel.Controls.Add(lblSubtitle);
            leftPanel.Controls.Add(lblTitle);
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Location = new Point(0, 0);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(457, 600);
            leftPanel.TabIndex = 1;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Microsoft Sans Serif", 15F);
            lblSubtitle.ForeColor = Color.DimGray;
            lblSubtitle.Location = new Point(111, 274);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(221, 87);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "Effortlessly handle\r\nyour invoices right\r\nhere.";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 30F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(106, 194);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(292, 58);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Invoice app";
            // 
            // rightPanel
            // 
            rightPanel.BackColor = Color.Transparent;
            rightPanel.BackgroundImage = Resources.login_bg2;
            rightPanel.BackgroundImageLayout = ImageLayout.Stretch;
            rightPanel.Controls.Add(loginBox);
            rightPanel.Controls.Add(btnClose);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(457, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(457, 600);
            rightPanel.TabIndex = 0;
            // 
            // loginBox
            // 
            loginBox.Anchor = AnchorStyles.None;
            loginBox.BackColor = Color.White;
            loginBox.Controls.Add(label2);
            loginBox.Controls.Add(label1);
            loginBox.Controls.Add(pictureBox1);
            loginBox.Controls.Add(chkShowPassword);
            loginBox.Controls.Add(btnLogin);
            loginBox.Controls.Add(txtPassword);
            loginBox.Controls.Add(txtUsername);
            loginBox.Controls.Add(lblLoginPortal);
            loginBox.Controls.Add(picLogo);
            loginBox.Location = new Point(29, 67);
            loginBox.Name = "loginBox";
            loginBox.Size = new Size(400, 467);
            loginBox.TabIndex = 0;
            loginBox.Paint += loginBox_Paint;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            label2.ForeColor = Color.DimGray;
            label2.Location = new Point(46, 257);
            label2.Name = "label2";
            label2.Size = new Size(73, 19);
            label2.TabIndex = 0;
            label2.Text = "Password";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            label1.ForeColor = Color.DimGray;
            label1.Location = new Point(46, 184);
            label1.Name = "label1";
            label1.Size = new Size(55, 19);
            label1.TabIndex = 1;
            label1.Text = "POS ID";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Resources.PRAL_3D_LOGO;
            pictureBox1.Location = new Point(46, 27);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(158, 80);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(84, 105, 254);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(46, 354);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(309, 37);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Microsoft Sans Serif", 9F);
            txtPassword.Location = new Point(46, 280);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Password";
            txtPassword.Size = new Size(308, 24);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("Microsoft Sans Serif", 9F);
            txtUsername.Location = new Point(46, 207);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "123456";
            txtUsername.Size = new Size(308, 24);
            txtUsername.TabIndex = 0;
            // 
            // lblLoginPortal
            // 
            lblLoginPortal.AutoSize = true;
            lblLoginPortal.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblLoginPortal.ForeColor = Color.FromArgb(84, 105, 254);
            lblLoginPortal.Location = new Point(126, 129);
            lblLoginPortal.Name = "lblLoginPortal";
            lblLoginPortal.Size = new Size(179, 32);
            lblLoginPortal.TabIndex = 4;
            lblLoginPortal.Text = "Login Portal";
            // 
            // picLogo
            // 
            picLogo.Image = Resources.pra_logo;
            picLogo.Location = new Point(201, 32);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(154, 80);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 5;
            picLogo.TabStop = false;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(914, 600);
            Controls.Add(rightPanel);
            Controls.Add(leftPanel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            rightPanel.ResumeLayout(false);
            loginBox.ResumeLayout(false);
            loginBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);

        }
        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private Label label2;
    }
}
