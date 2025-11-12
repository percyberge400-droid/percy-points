using Pos.Domain.Entities;

namespace POSPRA.Application.Services.FiscalService
{
    /// <summary>
    /// Simple DTO to return overall validity and all validation messages.
    /// </summary>
    public record ValidationResult(bool IsValid, string ErrorMessages);

    public class InvoiceValidatorService
    {
        private const decimal MAX_SAFE_VALUE = 999_999_999_999_999_999m;
        private const int MAX_DECIMAL_PLACES = 2;

        /// <summary>
        /// Validates an entire invoice, including all items.
        /// </summary>
        public ValidationResult ValidateInvoice(Invoice invoice)
        {
            if (invoice == null)
                return new ValidationResult(false, "Invoice cannot be null.");

            var errors = new List<string>();
            bool isValid = true;

            void AddError(string msg)
            {
                errors.Add(msg);
                isValid = false;
            }

            static bool IsEmpty(string? s) => string.IsNullOrWhiteSpace(s);
            static string TrimSafe(string? s) => s?.Trim() ?? string.Empty;

            // -------- POS ID --------
            if (invoice.POSID == 0)
                AddError("POS ID is required");
            else
            {
                string posIdStr = invoice.POSID.ToString();
                if (!posIdStr.All(char.IsDigit))
                    AddError("POS ID must be numeric");
            }

            // -------- Buyer Info --------
            ValidateBuyer(invoice, AddError);

            // -------- Invoice Details --------
            if (invoice.InvoiceType < 1 || invoice.InvoiceType > 4)
                AddError("Invalid invoice type");

            if ((invoice.InvoiceType == 3 || invoice.InvoiceType == 4) && IsEmpty(invoice.RefUSIN))
                AddError("Ref USIN required for debit/credit invoices");

            // Validate RefUSIN length if provided
            if (!IsEmpty(invoice.RefUSIN) && TrimSafe(invoice.RefUSIN).Length > 50)
                AddError("Ref USIN cannot exceed 50 characters");

            // Validate USIN length if provided
            if (!IsEmpty(invoice.USIN) && TrimSafe(invoice.USIN).Length > 50)
                AddError("USIN cannot exceed 50 characters");

            if (invoice.PaymentMode < 1 || invoice.PaymentMode > 3)
                AddError("Payment mode must be between 1 and 3");

            if (invoice.EntryDate == DateTime.MinValue)
                AddError("Invalid invoice date");

            // Validate invoice-level totals with overflow protection
            if (!ValidateSafeDecimal(invoice.TotalBillAmount, "Total bill amount", AddError))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            if (!ValidateSafeDecimal(invoice.TotalQuantity, "Total quantity", AddError))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            if (!ValidateSafeDecimal(invoice.TotalSaleValue, "Total sale value", AddError, allowZero: true))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            if (!ValidateSafeDecimal(invoice.TotalTaxCharged, "Total tax charged", AddError, allowZero: true))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            if (!ValidateSafeDecimal(invoice.Discount, "Discount", AddError, allowZero: true))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            // -------- Invoice Items --------
            if (invoice.InvoiceItems == null || invoice.InvoiceItems.Count == 0)
            {
                AddError("Invoice must contain at least one item");
            }
            else
            {
                int index = 0;
                foreach (var item in invoice.InvoiceItems)
                {
                    index++;
                    string prefix = $"Item {index}: ";

                    var itemResult = ValidateInvoiceItem(item);
                    if (!itemResult.IsValid)
                    {
                        foreach (var line in itemResult.ErrorMessages.Split(Environment.NewLine))
                            AddError($"{prefix}{line}");
                    }

                    // Validate calculations
                    ValidateItemCalculations(item, AddError, prefix);
                }

                // Validate invoice-level totals match sum of items
                ValidateInvoiceTotals(invoice, AddError);
            }

            string message = errors.Count > 0
                ? string.Join(Environment.NewLine, errors)
                : string.Empty;

            return new ValidationResult(isValid, message);
        }

