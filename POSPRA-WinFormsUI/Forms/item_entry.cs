using POSPRA.Application.Services.FiscalService;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;
using System.Drawing.Drawing2D;

namespace POSPRA_WinFormsUI
{
    public partial class item_entry : Form
    {
        #region Fields

        // Responsive layout capture
        private readonly Dictionary<Control, Rectangle> _originalBounds = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Size> _originalParentSizes = new Dictionary<Control, Size>();
        private readonly Dictionary<Control, Font> _originalFonts = new Dictionary<Control, Font>();
        private Size _originalClientSize = Size.Empty;
        private bool _originalLayoutCaptured = false;

        // Session data (persisted across instances)
        private static List<InvoiceItemDetail> _sessionItems = new List<InvoiceItemDetail>();
        private static ItemEntryFormState _sessionFormState = new ItemEntryFormState();

        // Current invoice & item list
        public static Invoice CurrentInvoice;
        private readonly List<InvoiceItemDetail> addedItems;
        private readonly IFiscalService _fiscalService;


        #endregion

        #region Constructor / Initialization

        public item_entry(IFiscalService fiscalService)
        {
            InitializeComponent();

            _fiscalService = fiscalService;

            // Use static session list
            addedItems = _sessionItems;

            // Restore previous session state & grid items
            RestoreSessionState();

            // UI & event wiring
            this.Resize += Item_entry_Resize;
            pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);

            btnProceed.Click += BtnProceed_Click;      // Add / Update item behaviour (kept name: Proceed)
            btnSave.Click += BtnSave_Click;            // Persist invoice + items
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            chkSaleInvoice.CheckedChanged += ChkSaleInvoice_CheckedChanged;
            chkDebitInvoice.CheckedChanged += ChkDebitInvoice_CheckedChanged;
            chkRegistered.CheckedChanged += ChkRegistered_CheckedChanged;
            chkUnregistered.CheckedChanged += ChkUnregistered_CheckedChanged;

            SetupContextMenu();
            CaptureOriginalLayout();

            // Persist session on close / deactivate
            this.FormClosing += Item_entry_FormClosing;
            this.Leave += Item_entry_Leave;
            this.Deactivate += Item_entry_Deactivate;
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Small DTO to hold the visible header fields to restore the form later.
        /// </summary>
        public class ItemEntryFormState
        {
            public string ProductCode { get; set; } = "";
            public string UOM { get; set; } = "";
            public string Rate { get; set; } = "";
            public string ProductDescription { get; set; } = "";
            public string TotalValues { get; set; } = "";
            public string SalesTaxApplicable { get; set; } = "";
            public string ExtraTax { get; set; } = "";
            public string FurtherTax { get; set; } = "";
            public string SroScheduleNo { get; set; } = "";
            public string HsCode { get; set; } = "";
            public string Quantity { get; set; } = "";
            public string RetailPrice { get; set; } = "";
            public string FedPayable { get; set; } = "";
            public string SaleType { get; set; } = "";
            public string ValueSalesExcludingST { get; set; } = "";
            public string STWithheldAtSource { get; set; } = "";
            public string CVT { get; set; } = "";
            public string WHIT_1 { get; set; } = "";
            public string WHIT_2 { get; set; } = "";
            public string WHIT_Section_1 { get; set; } = "";

            // Invoice header-related state (from InvoiceEntry)
            public DateTime InvoiceDate { get; set; } = DateTime.Now;
            public bool SaleInvoiceChecked { get; set; } = true;
            public bool DebitInvoiceChecked { get; set; } = false;
            public bool RegisteredChecked { get; set; } = true;
            public bool UnregisteredChecked { get; set; } = false;
            public string SellerBusiness { get; set; } = "";
            public string SellerRegNo { get; set; } = "";
            public string BuyerBusiness { get; set; } = "";
            public string BuyerAddress { get; set; } = "";
            public string BuyerRegNo { get; set; } = "";
            public string SellerProvince { get; set; } = "";
            public string BuyerProvince { get; set; } = "";
        }

