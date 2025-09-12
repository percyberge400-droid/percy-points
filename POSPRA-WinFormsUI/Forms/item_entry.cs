using POSPRA.Application.Services.FiscalService;
using POSPRA.Domain.Entities;
using System.Drawing.Drawing2D;

namespace POSPRA_WinFormsUI
{
    public partial class item_entry : Form
    {
        // Original bounds, parent sizes, and fonts (for responsive logic)
        private readonly Dictionary<Control, Rectangle> _originalBounds = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Size> _originalParentSizes = new Dictionary<Control, Size>();
        private readonly Dictionary<Control, Font> _originalFonts = new Dictionary<Control, Font>();
        private Size _originalClientSize = Size.Empty;
        private bool _originalLayoutCaptured = false;

        // Static session data to persist across form instances
        private static List<InvoiceItemDetail> _sessionItems = new List<InvoiceItemDetail>();
        private static ItemEntryFormState _sessionFormState = new ItemEntryFormState();

        // Instance reference to session data
        private readonly List<InvoiceItemDetail> addedItems;
        private readonly IFiscalService _fiscalService;

        public item_entry(IFiscalService fiscalService)
        {
            InitializeComponent();

            // Use static session data
            addedItems = _sessionItems;

            // Restore previous session state
            RestoreSessionState();

            // UI event wiring
            this.Resize += Item_entry_Resize;
            pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);

            btnProceed.Click += BtnProceed_Click;

            // Save button (must exist in designer)
            btnSave.Click += BtnSave_Click;

            // Grid editing & context menu
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            SetupContextMenu();

            CaptureOriginalLayout();

            // Save form state when form is closing or losing focus
            this.FormClosing += Item_entry_FormClosing;
            this.Leave += Item_entry_Leave;
            this.Deactivate += Item_entry_Deactivate;
            _fiscalService=fiscalService;
        }

        #region Session Management

        /// <summary>
        /// Class to hold form state data
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
            public string WHIT_Section_2 { get; set; } = "";
        }

        private void SaveCurrentFormState()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                // Log error if needed, but don't show to user during navigation
                System.Diagnostics.Debug.WriteLine($"Error saving form state: {ex.Message}");
            }
        }

        private void RestoreSessionState()
        {
            try
            {
                // Restore textbox values
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

                // Restore grid items
                RestoreGridItems();

                // Update total items label
                lblTotalItems.Text = $"Total {addedItems.Count} items";

                // Focus on first textbox if no data exists
                if (string.IsNullOrEmpty(_sessionFormState.ProductCode))
                {
                    textBox1.Focus();
                }
            }
            catch (Exception ex)
            {
                // If restore fails, initialize empty
                System.Diagnostics.Debug.WriteLine($"Error restoring session state: {ex.Message}");
                InitializeEmptyGrid();
            }
        }

        private void RestoreGridItems()
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
                SroScheduleNo = int.TryParse(srosche.Text, out var sroVal) ? sroVal : null,
                HSCode = hscode.Text.Trim(),
                Quantity = decimal.TryParse(qty.Text, out var qtyVal) ? qtyVal : 0m,
                RetailPrice = decimal.TryParse(mrp.Text, out var retailPrice) ? retailPrice : 0m,
                FedPayable = decimal.TryParse(fed.Text, out var fedVal) ? fedVal : null,
                ValueSalesExcludingST = decimal.TryParse(SalesValueExclST.Text, out var exclST) ? exclST : 0m,
                STWithheldAtSource = decimal.TryParse(STWithheld.Text, out var stWithheld) ? stWithheld : null,
                CVT = decimal.TryParse(Discount.Text, out var cvt) ? cvt : null,
                WHIT_Section_1 = SROScheduleNo.Text.Trim(),
                // Note: WHIT_1, WHIT_2, WHIT_Section_2 would need additional form fields if required
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

        #region Button Handlers (Add / Save)
        private void BtnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                InvoiceItemDetail inputData = GetTextboxData();

                if (!ValidateItemEntry(inputData))
                    return;

                // detect duplicate by ProductCode
                var existingRow = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(r => (r.Cells["colInvoice"].Value?.ToString() ?? "") == inputData.ProductCode);

                if (existingRow != null)
                {
                    var result = MessageBox.Show($"Product Code '{inputData.ProductCode}' already exists. Do you want to update it?",
                        "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // update grid & in-memory list
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

        // Save button: persist session to DB
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            //var output = await _fiscalService.CreateAsync();
        }
        #endregion

        private void UpdateExistingItem(DataGridViewRow row, InvoiceItemDetail inputData)
        {
            // Make sure your column names match these keys in the designer
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

            // Remove row (user will add it back after editing)
            dataGridView1.Rows.RemoveAt(rowIndex);

            // Keep the in-memory list in sync
            if (rowIndex < addedItems.Count)
                addedItems.RemoveAt(rowIndex);

            UpdateSerialNumbers();
            lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
            textBox1.Focus();
        }

        #region Form Management Helpers
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
            saletype.Clear();       // Sale Type (not directly mapped to InvoiceItemDetail)
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

        #region Rounded Corners / Responsive Layout
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

        private void btn_remove_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }
        #endregion

        private void btnSave_Click_1(object sender, EventArgs e)
        {

        }
    }
}