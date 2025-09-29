namespace POSPRA_WinFormsUI.Forms
{
    partial class CatalogView
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnSave = new Button();
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
            ((System.ComponentModel.ISupportInitialize)ProductCatalogueDataGridView).BeginInit();
            SuspendLayout();
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnSave.BackColor = Color.FromArgb(0, 120, 215);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(1359, 568);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(212, 53);
            btnSave.TabIndex = 1;
            btnSave.Text = "💾 Save";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // labelInvoicesTitle
            // 
            labelInvoicesTitle.AutoSize = true;
            labelInvoicesTitle.Dock = DockStyle.Top;
            labelInvoicesTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelInvoicesTitle.ForeColor = Color.FromArgb(30, 30, 30);
            labelInvoicesTitle.Location = new Point(0, 0);
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
            ProductCatalogueDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProductCatalogueDataGridView.Columns.AddRange(new DataGridViewColumn[] { colItemSrno, colProductCode, colProductDesc, colHScode, colSaleType, PosUOM, colTaxRate, colSROno });
            ProductCatalogueDataGridView.Dock = DockStyle.Top;
            ProductCatalogueDataGridView.Location = new Point(0, 49);
            ProductCatalogueDataGridView.Margin = new Padding(3, 4, 3, 4);
            ProductCatalogueDataGridView.Name = "ProductCatalogueDataGridView";
            ProductCatalogueDataGridView.ReadOnly = true;
            ProductCatalogueDataGridView.RowHeadersVisible = false;
            ProductCatalogueDataGridView.RowHeadersWidth = 51;
            ProductCatalogueDataGridView.Size = new Size(1571, 512);
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
            // CatalogView
            // 
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(1571, 814);
            Controls.Add(ProductCatalogueDataGridView);
            Controls.Add(labelInvoicesTitle);
            Controls.Add(btnSave);
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
    }
}