        private void SaveCurrentFormState()
        {
            try
            {
                // item fields
                _sessionFormState.ProductCode = textBox1.Text;
                _sessionFormState.UOM = uom.Text;
                _sessionFormState.Rate = textBox3.Text;
                _sessionFormState.ProductDescription = textBox11.Text;
                _sessionFormState.TotalValues = textBox4.Text;
                _sessionFormState.SalesTaxApplicable = textBox5.Text;
                _sessionFormState.ExtraTax = textBox6.Text;
                _sessionFormState.FurtherTax = textBox7.Text;
                _sessionFormState.SroScheduleNo = srosche.Text;
                _sessionFormState.HsCode = hscode.Text;
                _sessionFormState.Quantity = qty.Text;
                _sessionFormState.RetailPrice = mrp.Text;
                _sessionFormState.FedPayable = fed.Text;
                _sessionFormState.SaleType = saletype.Text;
                _sessionFormState.ValueSalesExcludingST = SalesValueExclST.Text;
                _sessionFormState.STWithheldAtSource = STWithheld.Text;
                _sessionFormState.CVT = Discount.Text; // Mapping Discount to CVT
                _sessionFormState.WHIT_Section_1 = SROScheduleNo.Text;

                // invoice header fields
                _sessionFormState.InvoiceDate = txtInvoiceDate.Value;
                _sessionFormState.SaleInvoiceChecked = chkSaleInvoice.Checked;
                _sessionFormState.DebitInvoiceChecked = chkDebitInvoice.Checked;
                _sessionFormState.RegisteredChecked = chkRegistered.Checked;
                _sessionFormState.UnregisteredChecked = chkUnregistered.Checked;
                _sessionFormState.SellerBusiness = txtSellerBusiness.Text;
                _sessionFormState.SellerRegNo = txtSellerRegNo.Text;
                _sessionFormState.BuyerBusiness = txtBuyerBusiness.Text;
                _sessionFormState.BuyerAddress = txtBuyerAddress.Text;
                _sessionFormState.BuyerRegNo = txtBuyerRegNo.Text;
                _sessionFormState.SellerProvince = cmbSellerProvince.SelectedItem?.ToString() ?? "";
                _sessionFormState.BuyerProvince = cmbBuyerProvince.SelectedItem?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving form state: {ex.Message}");
            }
        }

        private void RestoreSessionState()
        {
            try
            {
                // Restore item fields
                textBox1.Text = _sessionFormState.ProductCode;
                uom.Text = _sessionFormState.UOM;
                textBox3.Text = _sessionFormState.Rate;
                textBox11.Text = _sessionFormState.ProductDescription;
                textBox4.Text = _sessionFormState.TotalValues;
                textBox5.Text = _sessionFormState.SalesTaxApplicable;
                textBox6.Text = _sessionFormState.ExtraTax;
                textBox7.Text = _sessionFormState.FurtherTax;
                srosche.Text = _sessionFormState.SroScheduleNo;
                hscode.Text = _sessionFormState.HsCode;
                qty.Text = _sessionFormState.Quantity;
                mrp.Text = _sessionFormState.RetailPrice;
                fed.Text = _sessionFormState.FedPayable;
                saletype.Text = _sessionFormState.SaleType;
                SalesValueExclST.Text = _sessionFormState.ValueSalesExcludingST;
                STWithheld.Text = _sessionFormState.STWithheldAtSource;
                Discount.Text = _sessionFormState.CVT;
                SROScheduleNo.Text = _sessionFormState.WHIT_Section_1;

                // Restore invoice header
                txtInvoiceDate.Value = _sessionFormState.InvoiceDate;
                chkSaleInvoice.Checked = _sessionFormState.SaleInvoiceChecked;
                chkDebitInvoice.Checked = _sessionFormState.DebitInvoiceChecked;
                chkRegistered.Checked = _sessionFormState.RegisteredChecked;
                chkUnregistered.Checked = _sessionFormState.UnregisteredChecked;

                txtSellerBusiness.Text = _sessionFormState.SellerBusiness;
                txtSellerRegNo.Text = _sessionFormState.SellerRegNo;
                txtBuyerBusiness.Text = _sessionFormState.BuyerBusiness;
                txtBuyerAddress.Text = _sessionFormState.BuyerAddress;
                txtBuyerRegNo.Text = _sessionFormState.BuyerRegNo;

                if (!string.IsNullOrWhiteSpace(_sessionFormState.SellerProvince))
                {
                    if (cmbSellerProvince.Items.Contains(_sessionFormState.SellerProvince))
                        cmbSellerProvince.SelectedItem = _sessionFormState.SellerProvince;
                }

                if (!string.IsNullOrWhiteSpace(_sessionFormState.BuyerProvince))
                {
                    if (cmbBuyerProvince.Items.Contains(_sessionFormState.BuyerProvince))
                        cmbBuyerProvince.SelectedItem = _sessionFormState.BuyerProvince;
                }

                // Restore grid items
                RestoreGridItems();

                // Update totals label
                lblTotalItems.Text = $"Total {addedItems.Count} items";

                // Focus first input if nothing entered
                if (string.IsNullOrEmpty(_sessionFormState.ProductCode))
                    textBox1.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring session state: {ex.Message}");
                InitializeEmptyGrid();
            }
        }

