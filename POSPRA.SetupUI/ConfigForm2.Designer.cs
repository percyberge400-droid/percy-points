using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace POSPRA.SetupUI
{
    partial class ConfigForm2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            mainPanel = new Panel();
            panel1 = new Panel();
            btnupdateLOGO = new RoundedButton();
            btnOk = new RoundedButton();
            btnCancel = new RoundedButton();
            lblWelcome = new Label();
            lblMessage = new Label();
            btnBrowseOLD = new RoundedButton();
            txtOldDB = new TextBox();
            label4 = new Label();
            rdoProduction = new RadioButton();
            rdoSandbox = new RadioButton();
            label1 = new Label();
            progressBar = new ProgressBar();
            headerPanel = new Panel();
            label10 = new Label();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            pictureBox1 = new PictureBox();
            LOGO_img = new PictureBox();
            btnBrowse = new RoundedButton();
            txtFilePath = new TextBox();
            label3 = new Label();
            txtPassword = new TextBox();
            label2 = new Label();
            txtUsername = new TextBox();
            toolTip1 = new ToolTip(components);
            mainPanel.SuspendLayout();
            panel1.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)LOGO_img).BeginInit();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.Controls.Add(panel1);
            mainPanel.Controls.Add(lblWelcome);
            mainPanel.Controls.Add(lblMessage);
            mainPanel.Controls.Add(btnBrowseOLD);
            mainPanel.Controls.Add(txtOldDB);
            mainPanel.Controls.Add(label4);
            mainPanel.Controls.Add(rdoProduction);
            mainPanel.Controls.Add(rdoSandbox);
            mainPanel.Controls.Add(label1);
            mainPanel.Controls.Add(progressBar);
            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(btnBrowse);
            mainPanel.Controls.Add(txtFilePath);
            mainPanel.Controls.Add(label3);
            mainPanel.Controls.Add(txtPassword);
            mainPanel.Controls.Add(label2);
            mainPanel.Controls.Add(txtUsername);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(727, 628);
            mainPanel.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(243, 243, 243);
            panel1.Controls.Add(btnupdateLOGO);
            panel1.Controls.Add(btnOk);
            panel1.Controls.Add(btnCancel);
            panel1.Location = new Point(0, 547);
            panel1.Name = "panel1";
            panel1.Size = new Size(727, 81);
            panel1.TabIndex = 21;
            // 
            // btnupdateLOGO
            // 
            btnupdateLOGO.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnupdateLOGO.BackColor = Color.FromArgb(46, 49, 146);
            btnupdateLOGO.Cursor = Cursors.Hand;
            btnupdateLOGO.FlatAppearance.BorderSize = 0;
            btnupdateLOGO.FlatStyle = FlatStyle.Flat;
            btnupdateLOGO.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnupdateLOGO.ForeColor = Color.Transparent;
            btnupdateLOGO.Location = new Point(58, 20);
            btnupdateLOGO.Name = "btnupdateLOGO";
            btnupdateLOGO.Radius = 10;
            btnupdateLOGO.Size = new Size(191, 45);
            btnupdateLOGO.TabIndex = 9;
            btnupdateLOGO.Text = "Upload Logo";
            btnupdateLOGO.UseVisualStyleBackColor = false;
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.FromArgb(46, 49, 146);
            btnOk.Cursor = Cursors.Hand;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnOk.ForeColor = Color.White;
            btnOk.Location = new Point(524, 20);
            btnOk.Name = "btnOk";
            btnOk.Radius = 10;
            btnOk.Size = new Size(170, 45);
            btnOk.TabIndex = 11;
            btnOk.Text = "Okay";
            btnOk.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.BackColor = Color.FromArgb(51, 51, 51);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(296, 20);
            btnCancel.Name = "btnCancel";
            btnCancel.Radius = 10;
            btnCancel.Size = new Size(182, 45);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.BackColor = Color.Transparent;
            lblWelcome.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblWelcome.ForeColor = Color.FromArgb(0, 2, 34, 34);
            lblWelcome.Location = new Point(325, 42);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(153, 38);
            lblWelcome.TabIndex = 22;
            lblWelcome.Text = "WELCOME!";
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblMessage
            // 
            lblMessage.AutoEllipsis = true;
            lblMessage.AutoSize = true;
            lblMessage.BackColor = Color.White;
            lblMessage.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMessage.ForeColor = Color.FromArgb(76, 175, 80);
            lblMessage.Location = new Point(325, 427);
            lblMessage.MaximumSize = new Size(600, 0);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(0, 25);
            lblMessage.TabIndex = 20;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Visible = false;
            // 
            // btnBrowseOLD
            // 
            btnBrowseOLD.BackColor = Color.FromArgb(46, 49, 146);
            btnBrowseOLD.Cursor = Cursors.Hand;
            btnBrowseOLD.FlatAppearance.BorderSize = 0;
            btnBrowseOLD.FlatStyle = FlatStyle.Flat;
            btnBrowseOLD.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnBrowseOLD.ForeColor = Color.White;
            btnBrowseOLD.Image = Resource.browseIcon;
            btnBrowseOLD.Location = new Point(644, 383);
            btnBrowseOLD.Name = "btnBrowseOLD";
            btnBrowseOLD.Radius = 10;
            btnBrowseOLD.Size = new Size(50, 32);
            btnBrowseOLD.TabIndex = 8;
            btnBrowseOLD.Text = "...";
            btnBrowseOLD.UseVisualStyleBackColor = false;
            // 
            // txtOldDB
            // 
            txtOldDB.BackColor = Color.WhiteSmoke;
            txtOldDB.BorderStyle = BorderStyle.FixedSingle;
            txtOldDB.Font = new Font("Segoe UI", 11F);
            txtOldDB.Location = new Point(325, 383);
            txtOldDB.Name = "txtOldDB";
            txtOldDB.ReadOnly = true;
            txtOldDB.Size = new Size(313, 32);
            txtOldDB.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label4.ForeColor = Color.FromArgb(64, 64, 64);
            label4.Location = new Point(325, 353);
            label4.Name = "label4";
            label4.Size = new Size(135, 23);
            label4.TabIndex = 18;
            label4.Text = "Old DB File Path";
            // 
            // rdoProduction
            // 
            rdoProduction.AutoSize = true;
            rdoProduction.Font = new Font("Segoe UI", 10.2F);
            rdoProduction.Location = new Point(579, 107);
            rdoProduction.Name = "rdoProduction";
            rdoProduction.Size = new Size(115, 27);
            rdoProduction.TabIndex = 2;
            rdoProduction.Text = "Production";
            rdoProduction.UseVisualStyleBackColor = true;
            // 
            // rdoSandbox
            // 
            rdoSandbox.AutoSize = true;
            rdoSandbox.Checked = true;
            rdoSandbox.Font = new Font("Segoe UI", 10.2F);
            rdoSandbox.Location = new Point(442, 107);
            rdoSandbox.Name = "rdoSandbox";
            rdoSandbox.Size = new Size(97, 27);
            rdoSandbox.TabIndex = 1;
            rdoSandbox.TabStop = true;
            rdoSandbox.Text = "SandBox";
            rdoSandbox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(64, 64, 64);
            label1.Location = new Point(325, 111);
            label1.Name = "label1";
            label1.Size = new Size(64, 23);
            label1.TabIndex = 12;
            label1.Text = "POS ID";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(325, 498);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(369, 21);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 23;
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(230, 245, 255);
            headerPanel.Controls.Add(label10);
            headerPanel.Controls.Add(label9);
            headerPanel.Controls.Add(label8);
            headerPanel.Controls.Add(label7);
            headerPanel.Controls.Add(label6);
            headerPanel.Controls.Add(label5);
            headerPanel.Controls.Add(pictureBox1);
            headerPanel.Controls.Add(LOGO_img);
            headerPanel.Dock = DockStyle.Left;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(296, 628);
            headerPanel.TabIndex = 0;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label10.ForeColor = Color.FromArgb(64, 64, 64);
            label10.Location = new Point(36, 307);
            label10.Name = "label10";
            label10.Size = new Size(86, 23);
            label10.TabIndex = 31;
            label10.Text = "with ease.";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label9.ForeColor = Color.FromArgb(64, 64, 64);
            label9.Location = new Point(36, 284);
            label9.Name = "label9";
            label9.Size = new Size(168, 23);
            label9.TabIndex = 30;
            label9.Text = "your POS operations";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label8.ForeColor = Color.FromArgb(64, 64, 64);
            label8.Location = new Point(36, 261);
            label8.Name = "label8";
            label8.Size = new Size(179, 23);
            label8.TabIndex = 29;
            label8.Text = "complete control over";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label7.ForeColor = Color.FromArgb(64, 64, 64);
            label7.Location = new Point(36, 238);
            label7.Name = "label7";
            label7.Size = new Size(189, 23);
            label7.TabIndex = 28;
            label7.Text = "real-time, and maintain";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label6.ForeColor = Color.FromArgb(64, 64, 64);
            label6.Location = new Point(36, 215);
            label6.Name = "label6";
            label6.Size = new Size(166, 23);
            label6.TabIndex = 27;
            label6.Text = "your data synced in ";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label5.ForeColor = Color.FromArgb(64, 64, 64);
            label5.Location = new Point(36, 192);
            label5.Name = "label5";
            label5.Size = new Size(174, 23);
            label5.TabIndex = 26;
            label5.Text = "Stay connected, keep";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Resource.pralLogo;
            pictureBox1.Location = new Point(78, 383);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(132, 106);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 32;
            pictureBox1.TabStop = false;
            // 
            // LOGO_img
            // 
            LOGO_img.BackColor = Color.Transparent;
            LOGO_img.Image = Resource.pra_Comp_logo;
            LOGO_img.Location = new Point(12, 42);
            LOGO_img.Name = "LOGO_img";
            LOGO_img.Size = new Size(271, 100);
            LOGO_img.SizeMode = PictureBoxSizeMode.Zoom;
            LOGO_img.TabIndex = 33;
            LOGO_img.TabStop = false;
            // 
            // btnBrowse
            // 
            btnBrowse.BackColor = Color.FromArgb(46, 49, 146);
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.Image = Resource.browseIcon;
            btnBrowse.Location = new Point(644, 302);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Radius = 10;
            btnBrowse.Size = new Size(50, 32);
            btnBrowse.TabIndex = 6;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = false;
            // 
            // txtFilePath
            // 
            txtFilePath.BackColor = Color.WhiteSmoke;
            txtFilePath.BorderStyle = BorderStyle.FixedSingle;
            txtFilePath.Font = new Font("Segoe UI", 11F);
            txtFilePath.Location = new Point(325, 302);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.ReadOnly = true;
            txtFilePath.Size = new Size(313, 32);
            txtFilePath.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(64, 64, 64);
            label3.Location = new Point(325, 272);
            label3.Name = "label3";
            label3.Size = new Size(147, 23);
            label3.TabIndex = 22;
            label3.Text = "Local DB File Path";
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.Location = new Point(325, 222);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '•';
            txtPassword.Size = new Size(369, 32);
            txtPassword.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(64, 64, 64);
            label2.Location = new Point(325, 192);
            label2.Name = "label2";
            label2.Size = new Size(55, 23);
            label2.TabIndex = 24;
            label2.Text = "Token";
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.White;
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.Location = new Point(325, 142);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(369, 32);
            txtUsername.TabIndex = 3;
            // 
            // ConfigForm2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(727, 628);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            Name = "ConfigForm2";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            panel1.ResumeLayout(false);
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
        private PictureBox pictureBox1;
        private PictureBox LOGO_img;
        private TextBox txtUsername;
        private Label label2;
        private TextBox txtPassword;
        private Label label3;
        private TextBox txtFilePath;
        private RoundedButton btnBrowse;
        private RoundedButton btnOk;
        private ProgressBar progressBar;
        private RoundedButton btnCancel;
        private Label label1;
        private RadioButton rdoProduction;
        private RadioButton rdoSandbox;
        private Label lblMessage;
        private RoundedButton btnBrowseOLD;
        private TextBox txtOldDB;
        private Label label4;
        private Panel panel1;

        private void headerPanel_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                       headerPanel.ClientRectangle,
                       Color.FromArgb(52, 168, 164),
                       Color.FromArgb(73, 160, 204),
                       LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
            }
        }
        private Label label5;
        private Label label7;
        private Label label6;
        private Label label10;
        private Label label9;
        private Label label8;
        private RoundedButton btnupdateLOGO;
        private ToolTip toolTip1;
    }

    // Rounded Button (Smooth 10px corners, anti-aliased)
    public class RoundedButton : Button
    {
        private int _radius = 10;
        public int Radius { get => _radius; set { _radius = value; Invalidate(); } }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rect = new RectangleF(0, 0, Width, Height);
            using (GraphicsPath path = GetRoundPath(rect, Radius))
            {
                this.Region = new Region(path);
                using (Pen pen = new Pen(Color.FromArgb(210, 210, 210), 1.4f))
                    e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundPath(RectangleF rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r2 = radius / 2f;

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
