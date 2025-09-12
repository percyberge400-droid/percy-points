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
        //private static List<ItemEntryModel> _sessionItems = new List<ItemEntryModel>();
        private static ItemEntryFormState _sessionFormState = new ItemEntryFormState();

        // Instance reference to session data
        //private readonly List<ItemEntryModel> addedItems;

        public item_entry()
        {
            InitializeComponent();

            // Use static session data
            //addedItems = _sessionItems;

            // Restore previous session state
            //RestoreSessionState();

            // UI event wiring
            this.Resize += Item_entry_Resize;
            pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);

            btnProceed.Click += BtnProceed_Click;

            // Save button (must exist in designer)
            btnSave.Click += BtnSave_Click;

            // Grid editing & context menu
            //dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            SetupContextMenu();

            CaptureOriginalLayout();

            // Save form state when form is closing or losing focus
            this.FormClosing += Item_entry_FormClosing;
            this.Leave += Item_entry_Leave;
            this.Deactivate += Item_entry_Deactivate;
        }

        #region Session Management

        /// <summary>
        /// Class to hold form state data
        /// </summary>
        public class ItemEntryFormState
        {
            public string ItemCode { get; set; } = "";
            public string UOM { get; set; } = "";
            public string Rate { get; set; } = "";
            public string ItemName { get; set; } = "";
            public string TotalValues { get; set; } = "";
            public string SalesTaxApplicable { get; set; } = "";
            public string ExtraTax { get; set; } = "";
            public string FurtherTax { get; set; } = "";
            public string SroScheduleNo { get; set; } = "";
            public string HsCode { get; set; } = "";
            public string Quantity { get; set; } = "";
            public string MRP { get; set; } = "";
            public string Fed { get; set; } = "";
            public string SaleType { get; set; } = "";
            public string SalesValueExclST { get; set; } = "";
            public string STWithheld { get; set; } = "";
            public string Discount { get; set; } = "";
            public string SROScheduleNoSerial { get; set; } = "";
        }

        private void SaveCurrentFormState()
        {
            try
            {
                _sessionFormState.ItemCode = textBox1.Text;
                _sessionFormState.UOM = uom.Text;
                _sessionFormState.Rate = textBox3.Text;
                _sessionFormState.ItemName = textBox11.Text;
                _sessionFormState.TotalValues = textBox4.Text;
                _sessionFormState.SalesTaxApplicable = textBox5.Text;
                _sessionFormState.ExtraTax = textBox6.Text;
                _sessionFormState.FurtherTax = textBox7.Text;
                _sessionFormState.SroScheduleNo = srosche.Text;
                _sessionFormState.HsCode = hscode.Text;
                _sessionFormState.Quantity = qty.Text;
                _sessionFormState.MRP = mrp.Text;
                _sessionFormState.Fed = fed.Text;
                _sessionFormState.SaleType = saletype.Text;
                _sessionFormState.SalesValueExclST = SalesValueExclST.Text;
                _sessionFormState.STWithheld = STWithheld.Text;
                _sessionFormState.Discount = Discount.Text;
                _sessionFormState.SROScheduleNoSerial = SROScheduleNo.Text;
            }
            catch (Exception ex)
            {
                // Log error if needed, but don't show to user during navigation
                System.Diagnostics.Debug.WriteLine($"Error saving form state: {ex.Message}");
            }
        }

        //private void RestoreSessionState()
        //{
        //    try
        //    {
        //        // Restore textbox values
        //        textBox1.Text = _sessionFormState.ItemCode;
        //        uom.Text = _sessionFormState.UOM;
        //        textBox3.Text = _sessionFormState.Rate;
        //        textBox11.Text = _sessionFormState.ItemName;
        //        textBox4.Text = _sessionFormState.TotalValues;
        //        textBox5.Text = _sessionFormState.SalesTaxApplicable;
        //        textBox6.Text = _sessionFormState.ExtraTax;
        //        textBox7.Text = _sessionFormState.FurtherTax;
        //        srosche.Text = _sessionFormState.SroScheduleNo;
        //        hscode.Text = _sessionFormState.HsCode;
        //        qty.Text = _sessionFormState.Quantity;
        //        mrp.Text = _sessionFormState.MRP;
        //        fed.Text = _sessionFormState.Fed;
        //        saletype.Text = _sessionFormState.SaleType;
        //        SalesValueExclST.Text = _sessionFormState.SalesValueExclST;
        //        STWithheld.Text = _sessionFormState.STWithheld;
        //        Discount.Text = _sessionFormState.Discount;
        //        SROScheduleNo.Text = _sessionFormState.SROScheduleNoSerial;

        //        // Restore grid items
        //        RestoreGridItems();

        //        // Update total items label
        //        lblTotalItems.Text = $"Total {addedItems.Count} items";

        //        // Focus on first textbox if no data exists
        //        if (string.IsNullOrEmpty(_sessionFormState.ItemCode))
        //        {
        //            textBox1.Focus();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // If restore fails, initialize empty
        //        System.Diagnostics.Debug.WriteLine($"Error restoring session state: {ex.Message}");
        //        InitializeEmptyGrid();
        //    }
        //}

        //private void RestoreGridItems()
        //{
        //    dataGridView1.Rows.Clear();

        //    for (int i = 0; i < addedItems.Count; i++)
        //    {
        //        var item = addedItems[i];
        //        string srNo = (i + 1).ToString("D2");

        //        dataGridView1.Rows.Add(
        //            srNo,
        //            item.itemcode,
        //            item.itemname,
        //            item.SaleType,
        //            item.Rate,
        //            item.ValueSalesExcludingST.ToString("F2"),
        //            item.SalesTaxApplicable.ToString("F2"),
        //            item.Quantity.ToString()
        //        );
        //    }
        //}

        /// <summary>
        /// Clear the entire session (both grid and form data)
        /// </summary>
        //public static void ClearSession()
        //{
        //    _sessionItems.Clear();
        //    _sessionFormState = new ItemEntryFormState();
        //}

        /// <summary>
        /// Get current session item count
        /// </summary>
        //public static int GetSessionItemCount()
        //{
        //    return _sessionItems.Count;
        //}

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
            //deleteItem.Click += (s, e) => RemoveSelectedItem();
            contextMenu.Items.Add(deleteItem);

            dataGridView1.ContextMenuStrip = contextMenu;
        }

        //private ItemEntryModel GetTextboxData()
        //{
        //    return new ItemEntryModel
        //    {
        //        // Id will be generated by DB; not set here
        //        itemcode = textBox1.Text.Trim(),
        //        UOM = uom.Text.Trim(),
        //        Rate = textBox3.Text.Trim(),
        //        itemname = textBox11.Text.Trim(),
        //        TotalValues = decimal.TryParse(textBox4.Text, out var totalVal) ? totalVal : 0m,
        //        SalesTaxApplicable = decimal.TryParse(textBox5.Text, out var salesTax) ? salesTax : 0m,
        //        ExtraTax = decimal.TryParse(textBox6.Text, out var extraTax) ? extraTax : 0m,
        //        FurtherTax = decimal.TryParse(textBox7.Text, out var furtherTax) ? furtherTax : 0m,
        //        SroScheduleNo = srosche.Text.Trim(),
        //        HsCode = hscode.Text.Trim(),
        //        Quantity = int.TryParse(qty.Text, out var qtyVal) ? qtyVal : 0,
        //        FixedNotifiedValueOrRetailPrice = decimal.TryParse(mrp.Text, out var mrpVal) ? mrpVal : 0m,
        //        FedPayable = decimal.TryParse(fed.Text, out var fedVal) ? fedVal : 0m,
        //        SaleType = saletype.Text.Trim(),
        //        ValueSalesExcludingST = decimal.TryParse(SalesValueExclST.Text, out var exclST) ? exclST : 0m,
        //        SalesTaxWithheldAtSource = decimal.TryParse(STWithheld.Text, out var stWithheld) ? stWithheld : 0m,
        //        Discount = decimal.TryParse(Discount.Text, out var discountVal) ? discountVal : 0m,
        //        SroItemSerialNo = SROScheduleNo.Text.Trim()
        //    };
        //}

        //private bool ValidateItemEntry(ItemEntryModel inputData)
        //{
        //    // Item Code
        //    if (string.IsNullOrWhiteSpace(inputData.itemcode))
        //    {
        //        MessageBox.Show("Please enter an Item Code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        textBox1.Focus();
        //        return false;
        //    }

        //    // UOM
        //    if (string.IsNullOrWhiteSpace(inputData.UOM))
        //    {
        //        MessageBox.Show("Please select a UOM.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        uom.Focus();
        //        return false;
        //    }

        //    // Rate (we expect numeric value in Rate textbox; if Rate can contain % keep this check minimal)
        //    if (!decimal.TryParse(textBox3.Text, out var rate) || rate <= 0)
        //    {
        //        MessageBox.Show("Please enter a valid Rate greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        textBox3.Focus();
        //        return false;
        //    }

        //    // Sales Value Excluding ST
        //    if (!decimal.TryParse(SalesValueExclST.Text, out var exclST) || exclST < 0)
        //    {
        //        MessageBox.Show("Please enter a valid Sales Value Excluding ST (0 or higher).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        SalesValueExclST.Focus();
        //        return false;
        //    }

        //    // Sale Type
        //    if (string.IsNullOrWhiteSpace(inputData.SaleType))
        //    {
        //        MessageBox.Show("Please enter a Sale Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        saletype.Focus();
        //        return false;
        //    }

        //    // Quantity
        //    if (inputData.Quantity <= 0)
        //    {
        //        MessageBox.Show("Please enter a valid Quantity greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        qty.Focus();
        //        return false;
        //    }

        //    return true;
        //}

        #region Button Handlers (Add / Save)
        private void BtnProceed_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    ItemEntryModel inputData = GetTextboxData();

            //    if (!ValidateItemEntry(inputData))
            //        return;

            //    // detect duplicate by itemcode
            //    var existingRow = dataGridView1.Rows
            //        .Cast<DataGridViewRow>()
            //        .FirstOrDefault(r => (r.Cells["colInvoice"].Value?.ToString() ?? "") == inputData.itemcode);

            //    if (existingRow != null)
            //    {
            //        var result = MessageBox.Show($"Item Code '{inputData.itemcode}' already exists. Do you want to update it?",
            //            "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            //        if (result == DialogResult.Yes)
            //        {
            //            // update grid & in-memory list
            //            int rowIndex = existingRow.Index;
            //            UpdateExistingItem(existingRow, inputData);

            //            if (rowIndex < addedItems.Count)
            //                addedItems[rowIndex] = inputData;
            //        }
            //        else
            //        {
            //            textBox1.Focus();
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        // Add new row
            //        string newSrNo = (dataGridView1.Rows.Count + 1).ToString("D2");
            //        dataGridView1.Rows.Add(
            //            newSrNo,
            //            inputData.itemcode,
            //            inputData.itemname,
            //            inputData.SaleType,
            //            inputData.Rate,
            //            inputData.ValueSalesExcludingST.ToString("F2"),
            //            inputData.SalesTaxApplicable.ToString("F2"),
            //            inputData.Quantity.ToString()
            //        );

            //        addedItems.Add(inputData);
            //    }

            //    // Update UI
            //    lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
            //    ClearFormFields();

            //    MessageBox.Show($"Item '{inputData.itemcode}' added/updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    textBox1.Focus();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error processing item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        // Save button: persist session to DB (or for now show alert and clear)
        // Save button: persist session to DB
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (addedItems.Count == 0)
            //    {
            //        MessageBox.Show("No items to save.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }

            //    // Check if invoice data exists
            //    if (InvoiceEntry.CurrentInvoice == null)
            //    {
            //        MessageBox.Show("No invoice data found. Please go back to Invoice Entry and fill the required information.",
            //            "Missing Invoice Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }

            //    // Show loading cursor
            //    this.Cursor = Cursors.WaitCursor;

            //    using var context = new POSPRA.Infrastructure.Context.SqliteDbContext();

            //    // Ensure database is created
            //    await context.Database.EnsureCreatedAsync();

            //    // Start a transaction for data consistency
            //    using var transaction = await context.Database.BeginTransactionAsync();

            //    try
            //    {
            //        // Step 1: Save the Invoice first
            //        var invoiceToSave = new InvoiceModel
            //        {
            //            InvoiceType = InvoiceEntry.CurrentInvoice.InvoiceType,
            //            CustomerRegType = InvoiceEntry.CurrentInvoice.CustomerRegType,
            //            InvoiceDate = InvoiceEntry.CurrentInvoice.InvoiceDate,
            //            InvoiceRef = InvoiceEntry.CurrentInvoice.InvoiceRef,
            //            SellerBusiness = InvoiceEntry.CurrentInvoice.SellerBusiness,
            //            SellerAddress = InvoiceEntry.CurrentInvoice.SellerAddress,
            //            SellerProvince = InvoiceEntry.CurrentInvoice.SellerProvince,
            //            SellerRegNo = InvoiceEntry.CurrentInvoice.SellerRegNo,
            //            BuyerBusiness = InvoiceEntry.CurrentInvoice.BuyerBusiness,
            //            BuyerAddress = InvoiceEntry.CurrentInvoice.BuyerAddress,
            //            BuyerProvince = InvoiceEntry.CurrentInvoice.BuyerProvince,
            //            BuyerRegNo = InvoiceEntry.CurrentInvoice.BuyerRegNo
            //        };

            //        // Add invoice to context
            //        context.Invoices.Add(invoiceToSave);

            //        // Save to get the generated InvoiceID
            //        await context.SaveChangesAsync();

            //        // Step 2: Now save all items with the InvoiceID foreign key
            //        foreach (var item in addedItems)
            //        {
            //            var itemToSave = new ItemEntryModel
            //            {
            //                InvoiceID = invoiceToSave.InvoiceID, // Set the foreign key
            //                itemcode = item.itemcode,
            //                UOM = item.UOM,
            //                Rate = item.Rate,
            //                itemname = item.itemname,
            //                TotalValues = item.TotalValues,
            //                SalesTaxApplicable = item.SalesTaxApplicable,
            //                ExtraTax = item.ExtraTax,
            //                FurtherTax = item.FurtherTax,
            //                SroScheduleNo = item.SroScheduleNo,
            //                HsCode = item.HsCode,
            //                Quantity = item.Quantity,
            //                FixedNotifiedValueOrRetailPrice = item.FixedNotifiedValueOrRetailPrice,
            //                FedPayable = item.FedPayable,
            //                SaleType = item.SaleType,
            //                ValueSalesExcludingST = item.ValueSalesExcludingST,
            //                SalesTaxWithheldAtSource = item.SalesTaxWithheldAtSource,
            //                Discount = item.Discount,
            //                SroItemSerialNo = item.SroItemSerialNo
            //            };

            //            context.ItemEntry.Add(itemToSave);
            //        }

            //        // Save all items
            //        await context.SaveChangesAsync();

            //        // Commit the transaction
            //        await transaction.CommitAsync();

            //        // Success message
            //        MessageBox.Show($"Invoice saved successfully!\nInvoice ID: {invoiceToSave.InvoiceID}\nItems saved: {addedItems.Count}",
            //            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //        // Clear entire session after successful save
            //        ClearAllItems();
            //        ClearSession(); // This clears both grid and form data permanently

            //        // Reset invoice session as well
            //        InvoiceEntry.CurrentInvoice = null;
            //        InvoiceEntry.Proceeded = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        // Rollback transaction on error
            //        await transaction.RollbackAsync();
            //        throw; // Re-throw to outer catch block
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Save failed: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
            //        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //    // Log the error for debugging
            //    System.Diagnostics.Debug.WriteLine($"Save Error: {ex}");
            //}
            //finally
            //{
            //    // Restore cursor
            //    this.Cursor = Cursors.Default;
            //}
        }
        #endregion

        //private void UpdateExistingItem(DataGridViewRow row, ItemEntryModel inputData)
        //{
        //    // Make sure your column names match these keys in the designer
        //    row.Cells["colPosId"].Value = inputData.itemname;                      // Item Name
        //    row.Cells["colInvoiceSynced"].Value = inputData.SaleType;             // Sale Type
        //    row.Cells["colDueDate"].Value = inputData.Rate;                       // Rate (displayed)
        //    row.Cells["colStatus"].Value = inputData.ValueSalesExcludingST.ToString("F2"); // Sales Excl ST
        //    row.Cells["colQuantity"].Value = inputData.Quantity.ToString();       // Quantity
        //}

        //private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex >= 0) LoadItemForEditing(e.RowIndex);
        //}

        //private void LoadItemForEditing(int rowIndex)
        //{
        //    if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count) return;

        //    var row = dataGridView1.Rows[rowIndex];

        //    textBox1.Text = row.Cells["colInvoice"].Value?.ToString() ?? "";
        //    textBox11.Text = row.Cells["colPosId"].Value?.ToString() ?? "";
        //    saletype.Text = row.Cells["colInvoiceSynced"].Value?.ToString() ?? "";
        //    textBox3.Text = row.Cells["colDueDate"].Value?.ToString() ?? "";
        //    SalesValueExclST.Text = row.Cells["colStatus"].Value?.ToString() ?? "";
        //    qty.Text = row.Cells["colQuantity"].Value?.ToString() ?? "";

        //    // Remove row (user will add it back after editing)
        //    dataGridView1.Rows.RemoveAt(rowIndex);

        //    // Keep the in-memory list in sync
        //    if (rowIndex < addedItems.Count)
        //        addedItems.RemoveAt(rowIndex);

        //    UpdateSerialNumbers();
        //    lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
        //    textBox1.Focus();
        //}

        #region Form Management Helpers
        private void ClearFormFields()
        {
            textBox1.Clear();
            uom.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBox7.Clear();
            hscode.Clear();
            qty.Clear();
            mrp.Clear();
            fed.Clear();
            saletype.Clear();
            srosche.Clear();
            SROScheduleNo.Clear();
            textBox11.Clear();
            SalesValueExclST.Clear();
            STWithheld.Clear();
            Discount.Clear();

            // Also update the session form state
            _sessionFormState = new ItemEntryFormState();
        }

        //private void RemoveSelectedItem()
        //{
        //    if (dataGridView1.SelectedRows.Count > 0)
        //    {
        //        var result = MessageBox.Show("Are you sure you want to remove the selected item?",
        //            "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        //        if (result == DialogResult.Yes)
        //        {
        //            int selectedIndex = dataGridView1.SelectedRows[0].Index;
        //            if (selectedIndex < addedItems.Count)
        //                addedItems.RemoveAt(selectedIndex);

        //            dataGridView1.Rows.RemoveAt(selectedIndex);
        //            UpdateSerialNumbers();
        //            lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items";
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Please select a row to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //}

        private void UpdateSerialNumbers()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = (i + 1).ToString("D2");
            }
        }

        //public List<ItemEntryModel> GetAllItems()
        //{
        //    return addedItems.ToList();
        //}

        public void ClearAllItems()
        {
            dataGridView1.Rows.Clear();
            //addedItems.Clear();
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
            //RemoveSelectedItem();
        }
        #endregion
    }
}