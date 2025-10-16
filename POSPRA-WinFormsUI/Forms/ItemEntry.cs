using Azure;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.DTOs.ProductCatalogDtos;
using POSPRA.SecurityEncryption;
using POSPRA_WinFormsUI.AlertClasses;
using POSPRA_WinFormsUI.Forms;
using System.Configuration;
using System.Drawing.Drawing2D;
using AlertType = POSPRA.Application.Utility.AlertType;

namespace POSPRA_WinFormsUI
{
    public partial class ItemEntry : Form
    {
        #region Fields

        private readonly Dictionary<Control, Rectangle> _originalBounds = new();
        private readonly Dictionary<Control, Size> _originalParentSizes = new();
        private readonly Dictionary<Control, Font> _originalFonts = new();
        private Size _originalClientSize = Size.Empty;
        private bool _originalLayoutCaptured = false;

        private static List<InvoiceItems> _sessionItems = new();
        public static Invoice CurrentInvoice;
        private readonly List<InvoiceItems> addedItems;
        private bool _isSaving = false;

        //logs
        private readonly ILogService _logService;
        private readonly IInvoiceService _invoiceService;
        private readonly IProductCatalogueService _productCatalogueService;


        #endregion

        #region Constructor / Initialization

        public ItemEntry(
            IHttpClientFactory httpClientFactory,
            ILogService logService,
            IProductCatalogueService productCatalogueService,
            IInvoiceService invoiceService)
        {
            InitializeComponent();
            this.Load += item_entry_Load;

            this.Shown += (s, e) =>
            {
                if (!_originalLayoutCaptured)
                {
                    CaptureOriginalLayout();
                }
            };

            //FixLayoutIssues();
            SetupPanelResizeHandlers();

            // Load POSID from app.config (stored encrypted)
            var encryptedPosId = ConfigurationManager.AppSettings["Username"] ?? "0";
            var decryptedPosId = AesEncryptionHelper.Decrypt(encryptedPosId);
            posid.Text = decryptedPosId;   // show real POSID in UI

            addedItems = _sessionItems;

            // Event wiring
            this.Resize += Item_entry_Resize;
            //pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);
            //panel1.Resize += (s, e) => MakeRoundedControl(panel1, 25);


            btnProceed.Click += BtnProceed_Click;
            btnSave.Click += BtnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btn_remove.Click += btn_remove_Click;
            btnclear.Click += btnclear_Click;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            btnsearch.Click += btnsearch_Click;

            qty.TextChanged += RecalculateTotals;
            salevalue.TextChanged += RecalculateTotals;
            TaxRatebox.TextChanged += RecalculateTotals;
            FurtureTax.TextChanged += RecalculateTotals;

            buyercnic.KeyPress += NumericOnlyWithLength_KeyPress;
            buyerntn.KeyPress += NumericOnlyWithLength_KeyPress;
            BuyerBname.KeyPress += NumericOnlyWithLength_KeyPress;
            buyerphone.KeyPress += NumericOnlyWithLength_KeyPress;

            // invoice item fields
            ItemCode.KeyPress += NumericOnlyWithLength_KeyPress;
            pctCode.KeyPress += NumericOnlyWithLength_KeyPress;
            totalamount.KeyPress += NumericOnlyWithLength_KeyPress;
            TaxRatebox.KeyPress += NumericOnlyWithLength_KeyPress;
            qty.KeyPress += NumericOnlyWithLength_KeyPress;
            salevalue.KeyPress += NumericOnlyWithLength_KeyPress;
            FurtureTax.KeyPress += NumericOnlyWithLength_KeyPress;
            TaxCharged.KeyPress += NumericOnlyWithLength_KeyPress;
            itemDiscountPercent.TextChanged += RecalculateTotals;
            itemDiscountPercent.KeyPress += NumericOnlyWithLength_KeyPress;

            invoicetype.SelectedIndexChanged += Invoicetype_SelectedIndexChanged;

            SetupContextMenu();
            CaptureOriginalLayout();
            CaptureOriginalLayout();
            InitializeEmptyGrid();

            StyleProductDataGridView();
            SetButtonImage(btnsearch, Resources.search, Color.Black);

            _logService = logService;
            _productCatalogueService = productCatalogueService;
            _invoiceService = invoiceService;
        }

        #endregion

        #region Panel Content Resize Handlers

        private void SetupPanelResizeHandlers()
        {
            // Wire up form resize to handle panel positioning
            this.Resize += Form_Resize_Panels;

            panel2.Resize += Panel2_Resize;
            panel3.Resize += Panel3_Resize;
            pnlBasicInfo.Resize += PnlBasicInfo_Resize;
            panel1.Resize += Panel1_Resize;
        }
        private void Form_Resize_Panels(object sender, EventArgs e)
        {
            if (this.ClientSize.Width == 0) return;

            // Calculate 45% width for each panel
            int formWidth = this.ClientSize.Width;
            int leftMargin = (int)(formWidth * 0.015); // 1.5% left margin
            int middleGap = (int)(formWidth * 0.010);   // 1% gap between panels
            int rightMargin = (int)(formWidth * 0.015); // 1.5% right margin

            int panelWidth = (int)(formWidth * 0.48);  // 48% for each panel

            // Position Panel 2 (Buyer Information)
            panel2.Location = new Point(leftMargin, panel2.Location.Y);
            panel2.Size = new Size(panelWidth, 87);

            // Position Panel 3 (Invoice Information) 
            int panel3X = leftMargin + panelWidth + middleGap;
            panel3.Location = new Point(panel3X, panel3.Location.Y);
            panel3.Size = new Size(panelWidth, 87);

            // Update label positions if needed
            if (label3 != null) // "BUYER INFORMATION" label
            {
                label3.Location = new Point(leftMargin, label3.Location.Y);
            }

            if (label2 != null) // "INVOICE INFORMATION" label
            {
                label2.Location = new Point(panel3X, label2.Location.Y);
            }
        }

        private void Panel2_Resize(object sender, EventArgs e)
        {
            if (panel2.Width == 0 || panel2.Height == 0) return;

            // Panel 2: 4 columns - Buyer CNIC, Buyer NTN, Buyer Name, Buyer Phone
            int leftPadding = 16;
            int rightPadding = 16;
            int spacing = 12;

            int availableWidth = panel2.Width - leftPadding - rightPadding - (spacing * 3);
            int colWidth = availableWidth / 4;

            int labelY = 18;
            int controlY = 41;
            int controlHeight = 24;

            // Column 1 - Buyer CNIC
            int col1X = leftPadding;
            label20.Location = new Point(col1X, labelY);
            label20.AutoSize = true;
            buyercnic.Location = new Point(col1X, controlY);
            buyercnic.Size = new Size(colWidth, controlHeight);

            // Column 2 - Buyer NTN
            int col2X = col1X + colWidth + spacing;
            label19.Location = new Point(col2X, labelY);
            label19.AutoSize = true;
            buyerntn.Location = new Point(col2X, controlY);
            buyerntn.Size = new Size(colWidth, controlHeight);

            // Column 3 - Buyer Name
            int col3X = col2X + colWidth + spacing;
            label18.Location = new Point(col3X, labelY);
            label18.AutoSize = true;
            BuyerBname.Location = new Point(col3X, controlY);
            BuyerBname.Size = new Size(colWidth, controlHeight);

            // Column 4 - Buyer Phone
            int col4X = col3X + colWidth + spacing;
            label17.Location = new Point(col4X, labelY);
            label17.AutoSize = true;
            buyerphone.Location = new Point(col4X, controlY);
            buyerphone.Size = new Size(colWidth, controlHeight);
        }

