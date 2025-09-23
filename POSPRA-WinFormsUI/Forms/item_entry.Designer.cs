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
            TaxChargedlbl = new Label();
            TaxCharged = new TextBox();
            pctCode = new TextBox();
            TaxRatelbl = new Label();
            salevalue = new TextBox();
            salevaluelbl = new Label();
            TaxRatebox = new TextBox();
            itemDiscount = new TextBox();
            itemDiscountlbl = new Label();
            totalamount = new TextBox();
            ItemCode = new TextBox();
            ItemNamelbl = new Label();
            FurtureTaxlbl = new Label();
            FurtureTax = new TextBox();
            ItemName = new TextBox();
            ItemCodelbl = new Label();
            lblSellerAddress = new Label();
            qty = new TextBox();
            lblCustomerRegType = new Label();
            totalamountlbl = new Label();
            lblItemEntry = new Label();
            btnEdit = new Button();
            btnSave = new Button();
            dataGridView1 = new DataGridView();
            lblInvoicesListing = new Label();
            btn_remove = new Button();
            btnProceed = new Button();
            contentPanel = new Panel();
            panel1 = new Panel();
            paymentmode = new ComboBox();
            invoicetype = new ComboBox();
            refUSIN = new TextBox();
            refUSINlbl = new Label();
            paymentmodelbl = new Label();
            TotalSaleValuelbl = new Label();
            TotalSaleValue = new TextBox();
            TotalQuantitylbl = new Label();
            TotalQuantity = new TextBox();
            totalFurtherTaxlbl = new Label();
            TotalFurtherTax = new TextBox();
            Discountlbl = new Label();
            Discount = new TextBox();
            TotalTaxChargedlbl = new Label();
            TotalTaxCharged = new TextBox();
            TotalBillAmountlbl = new Label();
            TotalBillAmount = new TextBox();
            USIN = new TextBox();
            USINlbl = new Label();
            InvoiceNumberlbl = new Label();
            InvoiceNumber = new TextBox();
            invoicetypelbl = new Label();
            buyerphonelbl = new Label();
            buyerphone = new TextBox();
            sellerBnamelbl = new Label();
            BuyerBname = new TextBox();
            buyerntnlabel = new Label();
            buyerntn = new TextBox();
            buyercniclabel = new Label();
            posid = new TextBox();
            buyercnic = new TextBox();
            label15 = new Label();
            label31 = new Label();
            pnlBasicInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            contentPanel.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBasicInfo
            // 
            pnlBasicInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlBasicInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlBasicInfo.BackColor = Color.White;
            pnlBasicInfo.BackgroundImageLayout = ImageLayout.None;
            pnlBasicInfo.Controls.Add(TaxChargedlbl);
            pnlBasicInfo.Controls.Add(TaxCharged);
            pnlBasicInfo.Controls.Add(pctCode);
            pnlBasicInfo.Controls.Add(TaxRatelbl);
            pnlBasicInfo.Controls.Add(salevalue);
            pnlBasicInfo.Controls.Add(salevaluelbl);
            pnlBasicInfo.Controls.Add(TaxRatebox);
            pnlBasicInfo.Controls.Add(itemDiscount);
            pnlBasicInfo.Controls.Add(itemDiscountlbl);
            pnlBasicInfo.Controls.Add(totalamount);
            pnlBasicInfo.Controls.Add(ItemCode);
            pnlBasicInfo.Controls.Add(ItemNamelbl);
            pnlBasicInfo.Controls.Add(FurtureTaxlbl);
            pnlBasicInfo.Controls.Add(FurtureTax);
            pnlBasicInfo.Controls.Add(ItemName);
            pnlBasicInfo.Controls.Add(ItemCodelbl);
            pnlBasicInfo.Controls.Add(lblSellerAddress);
            pnlBasicInfo.Controls.Add(qty);
            pnlBasicInfo.Controls.Add(lblCustomerRegType);
            pnlBasicInfo.Controls.Add(totalamountlbl);
            pnlBasicInfo.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            pnlBasicInfo.Location = new Point(30, 354);
            pnlBasicInfo.Margin = new Padding(10, 0, 10, 0);
            pnlBasicInfo.Name = "pnlBasicInfo";
            pnlBasicInfo.Size = new Size(1145, 168);
            pnlBasicInfo.TabIndex = 6;
            // 
            // TaxChargedlbl
            // 
            TaxChargedlbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TaxChargedlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TaxChargedlbl.ForeColor = Color.FromArgb(75, 85, 99);
            TaxChargedlbl.Location = new Point(462, 88);
            TaxChargedlbl.Name = "TaxChargedlbl";
            TaxChargedlbl.Size = new Size(209, 20);
            TaxChargedlbl.TabIndex = 56;
            TaxChargedlbl.Text = "Tax Charged";
            // 
            // TaxCharged
            // 
            TaxCharged.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TaxCharged.Font = new Font("Microsoft Sans Serif", 9F);
            TaxCharged.Location = new Point(462, 111);
            TaxCharged.Name = "TaxCharged";
            TaxCharged.PlaceholderText = "Tax Charged";
            TaxCharged.Size = new Size(197, 24);
            TaxCharged.TabIndex = 57;
            // 
            // pctCode
            // 
            pctCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pctCode.BackColor = Color.White;
            pctCode.Font = new Font("Microsoft Sans Serif", 9F);
            pctCode.ImeMode = ImeMode.Disable;
            pctCode.Location = new Point(240, 42);
            pctCode.Name = "pctCode";
            pctCode.PlaceholderText = "PCT Code";
            pctCode.Size = new Size(197, 24);
            pctCode.TabIndex = 55;
            pctCode.Text = "78";
            // 
            // TaxRatelbl
            // 
            TaxRatelbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TaxRatelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TaxRatelbl.ForeColor = Color.FromArgb(75, 85, 99);
            TaxRatelbl.Location = new Point(682, 19);
            TaxRatelbl.Name = "TaxRatelbl";
            TaxRatelbl.Size = new Size(93, 20);
            TaxRatelbl.TabIndex = 19;
            TaxRatelbl.Text = "Tax Rate";
            // 
            // salevalue
            // 
            salevalue.Font = new Font("Microsoft Sans Serif", 9F);
            salevalue.Location = new Point(920, 111);
            salevalue.Name = "salevalue";
            salevalue.PlaceholderText = "Sale Value";
            salevalue.Size = new Size(183, 24);
            salevalue.TabIndex = 28;
            salevalue.Text = "78";
            // 
            // salevaluelbl
            // 
            salevaluelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            salevaluelbl.ForeColor = Color.FromArgb(75, 85, 99);
            salevaluelbl.Location = new Point(920, 88);
            salevaluelbl.Name = "salevaluelbl";
            salevaluelbl.Size = new Size(95, 20);
            salevaluelbl.TabIndex = 44;
            salevaluelbl.Text = "Sale Value";
            // 
            // TaxRatebox
            // 
            TaxRatebox.Font = new Font("Microsoft Sans Serif", 9F);
            TaxRatebox.Location = new Point(685, 44);
            TaxRatebox.Name = "TaxRatebox";
            TaxRatebox.PlaceholderText = "Tax Rate";
            TaxRatebox.Size = new Size(197, 24);
            TaxRatebox.TabIndex = 22;
            // 
            // itemDiscount
            // 
            itemDiscount.Font = new Font("Microsoft Sans Serif", 9F);
            itemDiscount.Location = new Point(920, 42);
            itemDiscount.Name = "itemDiscount";
            itemDiscount.PlaceholderText = "Discount";
            itemDiscount.Size = new Size(183, 24);
            itemDiscount.TabIndex = 23;
            // 
            // itemDiscountlbl
            // 
            itemDiscountlbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            itemDiscountlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            itemDiscountlbl.ForeColor = Color.FromArgb(75, 85, 99);
            itemDiscountlbl.Location = new Point(920, 17);
            itemDiscountlbl.Name = "itemDiscountlbl";
            itemDiscountlbl.Size = new Size(172, 20);
            itemDiscountlbl.TabIndex = 14;
            itemDiscountlbl.Text = "Discount";
            // 
            // totalamount
            // 
            totalamount.Font = new Font("Microsoft Sans Serif", 9F);
            totalamount.Location = new Point(465, 42);
            totalamount.Name = "totalamount";
            totalamount.PlaceholderText = "Total Amount";
            totalamount.Size = new Size(194, 24);
            totalamount.TabIndex = 20;
            totalamount.Text = "8";
            // 
            // ItemCode
            // 
            ItemCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ItemCode.BackColor = Color.White;
            ItemCode.Font = new Font("Microsoft Sans Serif", 9F);
            ItemCode.ImeMode = ImeMode.Disable;
            ItemCode.Location = new Point(17, 42);
            ItemCode.Name = "ItemCode";
            ItemCode.PlaceholderText = "Item Code";
            ItemCode.Size = new Size(200, 24);
            ItemCode.TabIndex = 17;
            ItemCode.Text = "78";
            // 
            // ItemNamelbl
            // 
            ItemNamelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            ItemNamelbl.ForeColor = Color.FromArgb(75, 85, 99);
            ItemNamelbl.Location = new Point(20, 90);
            ItemNamelbl.Name = "ItemNamelbl";
            ItemNamelbl.Size = new Size(88, 20);
            ItemNamelbl.TabIndex = 27;
            ItemNamelbl.Text = "Item Name";
            // 
            // FurtureTaxlbl
            // 
            FurtureTaxlbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FurtureTaxlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            FurtureTaxlbl.ForeColor = Color.FromArgb(75, 85, 99);
            FurtureTaxlbl.Location = new Point(682, 90);
            FurtureTaxlbl.Name = "FurtureTaxlbl";
            FurtureTaxlbl.Size = new Size(212, 20);
            FurtureTaxlbl.TabIndex = 28;
            FurtureTaxlbl.Text = "Further Tax";
            // 
            // FurtureTax
            // 
            FurtureTax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FurtureTax.Font = new Font("Microsoft Sans Serif", 9F);
            FurtureTax.Location = new Point(682, 113);
            FurtureTax.Name = "FurtureTax";
            FurtureTax.PlaceholderText = "Furture Tax";
            FurtureTax.Size = new Size(200, 24);
            FurtureTax.TabIndex = 30;
            // 
            // ItemName
            // 
            ItemName.Font = new Font("Microsoft Sans Serif", 9F);
            ItemName.Location = new Point(20, 113);
            ItemName.Name = "ItemName";
            ItemName.PlaceholderText = "Item Name";
            ItemName.Size = new Size(197, 24);
            ItemName.TabIndex = 26;
            ItemName.Text = "12345678";
            // 
            // ItemCodelbl
            // 
            ItemCodelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            ItemCodelbl.ForeColor = Color.FromArgb(75, 85, 99);
            ItemCodelbl.Location = new Point(20, 19);
            ItemCodelbl.Name = "ItemCodelbl";
            ItemCodelbl.Size = new Size(120, 20);
            ItemCodelbl.TabIndex = 34;
            ItemCodelbl.Text = "Item Code";
            // 
            // lblSellerAddress
            // 
            lblSellerAddress.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblSellerAddress.ForeColor = Color.FromArgb(75, 85, 99);
            lblSellerAddress.Location = new Point(240, 90);
            lblSellerAddress.Name = "lblSellerAddress";
            lblSellerAddress.Size = new Size(74, 20);
            lblSellerAddress.TabIndex = 35;
            lblSellerAddress.Text = "Quantity";
            // 
            // qty
            // 
            qty.Font = new Font("Microsoft Sans Serif", 9F);
            qty.Location = new Point(240, 113);
            qty.Name = "qty";
            qty.PlaceholderText = "QUANTITY";
            qty.Size = new Size(197, 24);
            qty.TabIndex = 27;
            qty.Text = "78";
            // 
            // lblCustomerRegType
            // 
            lblCustomerRegType.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblCustomerRegType.ForeColor = Color.FromArgb(75, 85, 99);
            lblCustomerRegType.Location = new Point(240, 19);
            lblCustomerRegType.Name = "lblCustomerRegType";
            lblCustomerRegType.Size = new Size(106, 20);
            lblCustomerRegType.TabIndex = 37;
            lblCustomerRegType.Text = "PCT Code";
            // 
            // totalamountlbl
            // 
            totalamountlbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            totalamountlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            totalamountlbl.ForeColor = Color.FromArgb(75, 85, 99);
            totalamountlbl.Location = new Point(462, 17);
            totalamountlbl.Name = "totalamountlbl";
            totalamountlbl.Size = new Size(197, 20);
            totalamountlbl.TabIndex = 38;
            totalamountlbl.Text = "Total Amount";
            // 
            // lblItemEntry
            // 
            lblItemEntry.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblItemEntry.ForeColor = Color.FromArgb(17, 24, 39);
            lblItemEntry.Location = new Point(30, 317);
            lblItemEntry.Name = "lblItemEntry";
            lblItemEntry.Size = new Size(191, 37);
            lblItemEntry.TabIndex = 5;
            lblItemEntry.Text = "Add Item";
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.BackColor = Color.DimGray;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnEdit.ForeColor = Color.Transparent;
            btnEdit.Location = new Point(859, 527);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(155, 44);
            btnEdit.TabIndex = 41;
            btnEdit.Text = "🖊️ Edit";
            btnEdit.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.BackColor = Color.SeaGreen;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.ForeColor = Color.Transparent;
            btnSave.Location = new Point(1020, 527);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(155, 44);
            btnSave.TabIndex = 42;
            btnSave.Text = "💾 Save";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(229, 231, 235);
            dataGridView1.Location = new Point(30, 649);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.RowTemplate.Height = 50;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1145, 239);
            dataGridView1.TabIndex = 43;
            // 
            // dataGridViewCellStyle1
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(107, 114, 128);
            dataGridViewCellStyle1.SelectionBackColor = Color.White;
            dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(107, 114, 128);
            // 
            // dataGridViewCellStyle2
            // 
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(75, 85, 99);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(75, 85, 99);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            // 
            // lblInvoicesListing
            // 
            lblInvoicesListing.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInvoicesListing.ForeColor = Color.FromArgb(17, 24, 39);
            lblInvoicesListing.Location = new Point(30, 555);
            lblInvoicesListing.Name = "lblInvoicesListing";
            lblInvoicesListing.Size = new Size(191, 37);
            lblInvoicesListing.TabIndex = 0;
            lblInvoicesListing.Text = "Items List";
            // 
            // btn_remove
            // 
            btn_remove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_remove.BackColor = Color.Red;
            btn_remove.FlatAppearance.BorderSize = 0;
            btn_remove.FlatStyle = FlatStyle.Flat;
            btn_remove.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_remove.ForeColor = Color.Transparent;
            btn_remove.Location = new Point(698, 527);
            btn_remove.Name = "btn_remove";
            btn_remove.Size = new Size(155, 44);
            btn_remove.TabIndex = 40;
            btn_remove.Text = "➖ Remove Item";
            btn_remove.UseVisualStyleBackColor = false;
            // 
            // btnProceed
            // 
            btnProceed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProceed.BackColor = Color.FromArgb(99, 102, 241);
            btnProceed.FlatAppearance.BorderSize = 0;
            btnProceed.FlatStyle = FlatStyle.Flat;
            btnProceed.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnProceed.ForeColor = Color.Transparent;
            btnProceed.Location = new Point(537, 527);
            btnProceed.Name = "btnProceed";
            btnProceed.Size = new Size(155, 44);
            btnProceed.TabIndex = 39;
            btnProceed.Text = "➕ Add Item";
            btnProceed.UseVisualStyleBackColor = false;
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.WhiteSmoke;
            contentPanel.Controls.Add(panel1);
            contentPanel.Controls.Add(label31);
            contentPanel.Controls.Add(btnProceed);
            contentPanel.Controls.Add(btn_remove);
            contentPanel.Controls.Add(lblInvoicesListing);
            contentPanel.Controls.Add(dataGridView1);
            contentPanel.Controls.Add(btnSave);
            contentPanel.Controls.Add(btnEdit);
            contentPanel.Controls.Add(lblItemEntry);
            contentPanel.Controls.Add(pnlBasicInfo);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 0);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(1202, 900);
            contentPanel.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(paymentmode);
            panel1.Controls.Add(invoicetype);
            panel1.Controls.Add(refUSIN);
            panel1.Controls.Add(refUSINlbl);
            panel1.Controls.Add(paymentmodelbl);
            panel1.Controls.Add(TotalSaleValuelbl);
            panel1.Controls.Add(TotalSaleValue);
            panel1.Controls.Add(TotalQuantitylbl);
            panel1.Controls.Add(TotalQuantity);
            panel1.Controls.Add(totalFurtherTaxlbl);
            panel1.Controls.Add(TotalFurtherTax);
            panel1.Controls.Add(Discountlbl);
            panel1.Controls.Add(Discount);
            panel1.Controls.Add(TotalTaxChargedlbl);
            panel1.Controls.Add(TotalTaxCharged);
            panel1.Controls.Add(TotalBillAmountlbl);
            panel1.Controls.Add(TotalBillAmount);
            panel1.Controls.Add(USIN);
            panel1.Controls.Add(USINlbl);
            panel1.Controls.Add(InvoiceNumberlbl);
            panel1.Controls.Add(InvoiceNumber);
            panel1.Controls.Add(invoicetypelbl);
            panel1.Controls.Add(buyerphonelbl);
            panel1.Controls.Add(buyerphone);
            panel1.Controls.Add(sellerBnamelbl);
            panel1.Controls.Add(BuyerBname);
            panel1.Controls.Add(buyerntnlabel);
            panel1.Controls.Add(buyerntn);
            panel1.Controls.Add(buyercniclabel);
            panel1.Controls.Add(posid);
            panel1.Controls.Add(buyercnic);
            panel1.Controls.Add(label15);
            panel1.Location = new Point(30, 67);
            panel1.Name = "panel1";
            panel1.Size = new Size(1145, 237);
            panel1.TabIndex = 57;
            // 
            // paymentmode
            // 
            paymentmode.DropDownStyle = ComboBoxStyle.DropDownList;
            paymentmode.Location = new Point(459, 44);
            paymentmode.Name = "paymentmode";
            paymentmode.Size = new Size(197, 26);
            paymentmode.TabIndex = 77;
            // 
            // invoicetype
            // 
            invoicetype.DropDownStyle = ComboBoxStyle.DropDownList;
            invoicetype.Location = new Point(240, 44);
            invoicetype.Name = "invoicetype";
            invoicetype.Size = new Size(197, 26);
            invoicetype.TabIndex = 76;
            // 
            // refUSIN
            // 
            refUSIN.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refUSIN.BackColor = Color.White;
            refUSIN.Font = new Font("Microsoft Sans Serif", 9F);
            refUSIN.ImeMode = ImeMode.Disable;
            refUSIN.Location = new Point(788, 44);
            refUSIN.Name = "refUSIN";
            refUSIN.PlaceholderText = "Ref USIN";
            refUSIN.Size = new Size(91, 24);
            refUSIN.TabIndex = 74;
            refUSIN.Text = "1";
            // 
            // refUSINlbl
            // 
            refUSINlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            refUSINlbl.ForeColor = Color.FromArgb(75, 85, 99);
            refUSINlbl.Location = new Point(791, 21);
            refUSINlbl.Name = "refUSINlbl";
            refUSINlbl.Size = new Size(88, 20);
            refUSINlbl.TabIndex = 75;
            refUSINlbl.Text = "Ref USIN";
            // 
            // paymentmodelbl
            // 
            paymentmodelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            paymentmodelbl.ForeColor = Color.FromArgb(75, 85, 99);
            paymentmodelbl.Location = new Point(459, 21);
            paymentmodelbl.Name = "paymentmodelbl";
            paymentmodelbl.Size = new Size(120, 20);
            paymentmodelbl.TabIndex = 72;
            paymentmodelbl.Text = "Payment Mode";
            // 
            // TotalSaleValuelbl
            // 
            TotalSaleValuelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalSaleValuelbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalSaleValuelbl.Location = new Point(240, 175);
            TotalSaleValuelbl.Name = "TotalSaleValuelbl";
            TotalSaleValuelbl.Size = new Size(142, 20);
            TotalSaleValuelbl.TabIndex = 69;
            TotalSaleValuelbl.Text = "Total Sale Value";
            // 
            // TotalSaleValue
            // 
            TotalSaleValue.Font = new Font("Microsoft Sans Serif", 9F);
            TotalSaleValue.Location = new Point(237, 198);
            TotalSaleValue.Name = "TotalSaleValue";
            TotalSaleValue.PlaceholderText = "Total Sale Value";
            TotalSaleValue.ReadOnly = true;
            TotalSaleValue.Size = new Size(200, 24);
            TotalSaleValue.TabIndex = 13;
            // 
            // TotalQuantitylbl
            // 
            TotalQuantitylbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalQuantitylbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalQuantitylbl.Location = new Point(20, 177);
            TotalQuantitylbl.Name = "TotalQuantitylbl";
            TotalQuantitylbl.Size = new Size(203, 20);
            TotalQuantitylbl.TabIndex = 67;
            TotalQuantitylbl.Text = "Total Quantity";
            // 
            // TotalQuantity
            // 
            TotalQuantity.Font = new Font("Microsoft Sans Serif", 9F);
            TotalQuantity.Location = new Point(17, 200);
            TotalQuantity.Name = "TotalQuantity";
            TotalQuantity.PlaceholderText = "Total Quantity";
            TotalQuantity.ReadOnly = true;
            TotalQuantity.Size = new Size(200, 24);
            TotalQuantity.TabIndex = 12;
            // 
            // totalFurtherTaxlbl
            // 
            totalFurtherTaxlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            totalFurtherTaxlbl.ForeColor = Color.FromArgb(75, 85, 99);
            totalFurtherTaxlbl.Location = new Point(911, 175);
            totalFurtherTaxlbl.Name = "totalFurtherTaxlbl";
            totalFurtherTaxlbl.Size = new Size(142, 20);
            totalFurtherTaxlbl.TabIndex = 65;
            totalFurtherTaxlbl.Text = "FurtherTax";
            // 
            // TotalFurtherTax
            // 
            TotalFurtherTax.Font = new Font("Microsoft Sans Serif", 9F);
            TotalFurtherTax.Location = new Point(908, 198);
            TotalFurtherTax.Name = "TotalFurtherTax";
            TotalFurtherTax.PlaceholderText = "Further Tax";
            TotalFurtherTax.ReadOnly = true;
            TotalFurtherTax.Size = new Size(195, 24);
            TotalFurtherTax.TabIndex = 16;
            // 
            // Discountlbl
            // 
            Discountlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            Discountlbl.ForeColor = Color.FromArgb(75, 85, 99);
            Discountlbl.Location = new Point(685, 175);
            Discountlbl.Name = "Discountlbl";
            Discountlbl.Size = new Size(142, 20);
            Discountlbl.TabIndex = 63;
            Discountlbl.Text = "Discount";
            // 
            // Discount
            // 
            Discount.Font = new Font("Microsoft Sans Serif", 9F);
            Discount.Location = new Point(682, 198);
            Discount.Name = "Discount";
            Discount.PlaceholderText = "Discount";
            Discount.ReadOnly = true;
            Discount.Size = new Size(200, 24);
            Discount.TabIndex = 15;
            // 
            // TotalTaxChargedlbl
            // 
            TotalTaxChargedlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalTaxChargedlbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalTaxChargedlbl.Location = new Point(465, 177);
            TotalTaxChargedlbl.Name = "TotalTaxChargedlbl";
            TotalTaxChargedlbl.Size = new Size(194, 20);
            TotalTaxChargedlbl.TabIndex = 61;
            TotalTaxChargedlbl.Text = "Total Tax Charged";
            // 
            // TotalTaxCharged
            // 
            TotalTaxCharged.Font = new Font("Microsoft Sans Serif", 9F);
            TotalTaxCharged.Location = new Point(462, 200);
            TotalTaxCharged.Name = "TotalTaxCharged";
            TotalTaxCharged.PlaceholderText = "Total Tax Charged";
            TotalTaxCharged.ReadOnly = true;
            TotalTaxCharged.Size = new Size(197, 24);
            TotalTaxCharged.TabIndex = 14;
            // 
            // TotalBillAmountlbl
            // 
            TotalBillAmountlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalBillAmountlbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalBillAmountlbl.Location = new Point(911, 95);
            TotalBillAmountlbl.Name = "TotalBillAmountlbl";
            TotalBillAmountlbl.Size = new Size(142, 20);
            TotalBillAmountlbl.TabIndex = 59;
            TotalBillAmountlbl.Text = "Total Bill Amount";
            // 
            // TotalBillAmount
            // 
            TotalBillAmount.Font = new Font("Microsoft Sans Serif", 9F);
            TotalBillAmount.Location = new Point(911, 118);
            TotalBillAmount.Name = "TotalBillAmount";
            TotalBillAmount.PlaceholderText = "Total Bill Amount";
            TotalBillAmount.ReadOnly = true;
            TotalBillAmount.Size = new Size(192, 24);
            TotalBillAmount.TabIndex = 6;
            // 
            // USIN
            // 
            USIN.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            USIN.BackColor = Color.White;
            USIN.Font = new Font("Microsoft Sans Serif", 9F);
            USIN.ImeMode = ImeMode.Disable;
            USIN.Location = new Point(682, 44);
            USIN.Name = "USIN";
            USIN.PlaceholderText = "USIN";
            USIN.Size = new Size(91, 24);
            USIN.TabIndex = 4;
            USIN.Text = "1";
            // 
            // USINlbl
            // 
            USINlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            USINlbl.ForeColor = Color.FromArgb(75, 85, 99);
            USINlbl.Location = new Point(685, 21);
            USINlbl.Name = "USINlbl";
            USINlbl.Size = new Size(76, 20);
            USINlbl.TabIndex = 56;
            USINlbl.Text = "USIN";
            // 
            // InvoiceNumberlbl
            // 
            InvoiceNumberlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            InvoiceNumberlbl.ForeColor = Color.FromArgb(75, 85, 99);
            InvoiceNumberlbl.Location = new Point(911, 23);
            InvoiceNumberlbl.Name = "InvoiceNumberlbl";
            InvoiceNumberlbl.Size = new Size(160, 20);
            InvoiceNumberlbl.TabIndex = 53;
            InvoiceNumberlbl.Text = "Invoice Number";
            // 
            // InvoiceNumber
            // 
            InvoiceNumber.Font = new Font("Microsoft Sans Serif", 9F);
            InvoiceNumber.Location = new Point(908, 46);
            InvoiceNumber.Name = "InvoiceNumber";
            InvoiceNumber.PlaceholderText = "Invoice Number";
            InvoiceNumber.Size = new Size(195, 24);
            InvoiceNumber.TabIndex = 5;
            InvoiceNumber.Text = "2";
            // 
            // invoicetypelbl
            // 
            invoicetypelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            invoicetypelbl.ForeColor = Color.FromArgb(75, 85, 99);
            invoicetypelbl.Location = new Point(240, 21);
            invoicetypelbl.Name = "invoicetypelbl";
            invoicetypelbl.Size = new Size(120, 20);
            invoicetypelbl.TabIndex = 52;
            invoicetypelbl.Text = "Invoice Type";
            // 
            // buyerphonelbl
            // 
            buyerphonelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            buyerphonelbl.ForeColor = Color.FromArgb(75, 85, 99);
            buyerphonelbl.Location = new Point(682, 94);
            buyerphonelbl.Name = "buyerphonelbl";
            buyerphonelbl.Size = new Size(174, 20);
            buyerphonelbl.TabIndex = 48;
            buyerphonelbl.Text = "Buyer Phone number";
            // 
            // buyerphone
            // 
            buyerphone.Font = new Font("Microsoft Sans Serif", 9F);
            buyerphone.Location = new Point(679, 117);
            buyerphone.Name = "buyerphone";
            buyerphone.PlaceholderText = "Buyer Phone Number";
            buyerphone.Size = new Size(200, 24);
            buyerphone.TabIndex = 10;
            buyerphone.Text = "5";
            // 
            // sellerBnamelbl
            // 
            sellerBnamelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            sellerBnamelbl.ForeColor = Color.FromArgb(75, 85, 99);
            sellerBnamelbl.Location = new Point(462, 96);
            sellerBnamelbl.Name = "sellerBnamelbl";
            sellerBnamelbl.Size = new Size(131, 20);
            sellerBnamelbl.TabIndex = 46;
            sellerBnamelbl.Text = "Buyer Name";
            // 
            // BuyerBname
            // 
            BuyerBname.Font = new Font("Microsoft Sans Serif", 9F);
            BuyerBname.Location = new Point(459, 119);
            BuyerBname.Name = "BuyerBname";
            BuyerBname.PlaceholderText = "Buyer Name";
            BuyerBname.Size = new Size(200, 24);
            BuyerBname.TabIndex = 9;
            BuyerBname.Text = "1";
            // 
            // buyerntnlabel
            // 
            buyerntnlabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            buyerntnlabel.ForeColor = Color.FromArgb(75, 85, 99);
            buyerntnlabel.Location = new Point(240, 95);
            buyerntnlabel.Name = "buyerntnlabel";
            buyerntnlabel.Size = new Size(142, 20);
            buyerntnlabel.TabIndex = 44;
            buyerntnlabel.Text = "Buyer NTN";
            // 
            // buyerntn
            // 
            buyerntn.Font = new Font("Microsoft Sans Serif", 9F);
            buyerntn.Location = new Point(237, 118);
            buyerntn.Name = "buyerntn";
            buyerntn.PlaceholderText = "Buyer NTN";
            buyerntn.Size = new Size(200, 24);
            buyerntn.TabIndex = 8;
            buyerntn.Text = "61101458";
            // 
            // buyercniclabel
            // 
            buyercniclabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            buyercniclabel.ForeColor = Color.FromArgb(75, 85, 99);
            buyercniclabel.Location = new Point(20, 97);
            buyercniclabel.Name = "buyercniclabel";
            buyercniclabel.Size = new Size(131, 20);
            buyercniclabel.TabIndex = 42;
            buyercniclabel.Text = "Buyer CNIC";
            // 
            // posid
            // 
            posid.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            posid.BackColor = Color.White;
            posid.Font = new Font("Microsoft Sans Serif", 9F);
            posid.ImeMode = ImeMode.Disable;
            posid.Location = new Point(17, 44);
            posid.Name = "posid";
            posid.PlaceholderText = "POS ID";
            posid.Size = new Size(200, 24);
            posid.TabIndex = 0;
            posid.Text = "123456";
            // 
            // buyercnic
            // 
            buyercnic.Font = new Font("Microsoft Sans Serif", 9F);
            buyercnic.Location = new Point(17, 120);
            buyercnic.Name = "buyercnic";
            buyercnic.PlaceholderText = "Buyer CNIC";
            buyercnic.Size = new Size(200, 24);
            buyercnic.TabIndex = 7;
            buyercnic.Text = "6101589";
            // 
            // label15
            // 
            label15.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label15.ForeColor = Color.FromArgb(75, 85, 99);
            label15.Location = new Point(20, 21);
            label15.Name = "label15";
            label15.Size = new Size(120, 20);
            label15.TabIndex = 43;
            label15.Text = "POS ID";
            // 
            // label31
            // 
            label31.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label31.ForeColor = Color.FromArgb(17, 24, 39);
            label31.Location = new Point(27, 16);
            label31.Name = "label31";
            label31.Size = new Size(191, 37);
            label31.TabIndex = 56;
            label31.Text = "Invoice Entry";
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
            Load += item_entry_Load;
            pnlBasicInfo.ResumeLayout(false);
            pnlBasicInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            contentPanel.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // Custom method to initialize DataGridView columns
        private void InitializeDataGridViewColumns()
        {
            // Clear any existing columns
            dataGridView1.Columns.Clear();

            // Set DataGridView properties
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Common cell styles
            var headerStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 9F),
                ForeColor = Color.FromArgb(107, 114, 128),
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.FromArgb(107, 114, 128)
            };

            var rowStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 9F),
                ForeColor = Color.FromArgb(75, 85, 99),
                SelectionBackColor = Color.FromArgb(248, 250, 252),
                SelectionForeColor = Color.FromArgb(75, 85, 99),
                WrapMode = DataGridViewTriState.False
            };

            // Update DataGridView styles
            dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView1.DefaultCellStyle = rowStyle;

            // Add columns with relative FillWeight
            dataGridView1.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colSrNo", HeaderText = "Sr. No.", ReadOnly = true, FillWeight = 50 },
                new DataGridViewTextBoxColumn { Name = "colProductCode", HeaderText = "Item Code", ReadOnly = true, FillWeight = 80 },
                new DataGridViewTextBoxColumn { Name = "colHSCode", HeaderText = "HS Code", ReadOnly = true, FillWeight = 80 },
                new DataGridViewTextBoxColumn { Name = "colProductDescription", HeaderText = "Item Name", ReadOnly = true, FillWeight = 150 },
                new DataGridViewTextBoxColumn { Name = "colQuantity", HeaderText = "Quantity", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 80 },
                new DataGridViewTextBoxColumn { Name = "colRate", HeaderText = "Rate", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 80 },
                new DataGridViewTextBoxColumn { Name = "colDiscount", HeaderText = "Discount (Amt)", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 90 },
                new DataGridViewTextBoxColumn { Name = "colSalesValueExcST", HeaderText = "Sales Value (exc ST)", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 120 },
                new DataGridViewTextBoxColumn { Name = "colTotalValue", HeaderText = "Total Value", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 120 },
                new DataGridViewTextBoxColumn { Name = "colSalesTax", HeaderText = "Tax Rate (%)", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 80 },
                new DataGridViewTextBoxColumn { Name = "colExtraTax", HeaderText = "Tax Charged", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 100 },
                new DataGridViewTextBoxColumn { Name = "colFutureTax", HeaderText = "Further Tax", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 90 },
                new DataGridViewTextBoxColumn { Name = "colInvoiceType", HeaderText = "Inv Type", ReadOnly = true, FillWeight = 70 },
                new DataGridViewTextBoxColumn { Name = "colRefUSIN", HeaderText = "Ref USIN", ReadOnly = true, FillWeight = 100 }
            });
        }

        // Custom method to initialize ComboBoxes
        private void InitializeComboBoxes()
        {
            // Payment mode ComboBox
            paymentmode.DataSource = new List<KeyValuePair<byte, string>>()
            {
                new KeyValuePair<byte, string>(1, "Card"),
                new KeyValuePair<byte, string>(2, "Cash"),
                new KeyValuePair<byte, string>(3, "Online")
            };
            paymentmode.DisplayMember = "Value";
            paymentmode.ValueMember = "Key";
            paymentmode.SelectedValue = (byte)2; // default: Cash

            // Invoice type ComboBox
            invoicetype.DataSource = new List<KeyValuePair<byte, string>>()
            {
                new KeyValuePair<byte, string>(1, "Sale"),
                new KeyValuePair<byte, string>(2, "Purchase"),
                new KeyValuePair<byte, string>(3, "Debit"),
                new KeyValuePair<byte, string>(4, "Credit")
            };
            invoicetype.DisplayMember = "Value";
            invoicetype.ValueMember = "Key";
            invoicetype.SelectedValue = (byte)1; // default: Sale
        }

        // Event handler for form load
        private void item_entry_Load(object sender, EventArgs e)
        {
            InitializeDataGridViewColumns();
            InitializeComboBoxes();
        }

        // Field declarations
        private System.Windows.Forms.GroupBox grpBlock1;
        private System.Windows.Forms.TextBox txtBuyerName;
        private Panel pnlBasicInfo;
        private TextBox extratax;
        private Label itemDiscountlbl;
        private Label label8;
        private TextBox totalamount;
        public TextBox PCTCode { get; private set; }
        private TextBox textBox5;
        private Label label7;
        private TextBox rate;
        private Label label5;
        private Label label2;
        private Label label1;
        private TextBox ItemCode;
        private Label ItemNamelbl;
        private Label FurtureTaxlbl;
        private TextBox FurtureTax;
        private TextBox ItemName;
        private Label lblBuyerAddress;
        private Label lblBasicInfo;
        private Label ItemCodelbl;
        private Label lblSellerAddress;
        private TextBox qty;
        private Label lblCustomerRegType;
        private Label totalamountlbl;
        private Label lblItemEntry;
        private Button btnEdit;
        private TextBox TaxRatebox;
        private TextBox itemDiscount;
        private Button btnSave;
        private DataGridView dataGridView1;
        private Label lblInvoicesListing;
        private Button btn_remove;
        private Button btnProceed;
        private Panel contentPanel;
        private Label label31;
        private Panel panel1;
        private TextBox posid;
        private Label label15;
        private Label buyerntnlabel;
        private TextBox buyerntn;
        private Label buyercniclabel;
        private TextBox buyercnic;
        private Label USINlbl;
        private TextBox USIN;
        private Label TotalQuantitylbl;
        private TextBox TotalQuantity;
        private Label buyerphonelbl;
        private TextBox buyerphone;
        private Label sellerBnamelbl;
        private TextBox BuyerBname;
        private Label invoicetypelbl;
        private Label InvoiceNumberlbl;
        private TextBox InvoiceNumber;
        private Label Discountlbl;
        private TextBox Discount;
        private Label TotalTaxChargedlbl;
        private TextBox TotalTaxCharged;
        private Label TotalBillAmountlbl;
        private TextBox TotalBillAmount;
        private Label totalFurtherTaxlbl;
        private TextBox TotalFurtherTax;
        private Label TotalSaleValuelbl;
        private TextBox TotalSaleValue;
        private Label SalesTaxApplicablelbl;
        private TextBox SalesTaxApplicable;
        private TextBox RetailPrice;
        private Label salevaluelbl;
        private TextBox salevalue;
        private Label TaxRatelbl;
        private Label paymentmodelbl;
        private TextBox refUSIN;
        private Label refUSINlbl;
        private Label TaxChargedlbl;
        private TextBox TaxCharged;
        private TextBox pctCode;
        private ComboBox paymentmode;
        private ComboBox invoicetype;
    }
}