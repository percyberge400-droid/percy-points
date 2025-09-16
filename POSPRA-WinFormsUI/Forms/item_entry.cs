using System.Drawing.Drawing2D;
using System.Net.Http.Json;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;

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
        private readonly HttpClient _httpClient;

        #endregion

        #region Constructor / Initialization

        public item_entry(IFiscalService fiscalService, HttpClient httpClient, IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();

            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));

            // Use static session list
            addedItems = _sessionItems;

            // UI & event wiring
            this.Resize += Item_entry_Resize;
            pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);

            btnProceed.Click += BtnProceed_Click;      // Add / Update item behaviour (kept name: Proceed)
            btnSave.Click += BtnSave_Click;            // Persist invoice + items
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            chkRegistered.CheckedChanged += ChkRegistered_CheckedChanged;
            chkUnregistered.CheckedChanged += ChkUnregistered_CheckedChanged;

            SetupContextMenu();
            CaptureOriginalLayout();
            _httpClient = httpClientFactory.CreateClient("SelfHostedApi");
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

        // This is the merged "Proceed" logic: it collects invoice header (if not already) and adds/updates an item row
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

                // detect duplicate by ProductCode (column name in your DataGridView designer was "colInvoice")
                var existingRow = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(r => (r.Cells["colInvoice"].Value?.ToString() ?? "") == inputData.ProductCode);

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
                    // Add new row
                    string newSrNo = (dataGridView1.Rows.Count + 1).ToString("D2");
                    dataGridView1.Rows.Add(
                        newSrNo,
                        inputData.ProductCode,
                        inputData.ProductDescription,
                        inputData.UoM.ToString(),
                        inputData.Rate.ToString("F2"),
                        inputData.ValueSalesExcludingST.ToString("F2"),
                        inputData.SalesTaxApplicable.ToString("F2"),
                        inputData.Quantity.ToString()
                    );

                    addedItems.Add(inputData);
                }

                // Update UI
                lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
                ClearFormFields();

                MessageBox.Show($"Item '{inputData.ProductCode}' added/updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ProductCode.Focus();
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

                await PostInvoiceAsync(dto);

                // Post to fiscal service
                var output = await _fiscalService.CreateAsync(dto);
                if (output.StatusCode == "200")
                {
                    MessageBox.Show(output.Message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear session
                    addedItems.Clear();
                    dataGridView1.Rows.Clear();
                    lblTotalItems.Text = "Total 0 items";
                    CurrentInvoice = null;
                    _sessionItems.Clear();
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

        #endregion

        #region Grid Edit / Remove Helpers

        private void UpdateExistingItem(DataGridViewRow row, InvoiceItemDetail inputData)
        {
            // Update cells — make sure names match your designer columns
            row.Cells["colPosId"].Value = inputData.ProductDescription;           // Product Description
            row.Cells["colInvoiceSynced"].Value = inputData.UoM.ToString();       // UOM
            row.Cells["colDueDate"].Value = inputData.Rate.ToString("F2");        // Rate
            row.Cells["colStatus"].Value = inputData.ValueSalesExcludingST.ToString("F2"); // Sales Excl ST
            row.Cells["colQuantity"].Value = inputData.Quantity.ToString();       // Quantity
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) LoadItemForEditing(e.RowIndex);
        }

        private void LoadItemForEditing(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count) return;

            var row = dataGridView1.Rows[rowIndex];

            // Load data back into form fields
            ProductCode.Text = row.Cells["colInvoice"].Value?.ToString() ?? "";           // Product Code
            ProductDescription.Text = row.Cells["colPosId"].Value?.ToString() ?? "";           // Product Description
            uom.Text = row.Cells["colInvoiceSynced"].Value?.ToString() ?? "";         // UOM
            rate.Text = row.Cells["colDueDate"].Value?.ToString() ?? "";          // Rate
            SalesValueExclST.Text = row.Cells["colStatus"].Value?.ToString() ?? "";   // Sales Excl ST
            qty.Text = row.Cells["colQuantity"].Value?.ToString() ?? "";              // Quantity

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
            ProductCode.Clear();       // Product Code
            uom.Clear();            // UOM
            rate.Clear();       // Rate
            textBox4.Clear();       // Total Values (mapped to TotalValue in designer)
            textBox5.Clear();       // Sales Tax Applicable (mapped to SalesTaxApplicable in designer)
            extratax.Clear();       // Extra Tax
            furturetax.Clear();       // Further Tax
            hscode.Clear();         // HS Code
            qty.Clear();            // Quantity
            RetailPrice.Clear();            // Retail Price
            fed.Clear();            // FED Payable
            SaleType.Clear();       // Sale Type
            SroScheduleNo.Clear();        // SRO Schedule No
            ProductDescription.Clear();      // Product Description
            SalesValueExclST.Clear(); // Value Sales Excluding ST
            SalesTaxWithheldatSource.Clear();     // ST Withheld at Source
            cvt.Clear();       // CVT
            whit1.Clear();          // WHIT-1
            whit2.Clear();          // WHIT-2
            WHIT_Section_1.Clear();  // WHIT Section-1
            WHIT_Section_2.Clear();  // WHIT Section-2
            TotalValue.Clear();      // Total Value
            SalesTaxApplicable.Clear(); // Sales Tax Applicable

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
                MessageBox.Show("Please select a row to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateSerialNumbers()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = (i + 1).ToString("D2");
            }
        }

        public List<InvoiceItemDetail> GetAllItems()
        {
            return addedItems.ToList();
        }

        public void ClearAllItems()
        {
            dataGridView1.Rows.Clear();
            addedItems.Clear();
            lblTotalItems.Text = "Total 0 items";
            ClearFormFields();
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

        /// <summary>
        /// Sends the given invoice to the self-hosted API and returns true on success.
        /// </summary>
        private async Task<bool> PostInvoiceAsync(InvoiceDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Fiscal/Create", dto);

                if (response.IsSuccessStatusCode)
                {
                    // If your API returns the created invoice
                    var apiResult = await response.Content
                                                  .ReadFromJsonAsync<ApiResponse<InvoiceDto>>();

                    MessageBox.Show(apiResult?.Message ?? "Invoice created successfully!",
                                    "Success",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                    // Optionally use apiResult.Data if you need the returned invoice
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error from API: {response.StatusCode}\n{error}",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return false;
                }
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