        private void Panel3_Resize(object sender, EventArgs e)
        {
            if (panel3.Width == 0 || panel3.Height == 0) return;

            // Panel 3: 4 columns - Invoice Type, Payment Mode, USIN, Ref USIN
            int leftPadding = 16;
            int rightPadding = 16;
            int spacing = 12;

            int availableWidth = panel3.Width - leftPadding - rightPadding - (spacing * 3);
            int colWidth = availableWidth / 4;

            int labelY = 18;
            int controlY = 41;
            int comboHeight = 26;
            int textHeight = 24;

            // Column 1 - Invoice Type
            int col1X = leftPadding;
            invoicetypelbl.Location = new Point(col1X, labelY);
            invoicetypelbl.AutoSize = true;
            invoicetype.Location = new Point(col1X, controlY);
            invoicetype.Size = new Size(colWidth, comboHeight);

            // Column 2 - Payment Mode
            int col2X = col1X + colWidth + spacing;
            paymentmodelbl.Location = new Point(col2X, labelY);
            paymentmodelbl.AutoSize = true;
            paymentmode.Location = new Point(col2X, controlY);
            paymentmode.Size = new Size(colWidth, comboHeight);

            // Column 3 - USIN
            int col3X = col2X + colWidth + spacing;
            USINlbl.Location = new Point(col3X, labelY);
            USINlbl.AutoSize = true;
            USIN.Location = new Point(col3X, controlY);
            USIN.Size = new Size(colWidth, textHeight);

            // Column 4 - Ref USIN
            int col4X = col3X + colWidth + spacing;
            refUSINlbl.Location = new Point(col4X, labelY);
            refUSINlbl.AutoSize = true;
            refUSIN.Location = new Point(col4X, controlY);
            refUSIN.Size = new Size(colWidth, textHeight);
        }

        private void PnlBasicInfo_Resize(object sender, EventArgs e)
        {
            if (pnlBasicInfo.Width == 0 || pnlBasicInfo.Height == 0) return;

            int padding = 18;
            int spacing = 8;
            int row1LabelY = 14;
            int row1ControlY = 36;
            int row2LabelY = 85;
            int row2ControlY = 107;
            int controlHeight = 24;

            // Calculate widths for Row 1 (6 columns with special handling for discount)
            int availableWidth = pnlBasicInfo.Width - (padding * 2) - (spacing * 5);

            // Widths: 4 equal columns + discount% (smaller) + discount Rs (smaller)
            int standardColWidth = (int)(availableWidth * 0.20); // 20% each for first 4
            int discountPercentWidth = (int)(availableWidth * 0.10); // 10% for discount %
            int discountAmountWidth = (int)(availableWidth * 0.10); // 10% for discount Rs

            // Row 1 Column positions
            int col1X = padding;
            int col2X = col1X + standardColWidth + spacing;
            int col3X = col2X + standardColWidth + spacing;
            int col4X = col3X + standardColWidth + spacing;
            int col5X = col4X + standardColWidth + spacing;
            int col6X = col5X + discountPercentWidth + spacing;

            // ROW 1 - Item Code
            ItemCodelbl.Location = new Point(col1X, row1LabelY);
            ItemCodelbl.AutoSize = true;
            ItemCode.Location = new Point(col1X, row1ControlY);
            ItemCode.Size = new Size(standardColWidth, controlHeight);

            // ROW 1 - PCT Code
            lblCustomerRegType.Location = new Point(col2X, row1LabelY);
            lblCustomerRegType.AutoSize = true;
            pctCode.Location = new Point(col2X, row1ControlY);
            pctCode.Size = new Size(standardColWidth, controlHeight);

            // ROW 1 - Total Amount
            totalamountlbl.Location = new Point(col3X, row1LabelY);
            totalamountlbl.AutoSize = true;
            totalamount.Location = new Point(col3X, row1ControlY);
            totalamount.Size = new Size(standardColWidth, controlHeight);

            // ROW 1 - Tax Rate
            TaxRatelbl.Location = new Point(col4X, row1LabelY);
            TaxRatelbl.AutoSize = true;
            TaxRatebox.Location = new Point(col4X, row1ControlY);
            TaxRatebox.Size = new Size(standardColWidth, controlHeight);

            // ROW 1 - Discount %
            itemDiscountlbl.Location = new Point(col5X, row1LabelY);
            itemDiscountlbl.AutoSize = true;
            itemDiscountPercent.Location = new Point(col5X, row1ControlY);
            itemDiscountPercent.Size = new Size(discountPercentWidth, controlHeight);

            // ROW 1 - Discount Rs.
            label4.Location = new Point(col6X, row1LabelY);
            label4.AutoSize = true;
            itemDiscountAmount.Location = new Point(col6X, row1ControlY);
            itemDiscountAmount.Size = new Size(discountAmountWidth, controlHeight);

            // Calculate widths for Row 2 (5 columns - last one spans 2)
            int row2Col1Width = (int)(availableWidth * 0.20);
            int row2Col2Width = (int)(availableWidth * 0.20);
            int row2Col3Width = (int)(availableWidth * 0.20);
            int row2Col4Width = (int)(availableWidth * 0.20);
            int row2Col5Width = (int)(availableWidth * 0.20); // Tax Charged spans remaining

            int row2Col1X = padding;
            int row2Col2X = row2Col1X + row2Col1Width + spacing;
            int row2Col3X = row2Col2X + row2Col2Width + spacing;
            int row2Col4X = row2Col3X + row2Col3Width + spacing;
            int row2Col5X = row2Col4X + row2Col4Width + spacing;

            // ROW 2 - Item Name
            ItemNamelbl.Location = new Point(row2Col1X, row2LabelY);
            ItemNamelbl.AutoSize = true;
            ItemName.Location = new Point(row2Col1X, row2ControlY);
            ItemName.Size = new Size(row2Col1Width, controlHeight);

            // ROW 2 - Quantity
            lblSellerAddress.Location = new Point(row2Col2X, row2LabelY);
            lblSellerAddress.AutoSize = true;
            qty.Location = new Point(row2Col2X, row2ControlY);
            qty.Size = new Size(row2Col2Width, controlHeight);

            // ROW 2 - Sale Value
            salevaluelbl.Location = new Point(row2Col3X, row2LabelY);
            salevaluelbl.AutoSize = true;
            salevalue.Location = new Point(row2Col3X, row2ControlY);
            salevalue.Size = new Size(row2Col3Width, controlHeight);

            // ROW 2 - Further Tax
            FurtureTaxlbl.Location = new Point(row2Col4X, row2LabelY);
            FurtureTaxlbl.AutoSize = true;
            FurtureTax.Location = new Point(row2Col4X, row2ControlY);
            FurtureTax.Size = new Size(row2Col4Width, controlHeight);

            // ROW 2 - Tax Charged (spans remaining width)
            TaxChargedlbl.Location = new Point(row2Col5X, row2LabelY);
            TaxChargedlbl.AutoSize = true;
            TaxCharged.Location = new Point(row2Col5X, row2ControlY);
            int remainingWidth = pnlBasicInfo.Width - row2Col5X - padding;
            TaxCharged.Size = new Size(remainingWidth, controlHeight);
        }

        private void Panel1_Resize(object sender, EventArgs e)
        {
            if (panel1.Width == 0 || panel1.Height == 0) return;

            // 7 equal columns with padding
            int padding = 20;
            int spacing = 8;
            int availableWidth = panel1.Width - (padding * 2) - (spacing * 6);
            int colWidth = availableWidth / 7;

            int labelY = 12;
            int controlY = 36;
            int controlHeight = 24;

            // Column positions
            int col1X = padding;
            int col2X = col1X + colWidth + spacing;
            int col3X = col2X + colWidth + spacing;
            int col4X = col3X + colWidth + spacing;
            int col5X = col4X + colWidth + spacing;
            int col6X = col5X + colWidth + spacing;
            int col7X = col6X + colWidth + spacing;

            // POS ID
            label15.Location = new Point(col1X, labelY);
            label15.AutoSize = true;
            posid.Location = new Point(col1X, controlY);
            posid.Size = new Size(colWidth, controlHeight);

            // Total Quantity
            TotalQuantitylbl.Location = new Point(col2X, labelY);
            TotalQuantitylbl.AutoSize = true;
            TotalQuantity.Location = new Point(col2X, controlY);
            TotalQuantity.Size = new Size(colWidth, controlHeight);

            // Total Sale Value
            TotalSaleValuelbl.Location = new Point(col3X, labelY);
            TotalSaleValuelbl.AutoSize = true;
            TotalSaleValue.Location = new Point(col3X, controlY);
            TotalSaleValue.Size = new Size(colWidth, controlHeight);

            // Total Tax Charged
            TotalTaxChargedlbl.Location = new Point(col4X, labelY);
            TotalTaxChargedlbl.AutoSize = true;
            TotalTaxCharged.Location = new Point(col4X, controlY);
            TotalTaxCharged.Size = new Size(colWidth, controlHeight);

            // Discount
            Discountlbl.Location = new Point(col5X, labelY);
            Discountlbl.AutoSize = true;
            Discount.Location = new Point(col5X, controlY);
            Discount.Size = new Size(colWidth, controlHeight);

            // Further Tax
            totalFurtherTaxlbl.Location = new Point(col6X, labelY);
            totalFurtherTaxlbl.AutoSize = true;
            TotalFurtherTax.Location = new Point(col6X, controlY);
            TotalFurtherTax.Size = new Size(colWidth, controlHeight);

            // Total Bill Amount
            TotalBillAmountlbl.Location = new Point(col7X, labelY);
            TotalBillAmountlbl.AutoSize = true;
            TotalBillAmount.Location = new Point(col7X, controlY);
            TotalBillAmount.Size = new Size(colWidth, controlHeight);
        }