        private void ValidateBuyer(Invoice invoice, Action<string> AddError)
        {
            static bool IsEmpty(string? s) => string.IsNullOrWhiteSpace(s);
            static string TrimSafe(string? s) => s?.Trim() ?? string.Empty;

            // Buyer Name - Optional but validated if provided
            var name = TrimSafe(invoice.BuyerName);
            if (!IsEmpty(name))
            {
                if (name.Length > 150)
                    AddError("Buyer name cannot exceed 150 characters");
                if (!name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                    AddError("Buyer name must contain only letters and spaces");
            }

            // Buyer CNIC - Optional but validated if provided
            var cnic = TrimSafe(invoice.BuyerCNIC);
            if (!IsEmpty(cnic))
            {
                if (!cnic.All(char.IsDigit))
                    AddError("Buyer CNIC must be 13 digits");
                else if (cnic.Length != 13)
                    AddError("Buyer CNIC must be 13 digits");
            }

            // Buyer NTN - Optional but validated if provided (7-8 alphanumeric)
            var ntn = TrimSafe(invoice.BuyerNTN);
            if (!IsEmpty(ntn))
            {
                if (!ntn.All(char.IsLetterOrDigit))
                    AddError("Buyer NTN must be alphanumeric");
                else if (ntn.Length != 7)
                    AddError("Buyer NTN must be 7 characters");
            }

            // Buyer Phone - Optional but validated if provided
            var phone = TrimSafe(invoice.BuyerPhoneNumber);
            if (!IsEmpty(phone))
            {
                if (!phone.All(char.IsDigit))
                    AddError("Buyer phone must be numeric");
                else if (phone.Length != 11 && phone.Length != 13)
                    AddError("Buyer phone must be 11 or 13 digits");
            }
        }

        /// <summary>
        /// Validates a single invoice item independently.
        /// </summary>
        public ValidationResult ValidateInvoiceItem(InvoiceItems item)
        {
            if (item == null)
                return new ValidationResult(false, "Item cannot be null");

            var errors = new List<string>();
            bool isValid = true;

            void AddError(string msg)
            {
                errors.Add(msg);
                isValid = false;
            }

            static bool IsEmpty(string? s) => string.IsNullOrWhiteSpace(s);
            static string TrimSafe(string? s) => s?.Trim() ?? string.Empty;

            // Item Code - Required, 8 digits max
            var code = TrimSafe(item.ItemCode);
            if (IsEmpty(code))
                AddError("Item code is required");
            else if (!code.All(char.IsDigit))
                AddError("Item code must be numeric");
            else if (code.Length > 8)
                AddError("Item code cannot exceed 8 digits");

            // PCT/HS Code - Required, exactly 8 digits
            var pct = TrimSafe(item.PCTCode);
            if (IsEmpty(pct))
                AddError("HS code is required");
            else if (!pct.All(char.IsDigit))
                AddError("HS code must be numeric");
            else if (pct.Length != 8)
                AddError("HS code must be exactly 8 digits");

            // Item Name - Required, max 150 characters
            var itemName = TrimSafe(item.ItemName);
            if (IsEmpty(itemName))
                AddError("Item description is required");
            else if (itemName.Length > 150)
                AddError("Item description cannot exceed 150 characters");

            // Quantity - Required, must be > 0, decimal(18,2)
            if (!ValidateSafeDecimal(item.Quantity, "Quantity", AddError))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            // Unit Price / Sale Value - Required, must be > 0, decimal(18,2)
            if (!ValidateSafeDecimal(item.SaleValue, "Unit price", AddError))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            // Tax Rate - 0-100, decimal(5,2)
            if (item.TaxRate < 0 || item.TaxRate > 100)
                AddError("Tax rate must be between 0 and 100");
            else if (DecimalPlaces((decimal)item.TaxRate) > MAX_DECIMAL_PLACES)
                AddError("Tax rate cannot have more than 2 decimal places");

            // Discount - Optional, 0-100, decimal(5,2) (this is discount percentage)
            // Note: In the calculation, we convert percentage to amount
            if (item.Discount.HasValue)
            {
                var disountRS = Math.Round(item.Discount.Value, 2);

                if (disountRS < 0)
                    AddError("Discount cannot be negative");
                else if (DecimalPlaces(disountRS) > MAX_DECIMAL_PLACES)
                    AddError("Discount cannot have more than 2 decimal places");
            }

            // Tax Charged - Must be >= 0, decimal(18,2)
            var taxCharged = Math.Round(item.TaxCharged.Value, 2);
            if (!ValidateSafeDecimal(taxCharged, "Tax charged", AddError, allowZero: true))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            // Total Amount - Required, must be > 0, decimal(18,2)
            var totalRS = Math.Round(item.TotalAmount.Value, 2);
            if (!ValidateSafeDecimal(totalRS, "Total amount", AddError))
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));

            // RefUSIN check for Debit/Credit items
            if ((item.InvoiceType == 3 || item.InvoiceType == 4) && IsEmpty(item.RefUSIN))
                AddError("Ref USIN required for debit/credit items");

            return new ValidationResult(isValid, string.Join(Environment.NewLine, errors));
        }

