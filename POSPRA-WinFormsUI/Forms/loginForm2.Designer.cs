namespace POSPRA_WinFormsUI.Forms
{
    partial class LoginForm2
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel rightPanel;

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
            rightPanel = new Panel();
            loginBox = new Panel();
            btnClose = new Button();
            pictureBox2 = new PictureBox();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            btnLogin = new Button();
            txtPassword = new TextBox();
            txtUsername = new TextBox();
            lblLoginPortal = new Label();
            leftPanel = new Panel();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            picLogo = new PictureBox();
            pictureBox1 = new PictureBox();
            eServicesNo = new Label();
            generalinquiryNo = new Label();
            label11 = new Label();
            label10 = new Label();
            prawebsite = new Label();
            rightPanel.SuspendLayout();
            loginBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // rightPanel
            // 
            rightPanel.BackColor = Color.Transparent;
            rightPanel.BackgroundImage = Resources.loginBgL2;
            rightPanel.BackgroundImageLayout = ImageLayout.Stretch;
            rightPanel.Controls.Add(loginBox);
            rightPanel.Location = new Point(582, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(396, 587);
            rightPanel.TabIndex = 0;
            // 
            // loginBox
            // 
            loginBox.BackColor = Color.Transparent;
            loginBox.Controls.Add(btnClose);
            loginBox.Controls.Add(pictureBox2);
            loginBox.Controls.Add(label3);
            loginBox.Controls.Add(label2);
            loginBox.Controls.Add(label1);
            loginBox.Controls.Add(btnLogin);
            loginBox.Controls.Add(txtPassword);
            loginBox.Controls.Add(txtUsername);
            loginBox.Controls.Add(lblLoginPortal);
            loginBox.Location = new Point(3, 0);
            loginBox.Name = "loginBox";
            loginBox.Size = new Size(393, 583);
            loginBox.TabIndex = 0;
            loginBox.Paint += loginBox_Paint;
            // 
            // btnClose
            // 
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClose.ForeColor = Color.FromArgb(48, 59, 78);
            btnClose.Location = new Point(339, 12);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(30, 30);
            btnClose.TabIndex = 11;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            pictureBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox2.Location = new Point(62, 102);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(260, 60);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 9;
            pictureBox2.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.White;
            label3.Location = new Point(62, 217);
            label3.Name = "label3";
            label3.Size = new Size(260, 25);
            label3.TabIndex = 5;
            label3.Text = "Please login to continue...";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            label2.ForeColor = Color.White;
            label2.Location = new Point(45, 338);
            label2.Name = "label2";
            label2.Size = new Size(57, 23);
            label2.TabIndex = 0;
            label2.Text = "Token";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            label1.ForeColor = Color.White;
            label1.Location = new Point(45, 261);
            label1.Name = "label1";
            label1.Size = new Size(66, 23);
            label1.TabIndex = 1;
            label1.Text = "POS ID";
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(48, 59, 78);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(45, 419);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(309, 46);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(45, 364);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "********";
            txtPassword.Size = new Size(309, 27);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = SystemColors.Window;
            txtUsername.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtUsername.Location = new Point(45, 287);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "123456";
            txtUsername.Size = new Size(309, 27);
            txtUsername.TabIndex = 0;
            // 
            // lblLoginPortal
            // 
            lblLoginPortal.AutoSize = true;
            lblLoginPortal.Font = new Font("Microsoft Sans Serif", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblLoginPortal.ForeColor = Color.White;
            lblLoginPortal.Location = new Point(82, 178);
            lblLoginPortal.Name = "lblLoginPortal";
            lblLoginPortal.Size = new Size(210, 39);
            lblLoginPortal.TabIndex = 4;
            lblLoginPortal.Text = "WELCOME!";
            // 
            // leftPanel
            // 
            leftPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            leftPanel.BackColor = Color.Transparent;
            leftPanel.BackgroundImage = Resources.loginBgR2;
            leftPanel.BackgroundImageLayout = ImageLayout.Stretch;
            leftPanel.Controls.Add(label8);
            leftPanel.Controls.Add(label7);
            leftPanel.Controls.Add(label6);
            leftPanel.Controls.Add(picLogo);
            leftPanel.Controls.Add(pictureBox1);
            leftPanel.Controls.Add(eServicesNo);
            leftPanel.Controls.Add(generalinquiryNo);
            leftPanel.Controls.Add(label11);
            leftPanel.Controls.Add(label10);
            leftPanel.Controls.Add(prawebsite);
            leftPanel.Location = new Point(-10, 0);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(611, 586);
            leftPanel.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label8.ForeColor = Color.DimGray;
            label8.Location = new Point(47, 386);
            label8.Name = "label8";
            label8.Size = new Size(380, 25);
            label8.TabIndex = 11;
            label8.Text = "Effortlessly handle your invoices right here.";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft Sans Serif", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.FromArgb(72, 167, 135);
            label7.Location = new Point(69, 347);
            label7.Name = "label7";
            label7.Size = new Size(302, 39);
            label7.TabIndex = 12;
            label7.Text = "YOUR INVOICES";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.FromArgb(72, 167, 135);
            label6.Location = new Point(38, 308);
            label6.Name = "label6";
            label6.Size = new Size(333, 39);
            label6.TabIndex = 11;
            label6.Text = "TAKE CHARGE OF";
            // 
            // picLogo
            // 
            picLogo.Location = new Point(116, 480);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(92, 55);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 8;
            picLogo.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Resources.PRAL_3D_LOGO;
            pictureBox1.Location = new Point(47, 480);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(66, 55);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // eServicesNo
            // 
            eServicesNo.AutoSize = true;
            eServicesNo.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            eServicesNo.ForeColor = Color.DimGray;
            eServicesNo.Location = new Point(453, 499);
            eServicesNo.Name = "eServicesNo";
            eServicesNo.Size = new Size(0, 18);
            eServicesNo.TabIndex = 18;
            // 
            // generalinquiryNo
            // 
            generalinquiryNo.AutoSize = true;
            generalinquiryNo.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            generalinquiryNo.ForeColor = Color.DimGray;
            generalinquiryNo.Location = new Point(468, 479);
            generalinquiryNo.Name = "generalinquiryNo";
            generalinquiryNo.Size = new Size(0, 18);
            generalinquiryNo.TabIndex = 17;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label11.ForeColor = Color.DimGray;
            label11.Location = new Point(362, 479);
            label11.Name = "label11";
            label11.Size = new Size(110, 18);
            label11.TabIndex = 16;
            label11.Text = "General Inquiry:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label10.ForeColor = Color.DimGray;
            label10.Location = new Point(378, 499);
            label10.Name = "label10";
            label10.Size = new Size(82, 18);
            label10.TabIndex = 15;
            label10.Text = "e-Services:";
            // 
            // prawebsite
            // 
            prawebsite.AutoSize = true;
            prawebsite.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            prawebsite.ForeColor = Color.FromArgb(72, 167, 135);
            prawebsite.Location = new Point(422, 517);
            prawebsite.Name = "prawebsite";
            prawebsite.Size = new Size(0, 18);
            prawebsite.TabIndex = 14;
            // 
            // LoginForm2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(976, 586);
            Controls.Add(rightPanel);
            Controls.Add(leftPanel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "LoginForm2";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            rightPanel.ResumeLayout(false);
            loginBox.ResumeLayout(false);
            loginBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private Panel loginBox;
        private Label label2;
        private Label label1;
        private Button btnLogin;
        private TextBox txtPassword;
        private TextBox txtUsername;
        private Label lblLoginPortal;
        private Panel leftPanel;
        private Label label3;
        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
        private Label label8;
        private Label label7;
        private Label label6;
        private PictureBox picLogo;
        private Label label13;
        private Label label12;
        private Label label11;
        private Label label10;
        private Label prawebsite;
        private Label eServicesNo;
        private Label generalinquiryNo;
        private Button btnClose;
    }
}