        private void RestoreGridItems()
        {
            try
            {
                dataGridView1.Rows.Clear();

                for (int i = 0; i < addedItems.Count; i++)
                {
                    var item = addedItems[i];
                    string srNo = (i + 1).ToString("D2");

                    dataGridView1.Rows.Add(
                        srNo,
                        item.ProductCode,
                        item.ProductDescription,
                        item.UoM.ToString(),
                        item.Rate.ToString("F2"),
                        item.ValueSalesExcludingST.ToString("F2"),
                        item.SalesTaxApplicable.ToString("F2"),
                        item.Quantity.ToString()
                    );
                }
            }
            catch
            {
                // fallback
                InitializeEmptyGrid();
            }
        }

        /// <summary>
        /// Clear the entire session (both grid and form data)
        /// </summary>
        public static void ClearSession()
        {
            _sessionItems.Clear();
            _sessionFormState = new ItemEntryFormState();
        }

        /// <summary>
        /// Get current session item count
        /// </summary>
        public static int GetSessionItemCount()
        {
            return _sessionItems.Count;
        }

        private void Item_entry_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveCurrentFormState();
        }

        private void Item_entry_Leave(object sender, EventArgs e)
        {
            SaveCurrentFormState();
        }

        private void Item_entry_Deactivate(object sender, EventArgs e)
        {
            SaveCurrentFormState();
        }

        #endregion

        #region Grid / Item Helpers

