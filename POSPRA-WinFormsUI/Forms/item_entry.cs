using System.Drawing.Drawing2D;
using System.Net.Http.Json;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;
using System.Drawing.Drawing2D;

namespace POSPRA_WinFormsUI
{
    public partial class item_entry : Form
    {
        #region Fields

        // Responsive layout capture
        private readonly Dictionary<Control, Rectangle> _originalBounds = new();
        private readonly Dictionary<Control, Size> _originalParentSizes = new();
        private readonly Dictionary<Control, Font> _originalFonts = new();
        private Size _originalClientSize = Size.Empty;
        private bool _originalLayoutCaptured = false;

        // Session data (persisted across instances)
        private static List<InvoiceItemDetail> _sessionItems = new();

        // Current invoice & item list
        public static Invoice CurrentInvoice;
        private readonly List<InvoiceItemDetail> addedItems;
        private readonly IFiscalService _fiscalService;

        #endregion

        #region Constructor / Initialization

        public item_entry(IFiscalService fiscalService, IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();

            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));

            // Use static session list
            addedItems = _sessionItems;

            // UI & event wiring
            this.Resize += Item_entry_Resize;
            pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);
            panel1.Resize += (s, e) => MakeRoundedControl(panel1, 25);

            btnProceed.Click += BtnProceed_Click;      // Add / Update item behaviour (kept name: Proceed)
            btnSave.Click += BtnSave_Click;            // Persist invoice + items
            btnEdit.Click += btnEdit_Click;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            chkRegistered.CheckedChanged += ChkRegistered_CheckedChanged;
            chkUnregistered.CheckedChanged += ChkUnregistered_CheckedChanged;

            SetupContextMenu();
            CaptureOriginalLayout();
        }

        #endregion

        #region Grid / Item Helpers

        private void InitializeEmptyGrid()
        {
            dataGridView1.Rows.Clear();
            lblTotalItems.Text = "Total 0 items";
            ProductCode.Focus();
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            var deleteItem = new ToolStripMenuItem("Delete Item");
            deleteItem.Click += (s, e) => RemoveSelectedItem();
            contextMenu.Items.Add(deleteItem);

            dataGridView1.ContextMenuStrip = contextMenu;
        }

        private InvoiceItemDetail GetTextboxData()
        {
            return new InvoiceItemDetail
            {
                ProductCode = ProductCode.Text.Trim(),
                UoM = int.TryParse(uom.Text, out var uomVal) ? uomVal : 0,
                Rate = decimal.TryParse(rate.Text, out var rateVal) ? rateVal : 0m,
                ProductDescription = ProductDescription.Text.Trim(),
                TotalValues = decimal.TryParse(TotalValue.Text, out var totalVal) ? totalVal : 0m,
                SalesTaxApplicable = decimal.TryParse(SalesTaxApplicable.Text, out var salesTax) ? salesTax : 0m,
                ExtraTax = decimal.TryParse(extratax.Text, out var extraTax) ? extraTax : (decimal?)null,
                FurtherTax = decimal.TryParse(furturetax.Text, out var furtherTax) ? furtherTax : (decimal?)null,
                SroScheduleNo = int.TryParse(SroScheduleNo.Text, out var sroVal) ? sroVal : (int?)null,
                HSCode = hscode.Text.Trim(),
                Quantity = decimal.TryParse(qty.Text, out var qtyVal) ? qtyVal : 0m,
                RetailPrice = decimal.TryParse(RetailPrice.Text, out var retailPrice) ? retailPrice : 0m,
                FedPayable = decimal.TryParse(fed.Text, out var fedVal) ? fedVal : (decimal?)null,
                ValueSalesExcludingST = decimal.TryParse(SalesValueExclST.Text, out var exclST) ? exclST : 0m,
                STWithheldAtSource = decimal.TryParse(SalesTaxWithheldatSource.Text, out var stWithheld) ? stWithheld : (decimal?)null,
                CVT = decimal.TryParse(cvt.Text, out var cvtVal) ? cvtVal : (decimal?)null,
                WHIT_1 = decimal.TryParse(whit1.Text, out var whit1Val) ? whit1Val : (decimal?)null,
                WHIT_2 = decimal.TryParse(whit2.Text, out var whit2Val) ? whit2Val : (decimal?)null,
                WHIT_Section_1 = WHIT_Section_1.Text.Trim(),
                WHIT_Section_2 = WHIT_Section_2.Text.Trim(),
            };
        }

        private bool ValidateItemEntry(InvoiceItemDetail inputData)
        {
            // Product Code
            if (string.IsNullOrWhiteSpace(inputData.ProductCode))
            {
                MessageBox.Show("Please enter a Product Code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ProductCode.Focus();
                return false;
            }

            // UOM
            if (inputData.UoM <= 0)
            {
                MessageBox.Show("Please enter a valid UOM.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uom.Focus();
                return false;
            }

            // Rate
            if (inputData.Rate <= 0)
            {
                MessageBox.Show("Please enter a valid Rate greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rate.Focus();
                return false;
            }

            // Product Description
            if (string.IsNullOrWhiteSpace(inputData.ProductDescription))
            {
                MessageBox.Show("Please enter a Product Description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ProductDescription.Focus();
                return false;
            }

            // Quantity
            if (inputData.Quantity <= 0)
            {
                MessageBox.Show("Please enter a valid Quantity greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                qty.Focus();
                return false;
            }

            // Value Sales Excluding ST
            if (inputData.ValueSalesExcludingST < 0)
            {
                MessageBox.Show("Please enter a valid Value Sales Excluding ST (0 or higher).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SalesValueExclST.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region Buttons: Proceed (Add/Update) & Save

        private void BtnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                // Ensure invoice header is filled enough to create a CurrentInvoice
                if (!AreInvoiceFieldsValid())
                {
                    var res = MessageBox.Show("Invoice header looks incomplete. Do you want to continue adding items anyway?", "Invoice Header", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.No) return;
                }

                // ensure CurrentInvoice updated with header values
                CurrentInvoice = CollectInvoiceData();

                InvoiceItemDetail inputData = GetTextboxData();

                if (!ValidateItemEntry(inputData))
                    return;

                // FIXED: detect duplicate by ProductCode using correct column name
                var existingRow = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(r => (r.Cells["colProductCode"].Value?.ToString() ?? "") == inputData.ProductCode);

                if (existingRow != null)
                {
                    var result = MessageBox.Show($"Product Code '{inputData.ProductCode}' already exists. Do you want to update it?",
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
                        ProductCode.Focus();
                        return;
                    }
                }
                else
                {
                    // FIXED: Use AddItemToDataGrid method instead of manual row addition
                    AddItemToDataGrid(inputData);
                    addedItems.Add(inputData);
                }

                // Update UI
                lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
                ClearFormFields();

                MessageBox.Show($"Item '{inputData.ProductCode}' added/updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ProductCode.Focus();
                UpdateInvoiceTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Save button: persist invoice + session to service
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (addedItems == null || !addedItems.Any())
                {
                    MessageBox.Show("Please add at least one item before saving the invoice.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Ensure invoice header is filled before saving
                if (!AreInvoiceFieldsValid())
                {
                    MessageBox.Show("Invoice header is incomplete. Please fill in the invoice header before saving.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Ask for confirmation here
                var confirm = MessageBox.Show(
                    "Are you sure you want to save this invoice?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    // User pressed No → stop execution
                    return;
                }

                // Ensure CurrentInvoice is updated
                if (CurrentInvoice == null)
                {
                    CurrentInvoice = CollectInvoiceData();
                }

                // Map InvoiceItemDetail -> InvoiceItemDetailDto
                var itemDtos = addedItems.Select(i => new InvoiceItemDetailDto
                {
                    HSCode = i.HSCode,
                    ProductCode = i.ProductCode,
                    ProductDescription = i.ProductDescription,
                    Rate = i.Rate,
                    UoM = i.UoM,
                    Quantity = i.Quantity,
                    ValueSalesExcludingST = i.ValueSalesExcludingST,
                    SalesTaxApplicable = i.SalesTaxApplicable,
                    RetailPrice = i.RetailPrice,
                    STWithheldAtSource = i.STWithheldAtSource,
                    ExtraTax = i.ExtraTax,
                    FurtherTax = i.FurtherTax,
                    SroScheduleNo = i.SroScheduleNo,
                    FedPayable = i.FedPayable,
                    CVT = i.CVT,
                    WHIT_1 = i.WHIT_1,
                    WHIT_2 = i.WHIT_2,
                    WHIT_Section_1 = i.WHIT_Section_1,
                    WHIT_Section_2 = i.WHIT_Section_2,
                    TotalValues = i.TotalValues
                }).ToList();

                // Map Invoice -> InvoiceDto
                var dto = new InvoiceDto
                {
                    BPOSID = int.TryParse(posid.Text, out var bposId) ? bposId : GlobalVariables.POS_ID,
                    InvoiceType = chkRegistered.Checked ? (short)1 : (chkUnregistered.Checked ? (short)2 : (short)0),
                    InvoiceDate = CurrentInvoice?.InvoiceDate ?? DateTime.Now,
                    NTN_CNIC = sellerntn.Text,
                    BuyerSellerName = sellerBname.Text,
                    DestinationAddress = DestinationAddress.Text,
                    SaleType = int.TryParse(SaleType.Text, out var saleType) ? saleType : 0,
                    TotalSalesTaxApplicable = decimal.TryParse(SalesTaxApplicable.Text, out var st) ? st : 0,
                    TotalRetailPrice = decimal.TryParse(RetailPrice.Text, out var retail) ? retail : itemDtos.Sum(x => x.RetailPrice),
                    TotalSTWithheldAtSource = decimal.TryParse(TotalSTWithheld.Text, out var withheld) ? withheld : 0,
                    TotalExtraTax = decimal.TryParse(extratax.Text, out var extraTax) ? extraTax : 0,
                    TotalFEDPayable = decimal.TryParse(TotalFEDPayable.Text, out var fed) ? fed : 0,
                    TotalWithheldIncomeTax = decimal.TryParse(TotalWithheldIncomeTax.Text, out var incomeTax) ? incomeTax : 0,
                    TotalCVT = decimal.TryParse(TotalCVT.Text, out var cvt) ? cvt : 0,
                    Distributor_NTN_CNIC = buyerntn.Text,
                    DistributorName = buyerBname.Text,
                    InvoiceItemDetails = itemDtos
                };

                //await PostInvoiceAsync(dto);

                // Post to fiscal service
                var output = await _fiscalService.CreateAsync(dto);
                if (output.StatusCode == ApiStatusCode.Success.ToString())
                {
                    MessageBox.Show(output.Message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear session
                    addedItems.Clear();
                    dataGridView1.Rows.Clear();
                    lblTotalItems.Text = "Total 0 items";
                    CurrentInvoice = null;
                    _sessionItems.Clear();
                    UpdateInvoiceTotals();
                }
                else
                {
                    MessageBox.Show(output.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) ensure a row is selected
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a row to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2) FIXED: get selected row using correct column name
                var row = dataGridView1.SelectedRows[0];
                string productCode = row.Cells["colProductCode"].Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(productCode))
                {
                    MessageBox.Show("Product code is missing for this row.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3) find item in list
                int itemIndex = addedItems.FindIndex(i => string.Equals(i.ProductCode?.Trim(), productCode, StringComparison.OrdinalIgnoreCase));

                if (itemIndex < 0)
                {
                    MessageBox.Show($"Item with Product Code '{productCode}' was not found in the current list.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var item = addedItems[itemIndex];

                // 4) populate form fields
                ProductCode.Text = item.ProductCode ?? "";
                ProductDescription.Text = item.ProductDescription ?? "";
                uom.Text = item.UoM.ToString();
                rate.Text = item.Rate.ToString("F2");
                TotalValue.Text = item.TotalValues.ToString("F2");
                SalesTaxApplicable.Text = item.SalesTaxApplicable.ToString("F2");
                extratax.Text = item.ExtraTax?.ToString("F2") ?? "";
                furturetax.Text = item.FurtherTax?.ToString("F2") ?? "";
                SroScheduleNo.Text = item.SroScheduleNo?.ToString() ?? "";
                hscode.Text = item.HSCode ?? "";
                qty.Text = item.Quantity.ToString();
                RetailPrice.Text = item.RetailPrice.ToString("F2");
                fed.Text = item.FedPayable?.ToString("F2") ?? "";
                SalesValueExclST.Text = item.ValueSalesExcludingST.ToString("F2");
                SalesTaxWithheldatSource.Text = item.STWithheldAtSource?.ToString("F2") ?? "";
                cvt.Text = item.CVT?.ToString("F2") ?? "";
                whit1.Text = item.WHIT_1?.ToString("F2") ?? "";
                whit2.Text = item.WHIT_2?.ToString("F2") ?? "";
                WHIT_Section_1.Text = item.WHIT_Section_1 ?? "";
                WHIT_Section_2.Text = item.WHIT_Section_2 ?? "";

                // 5) remove from memory + grid
                addedItems.RemoveAt(itemIndex);
                dataGridView1.Rows.Remove(row);

                // 6) update UI
                UpdateSerialNumbers();
                lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
                UpdateInvoiceTotals();

                // 7) focus first field
                ProductCode.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while trying to edit the item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Grid Edit / Remove Helpers

        private void UpdateExistingItem(DataGridViewRow row, InvoiceItemDetail inputData)
        {
            // Update cells with all form fields using correct property names and data types
            row.Cells["colProductCode"].Value = inputData.ProductCode?.ToString() ?? "";
            row.Cells["colHSCode"].Value = inputData.HSCode?.ToString() ?? "";
            row.Cells["colProductDescription"].Value = inputData.ProductDescription?.ToString() ?? "";
            row.Cells["colUOM"].Value = inputData.UoM.ToString(); // int (non-nullable)
            row.Cells["colQuantity"].Value = inputData.Quantity.ToString(); // decimal (non-nullable)
            row.Cells["colRate"].Value = inputData.Rate.ToString(); // decimal (non-nullable)
            row.Cells["colRetailPrice"].Value = inputData.RetailPrice.ToString(); // decimal (non-nullable)
            row.Cells["colSalesValueExcST"].Value = inputData.ValueSalesExcludingST.ToString(); // decimal (non-nullable)
            row.Cells["colTotalValue"].Value = inputData.TotalValues.ToString(); // decimal (non-nullable)
            row.Cells["colSalesTax"].Value = inputData.SalesTaxApplicable.ToString(); // decimal (non-nullable)
            row.Cells["colExtraTax"].Value = inputData.ExtraTax?.ToString() ?? "0"; // decimal? (nullable) - show 0 if null
            row.Cells["colFutureTax"].Value = inputData.FurtherTax?.ToString() ?? "0"; // decimal? (nullable) - show 0 if null
            row.Cells["colSROSNo"].Value = inputData.SroScheduleNo?.ToString() ?? "0"; // int? (nullable) - show 0 if null
            row.Cells["colCVT"].Value = inputData.CVT?.ToString() ?? "0"; // decimal? (nullable) - show 0 if null
            row.Cells["colSalesTaxApplicable"].Value = inputData.SalesTaxApplicable.ToString(); // decimal (non-nullable)
            row.Cells["colSalesTaxWithheldAtSource"].Value = inputData.STWithheldAtSource?.ToString() ?? "0"; // decimal? (nullable) - show 0 if null
            row.Cells["colWHIT1"].Value = inputData.WHIT_1?.ToString() ?? "0"; // decimal? (nullable) - show 0 if null
            row.Cells["colWHIT2"].Value = inputData.WHIT_2?.ToString() ?? "0"; // decimal? (nullable) - show 0 if null
            row.Cells["colFedPayable"].Value = inputData.FedPayable?.ToString() ?? "0"; // decimal? (nullable) - show 0 if null
            row.Cells["colTotalValuePayable"].Value = inputData.FedPayable?.ToString() ?? "0"; // Note: You have same value for both
            row.Cells["colWHITSection1"].Value = inputData.WHIT_Section_1?.ToString() ?? ""; // string? (nullable) - keep empty for strings
            row.Cells["colWHITSection2"].Value = inputData.WHIT_Section_2?.ToString() ?? ""; // string? (nullable) - keep empty for strings

        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) LoadItemForEditing(e.RowIndex);
        }

        private void LoadItemForEditing(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count) return;

            var row = dataGridView1.Rows[rowIndex];

            // Load data back into form fields using correct column names and property mappings
            ProductCode.Text = row.Cells["colProductCode"].Value?.ToString() ?? "";
            hscode.Text = row.Cells["colHSCode"].Value?.ToString() ?? "";
            ProductDescription.Text = row.Cells["colProductDescription"].Value?.ToString() ?? "";
            uom.Text = row.Cells["colUOM"].Value?.ToString() ?? "";
            qty.Text = row.Cells["colQuantity"].Value?.ToString() ?? "";
            rate.Text = row.Cells["colRate"].Value?.ToString() ?? "";
            RetailPrice.Text = row.Cells["colRetailPrice"].Value?.ToString() ?? "";
            SalesValueExclST.Text = row.Cells["colSalesValueExcST"].Value?.ToString() ?? "";
            TotalValue.Text = row.Cells["colTotalValue"].Value?.ToString() ?? "";
            SalesTaxApplicable.Text = row.Cells["colSalesTax"].Value?.ToString() ?? ""; // Note: colSalesTax maps to SalesTaxApplicable
            extratax.Text = row.Cells["colExtraTax"].Value?.ToString() ?? "";
            furturetax.Text = row.Cells["colFutureTax"].Value?.ToString() ?? "";
            SroScheduleNo.Text = row.Cells["colSROSNo"].Value?.ToString() ?? "";
            cvt.Text = row.Cells["colCVT"].Value?.ToString() ?? "";
            // Note: SalesTaxApplicable appears in both colSalesTax and colSalesTaxApplicable - using colSalesTaxApplicable here
            SalesTaxApplicable.Text = row.Cells["colSalesTaxApplicable"].Value?.ToString() ?? "";
            SalesTaxWithheldatSource.Text = row.Cells["colSalesTaxWithheldAtSource"].Value?.ToString() ?? "";
            whit1.Text = row.Cells["colWHIT1"].Value?.ToString() ?? "";
            whit2.Text = row.Cells["colWHIT2"].Value?.ToString() ?? "";
            fed.Text = row.Cells["colFedPayable"].Value?.ToString() ?? "";
            textBox4.Text = row.Cells["colTotalValuePayable"].Value?.ToString() ?? ""; // Total Value Payable
            WHIT_Section_1.Text = row.Cells["colWHITSection1"].Value?.ToString() ?? "";
            WHIT_Section_2.Text = row.Cells["colWHITSection2"].Value?.ToString() ?? "";

            // Remove row (user will re-add after editing)
            dataGridView1.Rows.RemoveAt(rowIndex);

            // Keep the in-memory list in sync
            if (rowIndex < addedItems.Count)
                addedItems.RemoveAt(rowIndex);

            UpdateSerialNumbers();
            lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
            ProductCode.Focus();
        }

        private void ClearFormFields()
        {
            // Basic Information
            ProductCode.Clear();       // Product Code
            hscode.Clear();           // HS Code
            ProductDescription.Clear(); // Product Description

            // Pricing Information
            rate.Clear();             // Rate
            uom.Clear();              // UOM
            qty.Clear();              // Quantity
            RetailPrice.Clear();      // Retail Price
            SalesValueExclST.Clear(); // Sales Value Excluding ST

            // Sales Tax Information
            TotalValue.Clear();       // Total Value
            SaleType.Clear();         // Sales Tax (mapped to SaleType)
            extratax.Clear();         // Extra Tax
            furturetax.Clear();       // Future Tax
            SroScheduleNo.Clear();    // SRO Schedule No
            cvt.Clear();              // CVT

            // Additional fields
            SalesTaxApplicable.Clear(); // Sales Tax Applicable
            SalesTaxWithheldatSource.Clear(); // Sales Tax Withheld at Source
            whit1.Clear();            // WHIT-1
            whit2.Clear();            // WHIT-2
            fed.Clear();              // Fed Payable
            textBox4.Clear();         // Total Value Payable (mapped to textBox4)
            WHIT_Section_1.Clear();   // WHIT Section-1
            WHIT_Section_2.Clear();   // WHIT Section-2
            textBox5.Clear();         // Additional field if needed
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
                    lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
                }
            }
            else
            {
                MessageBox.Show("Please select a row to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateSerialNumbers()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["colSrNo"].Value = (i + 1).ToString("D2");
            }
        }

        private void AddItemToDataGrid(InvoiceItemDetail item)
        {
            int rowIndex = dataGridView1.Rows.Add();
            var row = dataGridView1.Rows[rowIndex];

            // Set serial number
            row.Cells["colSrNo"].Value = (rowIndex + 1).ToString("D2");

            // For non-nullable numeric fields (decimal, int) → always show numeric value
            row.Cells["colUOM"].Value = item.UoM.ToString();
            row.Cells["colQuantity"].Value = item.Quantity.ToString("0.##");
            row.Cells["colRate"].Value = item.Rate.ToString("0.##");
            row.Cells["colRetailPrice"].Value = item.RetailPrice.ToString("0.##");
            row.Cells["colSalesValueExcST"].Value = item.ValueSalesExcludingST.ToString("0.##");
            row.Cells["colTotalValue"].Value = item.TotalValues.ToString("0.##");
            row.Cells["colSalesTax"].Value = item.SalesTaxApplicable.ToString("0.##");
            row.Cells["colSalesTaxApplicable"].Value = item.SalesTaxApplicable.ToString("0.##");

            // For nullable numeric fields → "0" if null
            row.Cells["colExtraTax"].Value = item.ExtraTax?.ToString("0.##") ?? "0";
            row.Cells["colFutureTax"].Value = item.FurtherTax?.ToString("0.##") ?? "0";
            row.Cells["colSROSNo"].Value = item.SroScheduleNo?.ToString() ?? "0";
            row.Cells["colCVT"].Value = item.CVT?.ToString("0.##") ?? "0";
            row.Cells["colSalesTaxWithheldAtSource"].Value = item.STWithheldAtSource?.ToString("0.##") ?? "0";
            row.Cells["colWHIT1"].Value = item.WHIT_1?.ToString("0.##") ?? "0";
            row.Cells["colWHIT2"].Value = item.WHIT_2?.ToString("0.##") ?? "0";
            row.Cells["colFedPayable"].Value = item.FedPayable?.ToString("0.##") ?? "0";
            row.Cells["colTotalValuePayable"].Value = item.FedPayable?.ToString("0.##") ?? "0";

            // For strings → keep empty
            row.Cells["colProductCode"].Value = item.ProductCode ?? "";
            row.Cells["colHSCode"].Value = item.HSCode ?? "";
            row.Cells["colProductDescription"].Value = item.ProductDescription ?? "";
            row.Cells["colWHITSection1"].Value = item.WHIT_Section_1 ?? "";
            row.Cells["colWHITSection2"].Value = item.WHIT_Section_2 ?? "";


            lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
        }

        #endregion

        #region maths 
        private void UpdateInvoiceTotals()
        {
            if (addedItems == null || !addedItems.Any())
            {
                // reset if no items
                STapplicable.Text = "0.00";
                TotalRetailPrice.Text = "0.00";
                TotalSTWithheld.Text = "0.00";
                TotalFEDPayable.Text = "0.00";
                TotalWithheldIncomeTax.Text = "0.00";
                TotalCVT.Text = "0.00";
                TotalExtraTax.Text = "0.00";
                return;
            }

            // Sum from addedItems
            decimal totalSalesTax = addedItems.Sum(i => i.SalesTaxApplicable);
            decimal totalRetail = addedItems.Sum(i => i.RetailPrice);
            decimal totalWithheld = addedItems.Sum(i => i.STWithheldAtSource ?? 0);
            decimal totalFED = addedItems.Sum(i => i.FedPayable ?? 0);
            decimal totalIncomeTax = addedItems.Sum(i => (i.WHIT_1 ?? 0) + (i.WHIT_2 ?? 0));
            decimal totalCVT = addedItems.Sum(i => i.CVT ?? 0);
            decimal totalExtraTax = addedItems.Sum(i => i.ExtraTax ?? 0);

            // Push values to UI textboxes
            STapplicable.Text = totalSalesTax.ToString("F2");
            TotalRetailPrice.Text = totalRetail.ToString("F2");
            TotalSTWithheld.Text = totalWithheld.ToString("F2");
            TotalFEDPayable.Text = totalFED.ToString("F2");
            TotalWithheldIncomeTax.Text = totalIncomeTax.ToString("F2");
            TotalCVT.Text = totalCVT.ToString("F2");
            TotalExtraTax.Text = totalExtraTax.ToString("F2");
        }

        #endregion

        #region Responsive / Rounded Corners

        public void MakeRoundedControl(Control control, int radius)
        {
            if (control == null) return;

            var rect = control.ClientRectangle;
            if (rect.Width == 0 || rect.Height == 0) return;

            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);                         // Top-left
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);            // Top-right
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Bottom-right
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);            // Bottom-left
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
            if (!_originalLayoutCaptured)
            {
                CaptureOriginalLayout();
                if (!_originalLayoutCaptured) return;
            }

            if (IsFormMaximized())
            {
                double formScaleX = _originalClientSize.Width > 0 ? this.ClientSize.Width / (double)_originalClientSize.Width : 1.0;
                double formScaleY = _originalClientSize.Height > 0 ? this.ClientSize.Height / (double)_originalClientSize.Height : 1.0;
                double globalScale = Math.Min(formScaleX, formScaleY);

                foreach (var kv in _originalBounds)
                {
                    var ctrl = kv.Key;
                    var orig = kv.Value;
                    var parent = ctrl.Parent;
                    if (parent == null) continue;
                    if (!_originalParentSizes.TryGetValue(parent, out Size parentOrigSize) || parentOrigSize.Width == 0 || parentOrigSize.Height == 0)
                        continue;

                    var parentCurrentSize = parent.ClientSize;
                    double scaleX = parentCurrentSize.Width / (double)parentOrigSize.Width;
                    double scaleY = parentCurrentSize.Height / (double)parentOrigSize.Height;
                    int newX = (int)Math.Round(orig.X * scaleX);
                    int newY = (int)Math.Round(orig.Y * scaleY);
                    int newW = Math.Max(1, (int)Math.Round(orig.Width * scaleX));
                    int newH = Math.Max(1, (int)Math.Round(orig.Height * scaleY));

                    try { ctrl.Bounds = new Rectangle(newX, newY, newW, newH); } catch { }

                    try
                    {
                        if (_originalFonts.TryGetValue(ctrl, out Font origFont) && origFont != null)
                        {
                            float newFontSize = (float)(origFont.Size * globalScale);
                            if (newFontSize < 6f) newFontSize = 6f;
                            if (newFontSize > 72f) newFontSize = 72f;
                            ctrl.Font = new Font(origFont.FontFamily, newFontSize, origFont.Style);
                        }
                    }
                    catch { }
                }
            }
            else
            {
                foreach (var kv in _originalBounds)
                {
                    var ctrl = kv.Key;
                    try { ctrl.Bounds = kv.Value; } catch { }
                }

                foreach (var kv in _originalFonts)
                {
                    var ctrl = kv.Key;
                    var font = kv.Value;
                    try { if (font != null) ctrl.Font = font; } catch { }
                }
            }
        }

        #endregion

        #region Invoice Header helpers (merged from InvoiceEntry)

        private Invoice CollectInvoiceData()
        {
            return new Invoice
            {
                //InvoiceType = chkSaleInvoice.Checked ? (short)1 : (short)2,
                //InvoiceDate = txtInvoiceDate.Value,
                //BuyerSellerName = txtBuyerBusiness.Text,
                //DestinationAddress = txtBuyerAddress.Text,
                //NTN_CNIC = txtBuyerRegNo.Text,
                //DistributorName = txtSellerBusiness.Text,
                //Distributor_NTN_CNIC = txtSellerRegNo.Text,
                //SaleType = chkRegistered.Checked ? 1 : 2,
                //TotalRetailPrice = 0 // will be computed by service or set before save
            };
        }

        private bool AreInvoiceFieldsValid()
        {
            //// Basic checks for header completeness (loose - adjust if you want stricter)
            if (!chkRegistered.Checked && !chkUnregistered.Checked) return false;
            if (string.IsNullOrWhiteSpace(posid.Text)) return false;
            if (string.IsNullOrWhiteSpace(buyerntn.Text)) return false;
            if (string.IsNullOrWhiteSpace(sellerBname.Text)) return false;
            if (string.IsNullOrWhiteSpace(buyerBname.Text)) return false;
            //// optional: require provinces if needed
            return true;
        }
        private void ChkRegistered_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRegistered.Checked) chkUnregistered.Checked = false;
        }

        private void ChkUnregistered_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUnregistered.Checked) chkRegistered.Checked = false;
        }

        #endregion

        #region Misc UI handlers

        private void btn_remove_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        #endregion

        private void extratax_TextChanged(object sender, EventArgs e)
        {

        }

        public class InvoicePoster
        {
            private readonly HttpClient _httpClient;
            public InvoicePoster(IHttpClientFactory factory)
            {
                _httpClient = factory.CreateClient("SelfHostedApi");
            }

            public async Task<bool> PostInvoiceAsync(InvoiceDto dto)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/Fiscal/Create", dto);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResult = await response.Content
                                                      .ReadFromJsonAsync<ApiResponse<InvoiceDto>>();

                        MessageBox.Show(apiResult?.Message ?? "Invoice created successfully!",
                                        "Success",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);

                        // You can use apiResult?.Data if needed
                        return true;
                    }

                    // Non-success HTTP status
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error from API: {response.StatusCode}\n{error}",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error posting invoice: {ex.Message}",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }
            }
        }

    }
}
