namespace POSPRA_WinFormsUI
{
    partial class item_entry
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            pnlBasicInfo = new Panel();
            uom = new TextBox();
            label10 = new Label();
            SROScheduleNo = new TextBox();
            label11 = new Label();
            label12 = new Label();
            STWithheld = new TextBox();
            textBox11 = new TextBox();
            label13 = new Label();
            Discount = new TextBox();
            label14 = new Label();
            SalesValueExclST = new TextBox();
            label6 = new Label();
            saletype = new TextBox();
            textBox6 = new TextBox();
            textBox7 = new TextBox();
            label8 = new Label();
            label9 = new Label();
            textBox4 = new TextBox();
            textBox5 = new TextBox();
            label7 = new Label();
            textBox3 = new TextBox();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            textBox1 = new TextBox();
            lblSellerBusiness = new Label();
            lblBuyerBusiness = new Label();
            mrp = new TextBox();
            hscode = new TextBox();
            lblBuyerAddress = new Label();
            lblBasicInfo = new Label();
            fed = new TextBox();
            lblInvoiceType = new Label();
            lblSellerAddress = new Label();
            qty = new TextBox();
            lblCustomerRegType = new Label();
            lblBuyerRegNo = new Label();
            lblInvoiceExtra = new Label();
            srosche = new TextBox();
            lblItemEntry = new Label();
            contentPanel = new Panel();
            btnProceed = new Button();
            btn_remove = new Button();
            lblInvoicesListing = new Label();
            lblTotalItems = new Label();
            dataGridView1 = new DataGridView();
            colSrNo = new DataGridViewTextBoxColumn();
            colInvoice = new DataGridViewTextBoxColumn();
            colPosId = new DataGridViewTextBoxColumn();
            colInvoiceSynced = new DataGridViewTextBoxColumn();
            colDueDate = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colQuantity = new DataGridViewTextBoxColumn();
            btnSave = new Button();
            button1 = new Button();
            pnlBasicInfo.SuspendLayout();
            contentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // pnlBasicInfo
            // 
            pnlBasicInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlBasicInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlBasicInfo.BackColor = Color.White;
            pnlBasicInfo.BackgroundImageLayout = ImageLayout.None;
            pnlBasicInfo.Controls.Add(uom);
            pnlBasicInfo.Controls.Add(label10);
            pnlBasicInfo.Controls.Add(SROScheduleNo);
            pnlBasicInfo.Controls.Add(label11);
            pnlBasicInfo.Controls.Add(label12);
            pnlBasicInfo.Controls.Add(STWithheld);
            pnlBasicInfo.Controls.Add(textBox11);
            pnlBasicInfo.Controls.Add(label13);
            pnlBasicInfo.Controls.Add(Discount);
            pnlBasicInfo.Controls.Add(label14);
            pnlBasicInfo.Controls.Add(SalesValueExclST);
            pnlBasicInfo.Controls.Add(label6);
            pnlBasicInfo.Controls.Add(saletype);
            pnlBasicInfo.Controls.Add(textBox6);
            pnlBasicInfo.Controls.Add(textBox7);
            pnlBasicInfo.Controls.Add(label8);
            pnlBasicInfo.Controls.Add(label9);
            pnlBasicInfo.Controls.Add(textBox4);
            pnlBasicInfo.Controls.Add(textBox5);
            pnlBasicInfo.Controls.Add(label7);
            pnlBasicInfo.Controls.Add(textBox3);
            pnlBasicInfo.Controls.Add(label5);
            pnlBasicInfo.Controls.Add(label4);
            pnlBasicInfo.Controls.Add(label3);
            pnlBasicInfo.Controls.Add(label2);
            pnlBasicInfo.Controls.Add(label1);
            pnlBasicInfo.Controls.Add(textBox1);
            pnlBasicInfo.Controls.Add(lblSellerBusiness);
            pnlBasicInfo.Controls.Add(lblBuyerBusiness);
            pnlBasicInfo.Controls.Add(mrp);
            pnlBasicInfo.Controls.Add(hscode);
            pnlBasicInfo.Controls.Add(lblBuyerAddress);
            pnlBasicInfo.Controls.Add(lblBasicInfo);
            pnlBasicInfo.Controls.Add(fed);
            pnlBasicInfo.Controls.Add(lblInvoiceType);
            pnlBasicInfo.Controls.Add(lblSellerAddress);
            pnlBasicInfo.Controls.Add(qty);
            pnlBasicInfo.Controls.Add(lblCustomerRegType);
            pnlBasicInfo.Controls.Add(lblBuyerRegNo);
            pnlBasicInfo.Controls.Add(lblInvoiceExtra);
            pnlBasicInfo.Controls.Add(srosche);
            pnlBasicInfo.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            pnlBasicInfo.Location = new Point(30, 65);
            pnlBasicInfo.Margin = new Padding(10, 0, 10, 0);
            pnlBasicInfo.Name = "pnlBasicInfo";
            pnlBasicInfo.Size = new Size(1148, 283);
            pnlBasicInfo.TabIndex = 6;
            // 
            // uom
            // 
            uom.Font = new Font("Microsoft Sans Serif", 9F);
            uom.Location = new Point(343, 90);
            uom.Name = "uom";
            uom.PlaceholderText = "uom";
            uom.Size = new Size(97, 24);
            uom.TabIndex = 41;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label10.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label10.ForeColor = Color.FromArgb(75, 85, 99);
            label10.Location = new Point(720, 204);
            label10.Name = "label10";
            label10.Size = new Size(107, 20);
            label10.TabIndex = 0;
            label10.Text = "Discount";
            // 
            // SROScheduleNo
            // 
            SROScheduleNo.Font = new Font("Microsoft Sans Serif", 9F);
            SROScheduleNo.Location = new Point(940, 227);
            SROScheduleNo.Name = "SROScheduleNo";
            SROScheduleNo.Size = new Size(200, 24);
            SROScheduleNo.TabIndex = 1;
            // 
            // label11
            // 
            label11.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label11.ForeColor = Color.FromArgb(75, 85, 99);
            label11.Location = new Point(20, 204);
            label11.Name = "label11";
            label11.Size = new Size(156, 20);
            label11.TabIndex = 2;
            label11.Text = "Product Description";
            // 
            // label12
            // 
            label12.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label12.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label12.ForeColor = Color.FromArgb(75, 85, 99);
            label12.Location = new Point(480, 204);
            label12.Name = "label12";
            label12.Size = new Size(212, 20);
            label12.TabIndex = 3;
            label12.Text = "Sales Tax Withheld at Source";
            // 
            // STWithheld
            // 
            STWithheld.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            STWithheld.Font = new Font("Microsoft Sans Serif", 9F);
            STWithheld.Location = new Point(480, 227);
            STWithheld.Name = "STWithheld";
            STWithheld.Size = new Size(200, 24);
            STWithheld.TabIndex = 4;
            // 
            // textBox11
            // 
            textBox11.Font = new Font("Microsoft Sans Serif", 9F);
            textBox11.Location = new Point(20, 227);
            textBox11.Name = "textBox11";
            textBox11.Size = new Size(200, 24);
            textBox11.TabIndex = 5;
            // 
            // label13
            // 
            label13.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label13.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label13.ForeColor = Color.FromArgb(75, 85, 99);
            label13.Location = new Point(942, 204);
            label13.Name = "label13";
            label13.Size = new Size(164, 20);
            label13.TabIndex = 6;
            label13.Text = "SRO Item Serial No.";
            // 
            // Discount
            // 
            Discount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Discount.Font = new Font("Microsoft Sans Serif", 9F);
            Discount.Location = new Point(720, 227);
            Discount.Name = "Discount";
            Discount.Size = new Size(200, 24);
            Discount.TabIndex = 7;
            // 
            // label14
            // 
            label14.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label14.ForeColor = Color.FromArgb(75, 85, 99);
            label14.Location = new Point(240, 204);
            label14.Name = "label14";
            label14.Size = new Size(200, 20);
            label14.TabIndex = 8;
            label14.Text = "Sales Value Excluding ST";
            // 
            // SalesValueExclST
            // 
            SalesValueExclST.Font = new Font("Microsoft Sans Serif", 9F);
            SalesValueExclST.Location = new Point(240, 227);
            SalesValueExclST.Name = "SalesValueExclST";
            SalesValueExclST.Size = new Size(200, 24);
            SalesValueExclST.TabIndex = 9;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label6.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label6.ForeColor = Color.FromArgb(75, 85, 99);
            label6.Location = new Point(720, 136);
            label6.Name = "label6";
            label6.Size = new Size(107, 20);
            label6.TabIndex = 10;
            label6.Text = "Fed Payable";
            // 
            // saletype
            // 
            saletype.Font = new Font("Microsoft Sans Serif", 9F);
            saletype.Location = new Point(940, 159);
            saletype.Name = "saletype";
            saletype.PlaceholderText = "Sale Type";
            saletype.Size = new Size(200, 24);
            saletype.TabIndex = 11;
            // 
            // textBox6
            // 
            textBox6.Font = new Font("Microsoft Sans Serif", 9F);
            textBox6.Location = new Point(718, 90);
            textBox6.Name = "textBox6";
            textBox6.PlaceholderText = "Extra Tax";
            textBox6.Size = new Size(90, 24);
            textBox6.TabIndex = 12;
            // 
            // textBox7
            // 
            textBox7.Font = new Font("Microsoft Sans Serif", 9F);
            textBox7.Location = new Point(824, 90);
            textBox7.Name = "textBox7";
            textBox7.PlaceholderText = "Furture Tax";
            textBox7.Size = new Size(87, 24);
            textBox7.TabIndex = 13;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label8.ForeColor = Color.FromArgb(75, 85, 99);
            label8.Location = new Point(821, 65);
            label8.Name = "label8";
            label8.Size = new Size(103, 20);
            label8.TabIndex = 14;
            label8.Text = "Furture Tax";
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label9.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label9.ForeColor = Color.FromArgb(75, 85, 99);
            label9.Location = new Point(718, 65);
            label9.Name = "label9";
            label9.Size = new Size(90, 20);
            label9.TabIndex = 15;
            label9.Text = "Extra Tax";
            // 
            // textBox4
            // 
            textBox4.Font = new Font("Microsoft Sans Serif", 9F);
            textBox4.Location = new Point(480, 88);
            textBox4.Name = "textBox4";
            textBox4.PlaceholderText = "Total Value";
            textBox4.Size = new Size(90, 24);
            textBox4.TabIndex = 16;
            // 
            // textBox5
            // 
            textBox5.Font = new Font("Microsoft Sans Serif", 9F);
            textBox5.Location = new Point(581, 88);
            textBox5.Name = "textBox5";
            textBox5.PlaceholderText = "Sales Tax";
            textBox5.Size = new Size(87, 24);
            textBox5.TabIndex = 17;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label7.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label7.ForeColor = Color.FromArgb(75, 85, 99);
            label7.Location = new Point(578, 63);
            label7.Name = "label7";
            label7.Size = new Size(90, 20);
            label7.TabIndex = 18;
            label7.Text = "Sales Tax";
            // 
            // textBox3
            // 
            textBox3.Font = new Font("Microsoft Sans Serif", 9F);
            textBox3.Location = new Point(240, 90);
            textBox3.Name = "textBox3";
            textBox3.PlaceholderText = "--%";
            textBox3.Size = new Size(80, 24);
            textBox3.TabIndex = 19;
            // 
            // label5
            // 
            label5.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label5.ForeColor = Color.FromArgb(75, 85, 99);
            label5.Location = new Point(343, 67);
            label5.Name = "label5";
            label5.Size = new Size(58, 20);
            label5.TabIndex = 21;
            label5.Text = "UOM";
            // 
            // label4
            // 
            label4.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.FromArgb(17, 24, 39);
            label4.Location = new Point(940, 27);
            label4.Name = "label4";
            label4.Size = new Size(205, 25);
            label4.TabIndex = 22;
            label4.Text = "SRO Schedule Number";
            // 
            // label3
            // 
            label3.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.FromArgb(17, 24, 39);
            label3.Location = new Point(707, 27);
            label3.Name = "label3";
            label3.Size = new Size(227, 25);
            label3.TabIndex = 23;
            label3.Text = "Tax Discount Information";
            // 
            // label2
            // 
            label2.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.FromArgb(17, 24, 39);
            label2.Location = new Point(480, 27);
            label2.Name = "label2";
            label2.Size = new Size(198, 25);
            label2.TabIndex = 24;
            label2.Text = "Sales Tax Information";
            // 
            // label1
            // 
            label1.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(17, 24, 39);
            label1.Location = new Point(240, 27);
            label1.Name = "label1";
            label1.Size = new Size(186, 25);
            label1.TabIndex = 25;
            label1.Text = "Pricing Information";
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBox1.BackColor = Color.White;
            textBox1.Font = new Font("Microsoft Sans Serif", 9F);
            textBox1.ImeMode = ImeMode.Disable;
            textBox1.Location = new Point(20, 88);
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "Item Code";
            textBox1.Size = new Size(171, 24);
            textBox1.TabIndex = 26;
            // 
            // lblSellerBusiness
            // 
            lblSellerBusiness.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblSellerBusiness.ForeColor = Color.FromArgb(75, 85, 99);
            lblSellerBusiness.Location = new Point(20, 136);
            lblSellerBusiness.Name = "lblSellerBusiness";
            lblSellerBusiness.Size = new Size(88, 20);
            lblSellerBusiness.TabIndex = 27;
            lblSellerBusiness.Text = "HS Code";
            // 
            // lblBuyerBusiness
            // 
            lblBuyerBusiness.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerBusiness.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblBuyerBusiness.ForeColor = Color.FromArgb(75, 85, 99);
            lblBuyerBusiness.Location = new Point(480, 136);
            lblBuyerBusiness.Name = "lblBuyerBusiness";
            lblBuyerBusiness.Size = new Size(212, 20);
            lblBuyerBusiness.TabIndex = 28;
            lblBuyerBusiness.Text = "MRP(Fixed Notified Value)";
            // 
            // mrp
            // 
            mrp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            mrp.Font = new Font("Microsoft Sans Serif", 9F);
            mrp.Location = new Point(480, 159);
            mrp.Name = "mrp";
            mrp.PlaceholderText = "MRP";
            mrp.Size = new Size(200, 24);
            mrp.TabIndex = 29;
            // 
            // hscode
            // 
            hscode.Font = new Font("Microsoft Sans Serif", 9F);
            hscode.Location = new Point(20, 159);
            hscode.Name = "hscode";
            hscode.PlaceholderText = "HS Code";
            hscode.Size = new Size(200, 24);
            hscode.TabIndex = 30;
            // 
            // lblBuyerAddress
            // 
            lblBuyerAddress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerAddress.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblBuyerAddress.ForeColor = Color.FromArgb(75, 85, 99);
            lblBuyerAddress.Location = new Point(942, 136);
            lblBuyerAddress.Name = "lblBuyerAddress";
            lblBuyerAddress.Size = new Size(95, 20);
            lblBuyerAddress.TabIndex = 31;
            lblBuyerAddress.Text = "Sale Type";
            // 
            // lblBasicInfo
            // 
            lblBasicInfo.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBasicInfo.ForeColor = Color.FromArgb(17, 24, 39);
            lblBasicInfo.Location = new Point(20, 27);
            lblBasicInfo.Name = "lblBasicInfo";
            lblBasicInfo.Size = new Size(186, 25);
            lblBasicInfo.TabIndex = 32;
            lblBasicInfo.Text = "Basic Information";
            // 
            // fed
            // 
            fed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            fed.Font = new Font("Microsoft Sans Serif", 9F);
            fed.Location = new Point(720, 159);
            fed.Name = "fed";
            fed.PlaceholderText = "FED Payable";
            fed.Size = new Size(200, 24);
            fed.TabIndex = 33;
            // 
            // lblInvoiceType
            // 
            lblInvoiceType.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblInvoiceType.ForeColor = Color.FromArgb(75, 85, 99);
            lblInvoiceType.Location = new Point(20, 65);
            lblInvoiceType.Name = "lblInvoiceType";
            lblInvoiceType.Size = new Size(120, 20);
            lblInvoiceType.TabIndex = 34;
            lblInvoiceType.Text = "Item Code";
            // 
            // lblSellerAddress
            // 
            lblSellerAddress.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblSellerAddress.ForeColor = Color.FromArgb(75, 85, 99);
            lblSellerAddress.Location = new Point(240, 136);
            lblSellerAddress.Name = "lblSellerAddress";
            lblSellerAddress.Size = new Size(74, 20);
            lblSellerAddress.TabIndex = 35;
            lblSellerAddress.Text = "Quantity";
            // 
            // qty
            // 
            qty.Font = new Font("Microsoft Sans Serif", 9F);
            qty.Location = new Point(240, 159);
            qty.Name = "qty";
            qty.PlaceholderText = "QUANTITY";
            qty.Size = new Size(200, 24);
            qty.TabIndex = 36;
            // 
            // lblCustomerRegType
            // 
            lblCustomerRegType.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblCustomerRegType.ForeColor = Color.FromArgb(75, 85, 99);
            lblCustomerRegType.Location = new Point(240, 65);
            lblCustomerRegType.Name = "lblCustomerRegType";
            lblCustomerRegType.Size = new Size(58, 20);
            lblCustomerRegType.TabIndex = 37;
            lblCustomerRegType.Text = "Rate";
            // 
            // lblBuyerRegNo
            // 
            lblBuyerRegNo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBuyerRegNo.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblBuyerRegNo.ForeColor = Color.FromArgb(75, 85, 99);
            lblBuyerRegNo.Location = new Point(480, 63);
            lblBuyerRegNo.Name = "lblBuyerRegNo";
            lblBuyerRegNo.Size = new Size(90, 20);
            lblBuyerRegNo.TabIndex = 38;
            lblBuyerRegNo.Text = "Total Value";
            // 
            // lblInvoiceExtra
            // 
            lblInvoiceExtra.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblInvoiceExtra.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblInvoiceExtra.ForeColor = Color.FromArgb(75, 85, 99);
            lblInvoiceExtra.Location = new Point(942, 65);
            lblInvoiceExtra.Name = "lblInvoiceExtra";
            lblInvoiceExtra.Size = new Size(93, 20);
            lblInvoiceExtra.TabIndex = 39;
            lblInvoiceExtra.Text = "SRO S No";
            // 
            // srosche
            // 
            srosche.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            srosche.Font = new Font("Microsoft Sans Serif", 9F);
            srosche.Location = new Point(940, 90);
            srosche.Name = "srosche";
            srosche.PlaceholderText = "SRO Schedule No";
            srosche.Size = new Size(200, 24);
            srosche.TabIndex = 40;
            // 
            // lblItemEntry
            // 
            lblItemEntry.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblItemEntry.ForeColor = Color.FromArgb(17, 24, 39);
            lblItemEntry.Location = new Point(30, 17);
            lblItemEntry.Name = "lblItemEntry";
            lblItemEntry.Size = new Size(191, 37);
            lblItemEntry.TabIndex = 5;
            lblItemEntry.Text = "Add Item";
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.WhiteSmoke;
            contentPanel.Controls.Add(btnProceed);
            contentPanel.Controls.Add(btn_remove);
            contentPanel.Controls.Add(lblInvoicesListing);
            contentPanel.Controls.Add(lblTotalItems);
            contentPanel.Controls.Add(dataGridView1);
            contentPanel.Controls.Add(btnSave);
            contentPanel.Controls.Add(button1);
            contentPanel.Controls.Add(lblItemEntry);
            contentPanel.Controls.Add(pnlBasicInfo);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 0);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(1202, 900);
            contentPanel.TabIndex = 0;
            // 
            // btnProceed
            // 
            btnProceed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProceed.BackColor = Color.FromArgb(99, 102, 241);
            btnProceed.FlatAppearance.BorderSize = 0;
            btnProceed.FlatStyle = FlatStyle.Flat;
            btnProceed.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnProceed.ForeColor = Color.Transparent;
            btnProceed.Location = new Point(638, 356);
            btnProceed.Name = "btnProceed";
            btnProceed.Size = new Size(126, 44);
            btnProceed.TabIndex = 10;
            btnProceed.Text = "➕ Add Item";
            btnProceed.UseVisualStyleBackColor = false;
            // 
            // btn_remove
            // 
            btn_remove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_remove.BackColor = Color.Red;
            btn_remove.FlatAppearance.BorderSize = 0;
            btn_remove.FlatStyle = FlatStyle.Flat;
            btn_remove.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_remove.ForeColor = Color.Transparent;
            btn_remove.Location = new Point(770, 356);
            btn_remove.Name = "btn_remove";
            btn_remove.Size = new Size(144, 44);
            btn_remove.TabIndex = 8;
            btn_remove.Text = "- Remove Item";
            btn_remove.UseVisualStyleBackColor = false;
            btn_remove.Click += btn_remove_Click;
            // 
            // lblInvoicesListing
            // 
            lblInvoicesListing.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInvoicesListing.ForeColor = Color.FromArgb(17, 24, 39);
            lblInvoicesListing.Location = new Point(30, 417);
            lblInvoicesListing.Name = "lblInvoicesListing";
            lblInvoicesListing.Size = new Size(191, 37);
            lblInvoicesListing.TabIndex = 0;
            lblInvoicesListing.Text = "Invoices Listing";
            // 
            // lblTotalItems
            // 
            lblTotalItems.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalItems.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalItems.ForeColor = Color.Blue;
            lblTotalItems.Location = new Point(1018, 860);
            lblTotalItems.Name = "lblTotalItems";
            lblTotalItems.Size = new Size(155, 25);
            lblTotalItems.TabIndex = 1;
            lblTotalItems.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(107, 114, 128);
            dataGridViewCellStyle1.SelectionBackColor = Color.White;
            dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(107, 114, 128);
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colSrNo, colInvoice, colPosId, colInvoiceSynced, colDueDate, colStatus, colQuantity });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(75, 85, 99);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(75, 85, 99);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(229, 231, 235);
            dataGridView1.Location = new Point(30, 465);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.RowTemplate.Height = 50;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1145, 392);
            dataGridView1.TabIndex = 9;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
            // 
            // colSrNo
            // 
            colSrNo.HeaderText = "Sr. No.";
            colSrNo.MinimumWidth = 6;
            colSrNo.Name = "colSrNo";
            colSrNo.ReadOnly = true;
            // 
            // colInvoice
            // 
            colInvoice.HeaderText = "Item Code";
            colInvoice.MinimumWidth = 6;
            colInvoice.Name = "colInvoice";
            colInvoice.ReadOnly = true;
            // 
            // colPosId
            // 
            colPosId.HeaderText = "Item Name";
            colPosId.MinimumWidth = 6;
            colPosId.Name = "colPosId";
            colPosId.ReadOnly = true;
            // 
            // colInvoiceSynced
            // 
            colInvoiceSynced.HeaderText = "Sale Type";
            colInvoiceSynced.MinimumWidth = 6;
            colInvoiceSynced.Name = "colInvoiceSynced";
            colInvoiceSynced.ReadOnly = true;
            // 
            // colDueDate
            // 
            colDueDate.HeaderText = "Rate";
            colDueDate.MinimumWidth = 6;
            colDueDate.Name = "colDueDate";
            colDueDate.ReadOnly = true;
            // 
            // colStatus
            // 
            colStatus.HeaderText = "Sales Value Exc ST";
            colStatus.MinimumWidth = 6;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            // 
            // colQuantity
            // 
            colQuantity.HeaderText = "Quantity";
            colQuantity.MinimumWidth = 6;
            colQuantity.Name = "colQuantity";
            colQuantity.ReadOnly = true;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.BackColor = Color.SeaGreen;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.ForeColor = Color.Transparent;
            btnSave.Location = new Point(1052, 356);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(126, 44);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click_1;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = Color.DimGray;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button1.ForeColor = Color.Transparent;
            button1.Location = new Point(920, 356);
            button1.Name = "button1";
            button1.Size = new Size(126, 44);
            button1.TabIndex = 4;
            button1.Text = "Cancel";
            button1.UseVisualStyleBackColor = false;
            // 
            // item_entry
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1202, 900);
            Controls.Add(contentPanel);
            Font = new Font("Microsoft Sans Serif", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Name = "item_entry";
            WindowState = FormWindowState.Maximized;
            pnlBasicInfo.ResumeLayout(false);
            pnlBasicInfo.PerformLayout();
            contentPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grpBlock1;
        private System.Windows.Forms.TextBox txtBuyerName;
        private Panel pnlBasicInfo;
        private TextBox textBox1;
        private Label lblSellerBusiness;
        private TextBox srosche;
        private Label lblBuyerBusiness;
        private Button btn_remove;
        private TextBox mrp;
        private TextBox hscode;
        private Label lblBuyerAddress;
        private Label lblBasicInfo;
        private TextBox fed;
        private Label lblInvoiceType;
        private Label lblSellerAddress;
        private TextBox qty;
        private Label lblCustomerRegType;
        private Label lblBuyerRegNo;
        private Label lblInvoiceExtra;
        private TextBox txtInvoiceExtra;
        private Label lblItemEntry;
        private Panel contentPanel;
        private TextBox textBox3;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox textBox6;
        private TextBox textBox7;
        private Label label8;
        private Label label9;
        private TextBox textBox4;
        private TextBox textBox5;
        private Label label7;
        private TextBox saletype;
        private Label label6;
        private Label label10;
        private TextBox SROScheduleNo;
        private Label label11;
        private Label label12;
        private TextBox STWithheld;
        private TextBox textBox11;
        private Label label13;
        private TextBox Discount;
        private Label label14;
        private TextBox SalesValueExclST;
        private Button btnSave;
        private Button button1;
        private DataGridView dataGridView1;
        private Label lblInvoicesListing;
        private Label lblTotalItems;
        private DataGridViewTextBoxColumn colSrNo;
        private DataGridViewTextBoxColumn colInvoice;
        private DataGridViewTextBoxColumn colPosId;
        private DataGridViewTextBoxColumn colInvoiceSynced;
        private DataGridViewTextBoxColumn colDueDate;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colQuantity;

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Format the "Invoice Synced" column to show green "Yes" badges
            if (e.ColumnIndex == colInvoiceSynced.Index && e.Value?.ToString() == "Yes")
            {
                e.CellStyle.ForeColor = Color.FromArgb(34, 197, 94);
                e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                e.Value = "● Yes";
            }
            if (e.ColumnIndex == colInvoiceSynced.Index && e.Value?.ToString() == "No")
            {
                e.CellStyle.ForeColor = Color.FromArgb(255, 0, 0);
                e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                e.Value = "● No";
            }

            // Format the "Status" column to show green "Completed" badges
            if (e.ColumnIndex == colStatus.Index && e.Value?.ToString() == "Completed")
            {
                e.CellStyle.ForeColor = Color.FromArgb(34, 197, 94);
                e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                e.Value = "● Completed";
            }
            if (e.ColumnIndex == colStatus.Index && e.Value?.ToString() == "Incomplete")
            {
                e.CellStyle.ForeColor = Color.FromArgb(255, 0, 0);
                e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                e.Value = "● Incomplete";
            }
        }
        private Button btnProceed;
        private TextBox uom;
    }
}