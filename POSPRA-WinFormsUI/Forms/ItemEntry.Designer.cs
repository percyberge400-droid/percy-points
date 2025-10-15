namespace POSPRA_WinFormsUI
{
    partial class ItemEntry
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
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            pnlBasicInfo = new Panel();
            label4 = new Label();
            itemDiscountAmount = new TextBox();
            itemDiscountPercent = new TextBox();
            TaxChargedlbl = new Label();
            TaxCharged = new TextBox();
            pctCode = new TextBox();
            TaxRatelbl = new Label();
            salevalue = new TextBox();
            salevaluelbl = new Label();
            TaxRatebox = new TextBox();
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
            btnsearch = new Button();
            btnclear = new Button();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            panel3 = new Panel();
            refUSIN = new TextBox();
            refUSINlbl = new Label();
            paymentmode = new ComboBox();
            invoicetype = new ComboBox();
            invoicetypelbl = new Label();
            paymentmodelbl = new Label();
            USIN = new TextBox();
            USINlbl = new Label();
            panel2 = new Panel();
            label17 = new Label();
            buyerphone = new TextBox();
            label18 = new Label();
            BuyerBname = new TextBox();
            label19 = new Label();
            buyerntn = new TextBox();
            label20 = new Label();
            buyercnic = new TextBox();
            panel1 = new Panel();
            TotalBillAmountlbl = new Label();
            TotalBillAmount = new TextBox();
            TotalSaleValuelbl = new Label();
            TotalSaleValue = new TextBox();
            TotalQuantitylbl = new Label();
            TotalQuantity = new TextBox();
            totalFurtherTaxlbl = new Label();
            TotalFurtherTax = new TextBox();
            Discountlbl = new Label();
            Discount = new TextBox();
            posid = new TextBox();
            label15 = new Label();
            TotalTaxChargedlbl = new Label();
            TotalTaxCharged = new TextBox();
            label31 = new Label();
            pnlBasicInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            contentPanel.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBasicInfo
            // 
            pnlBasicInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlBasicInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlBasicInfo.BackColor = Color.FromArgb(250, 250, 250);
            pnlBasicInfo.BackgroundImageLayout = ImageLayout.None;
            pnlBasicInfo.BorderStyle = BorderStyle.FixedSingle;
            pnlBasicInfo.Controls.Add(label4);
            pnlBasicInfo.Controls.Add(itemDiscountAmount);
            pnlBasicInfo.Controls.Add(itemDiscountPercent);
            pnlBasicInfo.Controls.Add(TaxChargedlbl);
            pnlBasicInfo.Controls.Add(TaxCharged);
            pnlBasicInfo.Controls.Add(pctCode);
            pnlBasicInfo.Controls.Add(TaxRatelbl);
            pnlBasicInfo.Controls.Add(salevalue);
            pnlBasicInfo.Controls.Add(salevaluelbl);
            pnlBasicInfo.Controls.Add(TaxRatebox);
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
            pnlBasicInfo.Location = new Point(30, 207);
            pnlBasicInfo.Margin = new Padding(10, 0, 10, 0);
            pnlBasicInfo.Name = "pnlBasicInfo";
            pnlBasicInfo.Size = new Size(1145, 167);
            pnlBasicInfo.TabIndex = 6;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label4.ForeColor = Color.FromArgb(75, 85, 99);
            label4.Location = new Point(1011, 17);
            label4.Name = "label4";
            label4.Size = new Size(108, 20);
            label4.TabIndex = 60;
            label4.Text = "Discount (Rs.)";
            // 
            // itemDiscountAmount
            // 
            itemDiscountAmount.Font = new Font("Microsoft Sans Serif", 9F);
            itemDiscountAmount.Location = new Point(1011, 44);
            itemDiscountAmount.Name = "itemDiscountAmount";
            itemDiscountAmount.PlaceholderText = "Calculated Rs.";
            itemDiscountAmount.ReadOnly = true;
            itemDiscountAmount.Size = new Size(100, 24);
            itemDiscountAmount.TabIndex = 59;
            // 
            // itemDiscountPercent
            // 
            itemDiscountPercent.Font = new Font("Microsoft Sans Serif", 9F);
            itemDiscountPercent.Location = new Point(909, 44);
            itemDiscountPercent.Name = "itemDiscountPercent";
            itemDiscountPercent.PlaceholderText = "Discount %";
            itemDiscountPercent.Size = new Size(96, 24);
            itemDiscountPercent.TabIndex = 58;
            itemDiscountPercent.Text = "10";
            // 
            // TaxChargedlbl
            // 
            TaxChargedlbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TaxChargedlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TaxChargedlbl.ForeColor = Color.FromArgb(75, 85, 99);
            TaxChargedlbl.Location = new Point(909, 92);
            TaxChargedlbl.Name = "TaxChargedlbl";
            TaxChargedlbl.Size = new Size(132, 20);
            TaxChargedlbl.TabIndex = 56;
            TaxChargedlbl.Text = "Tax Charged";
            // 
            // TaxCharged
            // 
            TaxCharged.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TaxCharged.Font = new Font("Microsoft Sans Serif", 9F);
            TaxCharged.Location = new Point(909, 113);
            TaxCharged.Name = "TaxCharged";
            TaxCharged.PlaceholderText = "Tax Charged";
            TaxCharged.ReadOnly = true;
            TaxCharged.Size = new Size(202, 24);
            TaxCharged.TabIndex = 57;
            // 
            // pctCode
            // 
            pctCode.BackColor = Color.White;
            pctCode.Font = new Font("Microsoft Sans Serif", 9F);
            pctCode.ImeMode = ImeMode.Disable;
            pctCode.Location = new Point(238, 42);
            pctCode.Name = "pctCode";
            pctCode.PlaceholderText = "PCT Code";
            pctCode.Size = new Size(197, 24);
            pctCode.TabIndex = 10;
            pctCode.Text = "78";
            // 
            // TaxRatelbl
            // 
            TaxRatelbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TaxRatelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TaxRatelbl.ForeColor = Color.FromArgb(75, 85, 99);
            TaxRatelbl.Location = new Point(677, 19);
            TaxRatelbl.Name = "TaxRatelbl";
            TaxRatelbl.Size = new Size(102, 20);
            TaxRatelbl.TabIndex = 19;
            TaxRatelbl.Text = "Tax Rate (%)";
            // 
            // salevalue
            // 
            salevalue.Font = new Font("Microsoft Sans Serif", 9F);
            salevalue.Location = new Point(462, 111);
            salevalue.Name = "salevalue";
            salevalue.PlaceholderText = "Sale Value";
            salevalue.Size = new Size(197, 24);
            salevalue.TabIndex = 16;
            salevalue.Text = "100";
            // 
            // salevaluelbl
            // 
            salevaluelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            salevaluelbl.ForeColor = Color.FromArgb(75, 85, 99);
            salevaluelbl.Location = new Point(465, 88);
            salevaluelbl.Name = "salevaluelbl";
            salevaluelbl.Size = new Size(95, 20);
            salevaluelbl.TabIndex = 44;
            salevaluelbl.Text = "Sale Value";
            // 
            // TaxRatebox
            // 
            TaxRatebox.Font = new Font("Microsoft Sans Serif", 9F);
            TaxRatebox.Location = new Point(682, 44);
            TaxRatebox.Name = "TaxRatebox";
            TaxRatebox.PlaceholderText = "Tax Rate";
            TaxRatebox.Size = new Size(200, 24);
            TaxRatebox.TabIndex = 12;
            TaxRatebox.Text = "10";
            // 
            // itemDiscountlbl
            // 
            itemDiscountlbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            itemDiscountlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            itemDiscountlbl.ForeColor = Color.FromArgb(75, 85, 99);
            itemDiscountlbl.Location = new Point(909, 17);
            itemDiscountlbl.Name = "itemDiscountlbl";
            itemDiscountlbl.Size = new Size(96, 20);
            itemDiscountlbl.TabIndex = 14;
            itemDiscountlbl.Text = "Discount (%)";
            // 
            // totalamount
            // 
            totalamount.Font = new Font("Microsoft Sans Serif", 9F);
            totalamount.Location = new Point(465, 42);
            totalamount.Name = "totalamount";
            totalamount.PlaceholderText = "Total Amount";
            totalamount.Size = new Size(194, 24);
            totalamount.TabIndex = 11;
            totalamount.Text = "105";
            // 
            // ItemCode
            // 
            ItemCode.BackColor = Color.White;
            ItemCode.Font = new Font("Microsoft Sans Serif", 9F);
            ItemCode.ImeMode = ImeMode.Disable;
            ItemCode.Location = new Point(18, 42);
            ItemCode.Name = "ItemCode";
            ItemCode.PlaceholderText = "Item Code";
            ItemCode.Size = new Size(197, 24);
            ItemCode.TabIndex = 9;
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
            FurtureTaxlbl.Location = new Point(677, 90);
            FurtureTaxlbl.Name = "FurtureTaxlbl";
            FurtureTaxlbl.Size = new Size(212, 20);
            FurtureTaxlbl.TabIndex = 28;
            FurtureTaxlbl.Text = "Further Tax";
            // 
            // FurtureTax
            // 
            FurtureTax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FurtureTax.Font = new Font("Microsoft Sans Serif", 9F);
            FurtureTax.Location = new Point(677, 113);
            FurtureTax.Name = "FurtureTax";
            FurtureTax.PlaceholderText = "Further Tax";
            FurtureTax.Size = new Size(203, 24);
            FurtureTax.TabIndex = 17;
            // 
            // ItemName
            // 
            ItemName.Font = new Font("Microsoft Sans Serif", 9F);
            ItemName.Location = new Point(20, 113);
            ItemName.Name = "ItemName";
            ItemName.PlaceholderText = "Item Name";
            ItemName.Size = new Size(197, 24);
            ItemName.TabIndex = 14;
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
            qty.TabIndex = 15;
            qty.Text = "1";
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
            totalamountlbl.Location = new Point(463, 17);
            totalamountlbl.Name = "totalamountlbl";
            totalamountlbl.Size = new Size(197, 20);
            totalamountlbl.TabIndex = 38;
            totalamountlbl.Text = "Total Amount";
            // 
            // lblItemEntry
            // 
            lblItemEntry.AutoSize = true;
            lblItemEntry.Font = new Font("Microsoft Sans Serif", 10.8F);
            lblItemEntry.ForeColor = Color.FromArgb(17, 24, 39);
            lblItemEntry.Location = new Point(30, 184);
            lblItemEntry.Name = "lblItemEntry";
            lblItemEntry.Size = new Size(202, 22);
            lblItemEntry.TabIndex = 5;
            lblItemEntry.Text = "📦PRODUCT DETAILS";
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.BackColor = Color.FromArgb(48, 59, 78);
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            btnEdit.ForeColor = Color.Transparent;
            btnEdit.Location = new Point(1020, 379);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(155, 33);
            btnEdit.TabIndex = 22;
            btnEdit.Text = "🖊️ Edit";
            btnEdit.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom;
            btnSave.BackColor = Color.SeaGreen;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.ForeColor = Color.Transparent;
            btnSave.Location = new Point(498, 844);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(188, 44);
            btnSave.TabIndex = 23;
            btnSave.Text = "🖨️ Save and Print";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(107, 114, 128);
            dataGridViewCellStyle3.SelectionBackColor = Color.White;
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(107, 114, 128);
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(75, 85, 99);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dataGridViewCellStyle4.SelectionForeColor = Color.FromArgb(75, 85, 99);
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle4;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(229, 231, 235);
            dataGridView1.Location = new Point(30, 419);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.RowTemplate.Height = 50;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1145, 293);
            dataGridView1.TabIndex = 43;
            // 
            // lblInvoicesListing
            // 
            lblInvoicesListing.AutoSize = true;
            lblInvoicesListing.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblInvoicesListing.ForeColor = Color.FromArgb(17, 24, 39);
            lblInvoicesListing.Location = new Point(30, 390);
            lblInvoicesListing.Name = "lblInvoicesListing";
            lblInvoicesListing.Size = new Size(129, 22);
            lblInvoicesListing.TabIndex = 0;
            lblInvoicesListing.Text = "📊ITEMS LIST";
            // 
            // btn_remove
            // 
            btn_remove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_remove.BackColor = Color.Red;
            btn_remove.FlatAppearance.BorderSize = 0;
            btn_remove.FlatStyle = FlatStyle.Flat;
            btn_remove.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            btn_remove.ForeColor = Color.Transparent;
            btn_remove.Location = new Point(859, 379);
            btn_remove.Name = "btn_remove";
            btn_remove.Size = new Size(155, 33);
            btn_remove.TabIndex = 21;
            btn_remove.Text = "➖ Remove Item";
            btn_remove.UseVisualStyleBackColor = false;
            // 
            // btnProceed
            // 
            btnProceed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProceed.BackColor = Color.FromArgb(99, 102, 241);
            btnProceed.FlatAppearance.BorderSize = 0;
            btnProceed.FlatStyle = FlatStyle.Flat;
            btnProceed.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            btnProceed.ForeColor = Color.Transparent;
            btnProceed.Location = new Point(695, 379);
            btnProceed.Name = "btnProceed";
            btnProceed.Size = new Size(155, 33);
            btnProceed.TabIndex = 20;
            btnProceed.Text = "➕ Add Item";
            btnProceed.UseVisualStyleBackColor = false;
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.WhiteSmoke;
            contentPanel.Controls.Add(btnsearch);
            contentPanel.Controls.Add(btnclear);
            contentPanel.Controls.Add(label3);
            contentPanel.Controls.Add(label2);
            contentPanel.Controls.Add(label1);
            contentPanel.Controls.Add(panel3);
            contentPanel.Controls.Add(panel2);
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
            // btnsearch
            // 
            btnsearch.BackColor = Color.Teal;
            btnsearch.FlatAppearance.BorderSize = 0;
            btnsearch.FlatStyle = FlatStyle.Flat;
            btnsearch.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnsearch.ForeColor = Color.Transparent;
            btnsearch.Location = new Point(238, 172);
            btnsearch.Name = "btnsearch";
            btnsearch.Size = new Size(50, 34);
            btnsearch.TabIndex = 18;
            btnsearch.Text = "🔍";
            btnsearch.UseVisualStyleBackColor = false;
            // 
            // btnclear
            // 
            btnclear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnclear.BackColor = Color.Teal;
            btnclear.FlatAppearance.BorderSize = 0;
            btnclear.FlatStyle = FlatStyle.Flat;
            btnclear.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            btnclear.ForeColor = Color.Transparent;
            btnclear.Location = new Point(531, 379);
            btnclear.Name = "btnclear";
            btnclear.Size = new Size(155, 33);
            btnclear.TabIndex = 19;
            btnclear.Text = "\U0001f9f9 Clear Form";
            btnclear.UseVisualStyleBackColor = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.FromArgb(17, 24, 39);
            label3.Location = new Point(30, 57);
            label3.Name = "label3";
            label3.Size = new Size(223, 22);
            label3.TabIndex = 81;
            label3.Text = "👤BUYER INFORMATION";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 10.8F);
            label2.ForeColor = Color.FromArgb(17, 24, 39);
            label2.Location = new Point(608, 57);
            label2.Name = "label2";
            label2.Size = new Size(233, 22);
            label2.TabIndex = 80;
            label2.Text = "📄INVOICE INFORMATION";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(17, 24, 39);
            label1.Location = new Point(27, 723);
            label1.Name = "label1";
            label1.Size = new Size(215, 22);
            label1.TabIndex = 72;
            label1.Text = "💰INVOICE SUMMARY";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel3.BackColor = Color.White;
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(refUSIN);
            panel3.Controls.Add(refUSINlbl);
            panel3.Controls.Add(paymentmode);
            panel3.Controls.Add(invoicetype);
            panel3.Controls.Add(invoicetypelbl);
            panel3.Controls.Add(paymentmodelbl);
            panel3.Controls.Add(USIN);
            panel3.Controls.Add(USINlbl);
            panel3.Location = new Point(608, 82);
            panel3.Name = "panel3";
            panel3.Size = new Size(567, 87);
            panel3.TabIndex = 79;
            // 
            // refUSIN
            // 
            refUSIN.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refUSIN.BackColor = Color.White;
            refUSIN.Font = new Font("Microsoft Sans Serif", 9F);
            refUSIN.ImeMode = ImeMode.Disable;
            refUSIN.Location = new Point(410, 41);
            refUSIN.Name = "refUSIN";
            refUSIN.PlaceholderText = "Ref USIN";
            refUSIN.Size = new Size(123, 24);
            refUSIN.TabIndex = 8;
            refUSIN.Text = "1";
            // 
            // refUSINlbl
            // 
            refUSINlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            refUSINlbl.ForeColor = Color.FromArgb(75, 85, 99);
            refUSINlbl.Location = new Point(415, 18);
            refUSINlbl.Name = "refUSINlbl";
            refUSINlbl.Size = new Size(88, 20);
            refUSINlbl.TabIndex = 75;
            refUSINlbl.Text = "Ref USIN";
            // 
            // paymentmode
            // 
            paymentmode.DropDownStyle = ComboBoxStyle.DropDownList;
            paymentmode.Location = new Point(154, 41);
            paymentmode.Name = "paymentmode";
            paymentmode.Size = new Size(123, 26);
            paymentmode.TabIndex = 6;
            // 
            // invoicetype
            // 
            invoicetype.DropDownStyle = ComboBoxStyle.DropDownList;
            invoicetype.Location = new Point(25, 41);
            invoicetype.Name = "invoicetype";
            invoicetype.Size = new Size(123, 26);
            invoicetype.TabIndex = 5;
            // 
            // invoicetypelbl
            // 
            invoicetypelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            invoicetypelbl.ForeColor = Color.FromArgb(75, 85, 99);
            invoicetypelbl.Location = new Point(25, 18);
            invoicetypelbl.Name = "invoicetypelbl";
            invoicetypelbl.Size = new Size(120, 20);
            invoicetypelbl.TabIndex = 52;
            invoicetypelbl.Text = "Invoice Type";
            // 
            // paymentmodelbl
            // 
            paymentmodelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            paymentmodelbl.ForeColor = Color.FromArgb(75, 85, 99);
            paymentmodelbl.Location = new Point(154, 18);
            paymentmodelbl.Name = "paymentmodelbl";
            paymentmodelbl.Size = new Size(120, 20);
            paymentmodelbl.TabIndex = 72;
            paymentmodelbl.Text = "Payment Mode";
            // 
            // USIN
            // 
            USIN.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            USIN.BackColor = Color.White;
            USIN.Font = new Font("Microsoft Sans Serif", 9F);
            USIN.ImeMode = ImeMode.Disable;
            USIN.Location = new Point(281, 41);
            USIN.Name = "USIN";
            USIN.PlaceholderText = "USIN";
            USIN.Size = new Size(123, 24);
            USIN.TabIndex = 7;
            USIN.Text = "1";
            // 
            // USINlbl
            // 
            USINlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            USINlbl.ForeColor = Color.FromArgb(75, 85, 99);
            USINlbl.Location = new Point(286, 18);
            USINlbl.Name = "USINlbl";
            USINlbl.Size = new Size(76, 20);
            USINlbl.TabIndex = 56;
            USINlbl.Text = "USIN";
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.White;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(label17);
            panel2.Controls.Add(buyerphone);
            panel2.Controls.Add(label18);
            panel2.Controls.Add(BuyerBname);
            panel2.Controls.Add(label19);
            panel2.Controls.Add(buyerntn);
            panel2.Controls.Add(label20);
            panel2.Controls.Add(buyercnic);
            panel2.Location = new Point(30, 82);
            panel2.Name = "panel2";
            panel2.Size = new Size(567, 87);
            panel2.TabIndex = 78;
            // 
            // label17
            // 
            label17.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label17.ForeColor = Color.FromArgb(75, 85, 99);
            label17.Location = new Point(413, 18);
            label17.Name = "label17";
            label17.Size = new Size(128, 20);
            label17.TabIndex = 48;
            label17.Text = "Buyer Phone no.";
            // 
            // buyerphone
            // 
            buyerphone.Font = new Font("Microsoft Sans Serif", 9F);
            buyerphone.Location = new Point(410, 41);
            buyerphone.Name = "buyerphone";
            buyerphone.PlaceholderText = "Buyer Phone Number";
            buyerphone.Size = new Size(123, 24);
            buyerphone.TabIndex = 4;
            buyerphone.Text = "12345678912";
            // 
            // label18
            // 
            label18.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label18.ForeColor = Color.FromArgb(75, 85, 99);
            label18.Location = new Point(284, 18);
            label18.Name = "label18";
            label18.Size = new Size(120, 20);
            label18.TabIndex = 46;
            label18.Text = "Buyer Name";
            // 
            // BuyerBname
            // 
            BuyerBname.Font = new Font("Microsoft Sans Serif", 9F);
            BuyerBname.Location = new Point(281, 41);
            BuyerBname.Name = "BuyerBname";
            BuyerBname.PlaceholderText = "Buyer Name";
            BuyerBname.Size = new Size(123, 24);
            BuyerBname.TabIndex = 3;
            BuyerBname.Text = "1";
            // 
            // label19
            // 
            label19.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label19.ForeColor = Color.FromArgb(75, 85, 99);
            label19.Location = new Point(155, 18);
            label19.Name = "label19";
            label19.Size = new Size(131, 20);
            label19.TabIndex = 44;
            label19.Text = "Buyer NTN";
            // 
            // buyerntn
            // 
            buyerntn.Font = new Font("Microsoft Sans Serif", 9F);
            buyerntn.Location = new Point(152, 41);
            buyerntn.Name = "buyerntn";
            buyerntn.PlaceholderText = "Buyer NTN";
            buyerntn.Size = new Size(123, 24);
            buyerntn.TabIndex = 2;
            buyerntn.Text = "1234567";
            // 
            // label20
            // 
            label20.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label20.ForeColor = Color.FromArgb(75, 85, 99);
            label20.Location = new Point(26, 18);
            label20.Name = "label20";
            label20.Size = new Size(120, 20);
            label20.TabIndex = 42;
            label20.Text = "Buyer CNIC";
            // 
            // buyercnic
            // 
            buyercnic.Font = new Font("Microsoft Sans Serif", 9F);
            buyercnic.Location = new Point(23, 41);
            buyercnic.Name = "buyercnic";
            buyercnic.PlaceholderText = "Buyer CNIC";
            buyercnic.Size = new Size(123, 24);
            buyercnic.TabIndex = 1;
            buyercnic.Text = "1234567891234";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.Gainsboro;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(TotalBillAmountlbl);
            panel1.Controls.Add(TotalBillAmount);
            panel1.Controls.Add(TotalSaleValuelbl);
            panel1.Controls.Add(TotalSaleValue);
            panel1.Controls.Add(TotalQuantitylbl);
            panel1.Controls.Add(TotalQuantity);
            panel1.Controls.Add(totalFurtherTaxlbl);
            panel1.Controls.Add(TotalFurtherTax);
            panel1.Controls.Add(Discountlbl);
            panel1.Controls.Add(Discount);
            panel1.Controls.Add(posid);
            panel1.Controls.Add(label15);
            panel1.Controls.Add(TotalTaxChargedlbl);
            panel1.Controls.Add(TotalTaxCharged);
            panel1.Location = new Point(27, 748);
            panel1.Name = "panel1";
            panel1.Size = new Size(1148, 83);
            panel1.TabIndex = 57;
            // 
            // TotalBillAmountlbl
            // 
            TotalBillAmountlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalBillAmountlbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalBillAmountlbl.Location = new Point(974, 15);
            TotalBillAmountlbl.Name = "TotalBillAmountlbl";
            TotalBillAmountlbl.Size = new Size(139, 24);
            TotalBillAmountlbl.TabIndex = 71;
            TotalBillAmountlbl.Text = "Total Bill Amount";
            // 
            // TotalBillAmount
            // 
            TotalBillAmount.Font = new Font("Microsoft Sans Serif", 9F);
            TotalBillAmount.Location = new Point(974, 38);
            TotalBillAmount.Name = "TotalBillAmount";
            TotalBillAmount.PlaceholderText = "Total Bill Amount";
            TotalBillAmount.ReadOnly = true;
            TotalBillAmount.Size = new Size(139, 24);
            TotalBillAmount.TabIndex = 70;
            // 
            // TotalSaleValuelbl
            // 
            TotalSaleValuelbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalSaleValuelbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalSaleValuelbl.Location = new Point(346, 15);
            TotalSaleValuelbl.Name = "TotalSaleValuelbl";
            TotalSaleValuelbl.Size = new Size(136, 24);
            TotalSaleValuelbl.TabIndex = 69;
            TotalSaleValuelbl.Text = "Total Sale Value";
            // 
            // TotalSaleValue
            // 
            TotalSaleValue.Font = new Font("Microsoft Sans Serif", 9F);
            TotalSaleValue.Location = new Point(343, 38);
            TotalSaleValue.Name = "TotalSaleValue";
            TotalSaleValue.PlaceholderText = "Total Sale Value";
            TotalSaleValue.ReadOnly = true;
            TotalSaleValue.Size = new Size(139, 24);
            TotalSaleValue.TabIndex = 13;
            // 
            // TotalQuantitylbl
            // 
            TotalQuantitylbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalQuantitylbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalQuantitylbl.Location = new Point(186, 17);
            TotalQuantitylbl.Name = "TotalQuantitylbl";
            TotalQuantitylbl.Size = new Size(136, 24);
            TotalQuantitylbl.TabIndex = 67;
            TotalQuantitylbl.Text = "Total Quantity";
            // 
            // TotalQuantity
            // 
            TotalQuantity.Font = new Font("Microsoft Sans Serif", 9F);
            TotalQuantity.Location = new Point(183, 38);
            TotalQuantity.Name = "TotalQuantity";
            TotalQuantity.PlaceholderText = "Total Quantity";
            TotalQuantity.ReadOnly = true;
            TotalQuantity.Size = new Size(139, 24);
            TotalQuantity.TabIndex = 12;
            // 
            // totalFurtherTaxlbl
            // 
            totalFurtherTaxlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            totalFurtherTaxlbl.ForeColor = Color.FromArgb(75, 85, 99);
            totalFurtherTaxlbl.Location = new Point(821, 15);
            totalFurtherTaxlbl.Name = "totalFurtherTaxlbl";
            totalFurtherTaxlbl.Size = new Size(136, 24);
            totalFurtherTaxlbl.TabIndex = 65;
            totalFurtherTaxlbl.Text = "FurtherTax";
            // 
            // TotalFurtherTax
            // 
            TotalFurtherTax.Font = new Font("Microsoft Sans Serif", 9F);
            TotalFurtherTax.Location = new Point(818, 38);
            TotalFurtherTax.Name = "TotalFurtherTax";
            TotalFurtherTax.PlaceholderText = "Further Tax";
            TotalFurtherTax.ReadOnly = true;
            TotalFurtherTax.Size = new Size(139, 24);
            TotalFurtherTax.TabIndex = 16;
            // 
            // Discountlbl
            // 
            Discountlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            Discountlbl.ForeColor = Color.FromArgb(75, 85, 99);
            Discountlbl.Location = new Point(668, 15);
            Discountlbl.Name = "Discountlbl";
            Discountlbl.Size = new Size(136, 24);
            Discountlbl.TabIndex = 63;
            Discountlbl.Text = "Discount";
            // 
            // Discount
            // 
            Discount.Font = new Font("Microsoft Sans Serif", 9F);
            Discount.Location = new Point(665, 40);
            Discount.Name = "Discount";
            Discount.PlaceholderText = "Discount";
            Discount.ReadOnly = true;
            Discount.Size = new Size(139, 24);
            Discount.TabIndex = 15;
            // 
            // posid
            // 
            posid.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            posid.BackColor = SystemColors.Control;
            posid.Font = new Font("Microsoft Sans Serif", 9F);
            posid.ImeMode = ImeMode.Disable;
            posid.Location = new Point(27, 40);
            posid.Name = "posid";
            posid.PlaceholderText = "POS ID";
            posid.ReadOnly = true;
            posid.Size = new Size(139, 24);
            posid.TabIndex = 0;
            // 
            // label15
            // 
            label15.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            label15.ForeColor = Color.FromArgb(75, 85, 99);
            label15.Location = new Point(29, 17);
            label15.Name = "label15";
            label15.Size = new Size(136, 24);
            label15.TabIndex = 43;
            label15.Text = "POS ID";
            // 
            // TotalTaxChargedlbl
            // 
            TotalTaxChargedlbl.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            TotalTaxChargedlbl.ForeColor = Color.FromArgb(75, 85, 99);
            TotalTaxChargedlbl.Location = new Point(504, 17);
            TotalTaxChargedlbl.Name = "TotalTaxChargedlbl";
            TotalTaxChargedlbl.Size = new Size(139, 24);
            TotalTaxChargedlbl.TabIndex = 61;
            TotalTaxChargedlbl.Text = "Total Tax Charged";
            // 
            // TotalTaxCharged
            // 
            TotalTaxCharged.Font = new Font("Microsoft Sans Serif", 9F);
            TotalTaxCharged.Location = new Point(504, 40);
            TotalTaxCharged.Name = "TotalTaxCharged";
            TotalTaxCharged.PlaceholderText = "Total Tax Charged";
            TotalTaxCharged.ReadOnly = true;
            TotalTaxCharged.Size = new Size(139, 24);
            TotalTaxCharged.TabIndex = 14;
            // 
            // label31
            // 
            label31.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label31.ForeColor = Color.FromArgb(17, 24, 39);
            label31.Location = new Point(27, 16);
            label31.Name = "label31";
            label31.Size = new Size(244, 37);
            label31.TabIndex = 56;
            label31.Text = "INVOICE ENTRY";
            // 
            // ItemEntry
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1202, 900);
            Controls.Add(contentPanel);
            Font = new Font("Microsoft Sans Serif", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "ItemEntry";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Item Entry";
            Load += item_entry_Load;
            pnlBasicInfo.ResumeLayout(false);
            pnlBasicInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            contentPanel.ResumeLayout(false);
            contentPanel.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // Custom method to initialize DataGridView columns
        //private void InitializeDataGridViewColumns()
        //{
        //    // Clear any existing columns
        //    dataGridView1.Columns.Clear();

        //    // Set DataGridView properties
        //    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        //    // Common cell styles
        //    var headerStyle = new DataGridViewCellStyle
        //    {
        //        Alignment = DataGridViewContentAlignment.MiddleLeft,
        //        BackColor = Color.White,
        //        Font = new Font("Microsoft Sans Serif", 9F),
        //        ForeColor = Color.FromArgb(107, 114, 128),
        //        SelectionBackColor = Color.White,
        //        SelectionForeColor = Color.FromArgb(107, 114, 128)
        //    };

        //    var rowStyle = new DataGridViewCellStyle
        //    {
        //        Alignment = DataGridViewContentAlignment.MiddleLeft,
        //        BackColor = Color.White,
        //        Font = new Font("Microsoft Sans Serif", 9F),
        //        ForeColor = Color.FromArgb(75, 85, 99),
        //        SelectionBackColor = Color.FromArgb(248, 250, 252),
        //        SelectionForeColor = Color.FromArgb(75, 85, 99),
        //        WrapMode = DataGridViewTriState.False
        //    };

        //    // Update DataGridView styles
        //    dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
        //    dataGridView1.DefaultCellStyle = rowStyle;

        //    // Add columns with relative FillWeight
        //    dataGridView1.Columns.AddRange(new DataGridViewColumn[]
        //    {
        //        new DataGridViewTextBoxColumn { Name = "colSrNo", HeaderText = "Sr. No.", ReadOnly = true, FillWeight = 50 },
        //        new DataGridViewTextBoxColumn { Name = "colProductCode", HeaderText = "Item Code", ReadOnly = true, FillWeight = 80 },
        //        new DataGridViewTextBoxColumn { Name = "colHSCode", HeaderText = "HS Code", ReadOnly = true, FillWeight = 80 },
        //        new DataGridViewTextBoxColumn { Name = "colProductDescription", HeaderText = "Item Name", ReadOnly = true, FillWeight = 150 },
        //        new DataGridViewTextBoxColumn { Name = "colQuantity", HeaderText = "Quantity", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 80 },
        //        new DataGridViewTextBoxColumn { Name = "colRate", HeaderText = "Rate", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 80 },
        //        new DataGridViewTextBoxColumn { Name = "colDiscount", HeaderText = "Discount (Amt)", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 90 },
        //        new DataGridViewTextBoxColumn { Name = "colSalesValueExcST", HeaderText = "Sales Value (exc ST)", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 120 },
        //        new DataGridViewTextBoxColumn { Name = "colTotalValue", HeaderText = "Total Value", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 120 },
        //        new DataGridViewTextBoxColumn { Name = "colSalesTax", HeaderText = "Tax Rate (%)", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 80 },
        //        new DataGridViewTextBoxColumn { Name = "colExtraTax", HeaderText = "Tax Charged", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 100 },
        //        new DataGridViewTextBoxColumn { Name = "colFutureTax", HeaderText = "Further Tax", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 90 },
        //        new DataGridViewTextBoxColumn { Name = "colInvoiceType", HeaderText = "Inv Type", ReadOnly = true, FillWeight = 70 },
        //        new DataGridViewTextBoxColumn { Name = "colRefUSIN", HeaderText = "Ref USIN", ReadOnly = true, FillWeight = 100 }
        //    });
        //}

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
            //InitializeDataGridViewColumns();
            InitializeComboBoxes();
            //ImproveLayoutFormatting();

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
        private Label label7;
        private TextBox rate;
        private Label label5;
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
        private Label USINlbl;
        private TextBox USIN;
        private Label TotalQuantitylbl;
        private TextBox TotalQuantity;
        private Label invoicetypelbl;
        private Label Discountlbl;
        private TextBox Discount;
        private Label TotalTaxChargedlbl;
        private TextBox TotalTaxCharged;
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
        private Panel panel2;
        private Label label17;
        private TextBox buyerphone;
        private Label label18;
        private TextBox BuyerBname;
        private Label label19;
        private TextBox buyerntn;
        private Label label20;
        private TextBox buyercnic;
        private Panel panel3;
        private Label TotalBillAmountlbl;
        private TextBox TotalBillAmount;
        private Label label3;
        private Label label1;
        private Label label2;
        private Button btnclear;
        private Button btnsearch;
        private TextBox SearchBox;
        private TextBox itemDiscountAmount;
        private TextBox itemDiscountPercent;
        private Label label4;
    }
}