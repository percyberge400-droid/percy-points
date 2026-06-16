namespace Pos.WinFormsUI.Forms
{
    partial class Profile
    {
        private System.ComponentModel.IContainer components = null;

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
            lblProfile = new Label();
            pnlDetails = new Panel();
            tableLayoutPanel = new TableLayoutPanel();
            lblPosID = new Label();
            lblUserPosID = new Label();
            lblBranchName = new Label();
            lblUserBranchName = new Label();
            lblPhoneNo = new Label();
            lblUserPhoneNo = new Label();
            lblBranchAddress = new Label();
            lblUserBranchAddress = new Label();
            lblBusinessName = new Label();
            lblUserBusinessName = new Label();
            pnlHeader = new Panel();
            lblStatus = new Label();
            lblBusinessDetails = new Label();
            pnlDetails.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // lblProfile
            // 
            lblProfile.AutoSize = true;
            lblProfile.Dock = DockStyle.Top;
            lblProfile.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
            lblProfile.ForeColor = Color.FromArgb(64, 64, 64);
            lblProfile.Location = new Point(0, 0);
            lblProfile.Name = "lblProfile";
            lblProfile.Size = new Size(133, 41);
            lblProfile.TabIndex = 1;
            lblProfile.Text = "PROFILE";
            lblProfile.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlDetails
            // 
            pnlDetails.Anchor = AnchorStyles.None;
            pnlDetails.BackColor = Color.WhiteSmoke;
            pnlDetails.BorderStyle = BorderStyle.FixedSingle;
            pnlDetails.Controls.Add(tableLayoutPanel);
            pnlDetails.Controls.Add(pnlHeader);
            pnlDetails.Location = new Point(272, 160);
            pnlDetails.Name = "pnlDetails";
            pnlDetails.Padding = new Padding(20);
            pnlDetails.Size = new Size(720, 350);
            pnlDetails.TabIndex = 0;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40.2654877F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 59.7345123F));
            tableLayoutPanel.Controls.Add(lblPosID, 0, 0);
            tableLayoutPanel.Controls.Add(lblUserPosID, 1, 0);
            tableLayoutPanel.Controls.Add(lblBranchName, 0, 1);
            tableLayoutPanel.Controls.Add(lblUserBranchName, 1, 1);
            tableLayoutPanel.Controls.Add(lblPhoneNo, 0, 2);
            tableLayoutPanel.Controls.Add(lblUserPhoneNo, 1, 2);
            tableLayoutPanel.Controls.Add(lblBranchAddress, 0, 3);
            tableLayoutPanel.Controls.Add(lblUserBranchAddress, 1, 3);
            tableLayoutPanel.Controls.Add(lblBusinessName, 0, 4);
            tableLayoutPanel.Controls.Add(lblUserBusinessName, 1, 4);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(20, 65);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.Padding = new Padding(0, 20, 0, 0);
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.Size = new Size(678, 263);
            tableLayoutPanel.TabIndex = 0;
            // 
            // lblPosID
            // 
            lblPosID.Dock = DockStyle.Fill;
            lblPosID.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPosID.ForeColor = Color.Gray;
            lblPosID.Location = new Point(3, 20);
            lblPosID.Name = "lblPosID";
            lblPosID.Size = new Size(267, 45);
            lblPosID.TabIndex = 0;
            lblPosID.Text = "POS ID";
            lblPosID.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUserPosID
            // 
            lblUserPosID.Dock = DockStyle.Fill;
            lblUserPosID.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblUserPosID.ForeColor = Color.FromArgb(64, 64, 64);
            lblUserPosID.Location = new Point(276, 20);
            lblUserPosID.Name = "lblUserPosID";
            lblUserPosID.Size = new Size(399, 45);
            lblUserPosID.TabIndex = 1;
            lblUserPosID.Text = "dummy text";
            lblUserPosID.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBranchName
            // 
            lblBranchName.Dock = DockStyle.Fill;
            lblBranchName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBranchName.ForeColor = Color.Gray;
            lblBranchName.Location = new Point(3, 65);
            lblBranchName.Name = "lblBranchName";
            lblBranchName.Size = new Size(267, 45);
            lblBranchName.TabIndex = 2;
            lblBranchName.Text = "Branch Name";
            lblBranchName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUserBranchName
            // 
            lblUserBranchName.Dock = DockStyle.Fill;
            lblUserBranchName.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblUserBranchName.ForeColor = Color.FromArgb(64, 64, 64);
            lblUserBranchName.Location = new Point(276, 65);
            lblUserBranchName.Name = "lblUserBranchName";
            lblUserBranchName.Size = new Size(399, 45);
            lblUserBranchName.TabIndex = 3;
            lblUserBranchName.Text = "dummy text";
            lblUserBranchName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPhoneNo
            // 
            lblPhoneNo.Dock = DockStyle.Fill;
            lblPhoneNo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPhoneNo.ForeColor = Color.Gray;
            lblPhoneNo.Location = new Point(3, 110);
            lblPhoneNo.Name = "lblPhoneNo";
            lblPhoneNo.Size = new Size(267, 45);
            lblPhoneNo.TabIndex = 4;
            lblPhoneNo.Text = "Phone Number";
            lblPhoneNo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUserPhoneNo
            // 
            lblUserPhoneNo.Dock = DockStyle.Fill;
            lblUserPhoneNo.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblUserPhoneNo.ForeColor = Color.FromArgb(64, 64, 64);
            lblUserPhoneNo.Location = new Point(276, 110);
            lblUserPhoneNo.Name = "lblUserPhoneNo";
            lblUserPhoneNo.Size = new Size(399, 45);
            lblUserPhoneNo.TabIndex = 5;
            lblUserPhoneNo.Text = "dummy text";
            lblUserPhoneNo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBranchAddress
            // 
            lblBranchAddress.Dock = DockStyle.Fill;
            lblBranchAddress.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBranchAddress.ForeColor = Color.Gray;
            lblBranchAddress.Location = new Point(3, 155);
            lblBranchAddress.Name = "lblBranchAddress";
            lblBranchAddress.Size = new Size(267, 45);
            lblBranchAddress.TabIndex = 6;
            lblBranchAddress.Text = "Branch Address";
            lblBranchAddress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUserBranchAddress
            // 
            lblUserBranchAddress.Dock = DockStyle.Fill;
            lblUserBranchAddress.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblUserBranchAddress.ForeColor = Color.FromArgb(64, 64, 64);
            lblUserBranchAddress.Location = new Point(276, 155);
            lblUserBranchAddress.Name = "lblUserBranchAddress";
            lblUserBranchAddress.Size = new Size(399, 45);
            lblUserBranchAddress.TabIndex = 7;
            lblUserBranchAddress.Text = "dummy text";
            lblUserBranchAddress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBusinessName
            // 
            lblBusinessName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBusinessName.ForeColor = Color.Gray;
            lblBusinessName.Location = new Point(3, 200);
            lblBusinessName.Name = "lblBusinessName";
            lblBusinessName.Size = new Size(267, 45);
            lblBusinessName.TabIndex = 8;
            lblBusinessName.Text = "Business Name";
            lblBusinessName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUserBusinessName
            // 
            lblUserBusinessName.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblUserBusinessName.ForeColor = Color.FromArgb(64, 64, 64);
            lblUserBusinessName.Location = new Point(276, 200);
            lblUserBusinessName.Name = "lblUserBusinessName";
            lblUserBusinessName.Size = new Size(399, 45);
            lblUserBusinessName.TabIndex = 9;
            lblUserBusinessName.Text = "dummy text";
            lblUserBusinessName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblStatus);
            pnlHeader.Controls.Add(lblBusinessDetails);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(20, 20);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(678, 45);
            pnlHeader.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.BackColor = Color.FromArgb(198, 239, 206);
            lblStatus.Dock = DockStyle.Right;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(0, 97, 0);
            lblStatus.Location = new Point(605, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(10, 4, 10, 4);
            lblStatus.Size = new Size(73, 28);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Active";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblBusinessDetails
            // 
            lblBusinessDetails.AutoSize = true;
            lblBusinessDetails.Dock = DockStyle.Left;
            lblBusinessDetails.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblBusinessDetails.ForeColor = Color.FromArgb(64, 64, 64);
            lblBusinessDetails.Location = new Point(0, 0);
            lblBusinessDetails.Name = "lblBusinessDetails";
            lblBusinessDetails.Size = new Size(158, 28);
            lblBusinessDetails.TabIndex = 1;
            lblBusinessDetails.Text = "Business Details";
            // 
            // Profile
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1264, 665);
            Controls.Add(pnlDetails);
            Controls.Add(lblProfile);
            Name = "Profile";
            Text = "Profile";
            pnlDetails.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblProfile;
        private Panel pnlDetails;
        private Panel pnlHeader;
        private Label lblBusinessDetails;
        private Label lblStatus;
        private TableLayoutPanel tableLayoutPanel;

        private Label lblPosID;
        private Label lblBranchName;
        private Label lblPhoneNo;
        private Label lblBranchAddress;
        private Label lblBusinessName;

        private Label lblUserPosID;
        private Label lblUserBranchName;
        private Label lblUserPhoneNo;
        private Label lblUserBranchAddress;
        private Label lblUserBusinessName;
    }
}
