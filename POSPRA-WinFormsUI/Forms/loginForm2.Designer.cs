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
            label4 = new Label();
            label5 = new Label();
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
            pictureBox4 = new PictureBox();
            pictureBox1 = new PictureBox();
            eServicesNo = new Label();
            generalinquiryNo = new Label();
            label11 = new Label();
            label10 = new Label();
            label9 = new Label();
            rightPanel.SuspendLayout();
            loginBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
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
            loginBox.Controls.Add(label4);
            loginBox.Controls.Add(label5);
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
            btnClose.Location = new Point(323, 33);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(30, 30);
            btnClose.TabIndex = 11;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            pictureBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox2.Image = Resources.posComponentWhite;
            pictureBox2.Location = new Point(82, 97);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(53, 59);
            pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox2.TabIndex = 9;
            pictureBox2.TabStop = false;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label4.Font = new Font("Microsoft Sans Serif", 12F);
            label4.ForeColor = Color.White;
            label4.Location = new Point(146, 126);
            label4.Name = "label4";
            label4.Size = new Size(149, 30);
            label4.TabIndex = 10;
            label4.Text = "COMPONENT";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label5.Font = new Font("Microsoft Sans Serif", 12F);
            label5.ForeColor = Color.White;
            label5.Location = new Point(146, 103);
            label5.Name = "label5";
            label5.Size = new Size(64, 23);
            label5.TabIndex = 8;
            label5.Text = "POS";
            label5.TextAlign = ContentAlignment.MiddleCenter;
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
            label2.Location = new Point(45, 334);
            label2.Name = "label2";
            label2.Size = new Size(107, 23);
            label2.TabIndex = 0;
            label2.Text = "Access Code";
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
            txtPassword.Font = new Font("Microsoft Sans Serif", 9F);
            txtPassword.Location = new Point(45, 360);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "********";
            txtPassword.Size = new Size(308, 24);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = SystemColors.Window;
            txtUsername.Font = new Font("Microsoft Sans Serif", 9F);
            txtUsername.Location = new Point(45, 287);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "123456";
            txtUsername.Size = new Size(308, 24);
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
            leftPanel.Controls.Add(pictureBox4);
            leftPanel.Controls.Add(pictureBox1);
            leftPanel.Controls.Add(eServicesNo);
            leftPanel.Controls.Add(generalinquiryNo);
            leftPanel.Controls.Add(label11);
            leftPanel.Controls.Add(label10);
            leftPanel.Controls.Add(label9);
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
            // pictureBox4
            // 
            pictureBox4.Image = Resources.loginPRAlogo;
            pictureBox4.Location = new Point(116, 480);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(92, 55);
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox4.TabIndex = 8;
            pictureBox4.TabStop = false;
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
            eServicesNo.Size = new Size(128, 18);
            eServicesNo.TabIndex = 18;
            eServicesNo.Text = "042-99205477-6";
            // 
            // generalinquiryNo
            // 
            generalinquiryNo.AutoSize = true;
            generalinquiryNo.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            generalinquiryNo.ForeColor = Color.DimGray;
            generalinquiryNo.Location = new Point(468, 479);
            generalinquiryNo.Name = "generalinquiryNo";
            generalinquiryNo.Size = new Size(113, 18);
            generalinquiryNo.TabIndex = 17;
            generalinquiryNo.Text = "042-99205481";
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
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label9.ForeColor = Color.FromArgb(72, 167, 135);
            label9.Location = new Point(422, 517);
            label9.Name = "label9";
            label9.Size = new Size(161, 18);
            label9.TabIndex = 14;
            label9.Text = "www.pra.punjab.gov.pk";
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
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
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
        private Label label4;
        private Label label5;
        private PictureBox pictureBox1;
        private Label label8;
        private Label label7;
        private Label label6;
        private PictureBox pictureBox4;
        private Label label13;
        private Label label12;
        private Label label11;
        private Label label10;
        private Label label9;
        private Label eServicesNo;
        private Label generalinquiryNo;
        private Button btnClose;
    }
}
