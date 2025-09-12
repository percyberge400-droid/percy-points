namespace POSPRA_WinFormsUI
{
    partial class InvoiceEntry
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
            contentPanel = new Panel();
            lblItemEntry = new Label();
            pnlBasicInfo = new Panel();
            txtInvoiceDate = new DateTimePicker();
            lblBuyerInfo = new Label();
            lblSellerBusiness = new Label();
            lblBuyerBusiness = new Label();
            lblSellerInfo = new Label();
            btnProceed = new Button();
            txtBuyerBusiness = new TextBox();
            lblBuyerProvince = new Label();
            txtSellerBusiness = new TextBox();
            cmbBuyerProvince = new ComboBox();
            lblSellerProvince = new Label();
            lblBuyerAddress = new Label();
            lblBasicInfo = new Label();
            txtBuyerAddress = new TextBox();
            cmbSellerProvince = new ComboBox();
            lblInvoiceType = new Label();
            lblSellerAddress = new Label();
            chkSaleInvoice = new CheckBox();
            txtSellerAddress = new TextBox();
            chkDebitInvoice = new CheckBox();
            lblCustomerRegType = new Label();
            chkRegistered = new CheckBox();
            chkUnregistered = new CheckBox();
            lblInvoiceDate = new Label();
            lblBuyerRegNo = new Label();
            txtBuyerRegNo = new TextBox();
            lblSellerRegNo = new Label();
            txtSellerRegNo = new TextBox();
            lblInvoiceExtra = new Label();
            txtInvoiceExtra = new TextBox();
            contentPanel.SuspendLayout();
            pnlBasicInfo.SuspendLayout();
            SuspendLayout();
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.WhiteSmoke;
            contentPanel.Controls.Add(lblItemEntry);
            contentPanel.Controls.Add(pnlBasicInfo);
            contentPanel.Location = new Point(0, 0);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(1190, 788);
            contentPanel.TabIndex = 0;
            // 
            // lblItemEntry
            // 
            lblItemEntry.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblItemEntry.ForeColor = Color.FromArgb(17, 24, 39);
            lblItemEntry.Location = new Point(30, 17);
            lblItemEntry.Name = "lblItemEntry";
            lblItemEntry.Size = new Size(191, 37);
            lblItemEntry.TabIndex = 0;
            lblItemEntry.Text = "Invoice Entry";
            // 
            // pnlBasicInfo
            // 
            pnlBasicInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlBasicInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlBasicInfo.BackColor = Color.White;
            pnlBasicInfo.BackgroundImageLayout = ImageLayout.None;
            pnlBasicInfo.Controls.Add(txtInvoiceDate);
            pnlBasicInfo.Controls.Add(lblBuyerInfo);
            pnlBasicInfo.Controls.Add(lblSellerBusiness);
            pnlBasicInfo.Controls.Add(lblBuyerBusiness);
            pnlBasicInfo.Controls.Add(lblSellerInfo);
            pnlBasicInfo.Controls.Add(btnProceed);
            pnlBasicInfo.Controls.Add(txtBuyerBusiness);
            pnlBasicInfo.Controls.Add(lblBuyerProvince);
            pnlBasicInfo.Controls.Add(txtSellerBusiness);
            pnlBasicInfo.Controls.Add(cmbBuyerProvince);
            pnlBasicInfo.Controls.Add(lblSellerProvince);
            pnlBasicInfo.Controls.Add(lblBuyerAddress);
            pnlBasicInfo.Controls.Add(lblBasicInfo);
            pnlBasicInfo.Controls.Add(txtBuyerAddress);
            pnlBasicInfo.Controls.Add(cmbSellerProvince);
            pnlBasicInfo.Controls.Add(lblInvoiceType);
            pnlBasicInfo.Controls.Add(lblSellerAddress);
            pnlBasicInfo.Controls.Add(chkSaleInvoice);
            pnlBasicInfo.Controls.Add(txtSellerAddress);
            pnlBasicInfo.Controls.Add(chkDebitInvoice);
            pnlBasicInfo.Controls.Add(lblCustomerRegType);
            pnlBasicInfo.Controls.Add(chkRegistered);
            pnlBasicInfo.Controls.Add(chkUnregistered);
            pnlBasicInfo.Controls.Add(lblInvoiceDate);
            pnlBasicInfo.Controls.Add(lblBuyerRegNo);
            pnlBasicInfo.Controls.Add(txtBuyerRegNo);
            pnlBasicInfo.Controls.Add(lblSellerRegNo);
            pnlBasicInfo.Controls.Add(txtSellerRegNo);
            pnlBasicInfo.Controls.Add(lblInvoiceExtra);
            pnlBasicInfo.Controls.Add(txtInvoiceExtra);
            pnlBasicInfo.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            pnlBasicInfo.Location = new Point(30, 65);
            pnlBasicInfo.Margin = new Padding(10, 0, 10, 0);
            pnlBasicInfo.Name = "pnlBasicInfo";
            pnlBasicInfo.Size = new Size(1136, 315);
            pnlBasicInfo.TabIndex = 1;
            // 
            // txtInvoiceDate
            // 
            txtInvoiceDate.CalendarFont = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtInvoiceDate.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtInvoiceDate.Format = DateTimePickerFormat.Short;
            txtInvoiceDate.Location = new Point(817, 85);
            txtInvoiceDate.Enabled = false;
            txtInvoiceDate.Name = "txtInvoiceDate";
            txtInvoiceDate.Size = new Size(146, 27);
            txtInvoiceDate.TabIndex = 2;
            txtInvoiceDate.Value = new DateTime(2025, 9, 11, 0, 0, 0, 0);
            // 
            // lblBuyerInfo
            // 
            lblBuyerInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerInfo.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            lblBuyerInfo.ForeColor = Color.FromArgb(17, 24, 39);
            lblBuyerInfo.Location = new Point(577, 142);
            lblBuyerInfo.Name = "lblBuyerInfo";
            lblBuyerInfo.Size = new Size(200, 35);
            lblBuyerInfo.TabIndex = 0;
            lblBuyerInfo.Text = "Buyer Information";
            // 
            // lblSellerBusiness
            // 
            lblSellerBusiness.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblSellerBusiness.ForeColor = Color.FromArgb(75, 85, 99);
            lblSellerBusiness.Location = new Point(20, 182);
            lblSellerBusiness.Name = "lblSellerBusiness";
            lblSellerBusiness.Size = new Size(88, 20);
            lblSellerBusiness.TabIndex = 1;
            lblSellerBusiness.Text = "Business";
            // 
            // lblBuyerBusiness
            // 
            lblBuyerBusiness.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerBusiness.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblBuyerBusiness.ForeColor = Color.FromArgb(75, 85, 99);
            lblBuyerBusiness.Location = new Point(577, 182);
            lblBuyerBusiness.Name = "lblBuyerBusiness";
            lblBuyerBusiness.Size = new Size(78, 20);
            lblBuyerBusiness.TabIndex = 2;
            lblBuyerBusiness.Text = "Business";
            // 
            // lblSellerInfo
            // 
            lblSellerInfo.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            lblSellerInfo.ForeColor = Color.FromArgb(17, 24, 39);
            lblSellerInfo.Location = new Point(20, 142);
            lblSellerInfo.Name = "lblSellerInfo";
            lblSellerInfo.Size = new Size(200, 25);
            lblSellerInfo.TabIndex = 3;
            lblSellerInfo.Text = "Seller Information";
            // 
            // btnProceed
            // 
            btnProceed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProceed.BackColor = Color.FromArgb(99, 102, 241);
            btnProceed.FlatAppearance.BorderSize = 0;
            btnProceed.FlatStyle = FlatStyle.Flat;
            btnProceed.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnProceed.ForeColor = Color.Transparent;
            btnProceed.Location = new Point(995, 255);
            btnProceed.Name = "btnProceed";
            btnProceed.Size = new Size(126, 44);
            btnProceed.TabIndex = 4;
            btnProceed.Text = "Proceed";
            btnProceed.UseVisualStyleBackColor = false;
            // 
            // txtBuyerBusiness
            // 
            txtBuyerBusiness.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtBuyerBusiness.Font = new Font("Microsoft Sans Serif", 9F);
            txtBuyerBusiness.Location = new Point(577, 207);
            txtBuyerBusiness.Name = "txtBuyerBusiness";
            txtBuyerBusiness.PlaceholderText = "Buyer Business";
            txtBuyerBusiness.Size = new Size(190, 24);
            txtBuyerBusiness.TabIndex = 5;
            // 
            // lblBuyerProvince
            // 
            lblBuyerProvince.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerProvince.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblBuyerProvince.ForeColor = Color.FromArgb(75, 85, 99);
            lblBuyerProvince.Location = new Point(777, 182);
            lblBuyerProvince.Name = "lblBuyerProvince";
            lblBuyerProvince.Size = new Size(84, 20);
            lblBuyerProvince.TabIndex = 6;
            lblBuyerProvince.Text = "Province";
            // 
            // txtSellerBusiness
            // 
            txtSellerBusiness.Font = new Font("Microsoft Sans Serif", 9F);
            txtSellerBusiness.Location = new Point(20, 207);
            txtSellerBusiness.Name = "txtSellerBusiness";
            txtSellerBusiness.PlaceholderText = "Seller Business";
            txtSellerBusiness.Size = new Size(190, 24);
            txtSellerBusiness.TabIndex = 7;
            // 
            // cmbBuyerProvince
            // 
            cmbBuyerProvince.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbBuyerProvince.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBuyerProvince.Font = new Font("Microsoft Sans Serif", 9F);
            cmbBuyerProvince.Items.AddRange(new object[] { "Islamabad", "Punjab", "Sindh", "Khyber Pakhtunkhwa", "Balochistan", "Gilgit-Baltistan", "Azad Kashmir" });
            cmbBuyerProvince.Location = new Point(777, 207);
            cmbBuyerProvince.Name = "cmbBuyerProvince";
            cmbBuyerProvince.Size = new Size(150, 26);
            cmbBuyerProvince.TabIndex = 8;
            // 
            // lblSellerProvince
            // 
            lblSellerProvince.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblSellerProvince.ForeColor = Color.FromArgb(75, 85, 99);
            lblSellerProvince.Location = new Point(223, 182);
            lblSellerProvince.Name = "lblSellerProvince";
            lblSellerProvince.Size = new Size(90, 20);
            lblSellerProvince.TabIndex = 9;
            lblSellerProvince.Text = "Province";
            // 
            // lblBuyerAddress
            // 
            lblBuyerAddress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerAddress.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblBuyerAddress.ForeColor = Color.FromArgb(75, 85, 99);
            lblBuyerAddress.Location = new Point(937, 182);
            lblBuyerAddress.Name = "lblBuyerAddress";
            lblBuyerAddress.Size = new Size(78, 20);
            lblBuyerAddress.TabIndex = 10;
            lblBuyerAddress.Text = "Address";
            // 
            // lblBasicInfo
            // 
            lblBasicInfo.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBasicInfo.ForeColor = Color.FromArgb(17, 24, 39);
            lblBasicInfo.Location = new Point(20, 15);
            lblBasicInfo.Name = "lblBasicInfo";
            lblBasicInfo.Size = new Size(186, 25);
            lblBasicInfo.TabIndex = 11;
            lblBasicInfo.Text = "Basic Information";
            // 
            // txtBuyerAddress
            // 
            txtBuyerAddress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtBuyerAddress.Font = new Font("Microsoft Sans Serif", 9F);
            txtBuyerAddress.Location = new Point(931, 207);
            txtBuyerAddress.Name = "txtBuyerAddress";
            txtBuyerAddress.PlaceholderText = "Buyer Address";
            txtBuyerAddress.Size = new Size(190, 24);
            txtBuyerAddress.TabIndex = 12;
            // 
            // cmbSellerProvince
            // 
            cmbSellerProvince.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSellerProvince.Font = new Font("Microsoft Sans Serif", 9F);
            cmbSellerProvince.Items.AddRange(new object[] { "Islamabad", "Punjab", "Sindh", "Khyber Pakhtunkhwa", "Balochistan", "Gilgit-Baltistan", "Azad Kashmir" });
            cmbSellerProvince.Location = new Point(223, 207);
            cmbSellerProvince.Name = "cmbSellerProvince";
            cmbSellerProvince.Size = new Size(150, 26);
            cmbSellerProvince.TabIndex = 13;
            // 
            // lblInvoiceType
            // 
            lblInvoiceType.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInvoiceType.ForeColor = Color.FromArgb(75, 85, 99);
            lblInvoiceType.Location = new Point(20, 65);
            lblInvoiceType.Name = "lblInvoiceType";
            lblInvoiceType.Size = new Size(120, 20);
            lblInvoiceType.TabIndex = 14;
            lblInvoiceType.Text = "Invoice Type";
            // 
            // lblSellerAddress
            // 
            lblSellerAddress.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblSellerAddress.ForeColor = Color.FromArgb(75, 85, 99);
            lblSellerAddress.Location = new Point(386, 182);
            lblSellerAddress.Name = "lblSellerAddress";
            lblSellerAddress.Size = new Size(74, 20);
            lblSellerAddress.TabIndex = 15;
            lblSellerAddress.Text = "Address";
            // 
            // chkSaleInvoice
            // 
            chkSaleInvoice.Checked = true;
            chkSaleInvoice.CheckState = CheckState.Checked;
            chkSaleInvoice.Font = new Font("Microsoft Sans Serif", 9F);
            chkSaleInvoice.ForeColor = Color.FromArgb(75, 85, 99);
            chkSaleInvoice.Location = new Point(20, 87);
            chkSaleInvoice.Name = "chkSaleInvoice";
            chkSaleInvoice.Size = new Size(120, 27);
            chkSaleInvoice.TabIndex = 16;
            chkSaleInvoice.Text = "Sale Invoice";
            // 
            // txtSellerAddress
            // 
            txtSellerAddress.Font = new Font("Microsoft Sans Serif", 9F);
            txtSellerAddress.Location = new Point(386, 207);
            txtSellerAddress.Name = "txtSellerAddress";
            txtSellerAddress.PlaceholderText = "Seller Address";
            txtSellerAddress.Size = new Size(190, 24);
            txtSellerAddress.TabIndex = 17;
            // 
            // chkDebitInvoice
            // 
            chkDebitInvoice.Font = new Font("Microsoft Sans Serif", 9F);
            chkDebitInvoice.ForeColor = Color.FromArgb(75, 85, 99);
            chkDebitInvoice.Location = new Point(140, 87);
            chkDebitInvoice.Name = "chkDebitInvoice";
            chkDebitInvoice.Size = new Size(120, 27);
            chkDebitInvoice.TabIndex = 18;
            chkDebitInvoice.Text = "Debit Invoice";
            // 
            // lblCustomerRegType
            // 
            lblCustomerRegType.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblCustomerRegType.ForeColor = Color.FromArgb(75, 85, 99);
            lblCustomerRegType.Location = new Point(264, 65);
            lblCustomerRegType.Name = "lblCustomerRegType";
            lblCustomerRegType.Size = new Size(161, 20);
            lblCustomerRegType.TabIndex = 19;
            lblCustomerRegType.Text = "Customer Reg Type";
            // 
            // chkRegistered
            // 
            chkRegistered.Checked = true;
            chkRegistered.CheckState = CheckState.Checked;
            chkRegistered.Font = new Font("Microsoft Sans Serif", 9F);
            chkRegistered.ForeColor = Color.FromArgb(75, 85, 99);
            chkRegistered.Location = new Point(264, 87);
            chkRegistered.Name = "chkRegistered";
            chkRegistered.Size = new Size(104, 27);
            chkRegistered.TabIndex = 20;
            chkRegistered.Text = "Registered";
            // 
            // chkUnregistered
            // 
            chkUnregistered.Font = new Font("Microsoft Sans Serif", 9F);
            chkUnregistered.ForeColor = Color.FromArgb(75, 85, 99);
            chkUnregistered.Location = new Point(374, 87);
            chkUnregistered.Name = "chkUnregistered";
            chkUnregistered.Size = new Size(120, 27);
            chkUnregistered.TabIndex = 21;
            chkUnregistered.Text = "Unregistered";
            // 
            // lblInvoiceDate
            // 
            lblInvoiceDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblInvoiceDate.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblInvoiceDate.ForeColor = Color.FromArgb(75, 85, 99);
            lblInvoiceDate.Location = new Point(817, 63);
            lblInvoiceDate.Name = "lblInvoiceDate";
            lblInvoiceDate.Size = new Size(102, 20);
            lblInvoiceDate.TabIndex = 22;
            lblInvoiceDate.Text = "Invoice Date";
            // 
            // lblBuyerRegNo
            // 
            lblBuyerRegNo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerRegNo.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBuyerRegNo.ForeColor = Color.FromArgb(75, 85, 99);
            lblBuyerRegNo.Location = new Point(494, 63);
            lblBuyerRegNo.Name = "lblBuyerRegNo";
            lblBuyerRegNo.Size = new Size(90, 20);
            lblBuyerRegNo.TabIndex = 24;
            lblBuyerRegNo.Text = "Buyer Reg No";
            // 
            // txtBuyerRegNo
            // 
            txtBuyerRegNo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtBuyerRegNo.BackColor = Color.White;
            txtBuyerRegNo.Font = new Font("Microsoft Sans Serif", 9F);
            txtBuyerRegNo.ImeMode = ImeMode.Disable;
            txtBuyerRegNo.Location = new Point(495, 87);
            txtBuyerRegNo.Name = "txtBuyerRegNo";
            txtBuyerRegNo.PlaceholderText = "Buyer Reg No";
            txtBuyerRegNo.Size = new Size(150, 24);
            txtBuyerRegNo.TabIndex = 25;
            // 
            // lblSellerRegNo
            // 
            lblSellerRegNo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSellerRegNo.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblSellerRegNo.ForeColor = Color.FromArgb(75, 85, 99);
            lblSellerRegNo.Location = new Point(655, 63);
            lblSellerRegNo.Name = "lblSellerRegNo";
            lblSellerRegNo.Size = new Size(90, 20);
            lblSellerRegNo.TabIndex = 26;
            lblSellerRegNo.Text = "Seller Reg No";
            // 
            // txtSellerRegNo
            // 
            txtSellerRegNo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSellerRegNo.Font = new Font("Microsoft Sans Serif", 9F);
            txtSellerRegNo.Location = new Point(655, 87);
            txtSellerRegNo.Name = "txtSellerRegNo";
            txtSellerRegNo.PlaceholderText = "Seller Reg No";
            txtSellerRegNo.Size = new Size(150, 24);
            txtSellerRegNo.TabIndex = 27;
            // 
            // lblInvoiceExtra
            // 
            lblInvoiceExtra.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblInvoiceExtra.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblInvoiceExtra.ForeColor = Color.FromArgb(75, 85, 99);
            lblInvoiceExtra.Location = new Point(977, 63);
            lblInvoiceExtra.Name = "lblInvoiceExtra";
            lblInvoiceExtra.Size = new Size(104, 20);
            lblInvoiceExtra.TabIndex = 28;
            lblInvoiceExtra.Text = "Invoice Ref";
            // 
            // txtInvoiceExtra
            // 
            txtInvoiceExtra.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtInvoiceExtra.Font = new Font("Microsoft Sans Serif", 9F);
            txtInvoiceExtra.Location = new Point(977, 87);
            txtInvoiceExtra.Name = "txtInvoiceExtra";
            txtInvoiceExtra.PlaceholderText = "Invoice Ref";
            txtInvoiceExtra.Size = new Size(150, 24);
            txtInvoiceExtra.TabIndex = 29;
            // 
            // InvoiceEntry
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1186, 784);
            ControlBox = false;
            Controls.Add(contentPanel);
            Font = new Font("Microsoft Sans Serif", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "InvoiceEntry";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            contentPanel.ResumeLayout(false);
            pnlBasicInfo.ResumeLayout(false);
            pnlBasicInfo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Label lblItemEntry;
        private System.Windows.Forms.Label lblBasicInfo;
        private System.Windows.Forms.Label lblInvoiceType;
        private System.Windows.Forms.CheckBox chkSaleInvoice;
        private System.Windows.Forms.CheckBox chkDebitInvoice;
        private System.Windows.Forms.Label lblCustomerRegType;
        private System.Windows.Forms.CheckBox chkRegistered;
        private System.Windows.Forms.CheckBox chkUnregistered;
        private System.Windows.Forms.Label lblInvoiceDate;
        private System.Windows.Forms.DateTimePicker txtInvoiceDate;
        private System.Windows.Forms.Label lblBuyerRegNo;
        private System.Windows.Forms.TextBox txtBuyerRegNo;
        private System.Windows.Forms.Label lblSellerRegNo;
        private System.Windows.Forms.TextBox txtSellerRegNo;
        private System.Windows.Forms.Label lblInvoiceExtra;
        private System.Windows.Forms.TextBox txtInvoiceExtra;
        private System.Windows.Forms.Label lblSellerInfo;
        private System.Windows.Forms.Label lblSellerBusiness;
        private System.Windows.Forms.TextBox txtSellerBusiness;
        private System.Windows.Forms.Label lblSellerProvince;
        private System.Windows.Forms.ComboBox cmbSellerProvince;
        private System.Windows.Forms.Label lblSellerAddress;
        private System.Windows.Forms.TextBox txtSellerAddress;
        private System.Windows.Forms.Label lblBuyerInfo;
        private System.Windows.Forms.Label lblBuyerBusiness;
        private System.Windows.Forms.TextBox txtBuyerBusiness;
        private System.Windows.Forms.Label lblBuyerProvince;
        private System.Windows.Forms.ComboBox cmbBuyerProvince;
        private System.Windows.Forms.Label lblBuyerAddress;
        private System.Windows.Forms.TextBox txtBuyerAddress;
        private System.Windows.Forms.Button btnProceed;
        private System.Windows.Forms.GroupBox grpBlock1;
        private System.Windows.Forms.TextBox txtBuyerName;
        private Panel pnlBasicInfo;
    }
}