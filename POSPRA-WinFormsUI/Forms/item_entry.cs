using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA_WinFormsUI.AlertClasses;
using System.Configuration;
using System.Drawing.Drawing2D;

namespace POSPRA_WinFormsUI
{
    public partial class item_entry : Form
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
        private readonly IFiscalService _fiscalService;
        private bool _isSaving = false;

        #endregion

        #region Constructor / Initialization

        public item_entry(IFiscalService fiscalService, IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();

            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));

            // Load POSID from app.config
            posid.Text = ConfigurationManager.AppSettings["Username"] ?? "0";

            addedItems = _sessionItems;

            // Event wiring
            this.Resize += Item_entry_Resize;
            pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);
            panel1.Resize += (s, e) => MakeRoundedControl(panel1, 25);

            btnProceed.Click += BtnProceed_Click;
            btnSave.Click += BtnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btn_remove.Click += btn_remove_Click;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;

            qty.TextChanged += RecalculateTotals;
            salevalue.TextChanged += RecalculateTotals;
            itemDiscount.TextChanged += RecalculateTotals;
            TaxRatebox.TextChanged += RecalculateTotals;
            FurtureTax.TextChanged += RecalculateTotals;

            buyercnic.KeyPress += NumericOnlyWithLength_KeyPress;
            buyerntn.KeyPress += NumericOnlyWithLength_KeyPress;
            buyerphone.KeyPress += NumericOnlyWithLength_KeyPress;

            // invoice item fields
            ItemCode.KeyPress += NumericOnlyWithLength_KeyPress;
            pctCode.KeyPress += NumericOnlyWithLength_KeyPress;
            totalamount.KeyPress += NumericOnlyWithLength_KeyPress;
            TaxRatebox.KeyPress += NumericOnlyWithLength_KeyPress;
            itemDiscount.KeyPress += NumericOnlyWithLength_KeyPress;
            qty.KeyPress += NumericOnlyWithLength_KeyPress;
            salevalue.KeyPress += NumericOnlyWithLength_KeyPress;
            FurtureTax.KeyPress += NumericOnlyWithLength_KeyPress;
            TaxCharged.KeyPress += NumericOnlyWithLength_KeyPress;

            invoicetype.SelectedIndexChanged += Invoicetype_SelectedIndexChanged;

            SetupContextMenu();
            CaptureOriginalLayout();
            InitializeEmptyGrid();
        }

        #endregion

        #region NumericOnly_KeyPress
        private void NumericOnlyWithLength_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = sender as TextBox;

            // Allow only digits & control keys
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Enforce per-field max length
            if (!char.IsControl(e.KeyChar))
            {
                switch (tb.Name)
                {
                    case "refUSIN":
                        if (tb.Text.Length >= 7) e.Handled = true;
                        break;

                    case "buyercnic":
                        if (tb.Text.Length >= 13) e.Handled = true;
                        break;

                    case "buyerphone":
                        if (tb.Text.Length >= 13) e.Handled = true; // 13 max
                        break;

                    case "itemcode":
                        if (tb.Text.Length >= 8) e.Handled = true; // example: 8 digits max
                        break;

                    case "pctcode":
                        if (tb.Text.Length >= 8) e.Handled = true;
                        break;

                    case "quantity":
                        if (tb.Text.Length >= 5) e.Handled = true; // example: 5-digit limit
                        break;

                    case "totalamount":
                    case "salevalue":
                    case "taxrate":
                    case "discount":
                    case "furthertax":
                    case "taxcharged":
                        if (tb.Text.Length >= 10) e.Handled = true; // example: 10-digit numeric
                        break;
                }
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
                refUSIN.BackColor = SystemColors.Window;   // normal white textbox background
                refUSIN.ForeColor = SystemColors.ControlText;
            }
            else
            {
                refUSIN.ReadOnly = true;
                refUSIN.Clear();
                refUSIN.BackColor = SystemColors.Control;  // gray background (like disabled)
                refUSIN.ForeColor = SystemColors.GrayText;
            }
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

        #endregion

        #region Buttons: Proceed (Add/Update), Save, Edit, Remove

        private void BtnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                // Ensure invoice header is filled enough to create a CurrentInvoice
                // Light-weight check: if header is totally empty, ask user whether to continue adding items.
                // Do NOT run full validation here and do NOT change focus — full validation happens on Save.
                if (IsInvoiceHeaderEmptyForAdding())
                {
                    var res = MessageBox.Show(
                        "Invoice header appears empty. You can add items now and fill header details later before saving. Do you want to continue?",
                        "Invoice Header Empty",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (res == DialogResult.No)
                        return;
                }

                // Ensure CurrentInvoice updated with header values (may be empty)
                CurrentInvoice = CollectInvoiceData();


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

                //WindowsLocalAppNotification.Show("Success", $"Item '{inputData.ItemCode}' added/updated successfully.");
                AlertManager.ShowSuccess($"Item '{inputData.ItemCode}' added/updated successfully.");
                ItemCode.Focus();
                UpdateInvoiceTotals();
            }
            catch (Exception ex)
            {
                //WindowsLocalAppNotification.Show("Error", $"Error processing item: {ex.Message}");
                AlertManager.ShowError($"Error processing item: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // Prevent multiple simultaneous saves
            if (_isSaving)
            {
                WindowsLocalAppNotification.Show("Information", "Save operation is already in progress. Please wait...");
                AlertManager.ShowInfo("Save operation is already in progress. Please wait...");
                return;
            }

            try
            {
                _isSaving = true;
                btnSave.Enabled = false; // Disable the button during save
                btnSave.Text = "Saving..."; // Visual feedback

                if (addedItems == null || !addedItems.Any())
                {
                    WindowsLocalAppNotification.Show("Validation Error", "Please add at least one item before saving the invoice.");
                    AlertManager.ShowError("Please add at least one item before saving the invoice.");
                    return;
                }

                if (!AreInvoiceFieldsValid())
                {
                    WindowsLocalAppNotification.Show("Validation Error", "Invoice header is incomplete. Please fill in the invoice header before saving.");
                    AlertManager.ShowError("Invoice header is incomplete. Please fill in the invoice header before saving.");
                    return;
                }

                var confirm = MessageBox.Show(
                    "Are you sure you want to save this invoice?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                // Create a fresh copy of the invoice data
                CurrentInvoice = CollectInvoiceData();

                // Create item DTOs from current items
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
                    TotalSaleValue = decimal.TryParse(TotalSaleValue.Text, out var saleVal) ? saleVal : itemDtos.Sum(x => x.SaleValue),
                    TotalTaxCharged = decimal.TryParse(TotalTaxCharged.Text, out var taxCharged) ? taxCharged : itemDtos.Sum(x => x.TaxCharged),
                    Discount = decimal.TryParse(Discount.Text, out var discount) ? discount : itemDtos.Sum(x => x.Discount),
                    FurtherTax = decimal.TryParse(TotalFurtherTax.Text, out var furtherTax) ? furtherTax : itemDtos.Sum(x => x.FurtherTax),
                    DateTime = DateTime.Now,
                    InvoiceItemDto = itemDtos
                };

                //WindowsLocalAppNotification.Show("Information", "Saving invoice...");
                AlertManager.ShowInfo("Saving invoice...");

                var output = await _fiscalService.CreateAsync(invoiceDto);

                if (output.StatusCode == ApiStatusCode.Success)
                {
                    WindowsLocalAppNotification.Show("Success", output.Message);
                    AlertManager.ShowSuccess(output.Message);

                    // -----------------------------
                    // CLEAR ALL DTOs AND UI FIELDS ONLY AFTER SUCCESSFUL SAVE
                    // -----------------------------
                    addedItems.Clear();                 // Clear item DTO list
                    CurrentInvoice = null;              // Clear main invoice DTO
                    _sessionItems.Clear();              // Clear session list if used
                    dataGridView1.Rows.Clear();         // Clear grid
                    ClearInvoiceFields();               // Reset invoice header fields
                    UpdateInvoiceTotals();              // Reset totals
                }
                else
                {
                    WindowsLocalAppNotification.Show("Error", output.Message);
                    AlertManager.ShowError(output.Message);
                }
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Error", $"Error saving invoice: {ex.Message}");
                AlertManager.ShowError($"Error saving invoice: {ex.Message}");
            }
            finally
            {
                // Always restore button state
                _isSaving = false;
                btnSave.Enabled = true;
                btnSave.Text = "Save";
            }
        }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    //WindowsLocalAppNotification.Show("No Selection", "Please select a row to edit.");
                    AlertManager.ShowError("Please select a row to edit.");
                    return;
                }

                var row = dataGridView1.SelectedRows[0];
                string itemCode = row.Cells["colProductCode"].Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(itemCode))
                {
                    //WindowsLocalAppNotification.Show("Error", "Item code is missing for this row.");
                    AlertManager.ShowError("Item code is missing for this row.");
                    return;
                }

                int itemIndex = addedItems.FindIndex(i => string.Equals(i.ItemCode?.Trim(), itemCode, StringComparison.OrdinalIgnoreCase));

                if (itemIndex < 0)
                {
                    //WindowsLocalAppNotification.Show("Not Found", $"Item with Item Code '{itemCode}' was not found in the current list.");
                    AlertManager.ShowError($"Item with Item Code '{itemCode}' was not found in the current list.");
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
                //WindowsLocalAppNotification.Show("Error", $"Error while trying to edit the item: {ex.Message}");
                AlertManager.ShowError($"Error while trying to edit the item: {ex.Message}");
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
                //WindowsLocalAppNotification.Show("No Selection", "Please select a row to delete.");
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
            return string.IsNullOrWhiteSpace(posid.Text)   // posid may be prefilled from config but keep check anyway
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
            else if (!buyerntn.Text.All(char.IsDigit) || buyerntn.Text.Length != 7)
            {
                MessageBox.Show("Buyer NTN must be exactly 7 digits.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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