        #endregion

        #region NumericOnly_KeyPress
        private void NumericOnlyWithLength_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is not TextBox tb) return;

            switch (tb.Name)
            {
                case "buyerntn":
                    // Alphanumeric only
                    if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
                    {
                        e.Handled = true;
                        return;
                    }
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 7)
                        e.Handled = true;
                    break;

                case "buyercnic":
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                        return;
                    }
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 13)
                        e.Handled = true;
                    break;

                case "BuyerBname":
                    if (tb.Text.Length >= 350)
                        e.Handled = true;
                    break;

                case "buyerphone":
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                        return;
                    }
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 13)
                        e.Handled = true;
                    break;

                case "itemcode":
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 35)
                        e.Handled = true;
                    break;

                case "pctcode":
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 35)
                        e.Handled = true;
                    break;

                case "quantity":
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    {
                        e.Handled = true;
                        return;
                    }
                    if (e.KeyChar == '.' && tb.Text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 10)
                        e.Handled = true;
                    break;

                case "totalamount":
                case "salevalue":
                case "taxrate":
                case "discount":
                case "furthertax":
                case "taxcharged":
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    {
                        e.Handled = true;
                        return;
                    }
                    if (e.KeyChar == '.' && tb.Text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 15)
                        e.Handled = true;
                    break;
                case "itemdiscountpercent":
                    // Allow digits, decimal point, and control characters
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    {
                        e.Handled = true;
                        return;
                    }
                    // Only one decimal point allowed
                    if (e.KeyChar == '.' && tb.Text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }
                    // Limit percentage to 100
                    if (!char.IsControl(e.KeyChar))
                    {
                        string futureText = tb.Text.Insert(tb.SelectionStart, e.KeyChar.ToString());
                        if (decimal.TryParse(futureText, out decimal val) && val > 100)
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                    // Max 6 characters (e.g., "100.00")
                    if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 6)
                        e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Invoice Type Logic

        private void Invoicetype_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = GetSelectedInvoiceType();

            if (type == 3 || type == 4) // Debit or Credit
            {
                refUSIN.ReadOnly = false;
                refUSIN.BackColor = SystemColors.Window;
                refUSIN.ForeColor = SystemColors.ControlText;
            }
            else
            {
                refUSIN.ReadOnly = true;
                refUSIN.Clear();
                refUSIN.BackColor = SystemColors.Control;
                refUSIN.ForeColor = SystemColors.GrayText;
            }
        }

        #endregion

        #region Grid / Item Helpers

        private void InitializeEmptyGrid()
        {
            dataGridView1.Rows.Clear();
            ItemCode.Focus();
        }

        private async Task CreateLog(string message, string type)
        {
            var log = new Logs
            {
                Message = message,
                Type = type,
            };

            await _logService.CreateLogAsync(log);
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            var deleteItem = new ToolStripMenuItem("Delete Item");
            deleteItem.Click += (s, e) => RemoveSelectedItem();
            contextMenu.Items.Add(deleteItem);

            dataGridView1.ContextMenuStrip = contextMenu;
        }

        private InvoiceItems GetTextboxData()
        {
            // Parse with better error handling
            decimal quantity = decimal.TryParse(qty.Text, out var q) ? Math.Max(0, q) : 0m;
            decimal saleValuePerUnit = decimal.TryParse(salevalue.Text, out var sv) ? Math.Max(0, sv) : 0m;
            decimal taxRatePercent = decimal.TryParse(TaxRatebox.Text, out var tr) ? Math.Max(0, tr) : 0m;
            decimal discountPercent = decimal.TryParse(itemDiscountPercent.Text, out var dp) ? Math.Max(0, Math.Min(100, dp)) : 0m;
            decimal furtherTaxPercent = decimal.TryParse(FurtureTax.Text, out var ft) ? Math.Max(0, ft) : 0m;

            // Calculations
            decimal grossAmount = quantity * saleValuePerUnit;
            decimal discountAmount = grossAmount * (discountPercent / 100m);
            decimal amountAfterDiscount = Math.Max(0, grossAmount - discountAmount);

            decimal taxAmount = amountAfterDiscount * (taxRatePercent / 100m);
            decimal furtherTaxAmount = amountAfterDiscount * (furtherTaxPercent / 100m);
            decimal totalAmount = amountAfterDiscount + taxAmount + furtherTaxAmount;

            // Update UI fields
            itemDiscountAmount.Text = discountAmount.ToString("0.00");
            totalamount.Text = totalAmount.ToString("0.00");
            TaxCharged.Text = taxAmount.ToString("0.00");

            return new InvoiceItems
            {
                ItemCode = pctCode.Text.Trim(),        // PCT Code becomes Item Code
                ItemName = ItemName.Text.Trim(),
                PCTCode = ItemCode.Text.Trim(),        // HS Code becomes PCT Code
                Quantity = quantity,
                SaleValue = saleValuePerUnit,
                Discount = discountAmount,             // Store calculated amount
                TaxRate = (double)taxRatePercent,      // Store percentage
                TaxCharged = taxAmount,                // Calculated tax amount
                FurtherTax = furtherTaxAmount,         // Calculated further tax amount
                TotalAmount = totalAmount,
                InvoiceType = GetSelectedInvoiceType(),
                RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim()
            };
        }
        #endregion

        #region Buttons: Proceed (Add/Update), Save, Edit, Remove

        private void BtnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsInvoiceHeaderEmptyForAdding())
                {
                    var res = MessageBox.Show(
                        "Invoice header appears empty. You can add items now and fill header details later before saving. Do you want to continue?",
                        "Invoice Header Empty",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (res == DialogResult.No)
                        return;
                    _ = CreateLog("Invoice header looks incomplete.", AlertType.Info);
                }

                CurrentInvoice = CollectInvoiceData();
                InvoiceItems inputData = GetTextboxData();

                if (!ValidateItemEntry(inputData))
                    return;

                var existingRow = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(r => (r.Cells["colProductCode"].Value?.ToString() ?? "") == inputData.ItemCode);

                if (existingRow != null)
                {
                    var result = MessageBox.Show($"Item Code '{inputData.ItemCode}' already exists. Do you want to update it?",
                        "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        int rowIndex = existingRow.Index;
                        UpdateExistingItem(existingRow, inputData);

                        if (rowIndex < addedItems.Count)
                            addedItems[rowIndex] = inputData;
                    }
                    else
                    {
                        ItemCode.Focus();
                        return;
                    }
                }
                else
                {
                    AddItemToDataGrid(inputData);
                    addedItems.Add(inputData);
                }

                ClearFormFields();
                AlertManager.ShowSuccess($"Item '{inputData.ItemCode}' added/updated successfully.");
                _ = CreateLog("Item added successfully", AlertType.Success);
                ItemCode.Focus();
                UpdateInvoiceTotals();
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error processing item: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (_isSaving)
            {
                AlertManager.ShowInfo("Save operation is already in progress. Please wait...");
                return;
            }

            try
            {
                _isSaving = true;
                btnSave.Enabled = false;
                btnSave.Text = "Saving...";

                if (addedItems == null || !addedItems.Any())
                {
                    AlertManager.ShowError("Please add at least one item before saving the invoice.");
                    _ = CreateLog("Validation Error: Items are not added", AlertType.Error);
                    return;
                }

                if (!AreInvoiceFieldsValid())
                {
                    AlertManager.ShowError("Invoice header is incomplete. Please fill in the invoice header before saving.");
                    _ = CreateLog("Invoice header is incomplete", AlertType.Error);
                    return;
                }

                var confirm = MessageBox.Show(
                    "Are you sure you want to save this invoice?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                CurrentInvoice = CollectInvoiceData();

                var itemDtos = addedItems.Select(item => new InvoiceItemDto
                {
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    PCTCode = item.PCTCode,
                    Quantity = item.Quantity ?? 0m,
                    SaleValue = item.SaleValue ?? 0m,
                    TotalAmount = item.TotalAmount ?? 0m,
                    TaxCharged = item.TaxCharged ?? 0m,
                    TaxRate = item.TaxRate,
                    Discount = item.Discount ?? 0m,
                    FurtherTax = item.FurtherTax ?? 0m,
                    InvoiceType = (byte)GetSelectedInvoiceType(),
                    RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim()
                }).ToList();

                var invoiceDto = new InvoiceDto
                {
                    POSID = int.TryParse(posid.Text, out var bposId) ? bposId : 0,


                    USIN = USIN.Text.Trim(),
                    RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim(),
                    InvoiceType = (byte)GetSelectedInvoiceType(),
                    BuyerNTN = buyerntn.Text.Trim(),
                    BuyerCNIC = buyercnic.Text.Trim(),
                    BuyerName = BuyerBname.Text.Trim(),
                    BuyerPhoneNumber = buyerphone.Text.Trim(),
                    PaymentMode = GetSelectedPaymentMode(),
                    TotalBillAmount = decimal.TryParse(TotalBillAmount.Text, out var billAmt) ? billAmt : itemDtos.Sum(x => x.TotalAmount),
                    TotalQuantity = decimal.TryParse(TotalQuantity.Text, out var qty) ? qty : itemDtos.Sum(x => x.Quantity),
                    TotalSaleValue = decimal.TryParse(TotalSaleValue.Text, out var saleVal) ? saleVal : itemDtos.Sum(x => x.SaleValue * x.Quantity),
                    TotalTaxCharged = decimal.TryParse(TotalTaxCharged.Text, out var taxCharged) ? taxCharged : itemDtos.Sum(x => x.TaxCharged),
                    Discount = decimal.TryParse(Discount.Text, out var discount) ? discount : itemDtos.Sum(x => x.Discount),
                    FurtherTax = decimal.TryParse(TotalFurtherTax.Text, out var furtherTax) ? furtherTax : itemDtos.Sum(x => x.FurtherTax),
                    DateTime = DateTime.Now,
                    InvoiceItemDto = itemDtos
                };

                AlertManager.ShowInfo("Saving invoice...");
                _ = CreateLog("Saving invoice", AlertType.Info);

                InvoiceReport printForm = new InvoiceReport(invoiceDto);
                printForm.ShowDialog();

                addedItems.Clear();
                CurrentInvoice = null;
                _sessionItems.Clear();
                dataGridView1.Rows.Clear();
                ClearInvoiceFields();
                UpdateInvoiceTotals();
                _isSaving = false;
                btnSave.Enabled = true;
                btnSave.Text = "🖨️ Save and Print";

                var output = await _invoiceService.CreateAsync(invoiceDto);
                
                // call invoice print generator
                //output.Data.FBRInvoiceNumber;

                if (output.StatusCode == ApiStatusCode.Success)
                {
                    WindowsLocalAppNotification.Show("Success", output.Message);
                    AlertManager.ShowSuccess(output.Message);
                    _ = CreateLog(output.Message, AlertType.Info);
                }
                else
                {
                    WindowsLocalAppNotification.Show("Error", output.Message);
                    AlertManager.ShowError(output.Message);
                    _ = CreateLog(output.Message, AlertType.Error);
                }
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Error", $"Error saving invoice: {ex.Message}");
                AlertManager.ShowError($"Error saving invoice: {ex.Message}");
                _ = CreateLog("Error saving invoice", AlertType.Error);
            }
            finally
            {
                _isSaving = false;
                btnSave.Enabled = true;
                btnSave.Text = "🖨️ Save and Print";
            }
        }
        private void BtnSave_MouseEnter(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    AlertManager.ShowError("Please select a row to edit.");
                    return;
                }

                var row = dataGridView1.SelectedRows[0];
                string itemCode = row.Cells["colProductCode"].Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(itemCode))
                {
                    AlertManager.ShowError("Item code is missing for this row.");
                    return;
                }

                int itemIndex = addedItems.FindIndex(i => string.Equals(i.ItemCode?.Trim(), itemCode, StringComparison.OrdinalIgnoreCase));

                if (itemIndex < 0)
                {
                    AlertManager.ShowError($"Item with Item Code '{itemCode}' was not found in the current list.");
                    return;
                }

                var item = addedItems[itemIndex];
                LoadItemForEditing(item);
                addedItems.RemoveAt(itemIndex);
                dataGridView1.Rows.Remove(row);
                UpdateSerialNumbers();
                UpdateInvoiceTotals();
                ItemCode.Focus();
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error while trying to edit the item: {ex.Message}");
            }
        }

        private void btn_remove_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            ClearForm(this);
            var encryptedPosId = ConfigurationManager.AppSettings["Username"] ?? "0";
            var decryptedPosId = AesEncryptionHelper.Decrypt(encryptedPosId);
            posid.Text = decryptedPosId;   // show real POSID in UI
        }
        #endregion

        #region search functionality
        // Call this from your click handler (unchanged)
        private async void btnsearch_Click(object sender, EventArgs e)
        {
            var response = await _productCatalogueService.GetProductCatalogue();
            var allItems = response?.Data ?? Enumerable.Empty<ProductCatalogueDto>();

            using (var dlg = CreateRealtimeSearchDialog(allItems.ToList()))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Tag is ProductCatalogueDto selected)
                {
                    DisplayProductInfo(selected);
                }
            }
        }

        private Form CreateRealtimeSearchDialog(List<ProductCatalogueDto> allProducts)
        {
            Color accentTeal = ColorTranslator.FromHtml("#48A787");
            Color accentBlue = ColorTranslator.FromHtml("#197FC2");
            Color shadow = ColorTranslator.FromHtml("#E6E9EE");

            var dialog = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(650, 500),
                BackColor = shadow,
                ShowInTaskbar = false,
                KeyPreview = true
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

            // ----- HEADER -----
            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            header.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    header.ClientRectangle, accentTeal, accentBlue, 0f);
                e.Graphics.FillRectangle(brush, header.ClientRectangle);
            };

            var lblTitle = new Label
            {
                Text = "🔍 Search Products",
                Font = new Font("Segoe UI Semibold", 14F),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = "Type to search by code, name, or HS code",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(220, Color.White),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var btnClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnClose.Click += (s, e) => dialog.Close();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(255, 230, 230);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.White;

            header.SizeChanged += (s, e) =>
            {
                btnClose.Location = new Point(header.Width - btnClose.Width - 16,
                                              (header.Height - btnClose.Height) / 2);
                lblTitle.Location = new Point(20, (header.Height - lblTitle.Height) / 2 - 6);
                lblSubtitle.Location = new Point(22, (header.Height - lblSubtitle.Height) / 2 + 14);
            };

            header.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnClose });

            // ----- CONTENT -----
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 10)
            };

            // Search box
            var inputWrapper = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = ColorTranslator.FromHtml("#F5F7F8"),
                Padding = new Padding(14, 6, 14, 6)
            };
            inputWrapper.Paint += (s, e) =>
            {
                using var p = new Pen(ColorTranslator.FromHtml("#D6E0E0"));
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, inputWrapper.Width - 1, inputWrapper.Height - 1));
            };

            var txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.Black
            };

            txtSearch.GotFocus += (s, e) => inputWrapper.BackColor = Color.White;
            txtSearch.LostFocus += (s, e) => inputWrapper.BackColor = ColorTranslator.FromHtml("#F5F7F8");

            inputWrapper.Controls.Add(txtSearch);

            // Results count label
            var lblResultCount = new Label
            {
                Text = $"Showing all {allProducts.Count} products",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(2, 5, 0, 0)
            };

            // Results ListView with columns
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.None,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = Color.White
            };

            listView.Columns.Add("Product Code", 120, HorizontalAlignment.Left);
            listView.Columns.Add("Product Description", 280, HorizontalAlignment.Left);
            listView.Columns.Add("HS Code", 120, HorizontalAlignment.Left);
            listView.Columns.Add("Tax Rate", 80, HorizontalAlignment.Center);

            // Product detail panel (shown when item selected)
            var detailPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0,
                BackColor = ColorTranslator.FromHtml("#F0F4F8"),
                Padding = new Padding(12, 8, 12, 8),
                Visible = false
            };
            detailPanel.Paint += (s, e) =>
            {
                using var p = new Pen(ColorTranslator.FromHtml("#D6E0E0"));
                e.Graphics.DrawLine(p, 0, 0, detailPanel.Width, 0);
            };

            var lblDetail = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Text = "",
                AutoSize = false
            };
            detailPanel.Controls.Add(lblDetail);

            // Initial population
            var currentResults = new List<ProductCatalogueDto>(allProducts);
            foreach (var p in allProducts)
            {
                var item = new ListViewItem(p.ProductCode?.ToString() ?? "");
                item.SubItems.Add(p.ProductDescription ?? "");
                item.SubItems.Add(p.HSCode ?? "");
                item.SubItems.Add(p.TaxRate?.ToString() ?? "0");
                listView.Items.Add(item);
            }

            // Real-time search on text changed
            txtSearch.TextChanged += (s, e) =>
            {
                string searchText = txtSearch.Text.Trim();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    currentResults = new List<ProductCatalogueDto>(allProducts);
                    listView.Items.Clear();
                    foreach (var p in allProducts)
                    {
                        var item = new ListViewItem(p.ProductCode?.ToString() ?? "");
                        item.SubItems.Add(p.ProductDescription ?? "");
                        item.SubItems.Add(p.HSCode ?? "");
                        item.SubItems.Add(p.TaxRate?.ToString() ?? "0");
                        listView.Items.Add(item);
                    }
                    lblResultCount.Text = $"Showing all {allProducts.Count} products";
                }
                else
                {
                    currentResults = allProducts
                        .Where(p =>
                            (p.ProductCode.HasValue && p.ProductCode.Value.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            || (p.ProductDescription?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            || (p.HSCode?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();

                    listView.Items.Clear();
                    foreach (var p in currentResults)
                    {
                        var item = new ListViewItem(p.ProductCode?.ToString() ?? "");
                        item.SubItems.Add(p.ProductDescription ?? "");
                        item.SubItems.Add(p.HSCode ?? "");
                        item.SubItems.Add(p.TaxRate?.ToString() ?? "0");
                        listView.Items.Add(item);
                    }

                    lblResultCount.Text = currentResults.Count == 1
                        ? "1 product found"
                        : $"{currentResults.Count} products found";

                    lblResultCount.ForeColor = currentResults.Count == 0
                        ? ColorTranslator.FromHtml("#E74C3C")
                        : ColorTranslator.FromHtml("#7F8C8D");
                }

                // Auto-select if only one result
                if (currentResults.Count == 1)
                {
                    listView.Items[0].Selected = true;
                }
            };

            // Show details on selection
            listView.SelectedIndexChanged += (s, e) =>
            {
                if (listView.SelectedIndices.Count > 0 && listView.SelectedIndices[0] < currentResults.Count)
                {
                    var p = currentResults[listView.SelectedIndices[0]];
                    lblDetail.Text = $"📦 Code: {p.ProductCode}   |   🏷️ HS Code: {p.HSCode ?? "N/A"}   |   💰 Tax Rate: {p.TaxRate?.ToString() ?? "0"}%";
                    detailPanel.Height = 60;
                    detailPanel.Visible = true;
                }
                else
                {
                    detailPanel.Height = 0;
                    detailPanel.Visible = false;
                }
            };

            // Double-click to select
            listView.DoubleClick += (s, e) =>
            {
                if (listView.SelectedIndices.Count > 0)
                {
                    dialog.Tag = currentResults[listView.SelectedIndices[0]];
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
            };

            content.Controls.Add(detailPanel);
            content.Controls.Add(listView);
            content.Controls.Add(lblResultCount);
            content.Controls.Add(inputWrapper);

            // ----- FOOTER -----
            var footerFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 12, 16, 12),
                BackColor = Color.White,
                WrapContents = false
            };

            var btnSelect = new Button
            {
                Text = "✓ Select",
                Size = new Size(140, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentTeal,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#5BC0A0");

            var btnCancel = new Button
            {
                Text = "✖ Cancel",
                Size = new Size(110, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#F0F0F0"),
                ForeColor = ColorTranslator.FromHtml("#5A5A5A"),
                Font = new Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#E0E0E0");

            btnCancel.Click += (s, e) => { dialog.DialogResult = DialogResult.Cancel; dialog.Close(); };

            btnSelect.Click += (s, e) =>
            {
                if (listView.SelectedIndices.Count > 0)
                {
                    dialog.Tag = currentResults[listView.SelectedIndices[0]];
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Please select a product first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            // Keyboard shortcuts
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down && listView.Items.Count > 0)
                {
                    listView.Focus();
                    listView.Items[0].Selected = true;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter && currentResults.Count == 1)
                {
                    dialog.Tag = currentResults[0];
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                    e.Handled = true;
                }
            };

            listView.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && listView.SelectedIndices.Count > 0)
                {
                    dialog.Tag = currentResults[listView.SelectedIndices[0]];
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                    e.Handled = true;
                }
            };

            dialog.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    dialog.DialogResult = DialogResult.Cancel;
                    dialog.Close();
                }
            };

            footerFlow.Controls.Add(btnSelect);
            footerFlow.Controls.Add(btnCancel);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(content, 0, 1);
            layout.Controls.Add(footerFlow, 0, 2);

            dialog.Controls.Add(layout);

            dialog.AcceptButton = btnSelect;
            dialog.CancelButton = btnCancel;

            // Focus on textbox when shown
            dialog.Shown += (s, e) => txtSearch.Focus();

            return dialog;
        }

        private void DisplayProductInfo(ProductCatalogueDto product)
        {
            ItemCode.Text = product.ProductCode?.ToString() ?? "";     // PCT Code in pctCode field
            ItemName.Text = product.ProductDescription ?? "";
            pctCode.Text = product.HSCode ?? "";                      // HS Code in ItemCode field

            // Set tax rate percentage from product catalog
            // TaxRate is already a string or nullable decimal - handle both cases
            if (product.TaxRate != null)
            {
                TaxRatebox.Text = product.TaxRate.ToString();
            }
            else
            {
                TaxRatebox.Text = "0";
            }
        }

        #endregion

        #region Grid Edit / Remove Helpers

        private void UpdateExistingItem(DataGridViewRow row, InvoiceItems inputData)
        {
            row.Cells["colProductCode"].Value = inputData.ItemCode ?? "";         // PCT Code
            row.Cells["colProductDescription"].Value = inputData.ItemName ?? "";
            row.Cells["colHSCode"].Value = inputData.PCTCode ?? "";               // HS Code
            row.Cells["colQuantity"].Value = (inputData.Quantity ?? 0m).ToString("0.00");
            row.Cells["colRate"].Value = (inputData.SaleValue ?? 0m).ToString("0.00");
            row.Cells["colDiscount"].Value = (inputData.Discount ?? 0m).ToString("0.00");  // Discount amount

            // Sales value excluding sales tax: (quantity × rate) - discount
            decimal salesValueExcTax = (inputData.Quantity ?? 0) * (inputData.SaleValue ?? 0) - (inputData.Discount ?? 0);
            row.Cells["colSalesValueExcST"].Value = salesValueExcTax.ToString("0.00");

            row.Cells["colTotalValue"].Value = (inputData.TotalAmount ?? 0m).ToString("0.00");
            row.Cells["colSalesTax"].Value = inputData.TaxRate.ToString("0.00");      // Tax rate percentage
            row.Cells["colExtraTax"].Value = (inputData.TaxCharged ?? 0m).ToString("0.00");
            row.Cells["colFutureTax"].Value = (inputData.FurtherTax ?? 0m).ToString("0.00");
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridView1.Rows[e.RowIndex];
                string itemCode = row.Cells["colProductCode"].Value?.ToString()?.Trim();
                var item = addedItems.FirstOrDefault(i => i.ItemCode == itemCode);
                if (item != null)
                {
                    LoadItemForEditing(item);
                    addedItems.Remove(item);
                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                    UpdateSerialNumbers();
                    UpdateInvoiceTotals();
                }
            }
        }

        private void LoadItemForEditing(InvoiceItems item)
        {
            pctCode.Text = item.ItemCode ?? "";        // PCT Code
            ItemName.Text = item.ItemName ?? "";
            ItemCode.Text = item.PCTCode ?? "";        // HS Code
            qty.Text = (item.Quantity ?? 0m).ToString();
            salevalue.Text = (item.SaleValue ?? 0m).ToString();

            // Calculate back discount percentage from stored amount
            decimal grossAmount = (item.Quantity ?? 0) * (item.SaleValue ?? 0);
            if (grossAmount > 0 && item.Discount.HasValue && item.Discount.Value > 0)
            {
                decimal discountPercent = (item.Discount.Value / grossAmount) * 100m;
                itemDiscountPercent.Text = Math.Round(discountPercent, 2).ToString();
                itemDiscountAmount.Text = Math.Round(item.Discount.Value, 2).ToString("0.00");
            }
            else
            {
                itemDiscountPercent.Text = "0";
                itemDiscountAmount.Text = "0.00";
            }

            // Set tax rate percentage in textbox
            TaxRatebox.Text = item.TaxRate.ToString();

            // Calculate back the further tax percentage from stored amount
            decimal afterDiscount = grossAmount - (item.Discount ?? 0);
            if (afterDiscount > 0 && item.FurtherTax.HasValue && item.FurtherTax.Value > 0)
            {
                decimal furtherTaxPercent = (item.FurtherTax.Value / afterDiscount) * 100m;
                FurtureTax.Text = Math.Round(furtherTaxPercent, 2).ToString();
            }
            else
            {
                FurtureTax.Text = "0";
            }

            totalamount.Text = (item.TotalAmount ?? 0m).ToString();
            TaxCharged.Text = (item.TaxCharged ?? 0m).ToString();
        }

        private void ClearFormFields()
        {
            ItemCode.Clear();
            ItemName.Clear();
            pctCode.Clear();
            qty.Clear();
            salevalue.Clear();
            totalamount.Clear();
            TaxRatebox.Clear();
            TaxCharged.Clear();
            itemDiscountPercent.Clear();
            itemDiscountAmount.Clear();
            FurtureTax.Clear();
        }

        private void ClearForm(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox textBox)
                    textBox.Clear();
                else if (c is ComboBox comboBox)
                    comboBox.SelectedIndex = -1;
                else if (c is CheckBox checkBox)
                    checkBox.Checked = false;
                else if (c is RadioButton radioButton)
                    radioButton.Checked = false;
                else if (c is DateTimePicker dateTimePicker)
                    dateTimePicker.Value = DateTime.Now;

                // Recursive call for nested panels or group boxes
                if (c.HasChildren)
                    ClearForm(c);
            }
        }


        private void ClearInvoiceFields()
        {
            USIN.Clear();
            refUSIN.Clear();
            buyerntn.Clear();
            buyercnic.Clear();
            BuyerBname.Clear();
            buyerphone.Clear();
            TotalBillAmount.Clear();
            TotalQuantity.Clear();
            TotalSaleValue.Clear();
            TotalTaxCharged.Clear();
            Discount.Clear();
            TotalFurtherTax.Clear();
        }

        private void RemoveSelectedItem()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to remove the selected item?",
                    "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int selectedIndex = dataGridView1.SelectedRows[0].Index;

                    if (selectedIndex < addedItems.Count)
                        addedItems.RemoveAt(selectedIndex);

                    dataGridView1.Rows.RemoveAt(selectedIndex);
                    UpdateSerialNumbers();
                    UpdateInvoiceTotals();
                }
            }
            else
            {
                AlertManager.ShowError("Please select a row to delete.");
            }
        }

        private void UpdateSerialNumbers()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["colSrNo"].Value = (i + 1).ToString();
            }
        }

        private void AddItemToDataGrid(InvoiceItems item)
        {
            int rowIndex = dataGridView1.Rows.Add();
            var row = dataGridView1.Rows[rowIndex];

            row.Cells["colSrNo"].Value = (rowIndex + 1).ToString();
            row.Cells["colProductCode"].Value = item.ItemCode ?? "";         // PCT Code
            row.Cells["colHSCode"].Value = item.PCTCode ?? "";               // HS Code
            row.Cells["colProductDescription"].Value = item.ItemName ?? "";

            row.Cells["colQuantity"].Value = (item.Quantity ?? 0m).ToString("0.00");
            row.Cells["colRate"].Value = (item.SaleValue ?? 0m).ToString("0.00");
            row.Cells["colDiscount"].Value = (item.Discount ?? 0m).ToString("0.00");  // Discount amount

            // Sales value excluding sales tax: (quantity × rate) - discount
            decimal qty = item.Quantity ?? 0m;
            decimal rate = item.SaleValue ?? 0m;
            decimal discountAmount = item.Discount ?? 0m;
            decimal salesValueExcTax = Math.Round((qty * rate) - discountAmount, 2);
            row.Cells["colSalesValueExcST"].Value = salesValueExcTax.ToString("0.00");

            row.Cells["colTotalValue"].Value = (item.TotalAmount ?? 0m).ToString("0.00");
            row.Cells["colSalesTax"].Value = (item.TaxRate).ToString("0.00");          // Tax rate percentage
            row.Cells["colExtraTax"].Value = (item.TaxCharged ?? 0m).ToString("0.00");
            row.Cells["colFutureTax"].Value = (item.FurtherTax ?? 0m).ToString("0.00");

            row.Cells["colInvoiceType"].Value = GetInvoiceTypeName(GetSelectedInvoiceType());
            row.Cells["colRefUSIN"].Value = item.RefUSIN ?? "";
        }

        #endregion

        #region Helper Methods

        private string GetInvoiceTypeName(byte invoiceType)
        {
            return invoiceType switch
            {
                1 => "Sale",
                2 => "Purchase",
                3 => "Debit",
                4 => "Credit",
                _ => "Unknown"
            };
        }

        #endregion

        #region Calculations

        private void CalculateItemTotals()
        {
            // Parse input values
            decimal quantity = decimal.TryParse(qty.Text, out var q) ? q : 0m;
            decimal saleValuePerUnit = decimal.TryParse(salevalue.Text, out var sv) ? sv : 0m;

            // Get tax rate percentage from textbox
            decimal taxRatePercent = decimal.TryParse(TaxRatebox.Text, out var tr) ? tr : 0m;

            decimal discountPercent = decimal.TryParse(itemDiscountPercent.Text, out var dp) ? dp : 0m;
            decimal furtherTaxPercent = decimal.TryParse(FurtureTax.Text, out var ft) ? ft : 0m;

            // Step 1: Calculate gross sale amount
            decimal grossAmount = quantity * saleValuePerUnit;

            // Step 2: Calculate discount amount from percentage
            decimal discountAmount = grossAmount * (discountPercent / 100m);

            // Update readonly discount amount field
            itemDiscountAmount.Text = Math.Round(discountAmount, 2).ToString("0.00");

            // Step 3: Deduct discount
            decimal amountAfterDiscount = grossAmount - discountAmount;
            if (amountAfterDiscount < 0) amountAfterDiscount = 0;

            // Step 4: Calculate tax (percentage of amount after discount)
            decimal taxAmount = amountAfterDiscount * (taxRatePercent / 100m);

            // Step 5: Calculate further tax (percentage of amount after discount)
            decimal furtherTaxAmount = amountAfterDiscount * (furtherTaxPercent / 100m);

            // Step 6: Calculate final total
            decimal totalAmount = amountAfterDiscount + taxAmount + furtherTaxAmount;

            // Update UI
            totalamount.Text = Math.Round(totalAmount, 2).ToString("0.00");
            TaxCharged.Text = Math.Round(taxAmount, 2).ToString("0.00");
        }

        private void CalculateItemTotals(object sender, EventArgs e)
        {
            CalculateItemTotals();
        }

        private void RecalculateTotals(object sender, EventArgs e)
        {
            CalculateItemTotals();
            UpdateInvoiceTotals();
        }

        private void UpdateInvoiceTotals()
        {
            if (addedItems == null || !addedItems.Any())
            {
                TotalQuantity.Text = "0.00";
                TotalSaleValue.Text = "0.00";
                TotalTaxCharged.Text = "0.00";
                TotalBillAmount.Text = "0.00";
                Discount.Text = "0.00";
                TotalFurtherTax.Text = "0.00";
                return;
            }

            // Calculate totals from items
            decimal totalQty = addedItems.Sum(i => i.Quantity ?? 0m);

            // Total Sale Value = Sum of (quantity × sale value per unit) for all items
            decimal totalGrossSaleValue = addedItems.Sum(i =>
                (i.Quantity ?? 0m) * (i.SaleValue ?? 0m));

            // Total Discount = Sum of all flat discount amounts
            decimal totalDiscount = addedItems.Sum(i => i.Discount ?? 0m);

            // Total Tax Charged = Sum of all calculated tax amounts
            decimal totalTaxCharged = addedItems.Sum(i => i.TaxCharged ?? 0m);

            // Total Further Tax = Sum of all calculated further tax amounts
            decimal totalFurtherTax = addedItems.Sum(i => i.FurtherTax ?? 0m);

            // Total Bill Amount = Sum of all item total amounts
            decimal totalBillAmount = addedItems.Sum(i => i.TotalAmount ?? 0m);

            // Update UI
            TotalQuantity.Text = totalQty.ToString("0.00");
            TotalSaleValue.Text = totalGrossSaleValue.ToString("0.00");
            TotalTaxCharged.Text = totalTaxCharged.ToString("0.00");
            Discount.Text = totalDiscount.ToString("0.00");
            TotalFurtherTax.Text = totalFurtherTax.ToString("0.00");
            TotalBillAmount.Text = totalBillAmount.ToString("0.00");
        }

        #endregion

        #region Helper Methods for ComboBoxes
        private byte GetSelectedInvoiceType()
        {
            if (invoicetype.SelectedItem is KeyValuePair<byte, string> kvp)
                return kvp.Key;

            return 1; // Default to Sale
        }

        private byte GetSelectedPaymentMode()
        {
            if (paymentmode.SelectedItem is KeyValuePair<byte, string> kvp)
                return kvp.Key;

            return 1; // Default to Card
        }

        #endregion

        #region Invoice Header Helpers

        private Invoice CollectInvoiceData()
        {
            return new Invoice
            {
                POSID = int.TryParse(posid.Text, out var posId) ? posId : 0,
                USIN = USIN.Text.Trim(),
                RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim(),
                InvoiceType = (byte)GetSelectedInvoiceType(),
                BuyerNTN = buyerntn.Text.Trim(),
                BuyerCNIC = buyercnic.Text.Trim(),
                BuyerName = BuyerBname.Text.Trim(),
                BuyerPhoneNumber = buyerphone.Text.Trim(),
                PaymentMode = GetSelectedPaymentMode(),
                TotalBillAmount = decimal.TryParse(TotalBillAmount.Text, out var billAmt) ? billAmt : 0m,
                TotalQuantity = decimal.TryParse(TotalQuantity.Text, out var qty) ? qty : 0m,
                TotalSaleValue = decimal.TryParse(TotalSaleValue.Text, out var saleVal) ? saleVal : 0m,
                TotalTaxCharged = decimal.TryParse(TotalTaxCharged.Text, out var taxCharged) ? taxCharged : 0m,
                Discount = decimal.TryParse(Discount.Text, out var discount) ? discount : 0m,
                FurtherTax = decimal.TryParse(TotalFurtherTax.Text, out var furtherTax) ? furtherTax : 0m,
                DateTime = DateTime.Now
            };
        }

        #endregion

        #region validation

        private bool ValidateItemEntry(InvoiceItems inputData)
        {
            // Item Code
            if (string.IsNullOrWhiteSpace(inputData.ItemCode))
            {
                AlertManager.ShowError("Please enter an Item Code.");
                this.BeginInvoke(new Action(() => ItemCode.Focus()));
                return false;
            }

            // Item Name
            if (string.IsNullOrWhiteSpace(inputData.ItemName))
            {
                AlertManager.ShowError("Please enter an Item Name.");
                this.BeginInvoke(new Action(() => ItemName.Focus()));
                return false;
            }

            // Quantity
            if (inputData.Quantity <= 0)
            {
                AlertManager.ShowError("Please enter a valid Quantity greater than 0.");
                this.BeginInvoke(new Action(() => qty.Focus()));
                return false;
            }

            // Sale Value
            if (inputData.SaleValue <= 0)
            {
                AlertManager.ShowError("Please enter a valid Sale Value greater than 0.");
                this.BeginInvoke(new Action(() => salevalue.Focus()));
                return false;
            }

            return true;
        }

        private bool IsInvoiceHeaderEmptyForAdding()
        {
            return string.IsNullOrWhiteSpace(posid.Text)
                && string.IsNullOrWhiteSpace(USIN.Text)
                && string.IsNullOrWhiteSpace(refUSIN.Text)
                && string.IsNullOrWhiteSpace(buyerntn.Text)
                && string.IsNullOrWhiteSpace(buyercnic.Text)
                && string.IsNullOrWhiteSpace(BuyerBname.Text)
                && string.IsNullOrWhiteSpace(buyerphone.Text);
        }

        private bool AreInvoiceFieldsValid()
        {
            if (string.IsNullOrWhiteSpace(posid.Text))
            {
                MessageBox.Show("POS ID is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => posid.Focus()));
                return false;
            }

            if (string.IsNullOrWhiteSpace(buyerntn.Text))
            {
                MessageBox.Show("Buyer NTN is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => buyerntn.Focus()));
                return false;
            }
            else if (!buyerntn.Text.All(char.IsLetterOrDigit) || buyerntn.Text.Length != 7)
            {
                MessageBox.Show("Buyer NTN must be exactly 7 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => buyerntn.Focus()));
                return false;
            }

            if (string.IsNullOrWhiteSpace(BuyerBname.Text))
            {
                MessageBox.Show("Buyer Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => BuyerBname.Focus()));
                return false;
            }

            if (string.IsNullOrWhiteSpace(buyercnic.Text))
            {
                MessageBox.Show("Buyer CNIC is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => buyercnic.Focus()));
                return false;
            }
            else if (!buyercnic.Text.All(char.IsDigit) || buyercnic.Text.Length != 13)
            {
                MessageBox.Show("Buyer CNIC must be exactly 13 digits.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => buyercnic.Focus()));
                return false;
            }

            if (string.IsNullOrWhiteSpace(buyerphone.Text))
            {
                MessageBox.Show("Buyer Phone Number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => buyerphone.Focus()));
                return false;
            }
            else if (!buyerphone.Text.All(char.IsDigit) || !(buyerphone.Text.Length == 11 || buyerphone.Text.Length == 13))
            {
                MessageBox.Show("Buyer Phone Number must be 11 or 13 digits long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new Action(() => buyerphone.Focus()));
                return false;
            }

            // RefUSIN (if visible → must be numeric and 7 digits)
            if (refUSIN.Visible && !string.IsNullOrWhiteSpace(refUSIN.Text))
            {
                if (!refUSIN.Text.All(char.IsDigit) || refUSIN.Text.Length != 7)
                {
                    MessageBox.Show("Ref USIN must be exactly 7 digits.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.BeginInvoke(new Action(() => refUSIN.Focus()));
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Responsive / Rounded Corners

        public void MakeRoundedControl(Control control, int radius)
        {
            if (control == null) return;

            var rect = control.ClientRectangle;
            if (rect.Width == 0 || rect.Height == 0) return;

            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            control.Region = new Region(path);
        }

        private void CaptureOriginalLayout()
        {
            try
            {
                _originalBounds.Clear();
                _originalParentSizes.Clear();
                _originalFonts.Clear();
                _originalClientSize = this.ClientSize;

                foreach (var ctrl in GetAllControls(this))
                {
                    if (!_originalBounds.ContainsKey(ctrl))
                        _originalBounds[ctrl] = ctrl.Bounds;

                    if (!_originalFonts.ContainsKey(ctrl) && ctrl.Font != null)
                        _originalFonts[ctrl] = ctrl.Font;

                    var parent = ctrl.Parent;
                    if (parent != null && !_originalParentSizes.ContainsKey(parent))
                        _originalParentSizes[parent] = parent.ClientSize;
                }

                if (!_originalParentSizes.ContainsKey(this))
                    _originalParentSizes[this] = this.ClientSize;

                _originalLayoutCaptured = true;
            }
            catch
            {
                _originalLayoutCaptured = false;
            }
        }

        private IEnumerable<Control> GetAllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (var child in GetAllControls(c))
                    yield return child;
            }
        }

        private bool IsFormMaximized()
        {
            return this.WindowState == FormWindowState.Maximized;
        }

        private void Item_entry_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized)
            {
                return; // Let WinForms handle normal state
            }

            if (!_originalLayoutCaptured)
            {
                CaptureOriginalLayout();
                if (!_originalLayoutCaptured) return;
            }

            // ✅ FIX: Only apply scaling when maximized
            if (IsFormMaximized())
            {
                double formScaleX = _originalClientSize.Width > 0
                    ? this.ClientSize.Width / (double)_originalClientSize.Width
                    : 1.0;
                double formScaleY = _originalClientSize.Height > 0
                    ? this.ClientSize.Height / (double)_originalClientSize.Height
                    : 1.0;

                // ✅ Use separate X and Y scaling instead of minimum
                foreach (var kv in _originalBounds)
                {
                    var ctrl = kv.Key;
                    var orig = kv.Value;
                    var parent = ctrl.Parent;

                    if (parent == null) continue;

                    // ✅ Skip controls with Dock or Anchor.All (let WinForms handle them)
                    if (ctrl.Dock != DockStyle.None || ctrl.Anchor == (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right))
                    {
                        continue;
                    }

                    if (!_originalParentSizes.TryGetValue(parent, out Size parentOrigSize)
                        || parentOrigSize.Width == 0 || parentOrigSize.Height == 0)
                        continue;

                    var parentCurrentSize = parent.ClientSize;
                    double scaleX = parentCurrentSize.Width / (double)parentOrigSize.Width;
                    double scaleY = parentCurrentSize.Height / (double)parentOrigSize.Height;

                    int newX = (int)Math.Round(orig.X * scaleX);
                    int newY = (int)Math.Round(orig.Y * scaleY);
                    int newW = Math.Max(orig.Width, (int)Math.Round(orig.Width * scaleX)); // ✅ Don't shrink below original
                    int newH = Math.Max(orig.Height, (int)Math.Round(orig.Height * scaleY));

                    try
                    {
                        ctrl.SetBounds(newX, newY, newW, newH, BoundsSpecified.All);
                    }
                    catch { }

                    // ✅ FIX: Use more conservative font scaling
                    try
                    {
                        if (_originalFonts.TryGetValue(ctrl, out Font origFont) && origFont != null)
                        {
                            // Use X scale only for fonts, and limit the range
                            float scaleFactor = (float)Math.Min(formScaleX, 1.5); // Cap at 150%
                            float newFontSize = Math.Max(8f, Math.Min(24f, origFont.Size * scaleFactor));

                            if (Math.Abs(ctrl.Font.Size - newFontSize) > 0.5f) // Only update if changed significantly
                            {
                                ctrl.Font = new Font(origFont.FontFamily, newFontSize, origFont.Style);
                            }
                        }
                    }
                    catch { }
                }
            }
            // ✅ When not maximized, don't restore - let WinForms Anchor/Dock handle it
        }
        #endregion

        #region datagrid style

        private void StyleDataGridView(DataGridView dgv)
        {
            // General settings
            dgv.BorderStyle = BorderStyle.None;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(226, 232, 240);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // ✅ Prevent selecting headers
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 51, 51);
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            dgv.RowHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            // Column header style
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 51, 51);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 52;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Cell style (adjusted)
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgv.DefaultCellStyle.Padding = new Padding(12, 6, 12, 6);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Adjust row height so text fits nicely
            dgv.RowTemplate.Height = dgv.DefaultCellStyle.Font.Height + dgv.DefaultCellStyle.Padding.Vertical + 12;
        }

        private void StyleProductDataGridView()
        {
            // Clear existing columns
            dataGridView1.Columns.Clear();

            // Apply your base style
            StyleDataGridView(dataGridView1);

            // Define product grid columns
            var colSrNo = new DataGridViewTextBoxColumn
            {
                Name = "colSrNo",
                HeaderText = "Sr. No.",
                Width = 80,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9F)
                }
            };

            var colProductCode = new DataGridViewTextBoxColumn
            {
                Name = "colProductCode",
                HeaderText = "Item Code",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            var colHSCode = new DataGridViewTextBoxColumn
            {
                Name = "colHSCode",
                HeaderText = "HS Code",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            var colProductDescription = new DataGridViewTextBoxColumn
            {
                Name = "colProductDescription",
                HeaderText = "Item Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            var colQuantity = new DataGridViewTextBoxColumn
            {
                Name = "colQuantity",
                HeaderText = "Quantity",
                Width = 100,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colRate = new DataGridViewTextBoxColumn
            {
                Name = "colRate",
                HeaderText = "Rate",
                Width = 100,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colDiscount = new DataGridViewTextBoxColumn
            {
                Name = "colDiscount",
                HeaderText = "Discount (Amt)",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colSalesValueExcST = new DataGridViewTextBoxColumn
            {
                Name = "colSalesValueExcST",
                HeaderText = "Sales Value (exc ST)",
                Width = 140,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colTotalValue = new DataGridViewTextBoxColumn
            {
                Name = "colTotalValue",
                HeaderText = "Total Value",
                Width = 140,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colSalesTax = new DataGridViewTextBoxColumn
            {
                Name = "colSalesTax",
                HeaderText = "Tax Rate (%)",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colExtraTax = new DataGridViewTextBoxColumn
            {
                Name = "colExtraTax",
                HeaderText = "Tax Charged",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colFutureTax = new DataGridViewTextBoxColumn
            {
                Name = "colFutureTax",
                HeaderText = "Further Tax",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00"
                }
            };

            var colInvoiceType = new DataGridViewTextBoxColumn
            {
                Name = "colInvoiceType",
                HeaderText = "Inv Type",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            var colRefUSIN = new DataGridViewTextBoxColumn
            {
                Name = "colRefUSIN",
                HeaderText = "Ref USIN",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            // Add all columns
            dataGridView1.Columns.AddRange(new DataGridViewColumn[]
            {
        colSrNo, colProductCode, colHSCode, colProductDescription, colQuantity,
        colRate, colDiscount, colSalesValueExcST, colTotalValue,
        colSalesTax, colExtraTax, colFutureTax, colInvoiceType, colRefUSIN
            });
        }
        #endregion

        #region Control Helper
        private void SetButtonImage(Button btn, Image img, Color bgColor)
        {
            if (btn == null) return;

            // Set background color
            btn.BackColor = bgColor;

            if (img == null)
            {
                btn.Image = null;
                return;
            }

            // Dispose previous image to prevent memory leaks
            if (btn.Image != null)
            {
                btn.Image.Dispose();
                btn.Image = null;
            }

            // Calculate maximum size to fit inside button, leaving some padding
            int padding = 8;
            int maxWidth = btn.Width - padding;
            int maxHeight = btn.Height - padding;

            // Calculate scaled size while keeping aspect ratio
            double ratioX = (double)maxWidth / img.Width;
            double ratioY = (double)maxHeight / img.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);

            // Resize the image
            Image resized = new Bitmap(img, new Size(newWidth, newHeight));

            // Apply image to button
            btn.Image = resized;
            btn.ImageAlign = ContentAlignment.MiddleCenter; // center
            btn.Text = ""; // remove text if needed
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackgroundImageLayout = ImageLayout.None;
        }

        #endregion

    }
}