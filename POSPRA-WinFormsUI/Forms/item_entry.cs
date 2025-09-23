using System.Drawing.Drawing2D;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDtos;

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
        private static List<InvoiceItems> _sessionItems = new();

        // Current invoice & item list
        public static Invoice CurrentInvoice;
        private readonly List<InvoiceItems> addedItems;
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

            btnProceed.Click += BtnProceed_Click;      // Add / Update item behaviour
            btnSave.Click += BtnSave_Click;            // Persist invoice + items
            btnEdit.Click += btnEdit_Click;
            btn_remove.Click += btn_remove_Click;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;

            // Field calculation events (only use RecalculateTotals)
            qty.TextChanged += RecalculateTotals;
            salevalue.TextChanged += RecalculateTotals;
            itemDiscount.TextChanged += RecalculateTotals;
            TaxRatebox.TextChanged += RecalculateTotals;
            FurtureTax.TextChanged += RecalculateTotals;

            SetupContextMenu();
            CaptureOriginalLayout();
            InitializeEmptyGrid();
        }

        #endregion

        #region Grid / Item Helpers

        private void InitializeEmptyGrid()
        {
            dataGridView1.Rows.Clear();
            // lblTotalItems.Text = "Total 0 items"; // Commented out - control doesn't exist
            ItemCode.Focus();
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
            decimal quantity = decimal.TryParse(qty.Text, out var q) ? q : 0m;
            decimal saleVal = decimal.TryParse(salevalue.Text, out var sv) ? sv : 0m;
            decimal taxRate = decimal.TryParse(TaxRatebox.Text, out var tr) ? tr : 0m;
            decimal discountPercent = decimal.TryParse(itemDiscount.Text, out var d) ? d : 0m;
            decimal furtherTax = decimal.TryParse(FurtureTax.Text, out var ft) ? ft : 0m;

            decimal subtotal = quantity * saleVal;
            decimal discountAmount = subtotal * (discountPercent / 100m);
            decimal afterDiscount = subtotal - discountAmount;
            decimal taxAmount = afterDiscount * (taxRate / 100m);
            decimal total = afterDiscount + taxAmount + furtherTax;

            return new InvoiceItems
            {
                ItemCode = ItemCode.Text.Trim(),
                ItemName = ItemName.Text.Trim(),
                PCTCode = pctCode.Text.Trim(),
                Quantity = quantity,
                SaleValue = saleVal,
                Discount = discountPercent,    // store percentage for consistency
                TaxRate = (double)taxRate,
                TaxCharged = taxAmount,
                FurtherTax = furtherTax,
                TotalAmount = total,
                InvoiceType = GetSelectedInvoiceType(),
                RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim()
            };
        }


        private bool ValidateItemEntry(InvoiceItems inputData)
        {
            // Item Code
            if (string.IsNullOrWhiteSpace(inputData.ItemCode))
            {
                MessageBox.Show("Please enter an Item Code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ItemCode.Focus();
                return false;
            }

            // Item Name
            if (string.IsNullOrWhiteSpace(inputData.ItemName))
            {
                MessageBox.Show("Please enter an Item Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ItemName.Focus();
                return false;
            }

            // Quantity
            if (inputData.Quantity <= 0)
            {
                MessageBox.Show("Please enter a valid Quantity greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                qty.Focus();
                return false;
            }

            // Sale Value
            if (inputData.SaleValue <= 0)
            {
                MessageBox.Show("Please enter a valid Sale Value greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                salevalue.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region Buttons: Proceed (Add/Update), Save, Edit, Remove

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

                // Ensure CurrentInvoice updated with header values
                CurrentInvoice = CollectInvoiceData();

                InvoiceItems inputData = GetTextboxData();

                if (!ValidateItemEntry(inputData))
                    return;

                // Check for duplicate by ItemCode
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

                // Update UI
                // lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items"; // Commented out - control doesn't exist
                ClearFormFields();

                MessageBox.Show($"Item '{inputData.ItemCode}' added/updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ItemCode.Focus();
                UpdateInvoiceTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

                // Ask for confirmation
                var confirm = MessageBox.Show(
                    "Are you sure you want to save this invoice?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                // Ensure CurrentInvoice is updated
                if (CurrentInvoice == null)
                {
                    CurrentInvoice = CollectInvoiceData();
                }

                // Map InvoiceItems to InvoiceItemDto
                var itemDtos = addedItems.Select(item => new InvoiceItemDto
                {
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    PCTCode = item.PCTCode,
                    Quantity = item.Quantity ?? 0m,           // decimal? → decimal
                    SaleValue = item.SaleValue ?? 0m,
                    TotalAmount = item.TotalAmount ?? 0m,
                    TaxCharged = item.TaxCharged ?? 0m,
                    TaxRate = item.TaxRate,
                    Discount = item.Discount ?? 0m,
                    FurtherTax = item.FurtherTax ?? 0m,
                    InvoiceType = (byte)GetSelectedInvoiceType(),
                    RefUSIN = string.IsNullOrWhiteSpace(refUSIN.Text) ? null : refUSIN.Text.Trim()
                }).ToList();


                // Map Invoice to InvoiceDto
                var invoiceDto = new InvoiceDto
                {
                    POSID = int.TryParse(posid.Text, out var bposId) ? bposId : 0,
                    InvoiceNumber = InvoiceNumber.Text.Trim(),
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
                    TotalSaleValue = decimal.TryParse(TotalSaleValue.Text, out var saleVal) ? saleVal : itemDtos.Sum(x => x.SaleValue),
                    TotalTaxCharged = decimal.TryParse(TotalTaxCharged.Text, out var taxCharged) ? taxCharged : itemDtos.Sum(x => x.TaxCharged),
                    Discount = decimal.TryParse(Discount.Text, out var discount) ? discount : itemDtos.Sum(x => x.Discount),
                    FurtherTax = decimal.TryParse(TotalFurtherTax.Text, out var furtherTax) ? furtherTax : itemDtos.Sum(x => x.FurtherTax),
                    DateTime = DateTime.Now,
                    InvoiceItemDto = itemDtos
                };

                // Post to fiscal service
                var output = await _fiscalService.CreateAsync(invoiceDto);

                if (output.StatusCode == "Success")
                {
                    MessageBox.Show(output.Message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear session
                    addedItems.Clear();
                    dataGridView1.Rows.Clear();
                    // lblTotalItems.Text = "Total 0 items"; // Commented out - control doesn't exist
                    CurrentInvoice = null;
                    _sessionItems.Clear();
                    UpdateInvoiceTotals();
                    ClearInvoiceFields();
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
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a row to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var row = dataGridView1.SelectedRows[0];
                string itemCode = row.Cells["colProductCode"].Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(itemCode))
                {
                    MessageBox.Show("Item code is missing for this row.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int itemIndex = addedItems.FindIndex(i => string.Equals(i.ItemCode?.Trim(), itemCode, StringComparison.OrdinalIgnoreCase));

                if (itemIndex < 0)
                {
                    MessageBox.Show($"Item with Item Code '{itemCode}' was not found in the current list.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var item = addedItems[itemIndex];

                // Populate form fields
                LoadItemForEditing(item);

                // Remove from memory + grid
                addedItems.RemoveAt(itemIndex);
                dataGridView1.Rows.Remove(row);

                // Update UI
                UpdateSerialNumbers();
                // lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items"; // Commented out - control doesn't exist
                UpdateInvoiceTotals();

                ItemCode.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while trying to edit the item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_remove_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        #endregion

        #region Grid Edit / Remove Helpers

        private void UpdateExistingItem(DataGridViewRow row, InvoiceItems inputData)
        {
            row.Cells["colProductCode"].Value = inputData.ItemCode ?? "";
            row.Cells["colProductDescription"].Value = inputData.ItemName ?? "";
            row.Cells["colHSCode"].Value = inputData.PCTCode ?? "";
            row.Cells["colQuantity"].Value = inputData.Quantity.ToString();
            row.Cells["colRate"].Value = inputData.SaleValue.ToString();
            row.Cells["colTotalValue"].Value = inputData.TotalAmount.ToString();
            row.Cells["colSalesTax"].Value = inputData.TaxRate.ToString();
            row.Cells["colExtraTax"].Value = inputData.TaxCharged.ToString();
            row.Cells["colFutureTax"].Value = inputData.FurtherTax.ToString();
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
                    // lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items"; // Commented out - control doesn't exist
                    UpdateInvoiceTotals();
                }
            }
        }

        private void LoadItemForEditing(InvoiceItems item)
        {
            ItemCode.Text = item.ItemCode ?? "";
            ItemName.Text = item.ItemName ?? "";
            pctCode.Text = item.PCTCode ?? "";
            qty.Text = item.Quantity.ToString();
            salevalue.Text = item.SaleValue.ToString();
            totalamount.Text = item.TotalAmount.ToString();
            TaxRatebox.Text = item.TaxRate.ToString();
            TaxCharged.Text = item.TaxCharged.ToString();
            itemDiscount.Text = item.Discount.ToString();
            FurtureTax.Text = item.FurtherTax.ToString();
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
            itemDiscount.Clear();
            FurtureTax.Clear();
        }

        private void ClearInvoiceFields()
        {
            posid.Clear();
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
                    // lblTotalItems.Text = $"Total {dataGridView1.Rows.Count} items"; // Commented out - control doesn't exist
                    UpdateInvoiceTotals();
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
                dataGridView1.Rows[i].Cells["colSrNo"].Value = (i + 1).ToString();
            }
        }

        private void AddItemToDataGrid(InvoiceItems item)
        {
            int rowIndex = dataGridView1.Rows.Add();
            var row = dataGridView1.Rows[rowIndex];

            row.Cells["colSrNo"].Value = (rowIndex + 1).ToString();
            row.Cells["colProductCode"].Value = item.ItemCode ?? "";
            row.Cells["colHSCode"].Value = item.PCTCode ?? "";
            row.Cells["colProductDescription"].Value = item.ItemName ?? "";

            row.Cells["colQuantity"].Value = (item.Quantity ?? 0m).ToString("0.00");
            row.Cells["colRate"].Value = (item.SaleValue ?? 0m).ToString("0.00");
            row.Cells["colDiscount"].Value = (item.Discount ?? 0m).ToString("0.00");

            // Sales value excluding sales tax: subtotal before taxes (quantity * rate - discount)
            decimal qty = item.Quantity ?? 0m;
            decimal rate = item.SaleValue ?? 0m;
            decimal discountAmt = item.Discount ?? 0m;
            decimal salesValueExcTax = Math.Round(qty * rate - discountAmt, 2);
            row.Cells["colSalesValueExcST"].Value = salesValueExcTax.ToString("0.00");

            row.Cells["colTotalValue"].Value = (item.TotalAmount ?? 0m).ToString("0.00");
            row.Cells["colSalesTax"].Value = (item.TaxRate).ToString("0.00");      // percent
            row.Cells["colExtraTax"].Value = (item.TaxCharged ?? 0m).ToString("0.00");    // absolute tax charged
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
            };
        }

        #endregion

        #region Calculations

        private void CalculateItemTotals()
        {
            decimal quantity = decimal.TryParse(qty.Text, out var q) ? q : 0m;
            decimal saleVal = decimal.TryParse(salevalue.Text, out var sv) ? sv : 0m;
            decimal taxRate = decimal.TryParse(TaxRatebox.Text, out var tr) ? tr : 0m; // percent
            decimal discountPercent = decimal.TryParse(itemDiscount.Text, out var d) ? d : 0m; // percent
            decimal furtherTax = decimal.TryParse(FurtureTax.Text, out var ft) ? ft : 0m;

            // Subtotal (without tax)
            decimal subtotal = quantity * saleVal;

            // Tax before discount
            decimal taxAmount = subtotal * (taxRate / 100m);

            // Apply discount on tax
            decimal taxDiscount = taxAmount * (discountPercent / 100m);
            decimal taxAfterDiscount = taxAmount - taxDiscount;

            // Final total
            decimal total = subtotal + taxAfterDiscount + furtherTax;

            // Update UI
            totalamount.Text = Math.Round(total, 2).ToString("0.00");
            TaxCharged.Text = Math.Round(taxAfterDiscount, 2).ToString("0.00");
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

            decimal totalQty = addedItems.Sum(i => i.Quantity ?? 0m);
            decimal totalSaleVal = addedItems.Sum(i => (i.Quantity ?? 0m) * (i.SaleValue ?? 0m));
            decimal totalDiscount = addedItems.Sum(i =>
            {
                var subtotal = (i.Quantity ?? 0m) * (i.SaleValue ?? 0m);
                return subtotal * ((i.Discount ?? 0m) / 100m);
            });
            decimal totalTaxCharged = addedItems.Sum(i => i.TaxCharged ?? 0m);
            decimal totalFurtherTax = addedItems.Sum(i => i.FurtherTax ?? 0m);
            decimal totalBillAmt = addedItems.Sum(i => i.TotalAmount ?? 0m);

            TotalQuantity.Text = totalQty.ToString("0.00");
            TotalSaleValue.Text = totalSaleVal.ToString("0.00");
            TotalTaxCharged.Text = totalTaxCharged.ToString("0.00");
            Discount.Text = totalDiscount.ToString("0.00");
            TotalFurtherTax.Text = totalFurtherTax.ToString("0.00");
            TotalBillAmount.Text = totalBillAmt.ToString("0.00");
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

        private bool AreInvoiceFieldsValid()
        {
            if (string.IsNullOrWhiteSpace(posid.Text)) return false;
            if (string.IsNullOrWhiteSpace(buyerntn.Text)) return false;
            if (string.IsNullOrWhiteSpace(BuyerBname.Text)) return false;
            if (string.IsNullOrWhiteSpace(buyerphone.Text)) return false;
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
    }
}