namespace POSPRA_WinFormsUI.Forms
{
    partial class CatalogView
    {
        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            ProductCatalogueDataGridView = new DataGridView();
            colItemSrno = new DataGridViewTextBoxColumn();
            colProductCode = new DataGridViewTextBoxColumn();
            colProductDesc = new DataGridViewTextBoxColumn();
            colHScode = new DataGridViewTextBoxColumn();
            colSaleType = new DataGridViewTextBoxColumn();
            PosUOM = new DataGridViewTextBoxColumn();
            colTaxRate = new DataGridViewTextBoxColumn();
            colSROno = new DataGridViewTextBoxColumn();
            btnNext = new Button();
            btnPrev = new Button();
            lblPageNumber = new Label();
            panel2 = new Panel();
            lblTotalRecords = new Label();
            btnLoad = new Button();
            panel1 = new Panel();
            lblCustomerRegType = new Label();
            labelInvoicesTitle = new Label();
            SearchBox = new TextBox();
            label1 = new Label();
            progressBar = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)ProductCatalogueDataGridView).BeginInit();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // ProductCatalogueDataGridView
            // 
            ProductCatalogueDataGridView.AllowUserToAddRows = false;
            ProductCatalogueDataGridView.AllowUserToDeleteRows = false;
            ProductCatalogueDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ProductCatalogueDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProductCatalogueDataGridView.Location = new Point(0, 49);
            ProductCatalogueDataGridView.Margin = new Padding(3, 4, 3, 4);
            ProductCatalogueDataGridView.Name = "ProductCatalogueDataGridView";
            ProductCatalogueDataGridView.ReadOnly = true;
            ProductCatalogueDataGridView.RowHeadersVisible = false;
            ProductCatalogueDataGridView.RowHeadersWidth = 51;
            ProductCatalogueDataGridView.Size = new Size(1587, 646);
            ProductCatalogueDataGridView.TabIndex = 9;
            
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Right;
            btnNext.Location = new Point(1494, 3);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(82, 30);
            btnNext.TabIndex = 5;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // btnPrev
            // 
            btnPrev.Anchor = AnchorStyles.Right;
            btnPrev.Location = new Point(1288, 2);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(82, 30);
            btnPrev.TabIndex = 4;
            btnPrev.Text = "Previous";
            btnPrev.UseVisualStyleBackColor = true;
            // 
            // lblPageNumber
            // 
            lblPageNumber.Anchor = AnchorStyles.Right;
            lblPageNumber.AutoSize = true;
            lblPageNumber.Location = new Point(1376, 8);
            lblPageNumber.Name = "lblPageNumber";
            lblPageNumber.Size = new Size(53, 20);
            lblPageNumber.TabIndex = 7;
            lblPageNumber.Text = "Page 1";
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(lblTotalRecords);
            panel2.Controls.Add(btnLoad);
            panel2.Controls.Add(lblPageNumber);
            panel2.Controls.Add(btnNext);
            panel2.Controls.Add(btnPrev);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 690);
            panel2.Name = "panel2";
            panel2.Size = new Size(1581, 83);
            panel2.TabIndex = 79;
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.AutoSize = true;
            lblTotalRecords.Dock = DockStyle.Left;
            lblTotalRecords.Location = new Point(0, 0);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(111, 20);
            lblTotalRecords.TabIndex = 8;
            lblTotalRecords.Text = "10000 products";
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnLoad.AutoSize = true;
            btnLoad.BackColor = Color.FromArgb(0, 120, 215);
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLoad.ForeColor = Color.White;
            btnLoad.Location = new Point(1288, 38);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(288, 40);
            btnLoad.TabIndex = 3;
            btnLoad.Text = "🔄Sync Products From Cloud";
            btnLoad.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(lblCustomerRegType);
            panel1.Controls.Add(labelInvoicesTitle);
            panel1.Controls.Add(SearchBox);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1581, 46);
            panel1.TabIndex = 80;
            // 
            // lblCustomerRegType
            // 
            lblCustomerRegType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCustomerRegType.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold);
            lblCustomerRegType.ForeColor = Color.FromArgb(75, 85, 99);
            lblCustomerRegType.Location = new Point(1254, 15);
            lblCustomerRegType.Name = "lblCustomerRegType";
            lblCustomerRegType.Size = new Size(62, 20);
            lblCustomerRegType.TabIndex = 41;
            lblCustomerRegType.Text = "Search";
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
            labelInvoicesTitle.Size = new Size(311, 49);
            labelInvoicesTitle.TabIndex = 40;
            labelInvoicesTitle.Text = "PRODUCT CATALOGUE";
            // 
            // SearchBox
            // 
            SearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SearchBox.Font = new Font("Microsoft Sans Serif", 9F);
            SearchBox.ForeColor = SystemColors.InfoText;
            SearchBox.Location = new Point(1322, 10);
            SearchBox.Name = "SearchBox";
            SearchBox.PlaceholderText = "Search";
            SearchBox.Size = new Size(252, 24);
            SearchBox.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(2819, -18);
            label1.Name = "label1";
            label1.Size = new Size(53, 20);
            label1.TabIndex = 3;
            label1.Text = "Page 1";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(590, 331);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(414, 34);
            progressBar.TabIndex = 81;
            // 
            // CatalogView
            // 
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(1581, 773);
            Controls.Add(progressBar);
            Controls.Add(panel1);
            Controls.Add(panel2);
            Controls.Add(ProductCatalogueDataGridView);
            Name = "CatalogView";
            ((System.ComponentModel.ISupportInitialize)ProductCatalogueDataGridView).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }
        private DataGridView ProductCatalogueDataGridView;
        private DataGridViewTextBoxColumn colItemSrno;
        private DataGridViewTextBoxColumn colProductCode;
        private DataGridViewTextBoxColumn colProductDesc;
        private DataGridViewTextBoxColumn colHScode;
        private DataGridViewTextBoxColumn colSaleType;
        private DataGridViewTextBoxColumn PosUOM;
        private DataGridViewTextBoxColumn colTaxRate;
        private DataGridViewTextBoxColumn colSROno;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Label lblPageNumber;
        private Panel panel2;
        private Button btnLoad;
        private Panel panel1;
        private Label lblCustomerRegType;
        private Label labelInvoicesTitle;
        private TextBox SearchBox;
        private Label label1;
        private ProgressBar progressBar;
        private Label lblTotalRecords;
    }
}
