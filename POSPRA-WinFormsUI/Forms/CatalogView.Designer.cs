namespace POSPRA_WinFormsUI.Forms
{
    partial class CatalogView
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnLoad;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnLoad = new Button();
            labelInvoicesTitle = new Label();
            ProductCatalogueDataGridView = new DataGridView();
            colItemSrno = new DataGridViewTextBoxColumn();
            colProductCode = new DataGridViewTextBoxColumn();
            colProductDesc = new DataGridViewTextBoxColumn();
            colHScode = new DataGridViewTextBoxColumn();
            colSaleType = new DataGridViewTextBoxColumn();
            PosUOM = new DataGridViewTextBoxColumn();
            colTaxRate = new DataGridViewTextBoxColumn();
            colSROno = new DataGridViewTextBoxColumn();
            SearchBox = new TextBox();
            btnNext = new Button();
            btnPrev = new Button();
            lblPageNumber = new Label();
            ((System.ComponentModel.ISupportInitialize)ProductCatalogueDataGridView).BeginInit();
            SuspendLayout();
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnLoad.BackColor = Color.FromArgb(0, 120, 215);
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLoad.ForeColor = Color.White;
            btnLoad.Location = new Point(1359, 729);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(240, 53);
            btnLoad.TabIndex = 1;
            btnLoad.Text = "⏳ Load";
            btnLoad.UseVisualStyleBackColor = false;
            // 
            // labelInvoicesTitle
            // 
            labelInvoicesTitle.Anchor = AnchorStyles.Left;
            labelInvoicesTitle.AutoSize = true;
            labelInvoicesTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelInvoicesTitle.ForeColor = Color.FromArgb(30, 30, 30);
            labelInvoicesTitle.Location = new Point(0, 18);
            labelInvoicesTitle.Name = "labelInvoicesTitle";
            labelInvoicesTitle.Padding = new Padding(6, 6, 0, 6);
            labelInvoicesTitle.Size = new Size(263, 49);
            labelInvoicesTitle.TabIndex = 2;
            labelInvoicesTitle.Text = "Product Catalogue";
            // 
            // ProductCatalogueDataGridView
            // 
            ProductCatalogueDataGridView.AllowUserToAddRows = false;
            ProductCatalogueDataGridView.AllowUserToDeleteRows = false;
            ProductCatalogueDataGridView.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ProductCatalogueDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProductCatalogueDataGridView.Columns.AddRange(new DataGridViewColumn[] { colItemSrno, colProductCode, colProductDesc, colHScode, colSaleType, PosUOM, colTaxRate, colSROno });
            ProductCatalogueDataGridView.Location = new Point(0, 71);
            ProductCatalogueDataGridView.Margin = new Padding(3, 4, 3, 4);
            ProductCatalogueDataGridView.Name = "ProductCatalogueDataGridView";
            ProductCatalogueDataGridView.ReadOnly = true;
            ProductCatalogueDataGridView.RowHeadersVisible = false;
            ProductCatalogueDataGridView.RowHeadersWidth = 51;
            ProductCatalogueDataGridView.Size = new Size(1599, 609);
            ProductCatalogueDataGridView.TabIndex = 3;
            // 
            // colItemSrno
            // 
            colItemSrno.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colItemSrno.HeaderText = "Sr. No.";
            colItemSrno.MinimumWidth = 50;
            colItemSrno.Name = "colItemSrno";
            colItemSrno.ReadOnly = true;
            colItemSrno.Width = 90;
            // 
            // colProductCode
            // 
            colProductCode.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colProductCode.HeaderText = "Product Code";
            colProductCode.MinimumWidth = 6;
            colProductCode.Name = "colProductCode";
            colProductCode.ReadOnly = true;
            // 
            // colProductDesc
            // 
            colProductDesc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colProductDesc.HeaderText = "Product Description";
            colProductDesc.MinimumWidth = 6;
            colProductDesc.Name = "colProductDesc";
            colProductDesc.ReadOnly = true;
            // 
            // colHScode
            // 
            colHScode.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colHScode.HeaderText = "HSCode";
            colHScode.MinimumWidth = 6;
            colHScode.Name = "colHScode";
            colHScode.ReadOnly = true;
            // 
            // colSaleType
            // 
            colSaleType.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSaleType.HeaderText = "Sale Type";
            colSaleType.MinimumWidth = 6;
            colSaleType.Name = "colSaleType";
            colSaleType.ReadOnly = true;
            // 
            // PosUOM
            // 
            PosUOM.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            PosUOM.HeaderText = "POS UOM";
            PosUOM.MinimumWidth = 6;
            PosUOM.Name = "PosUOM";
            PosUOM.ReadOnly = true;
            // 
            // colTaxRate
            // 
            colTaxRate.HeaderText = "Tax Rate";
            colTaxRate.MinimumWidth = 6;
            colTaxRate.Name = "colTaxRate";
            colTaxRate.ReadOnly = true;
            colTaxRate.Width = 125;
            // 
            // colSROno
            // 
            colSROno.HeaderText = "SRO Schedule No.";
            colSROno.MinimumWidth = 6;
            colSROno.Name = "colSROno";
            colSROno.ReadOnly = true;
            colSROno.Width = 125;
            // 
            // SearchBox
            // 
            SearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SearchBox.Font = new Font("Microsoft Sans Serif", 9F);
            SearchBox.ForeColor = SystemColors.InfoText;
            SearchBox.Location = new Point(1347, 40);
            SearchBox.Name = "SearchBox";
            SearchBox.PlaceholderText = "Search";
            SearchBox.Size = new Size(252, 24);
            SearchBox.TabIndex = 1;
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Right;
            btnNext.Location = new Point(1524, 690);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(75, 30);
            btnNext.TabIndex = 1;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // btnPrev
            // 
            btnPrev.Anchor = AnchorStyles.Right;
            btnPrev.Location = new Point(1362, 690);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(75, 30);
            btnPrev.TabIndex = 2;
            btnPrev.Text = "Previous";
            btnPrev.UseVisualStyleBackColor = true;
            // 
            // lblPageNumber
            // 
            lblPageNumber.Anchor = AnchorStyles.Right;
            lblPageNumber.AutoSize = true;
            lblPageNumber.Location = new Point(1452, 695);
            lblPageNumber.Name = "lblPageNumber";
            lblPageNumber.Size = new Size(53, 20);
            lblPageNumber.TabIndex = 3;
            lblPageNumber.Text = "Page 1";
            // 
            // CatalogView
            // 
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(1599, 820);
            Controls.Add(btnNext);
            Controls.Add(btnPrev);
            Controls.Add(lblPageNumber);
            Controls.Add(SearchBox);
            Controls.Add(ProductCatalogueDataGridView);
            Controls.Add(labelInvoicesTitle);
            Controls.Add(btnLoad);
            Name = "CatalogView";
            Text = "Product Catalogue";
            ((System.ComponentModel.ISupportInitialize)ProductCatalogueDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label labelInvoicesTitle;
        private DataGridView ProductCatalogueDataGridView;
        private DataGridViewTextBoxColumn colItemSrno;
        private DataGridViewTextBoxColumn colProductCode;
        private DataGridViewTextBoxColumn colProductDesc;
        private DataGridViewTextBoxColumn colHScode;
        private DataGridViewTextBoxColumn colSaleType;
        private DataGridViewTextBoxColumn PosUOM;
        private DataGridViewTextBoxColumn colTaxRate;
        private DataGridViewTextBoxColumn colSROno;
        private TextBox SearchBox;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Label lblPageNumber;
    }
}