        private void InitializeEmptyGrid()
        {
            dataGridView1.Rows.Clear();
            lblTotalItems.Text = "Total 0 items";
            textBox1.Focus();
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
                ProductCode = textBox1.Text.Trim(),
                UoM = int.TryParse(uom.Text, out var uomVal) ? uomVal : 0,
                Rate = decimal.TryParse(textBox3.Text, out var rate) ? rate : 0m,
                ProductDescription = textBox11.Text.Trim(),
                TotalValues = decimal.TryParse(textBox4.Text, out var totalVal) ? totalVal : 0m,
                SalesTaxApplicable = decimal.TryParse(textBox5.Text, out var salesTax) ? salesTax : 0m,
                ExtraTax = decimal.TryParse(textBox6.Text, out var extraTax) ? extraTax : 0m,
                FurtherTax = decimal.TryParse(textBox7.Text, out var furtherTax) ? furtherTax : 0m,
                SroScheduleNo = int.TryParse(srosche.Text, out var sroVal) ? sroVal : (int?)null,
                HSCode = hscode.Text.Trim(),
                Quantity = decimal.TryParse(qty.Text, out var qtyVal) ? qtyVal : 0m,
                RetailPrice = decimal.TryParse(mrp.Text, out var retailPrice) ? retailPrice : 0m,
                FedPayable = decimal.TryParse(fed.Text, out var fedVal) ? fedVal : (decimal?)null,
                ValueSalesExcludingST = decimal.TryParse(SalesValueExclST.Text, out var exclST) ? exclST : 0m,
                STWithheldAtSource = decimal.TryParse(STWithheld.Text, out var stWithheld) ? stWithheld : (decimal?)null,
                CVT = decimal.TryParse(Discount.Text, out var cvt) ? cvt : (decimal?)null,
                WHIT_Section_1 = SROScheduleNo.Text.Trim(),
                // WHIT_1, WHIT_2, WHIT_Section_2 left as default/null if not present
            };
        }

        private bool ValidateItemEntry(InvoiceItemDetail inputData)
        {
            // Product Code
            if (string.IsNullOrWhiteSpace(inputData.ProductCode))
            {
                MessageBox.Show("Please enter a Product Code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
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
                textBox3.Focus();
                return false;
            }

            // Product Description
            if (string.IsNullOrWhiteSpace(inputData.ProductDescription))
            {
                MessageBox.Show("Please enter a Product Description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox11.Focus();
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
                        textBox1.Focus();
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
                textBox1.Focus();
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

                // 🔒 Ensure invoice header is filled before saving
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
                    BPOSID = CurrentInvoice?.BPOSID ?? 0,
                    InvoiceType = CurrentInvoice?.InvoiceType ?? 0,
                    InvoiceDate = CurrentInvoice?.InvoiceDate ?? DateTime.Now,
                    NTN_CNIC = CurrentInvoice?.NTN_CNIC,
                    BuyerSellerName = CurrentInvoice?.BuyerSellerName,
                    DestinationAddress = CurrentInvoice?.DestinationAddress,
                    SaleType = CurrentInvoice?.SaleType ?? 0,
                    TotalSalesTaxApplicable = CurrentInvoice?.TotalSalesTaxApplicable,
                    TotalRetailPrice = CurrentInvoice?.TotalRetailPrice ?? itemDtos.Sum(x => x.RetailPrice),
                    TotalSTWithheldAtSource = CurrentInvoice?.TotalSTWithheldAtSource,
                    TotalExtraTax = CurrentInvoice?.TotalExtraTax,
                    TotalFEDPayable = CurrentInvoice?.TotalFEDPayable,
                    TotalWithheldIncomeTax = CurrentInvoice?.TotalWithheldIncomeTax,
                    TotalCVT = CurrentInvoice?.TotalCVT,
                    Distributor_NTN_CNIC = CurrentInvoice?.Distributor_NTN_CNIC,
                    DistributorName = CurrentInvoice?.DistributorName,
                    InvoiceItemDetails = itemDtos
                };

                // Post to fiscal service
                var output = await _fiscalService.CreateAsync(dto);

                if (output != null)
                {
                    MessageBox.Show("Invoice saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear session
                    addedItems.Clear();
                    dataGridView1.Rows.Clear();
                    lblTotalItems.Text = "Total 0 items";
                    CurrentInvoice = null;
                    _sessionItems.Clear();
                }
                else
                {
                    MessageBox.Show("Failed to save invoice. No response from service.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            textBox1.Text = row.Cells["colInvoice"].Value?.ToString() ?? "";           // Product Code
            textBox11.Text = row.Cells["colPosId"].Value?.ToString() ?? "";           // Product Description
            uom.Text = row.Cells["colInvoiceSynced"].Value?.ToString() ?? "";         // UOM
            textBox3.Text = row.Cells["colDueDate"].Value?.ToString() ?? "";          // Rate
            SalesValueExclST.Text = row.Cells["colStatus"].Value?.ToString() ?? "";   // Sales Excl ST
            qty.Text = row.Cells["colQuantity"].Value?.ToString() ?? "";              // Quantity

            // Remove row (user will re-add after editing)
            dataGridView1.Rows.RemoveAt(rowIndex);

            // Keep the in-memory list in sync
            if (rowIndex < addedItems.Count)
                addedItems.RemoveAt(rowIndex);

            UpdateSerialNumbers();
            lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
            textBox1.Focus();
        }

        private void ClearFormFields()
        {
            textBox1.Clear();       // Product Code
            uom.Clear();            // UOM
            textBox3.Clear();       // Rate
            textBox4.Clear();       // Total Values
            textBox5.Clear();       // Sales Tax Applicable
            textBox6.Clear();       // Extra Tax
            textBox7.Clear();       // Further Tax
            hscode.Clear();         // HS Code
            qty.Clear();            // Quantity
            mrp.Clear();            // Retail Price
            fed.Clear();            // FED Payable
            saletype.Clear();       // Sale Type
            srosche.Clear();        // SRO Schedule No
            SROScheduleNo.Clear();  // WHIT Section 1
            textBox11.Clear();      // Product Description
            SalesValueExclST.Clear(); // Value Sales Excluding ST
            STWithheld.Clear();     // ST Withheld at Source
            Discount.Clear();       // CVT

            // Also update the session form state
            _sessionFormState = new ItemEntryFormState();
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
                InvoiceType = chkSaleInvoice.Checked ? (short)1 : (short)2,
                InvoiceDate = txtInvoiceDate.Value,
                BuyerSellerName = txtBuyerBusiness.Text,
                DestinationAddress = txtBuyerAddress.Text,
                NTN_CNIC = txtBuyerRegNo.Text,
                DistributorName = txtSellerBusiness.Text,
                Distributor_NTN_CNIC = txtSellerRegNo.Text,
                SaleType = chkRegistered.Checked ? 1 : 2,
                TotalRetailPrice = 0 // will be computed by service or set before save
            };
        }

        private bool AreInvoiceFieldsValid()
        {
            // Basic checks for header completeness (loose - adjust if you want stricter)
            if (!chkSaleInvoice.Checked && !chkDebitInvoice.Checked) return false;
            if (!chkRegistered.Checked && !chkUnregistered.Checked) return false;
            if (string.IsNullOrWhiteSpace(txtSellerBusiness.Text)) return false;
            if (string.IsNullOrWhiteSpace(txtBuyerBusiness.Text)) return false;
            if (string.IsNullOrWhiteSpace(txtSellerRegNo.Text)) return false;
            if (string.IsNullOrWhiteSpace(txtBuyerRegNo.Text)) return false;
            // optional: require provinces if needed
            return true;
        }

        // Toggle handlers that you might wire in designer or in constructor (if not wired via designer)
        private void ChkSaleInvoice_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSaleInvoice.Checked) chkDebitInvoice.Checked = false;
        }

        private void ChkDebitInvoice_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDebitInvoice.Checked) chkSaleInvoice.Checked = false;
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
    }
}