        private void ValidateItemCalculations(InvoiceItems item, Action<string> AddError, string prefix)
        {
            if (!item.Quantity.HasValue || !item.SaleValue.HasValue)
                return;

            // Overflow-safe calculations
            if (!TryMultiply(item.Quantity.Value, item.SaleValue.Value, out decimal grossAmount))
            {
                AddError($"{prefix}Values too large for calculation");
                return;
            }

            // Tax is calculated on gross amount BEFORE discount
            decimal taxAmount = 0m;
            if (item.TaxRate > 0)
            {
                if (!TryMultiply(grossAmount, (decimal)item.TaxRate / 100m, out taxAmount))
                {
                    AddError($"{prefix}Tax calculation overflow");
                    return;
                }
            }

            // Amount after adding tax
            if (!TryAdd(grossAmount, taxAmount, out decimal amountAfterTax))
            {
                AddError($"{prefix}Total calculation overflow");
                return;
            }

            // Discount is calculated as percentage of amount after tax
            decimal discountAmount = item.Discount ?? 0m;

            // Final total
            decimal expectedTotal = Math.Round(Math.Max(0, amountAfterTax - discountAmount), 2);
            decimal actualTotal = Math.Round(item.TotalAmount ?? 0m, 2);

            // Allow 2 cent tolerance for rounding differences
            if (Math.Abs(expectedTotal - actualTotal) > 0.02m)
                AddError($"{prefix}Total mismatch. Expected {expectedTotal:F2}, got {actualTotal:F2}");

            // Validate tax charged
            if (item.TaxCharged.HasValue)
            {
                decimal expectedTax = Math.Round(taxAmount, 2);
                decimal actualTax = Math.Round(item.TaxCharged.Value, 2);
                if (Math.Abs(expectedTax - actualTax) > 0.02m)
                    AddError($"{prefix}Tax mismatch. Expected {expectedTax:F2}, got {actualTax:F2}");
            }

            // Discount cannot exceed amount after tax
            if (discountAmount > amountAfterTax)
                AddError($"{prefix}Discount cannot exceed total after tax");
        }

        private void ValidateInvoiceTotals(Invoice invoice, Action<string> AddError)
        {
            decimal sumQty = invoice.InvoiceItems.Sum(i => i.Quantity ?? 0m);
            if (Math.Abs((invoice.TotalQuantity ?? 0m) - sumQty) > 0.02m)
                AddError($"Total quantity mismatch: {invoice.TotalQuantity:F2} vs {sumQty:F2}");

            decimal sumSale = invoice.InvoiceItems.Sum(i => (i.Quantity ?? 0m) * (i.SaleValue ?? 0m));
            if (Math.Abs((invoice.TotalSaleValue ?? 0m) - sumSale) > 0.02m)
                AddError($"Total sale value mismatch: {invoice.TotalSaleValue:F2} vs {sumSale:F2}");

            decimal sumTax = invoice.InvoiceItems.Sum(i => i.TaxCharged ?? 0m);
            if (Math.Abs((invoice.TotalTaxCharged ?? 0m) - sumTax) > 0.02m)
                AddError($"Total tax mismatch: {invoice.TotalTaxCharged:F2} vs {sumTax:F2}");

            decimal sumDiscount = invoice.InvoiceItems.Sum(i => i.Discount ?? 0m);
            if (Math.Abs((invoice.Discount ?? 0m) - sumDiscount) > 0.02m)
                AddError($"Total discount mismatch: {invoice.Discount:F2} vs {sumDiscount:F2}");

            decimal sumTotal = invoice.InvoiceItems.Sum(i => i.TotalAmount ?? 0m);
            if (Math.Abs((invoice.TotalBillAmount ?? 0m) - sumTotal) > 0.02m)
                AddError($"Total bill amount mismatch: {invoice.TotalBillAmount:F2} vs {sumTotal:F2}");
        }

        /// <summary>
        /// Validates decimal field with overflow protection
        /// </summary>
        private bool ValidateSafeDecimal(decimal? value, string fieldName, Action<string> AddError, bool allowZero = false)
        {
            if (!value.HasValue || (!allowZero && value <= 0))
            {
                AddError($"{fieldName} must be greater than zero");
                return false;
            }

            if (allowZero && value < 0)
            {
                AddError($"{fieldName} cannot be negative");
                return false;
            }

            decimal v = value.Value;
            if (v > MAX_SAFE_VALUE)
            {
                AddError($"{fieldName} exceeds limit");
                return false;
            }

            if (DecimalPlaces(v) > MAX_DECIMAL_PLACES)
            {
                AddError($"{fieldName} cannot have more than 2 decimal places");
                return false;
            }

            return true;
        }

        // Safe arithmetic helpers
        private bool TryMultiply(decimal a, decimal b, out decimal result)
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

        private bool TryAdd(decimal a, decimal b, out decimal result)
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

        private int DecimalPlaces(decimal value)
        {
            return BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        }
    }
}
