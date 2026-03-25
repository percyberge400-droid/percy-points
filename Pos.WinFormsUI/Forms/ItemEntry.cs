using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Application.Services.InvoiceService;
using Pos.Application.Services.LogService;
using Pos.Application.Services.ProductCatalogService;
using Pos.Application.Services.ReferenceService.InvoiceTypeService;
using Pos.Application.Services.ReferenceService.PaymentService;
using Pos.Application.Services.ReferenceService.ServicesRenderedService;
using Pos.Application.Utility;
using Pos.Domain.Entities;
using Pos.SecurityEncryption;
using Pos.WinFormsUI.AlertClasses;
using Pos.WinFormsUI.Forms;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace Pos.WinFormsUI
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

        // Reference
        private readonly IInvoiceTypeService _invoiceTypeService;
        private readonly IPaymentService _paymentService;
        private readonly IServicesRenderedService _servicesRenderedService;

        private readonly HttpClient _httpClient;

        private ProgressBar progressBar;
        private int _isLoadingFlag = 0;

        #endregion

        #region Constructor / Initialization

        public ItemEntry(
            IHttpClientFactory httpClientFactory,
            ILogService logService,
            IProductCatalogueService productCatalogueService,
            IInvoiceService invoiceService,
            IInvoiceTypeService invoiceTypeService,
            IPaymentService paymentService,
            IServicesRenderedService servicesRenderedService)
        {
            InitializeComponent();
            InitializeProgressBar();
            this.Load += item_entry_Load;

            this.Shown += (s, e) =>
            {
                if (!_originalLayoutCaptured)
                {
                    CaptureOriginalLayout();
                }
            };


            this.Load += (s, e) => CenterProgressBar();
            this.Resize += (s, e) => CenterProgressBar();
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

            buyercnic.KeyPress += NumericOnlyWithLength_KeyPress;
            buyercnic.TextChanged += NumericOnlyWithLength_TextChanged;

            buyerntn.KeyPress += NumericOnlyWithLength_KeyPress;
            buyerntn.TextChanged += NumericOnlyWithLength_TextChanged;


            buyerphone.KeyPress += NumericOnlyWithLength_KeyPress;
            buyerphone.TextChanged += NumericOnlyWithLength_TextChanged;

            ItemCode.KeyPress += StringOnlyWithLength_KeyPress;
            ItemCode.TextChanged += StringOnlyWithLength_TextChanged;

            pctCode.KeyPress += NumericOnlyWithLength_KeyPress;
            pctCode.TextChanged += NumericOnlyWithLength_TextChanged;

            TaxRatebox.KeyPress += NumericOnlyWithLength_KeyPress;
            TaxRatebox.TextChanged += NumericOnlyWithLength_TextChanged;

            qty.KeyPress += NumericOnlyWithLength_KeyPress;
            qty.TextChanged += NumericOnlyWithLength_TextChanged;

            salevalue.KeyPress += NumericOnlyWithLength_KeyPress;
            salevalue.TextChanged += NumericOnlyWithLength_TextChanged;

            itemDiscountPercent.KeyPress += NumericOnlyWithLength_KeyPress;
            itemDiscountPercent.TextChanged += NumericOnlyWithLength_TextChanged;

            BuyerBname.KeyPress += StringOnlyWithLength_KeyPress;
            BuyerBname.TextChanged += StringOnlyWithLength_TextChanged;

            USIN.KeyPress += StringOnlyWithLength_KeyPress;
            USIN.TextChanged += StringOnlyWithLength_TextChanged;

            refUSIN.KeyPress += StringOnlyWithLength_KeyPress;
            refUSIN.TextChanged += StringOnlyWithLength_TextChanged;

            itemDiscountPercent.TextChanged += RecalculateTotals;

            invoicetype.SelectedIndexChanged += Invoicetype_SelectedIndexChanged;

            SetupContextMenu();
            CaptureOriginalLayout();
            CaptureOriginalLayout();
            InitializeEmptyGrid();

            StyleProductDataGridView();
            //SetButtonImage(btnsearch, Resources.search, Color.Black);

            _logService = logService;
            _productCatalogueService = productCatalogueService;
            _invoiceService = invoiceService;
            _httpClient = new HttpClient();
            _invoiceTypeService = invoiceTypeService;
            _invoiceTypeService = invoiceTypeService;
            _paymentService = paymentService;
            _servicesRenderedService = servicesRenderedService;
        }

        #endregion

        #region Progress Bar Setup

        private void InitializeProgressBar()
        {
            progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Size = new Size(300, 30),
                Visible = false
            };

            this.Controls.Add(progressBar);
            progressBar.BringToFront();

            CenterProgressBar();
        }

        private void CenterProgressBar()
        {
            if (progressBar != null && dataGridView1 != null)
            {
                var gridBounds = dataGridView1.Bounds;
                progressBar.Left = gridBounds.Left + (gridBounds.Width - progressBar.Width) / 2;
                progressBar.Top = gridBounds.Top + (gridBounds.Height - progressBar.Height) / 2;
                progressBar.BringToFront();
            }
        }

        private async Task RunSingleLoad(Func<Task> work)
        {
            if (Interlocked.Exchange(ref _isLoadingFlag, 1) == 1) return;

            try
            {
                if (progressBar != null)
                {
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                    progressBar.Value = 0;
                    progressBar.Update();
                }

                await work();
            }
            finally
            {
                if (progressBar != null)
                    progressBar.Visible = false;

                Interlocked.Exchange(ref _isLoadingFlag, 0);
            }
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

            // Calculate available width
            int availableWidth = pnlBasicInfo.Width - (padding * 2);

            // ROW 1: Sale Type | Item Code | Item Name | PCT Code | Total Amount (5 equal columns)
            int row1Spacing = (spacing * 4); // 4 gaps between 5 columns
            int row1ColWidth = (availableWidth - row1Spacing) / 5;

            int row1Col1X = padding;
            int row1Col2X = row1Col1X + row1ColWidth + spacing;
            int row1Col3X = row1Col2X + row1ColWidth + spacing;
            int row1Col4X = row1Col3X + row1ColWidth + spacing;
            int row1Col5X = row1Col4X + row1ColWidth + spacing;

            // ROW 1 - Sale Type (assuming this is the combobox shown as "New" in the image)
            // Note: You'll need to verify the control name for Sale Type
            // For now using FurtureTax controls as placeholder - replace with actual Sale Type controls
            FurtureTaxlbl.Location = new Point(row1Col1X, row1LabelY);
            FurtureTaxlbl.AutoSize = true;
            FurtureTax.Location = new Point(row1Col1X, row1ControlY);
            FurtureTax.Size = new Size(row1ColWidth, controlHeight);

            // ROW 1 - Item Code
            ItemCodelbl.Location = new Point(row1Col2X, row1LabelY);
            ItemCodelbl.AutoSize = true;
            ItemCode.Location = new Point(row1Col2X, row1ControlY);
            ItemCode.Size = new Size(row1ColWidth, controlHeight);

            // ROW 1 - Item Name
            ItemNamelbl.Location = new Point(row1Col3X, row1LabelY);
            ItemNamelbl.AutoSize = true;
            ItemName.Location = new Point(row1Col3X, row1ControlY);
            ItemName.Size = new Size(row1ColWidth, controlHeight);

            // ROW 1 - PCT Code
            lblCustomerRegType.Location = new Point(row1Col4X, row1LabelY);
            lblCustomerRegType.AutoSize = true;
            pctCode.Location = new Point(row1Col4X, row1ControlY);
            pctCode.Size = new Size(row1ColWidth, controlHeight);

            // ROW 1 - Unit Price
            salevaluelbl.Location = new Point(row1Col5X, row1LabelY);
            salevaluelbl.AutoSize = true;
            salevalue.Location = new Point(row1Col5X, row1ControlY);
            salevalue.Size = new Size(row1ColWidth, controlHeight);

            // ROW 2: Sale Value | Quantity | Tax Rate | Discount% | Discount Rs | Tax Charged
            // Calculate widths - 6 columns total
            int row2Spacing = (spacing * 5); // 5 gaps between 6 columns
            int row2StandardWidth = (int)((availableWidth - row2Spacing) * 0.18);  // 18% for first 3
            int discountPercentWidth = (int)((availableWidth - row2Spacing) * 0.12); // 12% for discount %
            int discountAmountWidth = (int)((availableWidth - row2Spacing) * 0.16); // 16% for discount Rs
            int taxChargedWidth = (int)((availableWidth - row2Spacing) * 0.18);     // 18% for tax charged

            int row2Col1X = padding;
            int row2Col2X = row2Col1X + row2StandardWidth + spacing;
            int row2Col3X = row2Col2X + row2StandardWidth + spacing;
            int row2Col4X = row2Col3X + row2StandardWidth + spacing;
            int row2Col5X = row2Col4X + discountPercentWidth + spacing;
            int row2Col6X = row2Col5X + discountAmountWidth + spacing;

            // ROW 2 - Quantity
            lblSellerAddress.Location = new Point(row2Col1X, row2LabelY);
            lblSellerAddress.AutoSize = true;
            qty.Location = new Point(row2Col1X, row2ControlY);
            qty.Size = new Size(row2StandardWidth, controlHeight);

            // ROW 2 - Tax Rate
            TaxRatelbl.Location = new Point(row2Col2X, row2LabelY);
            TaxRatelbl.AutoSize = true;
            TaxRatebox.Location = new Point(row2Col2X, row2ControlY);
            TaxRatebox.Size = new Size(row2StandardWidth, controlHeight);

            // ROW 2 - Discount (%)
            itemDiscountlbl.Location = new Point(row2Col3X, row2LabelY);
            itemDiscountlbl.AutoSize = true;
            itemDiscountPercent.Location = new Point(row2Col3X, row2ControlY);
            itemDiscountPercent.Size = new Size(row2StandardWidth, controlHeight);

            // ROW 2 - Discount Rs.
            label4.Location = new Point(row2Col4X, row2LabelY);
            label4.AutoSize = true;
            itemDiscountAmount.Location = new Point(row2Col4X, row2ControlY);
            itemDiscountAmount.Size = new Size(discountPercentWidth, controlHeight);

            // ROW 2 - Tax Charged
            TaxChargedlbl.Location = new Point(row2Col5X, row2LabelY);
            TaxChargedlbl.AutoSize = true;
            TaxCharged.Location = new Point(row2Col5X, row2ControlY);
            TaxCharged.Size = new Size(discountAmountWidth, controlHeight);

            // ROW 2 - Tax Charged
            totalamountlbl.Location = new Point(row2Col6X, row2LabelY);
            totalamountlbl.AutoSize = true;
            totalamount.Location = new Point(row2Col6X, row2ControlY);
            int remainingWidth = pnlBasicInfo.Width - row2Col6X - padding;
            totalamount.Size = new Size(Math.Max(remainingWidth, taxChargedWidth), controlHeight);
        }
        private void Panel1_Resize(object sender, EventArgs e)
        {
            if (panel1.Width == 0 || panel1.Height == 0) return;

            // 6 equal columns with padding
            int padding = 20;
            int spacing = 8;
            int availableWidth = panel1.Width - (padding * 2) - (spacing * 5);
            int colWidth = availableWidth / 6;

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

            // Total Bill Amount
            TotalBillAmountlbl.Location = new Point(col6X, labelY);
            TotalBillAmountlbl.AutoSize = true;
            TotalBillAmount.Location = new Point(col6X, controlY);
            TotalBillAmount.Size = new Size(colWidth, controlHeight);
        }

        #endregion

        #region mandatory fields

        private bool HighlightEmptyTextBoxes(params TextBox[] textBoxes)
        {
            bool hasEmpty = false;

            foreach (var tb in textBoxes)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    hasEmpty = true;
                    tb.BackColor = Color.MistyRose;

                    tb.Paint += (s, e) =>
                    {
                        ControlPaint.DrawBorder(e.Graphics, tb.ClientRectangle,
                            Color.Red, 2, ButtonBorderStyle.Solid,
                            Color.Red, 2, ButtonBorderStyle.Solid,
                            Color.Red, 2, ButtonBorderStyle.Solid,
                            Color.Red, 2, ButtonBorderStyle.Solid);
                    };
                }
                else
                {
                    tb.BackColor = Color.White;
                }

                tb.Invalidate();
            }

            return hasEmpty;
        }

        private void ResetTextBoxHighlights(params TextBox[] textBoxes)
        {
            foreach (var tb in textBoxes)
            {
                tb.BackColor = Color.White;
            }
        }

        #endregion


        #region Invoice Type Logic

        private void Invoicetype_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = GetSelectedInvoiceType();

            bool isDebitOrCredit = type.Name.Contains("Debit", StringComparison.OrdinalIgnoreCase)
                                || type.Name.Contains("Credit", StringComparison.OrdinalIgnoreCase);

            if (isDebitOrCredit)
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
            var log = new CreateLogDto
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

        private bool TryMultiplyDecimal(decimal a, decimal b, out decimal result)
        {
            try
            {
                result = a * b;
                return true;
            }
            catch (OverflowException)
            {
                result = 0m;
                return false;
            }
        }

        private bool TryAddDecimal(decimal a, decimal b, out decimal result)
        {
            try
            {
                result = a + b;
                return true;
            }
            catch (OverflowException)
            {
                result = 0m;
                return false;
            }
        }

        private bool TrySubtractDecimal(decimal a, decimal b, out decimal result)
        {
            try
            {
                result = a - b;
                return true;
            }
            catch (OverflowException)
            {
                result = 0m;
                return false;
            }
        }

        private InvoiceItems GetTextboxData()
        {
            // ✅ ADD: Safe limits BEFORE calculation
            const decimal MAX_QTY = 99999999.99m;        // (8,2) limit
            const decimal MAX_UNIT_PRICE = 9999999999.99m; // (10,2) limit
            const decimal MAX_RESULT = 999999999999999999m; // (18,2) limit for results

            decimal quantity = 0m;
            decimal saleValuePerUnit = 0m;
            decimal taxRatePercent = 0m;
            decimal discountPercent = 0m;

            // Parse with NEW limits
            if (decimal.TryParse(qty.Text, out var q))
            {
                if (q < 0 || q > MAX_QTY)
                {
                    AlertManager.ShowError($"Quantity must be between 0 and {MAX_QTY}");
                    qty.Focus();
                    return null;
                }
                quantity = q;
            }

            if (decimal.TryParse(salevalue.Text, out var sv))
            {
                if (sv < 0 || sv > MAX_UNIT_PRICE)
                {
                    AlertManager.ShowError($"Unit price must be between 0 and {MAX_UNIT_PRICE}");
                    salevalue.Focus();
                    return null;
                }
                saleValuePerUnit = sv;
            }

            if (quantity > 0 && saleValuePerUnit > 0)
            {
                if (quantity > MAX_RESULT / saleValuePerUnit)
                {
                    AlertManager.ShowError("Total exceeds limit. Reduce quantity or unit price.");
                    qty.Focus();
                    return null;
                }
            }

            if (decimal.TryParse(TaxRatebox.Text, out var tr))
            {
                taxRatePercent = Math.Clamp(tr, 0m, 100m);
            }

            if (decimal.TryParse(itemDiscountPercent.Text, out var dp))
            {
                discountPercent = Math.Clamp(dp, 0m, 100m);
            }

            // ✅ CORRECT CALCULATION ORDER
            decimal grossAmount = 0m;
            decimal taxAmount = 0m;
            decimal amountAfterTax = 0m;
            decimal discountAmount = 0m;
            decimal totalAmount = 0m;

            // Step 1: Calculate gross sale amount
            if (!TryMultiplyDecimal(quantity, saleValuePerUnit, out grossAmount))
            {
                AlertManager.ShowError("Values too large. Reduce quantity or price");
                return null;
            }

            // Step 2: Calculate tax on GROSS amount (BEFORE discount) ✅
            if (!TryMultiplyDecimal(grossAmount, taxRatePercent / 100m, out taxAmount))
            {
                AlertManager.ShowError("Tax calculation overflow. Reduce values");
                return null;
            }

            // Step 3: Calculate amount AFTER adding tax ✅
            if (!TryAddDecimal(grossAmount, taxAmount, out amountAfterTax))
            {
                AlertManager.ShowError("Total exceeds limit. Reduce values");
                return null;
            }

            // Step 4: Calculate discount on AMOUNT AFTER TAX ✅
            if (!TryMultiplyDecimal(amountAfterTax, discountPercent / 100m, out discountAmount))
            {
                AlertManager.ShowError("Discount calculation overflow");
                return null;
            }

            // Step 5: Calculate final total (after tax, minus discount) ✅
            if (!TrySubtractDecimal(amountAfterTax, discountAmount, out totalAmount))
            {
                totalAmount = 0m;
            }
            totalAmount = Math.Max(0, totalAmount);

            // Update UI with rounded values
            itemDiscountAmount.Text = Math.Round(discountAmount, 2).ToString("0.00");
            TaxCharged.Text = Math.Round(taxAmount, 2).ToString("0.00");
            totalamount.Text = Math.Round(totalAmount, 2).ToString("0.00");

            return new InvoiceItems
            {
                ItemCode = ItemCode.Text.Trim(),
                ItemName = ItemName.Text.Trim(),
                PCTCode = pctCode.Text.Trim(),
                Quantity = quantity,
                SaleValue = saleValuePerUnit,
                Discount = discountAmount,  // ✅ Discount in Rs. (from after-tax amount)
                TaxRate = (double)taxRatePercent,
                TaxCharged = taxAmount,     // ✅ Tax calculated on gross (before discount)
                FurtherTax = GetSelectedSaleType().Id,
                TotalAmount = totalAmount,  // ✅ (Gross + Tax) - Discount
                InvoiceType = GetSelectedInvoiceType().Id,
                RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim()
            };
        }
        #endregion

        #region Buttons: Proceed (Add/Update), Save, Edit, Remove

        private void BtnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentInvoice = CollectInvoiceData();
                InvoiceItems inputData = GetTextboxData();

                // Check if calculation failed
                if (inputData == null)
                {
                    // Error already shown in GetTextboxData()
                    return;
                }

                if (!ValidateItemEntry(inputData))
                {
                    HighlightEmptyTextBoxes(pctCode, USIN, ItemCode, ItemName, qty, salevalue);
                    return;
                }

                AddItemToDataGrid(inputData);
                addedItems.Add(inputData);

                ClearFormFields();
                AlertManager.ShowSuccess($"Item '{inputData.ItemCode}' added/updated successfully.");
                ResetTextBoxHighlights(pctCode, ItemCode, ItemName, qty, salevalue);
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

            if (string.IsNullOrWhiteSpace(USIN.Text))
            {
                AlertManager.ShowError("USIN is required.");
                HighlightEmptyTextBoxes(USIN);
                this.BeginInvoke(new Action(() => USIN.Focus()));
                return;
            }

            if (!AreInvoiceFieldsValid())
            {
                return;
            }

            ResetTextBoxHighlights(USIN);

            _isSaving = true;
            btnSave.Enabled = false;
            btnSave.Text = "Saving...";
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            var progressTaskCts = new CancellationTokenSource();

            try
            {
                // Smooth progress animation while save + print run
                var progressTask = Task.Run(async () =>
                {
                    while (!progressTaskCts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(80);
                        this.Invoke(new Action(() =>
                        {
                            progressBar.Value = (progressBar.Value + 1) % 100;
                        }));
                    }
                }, progressTaskCts.Token);

                // Validate
                if (addedItems == null || !addedItems.Any())
                {
                    AlertManager.ShowError("Please add at least one item before saving the invoice.");
                    ResetUI(progressTaskCts);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to save and print this invoice?",
                                    "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    ResetUI(progressTaskCts);
                    return;
                }

                // Build DTOs
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
                    InvoiceType = GetSelectedInvoiceType().Id,
                    RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim()
                }).ToList();

                var invoiceDto = new InvoiceDto
                {
                    POSID = int.TryParse(posid.Text, out var posId) ? posId : 0,
                    USIN = USIN.Text.Trim(),
                    RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim(),
                    InvoiceType = GetSelectedInvoiceType().Id,
                    BuyerPNTN = buyerntn.Text.Trim(),
                    BuyerCNIC = buyercnic.Text.Trim(),
                    BuyerName = BuyerBname.Text.Trim(),
                    BuyerPhoneNumber = buyerphone.Text.Trim(),
                    PaymentMode = GetSelectedPaymentMode().Id,
                    TotalBillAmount = decimal.TryParse(TotalBillAmount.Text, out var billAmt) ? billAmt : itemDtos.Sum(x => x.TotalAmount),
                    TotalQuantity = decimal.TryParse(TotalQuantity.Text, out var qty) ? qty : itemDtos.Sum(x => x.Quantity),
                    TotalSaleValue = decimal.TryParse(TotalSaleValue.Text, out var saleVal) ? saleVal : itemDtos.Sum(x => x.SaleValue * x.Quantity),
                    TotalTaxCharged = decimal.TryParse(TotalTaxCharged.Text, out var taxCharged) ? taxCharged : itemDtos.Sum(x => x.TaxCharged),
                    Discount = decimal.TryParse(Discount.Text, out var discount) ? discount : itemDtos.Sum(x => x.Discount),
                    FurtherTax = (byte)GetSelectedSaleType().Id,
                    DateTime = DateTime.Now,
                    Items = itemDtos
                };

                // Save + print coordination
                try
                {
                    //var _baseUrl = ConfigurationManager.AppSettings["SelfHostUrl"] ?? "";

                    //var url = $"{_baseUrl}{Endpoints.Create}";

                    //HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, invoiceDto);

                    //// Throw if status is not success (4xx or 5xx)
                    //response.EnsureSuccessStatusCode();

                    //var result = await response.Content.ReadFromJsonAsync<ApiResponse<InvoiceDto>>();
                    //if (result == null)
                    //    throw new Exception("Empty or invalid API response.");
                    var env = ConfigurationManager.AppSettings["Environment"];
                    var result = await _invoiceService.CreateAsync(invoiceDto, env);

                    if (result.StatusCode == ApiStatusCode.Success)
                    {
                        // Update invoice DTO with FBR invoice number
                        invoiceDto.InvoiceNumber = result.Data.InvoiceNumber;
                        //invoiceDto.InvoiceNumber = invoiceDto.InvoiceNumber;

                        // Reset after both save & print complete
                        progressTaskCts.Cancel();
                        await Task.Delay(300);

                        progressBar.Visible = false;
                        progressBar.Value = 0;
                        // Clear UI early
                        addedItems.Clear();
                        dataGridView1.Rows.Clear();
                        ClearInvoiceFields();
                        btnSave.Enabled = true;
                        btnSave.Text = "🖨️ Save and Print";
                        _isSaving = false;
                    }
                    else
                    {
                        AlertManager.ShowError($"Invoice save failed: {result.Message}");
                        ResetUI(progressTaskCts);
                        return;
                    }
                    try
                    {
                        // Print configuration
                        string printerName = InvoiceReport.FindThermalPrinter();
                        bool showDialog = string.IsNullOrWhiteSpace(printerName);
                        //bool showDialog = false; // Or set based on config/user choice

                        if (showDialog)
                        {
                            // ShowDialog blocks by design, run on UI thread
                            InvoiceReport printForm = new InvoiceReport(invoiceDto);
                            printForm.ShowDialog();
                        }
                        else
                        {
                            // Complete background printing - zero UI blocking
                            _ = Task.Run(() =>
                            {
                                try
                                {
                                    // Create form and print entirely in background thread
                                    InvoiceReport printForm = new InvoiceReport(invoiceDto);
                                    printForm.PrintDirectlyToThermal();
                                }
                                catch (Exception ex)
                                {
                                    // Log error without blocking UI
                                    try
                                    {
                                        this.BeginInvoke(new Action(() =>
                                        {
                                            _ = CreateLog($"Print error: {ex.Message}", AlertType.Error);
                                        }));
                                    }
                                    catch { /* Ignore if form is disposed */ }
                                }
                            });
                        }
                    }
                    catch
                    {
                        AlertManager.ShowInfo("Thermal Printer not Found!");
                        return;
                    }
                    finally
                    {
                        AlertManager.ShowSuccess("Invoice saved successfully");
                        _ = CreateLog("invoice saved successfully", AlertType.Success);
                    }

                }
                catch (Exception ex)
                {
                    AlertManager.ShowError($"Error saving invoice: {ex.Message}");
                    return;
                }
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error: {ex.Message}");
                ResetUI(progressTaskCts);
                return;
            }
        }

        private void ResetUI(CancellationTokenSource cts)
        {
            cts.Cancel();
            progressBar.Visible = false;
            progressBar.Value = 0;
            btnSave.Enabled = true;
            btnSave.Text = "🖨️ Save and Print";
            _isSaving = false;
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
                using var brush = new LinearGradientBrush(
                    header.ClientRectangle,
                    ColorTranslator.FromHtml("#1B8C76"),  // top teal
                    ColorTranslator.FromHtml("#125E8A"),  // bottom blue
                    90f  // vertical gradient
                );
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
            // Product detail panel (fixed footer-style)
            var detailPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = ColorTranslator.FromHtml("#F8FAFB"),
                Padding = new Padding(12, 8, 12, 8)
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
                Text = "Select a product to see details...",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
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
                }
                else
                {
                    lblDetail.Text = "Select a product to see details...";
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

            var listContainer = new Panel
            {
                Dock = DockStyle.Fill
            };
            listContainer.Controls.Add(listView);
            listContainer.Controls.Add(detailPanel);

            content.Controls.Add(listContainer);

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
            ResetTextBoxHighlights(pctCode, ItemCode, ItemName, qty, salevalue);

            return dialog;
        }

        private void DisplayProductInfo(ProductCatalogueDto product)
        {
            // Fill basic info
            ItemCode.Text = product.ProductCode?.ToString() ?? "";
            ItemName.Text = product.ProductDescription ?? "";
            pctCode.Text = product.HSCode ?? "";

            // Handle tax rate (remove % if present and ensure numeric)
            string rawTaxRate = product.TaxRate?.ToString() ?? "0";

            if (rawTaxRate.Contains("%"))
                rawTaxRate = rawTaxRate.Replace("%", "").Trim();

            TaxRatebox.Text = rawTaxRate;

            // 🔁 Trigger recalculations or dependent logic after selection
            try
            {
                CalculateItemTotals();
                UpdateInvoiceTotals();
            }
            catch { /* silently skip if events not defined */ }
        }

        #endregion

        #region Grid Edit / Remove Helpers

        private void UpdateExistingItem(DataGridViewRow row, InvoiceItems inputData)
        {
            row.Cells["colSaleType"].Value = GetSaleTypeName();
            row.Cells["colProductCode"].Value = inputData.ItemCode ?? "";
            row.Cells["colProductDescription"].Value = inputData.ItemName ?? "";
            row.Cells["colHSCode"].Value = inputData.PCTCode ?? "";

            decimal qty = inputData.Quantity ?? 0m;
            decimal rate = inputData.SaleValue ?? 0m;
            decimal discountAmount = inputData.Discount ?? 0m;
            decimal taxCharged = inputData.TaxCharged ?? 0m;
            decimal totalAmount = inputData.TotalAmount ?? 0m;

            // Explicitly cast if TaxRate is double
            decimal taxRate = (decimal)inputData.TaxRate;

            // Calculations
            decimal salesValueExcTax = Math.Round((qty * rate) - discountAmount, 2);

            // Assign values
            row.Cells["colUnitPrice"].Value = rate.ToString("0.00");
            row.Cells["colQuantity"].Value = qty.ToString("0.00");
            row.Cells["colTaxRate"].Value = taxRate.ToString("0.00");
            row.Cells["colTaxCharged"].Value = taxCharged.ToString("0.00");
            row.Cells["colDiscount"].Value = discountAmount.ToString("0.00");
            row.Cells["colTotalAmount"].Value = totalAmount.ToString("0.00");
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
            // Safely set text fields
            pctCode.Text = item.PCTCode ?? "";
            ItemName.Text = item.ItemName ?? "";
            ItemCode.Text = item.ItemCode ?? "";
            qty.Text = (item.Quantity ?? 0m).ToString("0.##");
            salevalue.Text = (item.SaleValue ?? 0m).ToString("0.##");

            const decimal SAFE_MAX = 999999999999999999m;

            decimal quantity = Math.Clamp(item.Quantity ?? 0m, 0m, SAFE_MAX);
            decimal saleValuePerUnit = Math.Clamp(item.SaleValue ?? 0m, 0m, SAFE_MAX);
            decimal taxRatePercent = Math.Clamp((decimal)(item.TaxRate), 0m, 100m);

            // Safe calculations
            if (!TryMultiplyDecimal(quantity, saleValuePerUnit, out decimal grossAmount))
            {
                itemDiscountPercent.Text = itemDiscountAmount.Text = TaxCharged.Text = totalamount.Text = "0.00";
                TaxRatebox.Text = Math.Round(taxRatePercent, 2).ToString("0.##");
                return;
            }

            if (!TryMultiplyDecimal(grossAmount, taxRatePercent / 100m, out decimal taxAmount))
            {
                itemDiscountPercent.Text = itemDiscountAmount.Text = TaxCharged.Text = totalamount.Text = "0.00";
                TaxRatebox.Text = Math.Round(taxRatePercent, 2).ToString("0.##");
                return;
            }

            if (!TryAddDecimal(grossAmount, taxAmount, out decimal amountAfterTax))
            {
                itemDiscountPercent.Text = itemDiscountAmount.Text = TaxCharged.Text = totalamount.Text = "0.00";
                TaxRatebox.Text = Math.Round(taxRatePercent, 2).ToString("0.##");
                return;
            }

            decimal discountAmount = Math.Clamp(item.Discount ?? 0m, 0m, amountAfterTax);
            decimal discountPercent = 0m;

            if (discountAmount > 0 && amountAfterTax > 0)
            {
                if (TryMultiplyDecimal(discountAmount / amountAfterTax, 100m, out discountPercent))
                {
                    // Success
                }
                else
                {
                    discountPercent = 0m;
                }
            }

            decimal totalAmount = Math.Max(0, amountAfterTax - discountAmount);

            // Update UI
            itemDiscountPercent.Text = Math.Round(discountPercent, 2).ToString("0.##");
            itemDiscountAmount.Text = Math.Round(discountAmount, 2).ToString("0.00");
            TaxRatebox.Text = Math.Round(taxRatePercent, 2).ToString("0.##");
            TaxCharged.Text = Math.Round(taxAmount, 2).ToString("0.00");
            totalamount.Text = Math.Round(totalAmount, 2).ToString("0.00");
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
        }

        private void ClearForm(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox textBox)
                    textBox.Clear();
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


        #endregion

        #region Calculations

        private void CalculateItemTotals()
        {
            const decimal SAFE_MAX = 999999999999999999m;

            // Parse inputs
            if (!decimal.TryParse(qty.Text, out var quantity) || quantity < 0 || quantity > SAFE_MAX)
            {
                return;
            }

            if (!decimal.TryParse(salevalue.Text, out var saleValuePerUnit) || saleValuePerUnit < 0 || saleValuePerUnit > SAFE_MAX)
            {
                return;
            }

            decimal taxRatePercent = decimal.TryParse(TaxRatebox.Text, out var tr) ? Math.Clamp(tr, 0m, 100m) : 0m;
            decimal discountPercent = decimal.TryParse(itemDiscountPercent.Text, out var dp) ? Math.Clamp(dp, 0m, 100m) : 0m;

            // Safe calculations
            if (!TryMultiplyDecimal(quantity, saleValuePerUnit, out decimal grossAmount))
            {
                itemDiscountAmount.Text = TaxCharged.Text = totalamount.Text = "ERROR";
                return;
            }

            if (!TryMultiplyDecimal(grossAmount, taxRatePercent / 100m, out decimal taxAmount))
            {
                itemDiscountAmount.Text = TaxCharged.Text = totalamount.Text = "ERROR";
                return;
            }

            if (!TryAddDecimal(grossAmount, taxAmount, out decimal amountAfterTax))
            {
                itemDiscountAmount.Text = TaxCharged.Text = totalamount.Text = "ERROR";
                return;
            }

            if (!TryMultiplyDecimal(amountAfterTax, discountPercent / 100m, out decimal discountAmount))
            {
                itemDiscountAmount.Text = TaxCharged.Text = totalamount.Text = "ERROR";
                return;
            }

            decimal totalAmount = Math.Max(0, amountAfterTax - discountAmount);

            // Update UI
            itemDiscountAmount.Text = Math.Round(discountAmount, 2).ToString("0.00");
            TaxCharged.Text = Math.Round(taxAmount, 2).ToString("0.00");
            totalamount.Text = Math.Round(totalAmount, 2).ToString("0.00");
        }


        /// <summary>
        /// Safely multiplies two decimals with overflow protection.
        /// </summary>
        private decimal SafeMultiply(decimal a, decimal b)
        {
            try
            {
                return a * b;
            }
            catch (OverflowException)
            {
                return 9999999999999999m; // cap at max safe value
            }
        }

        /// <summary>
        /// Safely adds two decimals with overflow protection.
        /// </summary>
        private decimal SafeAdd(decimal a, decimal b)
        {
            try
            {
                return a + b;
            }
            catch (OverflowException)
            {
                return 9999999999999999m;
            }
        }

        /// <summary>
        /// Safely subtracts two decimals with overflow protection.
        /// </summary>
        private decimal SafeSubtract(decimal a, decimal b)
        {
            try
            {
                return a - b;
            }
            catch (OverflowException)
            {
                return 0m;
            }
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

            // Total Bill Amount = Sum of all item total amounts
            decimal totalBillAmount = addedItems.Sum(i => i.TotalAmount ?? 0m);

            // Update UI
            TotalQuantity.Text = totalQty.ToString("0.00");
            TotalSaleValue.Text = totalGrossSaleValue.ToString("0.00");
            TotalTaxCharged.Text = totalTaxCharged.ToString("0.00");
            Discount.Text = totalDiscount.ToString("0.00");
            TotalBillAmount.Text = totalBillAmount.ToString("0.00");
        }

        #endregion

        #region Helper Methods for ComboBoxes
        private (byte Id, string Name) GetSelectedInvoiceType()
        {
            if (invoicetype.SelectedItem is KeyValuePair<byte, string> kvp)
                return (kvp.Key, kvp.Value);
            return (0, string.Empty);
        }

        private (byte Id, string Name) GetSelectedPaymentMode()
        {
            if (paymentmode.SelectedItem is KeyValuePair<byte, string> kvp)
                return (kvp.Key, kvp.Value);
            return (0, string.Empty);
        }

        private (byte Id, string Name) GetSelectedSaleType()
        {
            if (FurtureTax.SelectedItem is KeyValuePair<byte, string> kvp)
                return (kvp.Key, kvp.Value);
            return (0, string.Empty);
        }

        private string GetSaleTypeName()
        {
            var selected = GetSelectedSaleType();
            return string.IsNullOrEmpty(selected.Name) ? string.Empty : selected.Name;
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
                InvoiceType = GetSelectedInvoiceType().Id,
                BuyerNTN = buyerntn.Text.Trim(),
                BuyerCNIC = buyercnic.Text.Trim(),
                BuyerName = BuyerBname.Text.Trim(),
                BuyerPhoneNumber = buyerphone.Text.Trim(),
                PaymentMode = GetSelectedPaymentMode().Id,
                TotalBillAmount = decimal.TryParse(TotalBillAmount.Text, out var billAmt) ? billAmt : 0m,
                TotalQuantity = decimal.TryParse(TotalQuantity.Text, out var qty) ? qty : 0m,
                TotalSaleValue = decimal.TryParse(TotalSaleValue.Text, out var saleVal) ? saleVal : 0m,
                TotalTaxCharged = decimal.TryParse(TotalTaxCharged.Text, out var taxCharged) ? taxCharged : 0m,
                Discount = decimal.TryParse(Discount.Text, out var discount) ? discount : 0m,
                DateTime = DateTime.Now
            };
        }

        #endregion


        #region validation

        private bool ValidateItemEntry(InvoiceItems inputData)
        {
            // Item Code - max 50 characters
            if (string.IsNullOrWhiteSpace(inputData.ItemCode))
            {
                AlertManager.ShowError("Please enter the Item Code.");
                this.BeginInvoke(new Action(() => ItemCode.Focus()));
                return false;
            }

            if (inputData.ItemCode.Length > 50)
            {
                AlertManager.ShowError("Item Code cannot exceed 50 characters.");
                this.BeginInvoke(new Action(() => ItemCode.Focus()));
                return false;
            }

            // HS Code - exactly 8 digits
            if (string.IsNullOrWhiteSpace(inputData.PCTCode))
            {
                AlertManager.ShowError("Please enter the HS Code.");
                this.BeginInvoke(new Action(() => pctCode.Focus()));
                return false;
            }

            if (!inputData.PCTCode.All(char.IsDigit) || inputData.PCTCode.Length != 8)
            {
                AlertManager.ShowError("HS Code must be exactly 8 digits.");
                this.BeginInvoke(new Action(() => pctCode.Focus()));
                return false;
            }

            // Item Description - max 150 chars
            if (string.IsNullOrWhiteSpace(inputData.ItemName))
            {
                AlertManager.ShowError("Please enter an Item Description.");
                this.BeginInvoke(new Action(() => ItemName.Focus()));
                return false;
            }

            if (inputData.ItemName.Length > 150)
            {
                AlertManager.ShowError("Item Description cannot exceed 150 characters.");
                this.BeginInvoke(new Action(() => ItemName.Focus()));
                return false;
            }

            // Quantity - must be > 0, max 99,999.99 (5 digits total including decimals)
            if (!inputData.Quantity.HasValue || inputData.Quantity <= 0)
            {
                AlertManager.ShowError("Please enter a valid Quantity greater than 0.");
                this.BeginInvoke(new Action(() => qty.Focus()));
                return false;
            }

            if (inputData.Quantity > 99999.99m)
            {
                AlertManager.ShowError("Quantity cannot exceed 99,999.99");
                this.BeginInvoke(new Action(() => qty.Focus()));
                return false;
            }

            // Unit Price - must be > 0, max 9,999,999,999.99 (10 digits before decimal)
            if (!inputData.SaleValue.HasValue || inputData.SaleValue <= 0)
            {
                AlertManager.ShowError("Please enter a valid Unit Price greater than 0.");
                this.BeginInvoke(new Action(() => salevalue.Focus()));
                return false;
            }

            if (inputData.SaleValue > 9999999999.99m)
            {
                AlertManager.ShowError("Unit Price cannot exceed 9,999,999,999.99");
                this.BeginInvoke(new Action(() => salevalue.Focus()));
                return false;
            }

            // Tax Rate - 0-100%
            if (!decimal.TryParse(TaxRatebox.Text, out decimal taxRate))
            {
                AlertManager.ShowError("Tax Rate must be a valid number.");
                this.BeginInvoke(new Action(() => TaxRatebox.Focus()));
                return false;
            }

            if (taxRate < 0 || taxRate > 100)
            {
                AlertManager.ShowError("Tax Rate must be between 0 and 100.");
                this.BeginInvoke(new Action(() => TaxRatebox.Focus()));
                return false;
            }

            // Discount % - 0-100%
            if (!string.IsNullOrWhiteSpace(itemDiscountPercent.Text))
            {
                if (!decimal.TryParse(itemDiscountPercent.Text, out decimal discountPercent))
                {
                    AlertManager.ShowError("Discount percentage must be a valid number.");
                    this.BeginInvoke(new Action(() => itemDiscountPercent.Focus()));
                    return false;
                }

                if (discountPercent < 0 || discountPercent > 100)
                {
                    AlertManager.ShowError("Discount percentage must be between 0 and 100.");
                    this.BeginInvoke(new Action(() => itemDiscountPercent.Focus()));
                    return false;
                }
            }

            // Tax Charged must be >= 0
            if (inputData.TaxCharged < 0)
            {
                AlertManager.ShowError("Tax Charged must be greater than or equal to 0.");
                this.BeginInvoke(new Action(() => TaxCharged.Focus()));
                return false;
            }

            return true;
        }

        private bool AreInvoiceFieldsValid()
        {
            // POS ID: Required, Numeric, UP TO 6 DIGITS
            if (string.IsNullOrWhiteSpace(posid.Text))
            {
                AlertManager.ShowError("POS ID is required.");
                this.BeginInvoke(new Action(() => posid.Focus()));
                return false;
            }

            if (!posid.Text.All(char.IsDigit))
            {
                AlertManager.ShowError("POS ID must contain only numbers.");
                this.BeginInvoke(new Action(() => posid.Focus()));
                return false;
            }

            if (posid.Text.Length > 6)
            {
                AlertManager.ShowError("POS ID cannot exceed 6 digits.");
                this.BeginInvoke(new Action(() => posid.Focus()));
                return false;
            }

            if (posid.Text == "0" || int.Parse(posid.Text) == 0)
            {
                AlertManager.ShowError("POS ID must be greater than 0.");
                this.BeginInvoke(new Action(() => posid.Focus()));
                return false;
            }

            if (USIN.Text.Length > 50)
            {
                AlertManager.ShowError("USIN cannot exceed 50 characters.");
                this.BeginInvoke(new Action(() => USIN.Focus()));
                return false;
            }
            // Buyer NTN: Optional, 7 Alphanumeric
            if (!string.IsNullOrWhiteSpace(buyerntn.Text))
            {
                if (!buyerntn.Text.All(char.IsLetterOrDigit))
                {
                    AlertManager.ShowError("Buyer NTN must contain only letters and numbers.");
                    this.BeginInvoke(new Action(() => buyerntn.Focus()));
                    return false;
                }

                if (buyerntn.Text.Length < 7)
                {
                    AlertManager.ShowError("Buyer NTN must be 7 alphanumeric characters.");
                    this.BeginInvoke(new Action(() => buyerntn.Focus()));
                    return false;
                }
            }

            // Buyer CNIC: Optional, Exactly 13 Digits
            if (!string.IsNullOrWhiteSpace(buyercnic.Text))
            {
                if (!buyercnic.Text.All(char.IsDigit) || buyercnic.Text.Length != 13)
                {
                    AlertManager.ShowError("Buyer CNIC must be exactly 13 digits.");
                    this.BeginInvoke(new Action(() => buyercnic.Focus()));
                    return false;
                }
            }


            // Buyer Name: Optional, max 150 chars, letters and spaces only
            if (!string.IsNullOrWhiteSpace(BuyerBname.Text))
            {
                if (BuyerBname.Text.Length > 150)
                {
                    AlertManager.ShowError("Buyer Name cannot exceed 150 characters.");
                    this.BeginInvoke(new Action(() => BuyerBname.Focus()));
                    return false;
                }

                if (!BuyerBname.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                {
                    AlertManager.ShowError("Buyer Name can only contain letters and spaces.");
                    this.BeginInvoke(new Action(() => BuyerBname.Focus()));
                    return false;
                }
            }

            // Buyer Phone: Optional, UP TO 20 CHARACTERS (CHANGED FROM 11/13)
            if (!string.IsNullOrWhiteSpace(buyerphone.Text))
            {
                if (buyerphone.Text.Length > 20)
                {
                    AlertManager.ShowError("Buyer Phone cannot exceed 20 characters.");
                    this.BeginInvoke(new Action(() => buyerphone.Focus()));
                    return false;
                }

                // Allow digits, spaces, +, -, (, )
                if (!buyerphone.Text.All(c => char.IsDigit(c) || c == ' ' || c == '+' || c == '-' || c == '(' || c == ')'))
                {
                    AlertManager.ShowError("Buyer Phone contains invalid characters.");
                    this.BeginInvoke(new Action(() => buyerphone.Focus()));
                    return false;
                }
            }

            // Ref USIN validation
            if (refUSIN.Visible && !string.IsNullOrWhiteSpace(refUSIN.Text))
            {
                if (refUSIN.Text.Length > 50)
                {
                    AlertManager.ShowError("Ref USIN cannot exceed 50 characters.");
                    this.BeginInvoke(new Action(() => refUSIN.Focus()));
                    return false;
                }
            }

            return true;
        }

        private void NumericOnlyWithLength_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is not TextBox tb) return;
            string name = tb.Name.ToLower();

            // Allow control characters
            if (char.IsControl(e.KeyChar)) return;

            switch (name)
            {
                case "posid":
                    // ✅ UP TO 6 DIGITS
                    if (!char.IsDigit(e.KeyChar) || tb.Text.Length >= 6)
                        e.Handled = true;
                    break;

                case "buyerntn":
                    if (!char.IsLetterOrDigit(e.KeyChar) || tb.Text.Length >= 7)
                        e.Handled = true;
                    break;

                case "buyercnic":
                    // 13 DIGITS
                    if (!char.IsDigit(e.KeyChar) || tb.Text.Length >= 13)
                        e.Handled = true;
                    break;

                case "buyerphone":
                    // ✅ UP TO 20 CHARACTERS (allows +, -, parentheses)
                    if (tb.Text.Length >= 20)
                    {
                        e.Handled = true;
                    }
                    else if (!char.IsDigit(e.KeyChar) && e.KeyChar != '+' && e.KeyChar != '-' && e.KeyChar != '(' && e.KeyChar != ')')
                    {
                        e.Handled = true;
                    }
                    break;

                case "pctcode":
                    // 8 DIGITS
                    if (!char.IsDigit(e.KeyChar) || tb.Text.Length >= 8)
                        e.Handled = true;
                    break;

                case "qty":
                    // 5 digits before decimal, 2 after
                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    {
                        e.Handled = true;
                        return;
                    }

                    if (e.KeyChar == '.' && tb.Text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }

                    string futureQty = tb.Text.Insert(tb.SelectionStart, e.KeyChar.ToString());

                    if (futureQty.Contains('.'))
                    {
                        string[] parts = futureQty.Split('.');
                        // Max 5 digits before decimal, 2 after
                        if (parts[0].Length > 5 || (parts.Length > 1 && parts[1].Length > 2))
                            e.Handled = true;
                    }
                    else
                    {
                        // Without decimal, max 5 digits
                        if (futureQty.Length > 5)
                            e.Handled = true;
                    }
                    break;



                case "salevalue":
                case "totalamount":
                case "discount":
                case "taxcharged":
                case "itemdiscountamount":
                    // ✅ 10 DIGITS BEFORE DECIMAL, 2 AFTER
                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    {
                        e.Handled = true;
                        return;
                    }

                    if (e.KeyChar == '.' && tb.Text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }

                    string futureText = tb.Text.Insert(tb.SelectionStart, e.KeyChar.ToString());

                    if (futureText.Contains('.'))
                    {
                        string[] parts = futureText.Split('.');
                        if (parts[0].Length > 10 || (parts.Length > 1 && parts[1].Length > 2))
                            e.Handled = true;
                    }
                    else
                    {
                        if (futureText.Length > 10)
                            e.Handled = true;
                    }
                    break;

                case "taxratebox":
                case "itemdiscountpercent":
                    // PERCENTAGE 0-100.00
                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    {
                        e.Handled = true;
                        return;
                    }

                    if (e.KeyChar == '.' && tb.Text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }

                    string futurePercent = tb.Text.Insert(tb.SelectionStart, e.KeyChar.ToString());

                    if (futurePercent.Contains('.'))
                    {
                        string[] parts = futurePercent.Split('.');
                        if (parts[0].Length > 3 || (parts.Length > 1 && parts[1].Length > 2))
                            e.Handled = true;
                    }
                    else if (futurePercent.Length > 3)
                    {
                        e.Handled = true;
                    }

                    if (decimal.TryParse(futurePercent, out decimal val) && val > 100)
                        e.Handled = true;
                    break;
            }
        }

        private void NumericOnlyWithLength_TextChanged(object sender, EventArgs e)
        {
            if (sender is not TextBox tb) return;
            string original = tb.Text;
            string clean = original;
            string name = tb.Name.ToLower();

            switch (name)
            {
                case "posid":
                    // ✅ UP TO 6 DIGITS
                    clean = new string(original.Where(char.IsDigit).ToArray());
                    if (clean.Length > 6) clean = clean[..6];
                    break;

                case "buyerntn":
                    clean = new string(original.Where(char.IsLetterOrDigit).ToArray());
                    if (clean.Length > 7) clean = clean[..7];
                    break;

                case "buyercnic":
                    // 13 DIGITS
                    clean = new string(original.Where(char.IsDigit).ToArray());
                    if (clean.Length > 13) clean = clean[..13];
                    break;

                case "buyerphone":
                    // ✅ UP TO 20 CHARACTERS
                    clean = new string(original.Where(c =>
                        char.IsDigit(c) || c == '+' || c == '-' || c == '(' || c == ')').ToArray());
                    if (clean.Length > 20) clean = clean[..20];
                    break;

                case "pctcode":
                    // 8 DIGITS
                    clean = new string(original.Where(char.IsDigit).ToArray());
                    if (clean.Length > 8) clean = clean[..8];
                    break;

                case "qty":
                    // ✅ 5 digits before decimal, 2 after
                    clean = Regex.Replace(original, @"[^0-9.]", "");
                    int dotIndex = clean.IndexOf('.');

                    if (dotIndex != -1)
                        clean = clean[..(dotIndex + 1)] + clean[(dotIndex + 1)..].Replace(".", "");

                    if (dotIndex != -1)
                    {
                        string intPart = clean[..dotIndex];
                        string decPart = clean[(dotIndex + 1)..];

                        if (intPart.Length > 5) intPart = intPart[..5];
                        if (decPart.Length > 2) decPart = decPart[..2];

                        clean = intPart + "." + decPart;
                    }
                    else if (clean.Length > 5)
                    {
                        clean = clean[..5];
                    }

                    if (clean.StartsWith(".")) clean = "0" + clean;
                    break;

                case "salevalue":
                case "totalamount":
                case "discount":
                case "taxcharged":
                case "itemdiscountamount":
                    // 10 DIGITS BEFORE DECIMAL, 2 AFTER
                    clean = Regex.Replace(original, @"[^0-9.]", "");
                    dotIndex = clean.IndexOf('.');

                    if (dotIndex != -1)
                        clean = clean[..(dotIndex + 1)] + clean[(dotIndex + 1)..].Replace(".", "");

                    if (dotIndex != -1)
                    {
                        string intPartSale = clean[..dotIndex];
                        string decPartSale = clean[(dotIndex + 1)..];

                        if (intPartSale.Length > 10) intPartSale = intPartSale[..10];
                        if (decPartSale.Length > 2) decPartSale = decPartSale[..2];

                        clean = intPartSale + "." + decPartSale;
                    }
                    else if (clean.Length > 10)
                    {
                        clean = clean[..10];
                    }

                    if (clean.StartsWith(".")) clean = "0" + clean;
                    break;

                case "taxratebox":
                case "itemdiscountpercent":
                    // PERCENTAGE 0-100.00
                    clean = Regex.Replace(original, @"[^0-9.]", "");
                    dotIndex = clean.IndexOf('.');

                    if (dotIndex != -1)
                        clean = clean[..(dotIndex + 1)] + clean[(dotIndex + 1)..].Replace(".", "");

                    string intPartPercent = "";
                    string decPartPercent = "";
                    if (dotIndex != -1)
                    {
                        intPartPercent = clean[..dotIndex];
                        decPartPercent = clean[(dotIndex + 1)..];
                    }
                    else
                    {
                        intPartPercent = clean;
                    }

                    if (intPartPercent.Length > 3) intPartPercent = intPartPercent[..3];
                    if (decPartPercent.Length > 2) decPartPercent = decPartPercent[..2];

                    clean = dotIndex != -1 ? intPartPercent + "." + decPartPercent : intPartPercent;

                    if (decimal.TryParse(clean, out decimal val) && val > 100)
                        clean = "100";
                    break;
            }

            if (tb.Text != clean)
            {
                int pos = tb.SelectionStart - (tb.Text.Length - clean.Length);
                tb.Text = clean;
                tb.SelectionStart = Math.Max(0, Math.Min(pos, tb.Text.Length));
            }
        }

        private void StringOnlyWithLength_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is not TextBox tb) return;

            // Allow control keys (Backspace, Delete, etc.)
            if (char.IsControl(e.KeyChar))
                return;

            string name = tb.Name.ToLower();

            switch (name)
            {

                case "buyerbname":
                    // Allow letters and spaces only
                    if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
                    {
                        e.Handled = true;
                        return;
                    }
                    if (tb.Text.Length >= 150)
                        e.Handled = true;
                    break;

                case "refusin":
                case "usin":
                case "itemcode":
                    // Allow letters, digits, and spaces
                    if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != ' ')
                    {
                        e.Handled = true;
                        return;
                    }
                    if (tb.Text.Length >= 50)
                        e.Handled = true;
                    break;

                default:
                    // Default: letters and spaces only
                    if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
                        e.Handled = true;
                    break;
            }
        }

        private void StringOnlyWithLength_TextChanged(object sender, EventArgs e)
        {
            if (sender is not TextBox tb) return;

            string name = tb.Name.ToLower();
            string original = tb.Text;
            string clean;

            // Default: allow letters and spaces
            if (name == "itemcode")
                clean = new string(original.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            else if (name == "refusin")
                clean = new string(original.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            else if (name == "usin")
                clean = new string(original.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            else
                clean = new string(original.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());

            int maxLength = name switch
            {
                "refusin" => 50,
                "usin" => 50,
                "buyerbname" => 150,
                "itemcode" => 50,
                _ => 150
            };

            // Enforce max length
            if (clean.Length > maxLength)
                clean = clean.Substring(0, maxLength);

            // Apply only if modified
            if (tb.Text != clean)
            {
                int pos = tb.SelectionStart - (tb.Text.Length - clean.Length);
                tb.Text = clean;
                tb.SelectionStart = Math.Max(0, Math.Min(pos, tb.Text.Length));
            }
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
            // When not maximized, don't restore - let WinForms Anchor/Dock handle it
        }
        #endregion

        #region datagrid style
        private void AddItemToDataGrid(InvoiceItems item)
        {
            int rowIndex = dataGridView1.Rows.Add();
            var row = dataGridView1.Rows[rowIndex];

            row.Cells["colSrNo"].Value = (rowIndex + 1).ToString();
            row.Cells["colSaleType"].Value = GetSaleTypeName();
            row.Cells["colProductCode"].Value = item.ItemCode ?? "";
            row.Cells["colProductDescription"].Value = item.ItemName ?? "";
            row.Cells["colHSCode"].Value = item.PCTCode ?? "";

            decimal qty = item.Quantity ?? 0m;
            decimal rate = item.SaleValue ?? 0m;       // Assuming SaleValue = Unit Price
            decimal discountAmount = item.Discount ?? 0m;
            decimal taxCharged = item.TaxCharged ?? 0m;
            decimal totalAmount = item.TotalAmount ?? 0m;

            // Explicitly cast TaxRate to decimal if it's double
            decimal taxRate = (decimal)item.TaxRate;

            // Calculations
            decimal salesValueExcTax = Math.Round((qty * rate) - discountAmount, 2);

            // Assign values
            row.Cells["colUnitPrice"].Value = rate.ToString("0.00");
            row.Cells["colQuantity"].Value = qty.ToString("0.00");
            row.Cells["colTaxRate"].Value = taxRate.ToString("0.00");
            row.Cells["colTaxCharged"].Value = taxCharged.ToString("0.00");
            row.Cells["colDiscount"].Value = discountAmount.ToString("0.00");
            row.Cells["colTotalAmount"].Value = totalAmount.ToString("0.00");
        }
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
            dataGridView1.Columns.Clear();
            StyleDataGridView(dataGridView1);

            // ✅ Make the grid responsive
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Common cell styles
            var centerStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter };
            var rightStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Format = "0.00"
            };
            var leftStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft };

            // Columns definition
            var colSrNo = new DataGridViewTextBoxColumn
            {
                Name = "colSrNo",
                HeaderText = "Sr. No.",
                FillWeight = 8,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = centerStyle
            };

            var colSaleType = new DataGridViewTextBoxColumn
            {
                Name = "colSaleType",
                HeaderText = "Services Rendered",
                FillWeight = 14,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = centerStyle
            };

            var colProductCode = new DataGridViewTextBoxColumn
            {
                Name = "colProductCode",
                HeaderText = "Item Code",
                FillWeight = 12,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = centerStyle
            };

            var colProductDescription = new DataGridViewTextBoxColumn
            {
                Name = "colProductDescription",
                HeaderText = "Item Description",
                FillWeight = 18,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = leftStyle
            };

            var colHSCode = new DataGridViewTextBoxColumn
            {
                Name = "colHSCode",
                HeaderText = "HS Code",
                FillWeight = 10,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = centerStyle
            };

            var colUnitPrice = new DataGridViewTextBoxColumn
            {
                Name = "colUnitPrice",
                HeaderText = "     Unit Price",
                FillWeight = 12,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = rightStyle
            };

            var colQuantity = new DataGridViewTextBoxColumn
            {
                Name = "colQuantity",
                HeaderText = " Quantity",
                FillWeight = 10,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = rightStyle
            };

            var colTaxRate = new DataGridViewTextBoxColumn
            {
                Name = "colTaxRate",
                HeaderText = "  Tax Rate (%)",
                FillWeight = 12,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = rightStyle
            };

            var colTaxCharged = new DataGridViewTextBoxColumn
            {
                Name = "colTaxCharged",
                HeaderText = "  Tax Charged",
                FillWeight = 12,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = rightStyle
            };

            var colDiscount = new DataGridViewTextBoxColumn
            {
                Name = "colDiscount",
                HeaderText = "Discount(Rs.)",
                FillWeight = 12,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = rightStyle
            };

            var colTotalAmount = new DataGridViewTextBoxColumn
            {
                Name = "colTotalAmount",
                HeaderText = "   Total Amount",
                FillWeight = 12,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = rightStyle
            };

            // Add columns
            dataGridView1.Columns.AddRange(new DataGridViewColumn[]
            {
        colSrNo,
        colSaleType,
        colProductCode,
        colProductDescription,
        colHSCode,
        colUnitPrice,
        colQuantity,
        colTaxRate,
        colTaxCharged,
        colDiscount,
        colTotalAmount
            });

            // ✅ Center all headers
            foreach (DataGridViewColumn col in dataGridView1.Columns)
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // ✅ Automatically size headers initially
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            // ✅ Then allow fill-based resizing when the form expands
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            colProductDescription.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        #endregion

    }
}