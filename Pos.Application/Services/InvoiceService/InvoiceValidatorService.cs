using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Domain.Entities;

namespace POSPRA.Application.Services.FiscalService
{
    /// <summary>
    /// Simple DTO to return overall validity and all validation messages.
    /// </summary>
    public record ValidationResult(bool IsValid, string ErrorMessages);

    public class InvoiceValidatorService
    {
        private const decimal MAX_SAFE_VALUE = 2_000_000_000_000_000_000m; // Safe for worst-case: qty*unitprice*100% tax
        private const decimal MAX_QTY = 99_999.99m;                   // (5,2) for quantity - 8 digits before decimal
        private const decimal MAX_UNIT_PRICE = 9_999_999_999.99m;        // (10,2) for unit price
        private const int MAX_DECIMAL_PLACES = 2;
        private const int MAX_POSID_DIGITS = 6;
        private readonly AppSettings _settings;

        public InvoiceValidatorService(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

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
            {
                AddError("POS ID is required and must be greater than 0");
            }
            else
            {
                string posIdStr = invoice.POSID.ToString();
                if (!posIdStr.All(char.IsDigit))
                {
                    AddError("POS ID must be numeric");
                }
                else if (posIdStr.Length > MAX_POSID_DIGITS)
                {
                    AddError($"POS ID cannot exceed {MAX_POSID_DIGITS} digits");
                }
                else if (invoice.POSID != _settings.POS)
                {
                    AddError($"POS ID does not match.");
                }
            }

            // -------- USIN --------
            if (IsEmpty(invoice.USIN))
            {
                AddError("USIN is required");
            }
            else if (TrimSafe(invoice.USIN).Length > 50)
            {
                AddError("USIN cannot exceed 50 characters");
            }

            // -------- Buyer Info --------
            ValidateBuyer(invoice, AddError);

            // -------- Invoice Details --------
            if (invoice.InvoiceType < 1 || invoice.InvoiceType > 4)
                AddError("Invalid invoice type (must be 1-4: Purchase/Sale/Debit/Credit)");

            if ((invoice.InvoiceType == 3 || invoice.InvoiceType == 4) && IsEmpty(invoice.RefUSIN))
                AddError("Ref USIN required for debit/credit invoices");

            // Validate RefUSIN length if provided
            if (!IsEmpty(invoice.RefUSIN) && TrimSafe(invoice.RefUSIN).Length > 50)
                AddError("Ref USIN cannot exceed 50 characters");

            if (invoice.PaymentMode < 1 || invoice.PaymentMode > 6)
                AddError("Payment mode must from 1 to 6 (Cash/Card/Gift Voucher/Loyalty Card/Mixed/Cheque)");

            // Validate USIN length if provided
            if (!IsEmpty(invoice.USIN) && TrimSafe(invoice.USIN).Length > 50)
                AddError("USIN cannot exceed 50 characters");

            // -------- DateTime --------
            if (invoice.DateTime == DateTime.MinValue)
            {
                AddError("Date is required");
            }
            else if (invoice.DateTime.Date > DateTime.Now.Date)
            {
                AddError("Date cannot be in the future");
            }

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
            if (invoice.Items == null || invoice.Items.Count == 0)
            {
                AddError("Invoice must contain at least one item");
            }
            else
            {
                int index = 0;
                foreach (var item in invoice.Items)
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

            // ✅ Buyer Name - Optional, max 150 chars, letters and spaces only
            var name = TrimSafe(invoice.BuyerName);
            if (!IsEmpty(name))
            {
                if (name.Length > 150)
                    AddError("Buyer name cannot exceed 150 characters");
                if (!name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                    AddError("Buyer name must contain only letters and spaces");
            }

            // ✅ Buyer CNIC - Optional, exactly 13 digits
            var cnic = TrimSafe(invoice.BuyerCNIC);
            if (!IsEmpty(cnic))
            {
                if (!cnic.All(char.IsDigit))
                    AddError("Buyer CNIC must be numeric");
                else if (cnic.Length != 13 && cnic.Length != 15)
                    AddError("Buyer CNIC must be 13 or 15 digits");

            }

            // ✅ UPDATED: Buyer NTN - Optional, 7-9 alphanumeric (changed from 7-8)
            var ntn = TrimSafe(invoice.BuyerNTN);
            if (!IsEmpty(ntn))
            {
                if (!ntn.All(char.IsLetterOrDigit))
                    AddError("Buyer NTN must be alphanumeric");
                else if (ntn.Length < 7 || ntn.Length > 9)
                    AddError("Buyer NTN must be 7-9 alphanumeric characters");
            }

            // ✅ UPDATED: Buyer Phone - Optional, up to 20 characters (changed from 11/13)
            var phone = TrimSafe(invoice.BuyerPhoneNumber);
            if (!IsEmpty(phone))
            {
                if (phone.Length > 20)
                    AddError("Buyer phone cannot exceed 20 characters");

                // Allow digits, spaces, +, -, (, )
                if (!phone.All(c => char.IsDigit(c) || c == ' ' || c == '+' || c == '-' || c == '(' || c == ')'))
                    AddError("Buyer phone contains invalid characters");
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

            // ✅ Item Code - Required, max 50 characters
            var code = TrimSafe(item.ItemCode);
            if (IsEmpty(code))
                AddError("Item code is required");
            else if (code.Length > 50)
                AddError("Item code cannot exceed 50 characters");

            // ✅ PCT/HS Code - Required, exactly 8 digits
            var pct = TrimSafe(item.PCTCode);
            if (IsEmpty(pct))
                AddError("HS code is required");
            else if (!pct.All(char.IsDigit))
                AddError("HS code must be numeric");
            else if (pct.Length != 8)
                AddError("HS code must be exactly 8 digits");

            // ✅ Item Name/Description - Required, max 150 characters
            var itemName = TrimSafe(item.ItemName);
            if (IsEmpty(itemName))
                AddError("Item description is required");
            else if (itemName.Length > 150)
                AddError("Item description cannot exceed 150 characters");

            // ✅ Quantity - Required, must be > 0, max 99,999.99 (8 digits total)
            if (!item.Quantity.HasValue || item.Quantity <= 0)
            {
                AddError("Quantity is required and must be greater than zero");
            }
            else if (item.Quantity > MAX_QTY)
            {
                AddError($"Quantity cannot exceed {MAX_QTY:F2}");
            }
            else if (DecimalPlaces(item.Quantity.Value) > MAX_DECIMAL_PLACES)
            {
                AddError("Quantity cannot have more than 2 decimal places");
            }

            // ✅ Unit Price / Sale Value - Required, must be > 0, max 9,999,999,999.99 (10 digits before decimal)
            if (!item.SaleValue.HasValue || item.SaleValue <= 0)
            {
                AddError("Unit price is required and must be greater than zero");
            }
            else if (item.SaleValue > MAX_UNIT_PRICE)
            {
                AddError($"Unit price cannot exceed {MAX_UNIT_PRICE:F2}");
            }
            else if (DecimalPlaces(item.SaleValue.Value) > MAX_DECIMAL_PLACES)
            {
                AddError("Unit price cannot have more than 2 decimal places");
            }

            // ✅ Check for overflow in multiplication (Quantity × Unit Price)
            if (item.Quantity.HasValue && item.SaleValue.HasValue)
            {
                if (item.Quantity > 0 && item.SaleValue > 0)
                {
                    if (item.Quantity > MAX_SAFE_VALUE / item.SaleValue)
                    {
                        AddError("Quantity × Unit Price exceeds maximum allowed total");
                    }
                }
            }

            // ✅ Tax Rate - 0-100%, max 2 decimal places
            if (item.TaxRate < 0 || item.TaxRate > 100)
                AddError("Tax rate must be between 0 and 100");
            else if (DecimalPlaces((decimal)item.TaxRate) > MAX_DECIMAL_PLACES)
                AddError("Tax rate cannot have more than 2 decimal places");

            // ✅ Tax Charged - Must be >= 0, max 2 decimal places
            if (item.TaxCharged.HasValue)
            {
                var taxCharged = Math.Round(item.TaxCharged.Value, 2);
                if (!ValidateSafeDecimal(taxCharged, "Tax charged", AddError, allowZero: true))
                    return new ValidationResult(false, string.Join(Environment.NewLine, errors));
            }

            // ✅ Total Amount - Required, must be > 0, max 2 decimal places
            if (item.TotalAmount.HasValue)
            {
                var totalRS = Math.Round(item.TotalAmount.Value, 2);
                if (!ValidateSafeDecimal(totalRS, "Total amount", AddError))
                    return new ValidationResult(false, string.Join(Environment.NewLine, errors));
            }

            // ✅ Discount - Must be >= 0, max 2 decimal places
            if (item.Discount.HasValue)
            {
                var discountRS = Math.Round(item.Discount.Value, 2);
                if (!ValidateSafeDecimal(discountRS, "Discount", AddError, allowZero: true))
                    return new ValidationResult(false, string.Join(Environment.NewLine, errors));
            }

            // ✅ RefUSIN check for Debit/Credit items (invoice type 3 or 4)
            if ((item.InvoiceType == 3 || item.InvoiceType == 4) && IsEmpty(item.RefUSIN))
                AddError("Ref USIN required for debit/credit items");

            return new ValidationResult(isValid, string.Join(Environment.NewLine, errors));
        }

        /// <summary>
        /// Validates item calculation: Total = (Quantity × Price) + Tax - Discount
        /// Tax is calculated on gross amount BEFORE discount
        /// Discount is applied to amount AFTER tax
        /// </summary>
        private void ValidateItemCalculations(InvoiceItems item, Action<string> AddError, string prefix)
        {
            if (!item.Quantity.HasValue || !item.SaleValue.HasValue)
                return;

            // ✅ Step 1: Calculate gross amount (Quantity × Unit Price)
            if (!TryMultiply(item.Quantity.Value, item.SaleValue.Value, out decimal grossAmount))
            {
                AddError($"{prefix}Values too large for calculation");
                return;
            }

            // ✅ Step 2: Calculate tax on GROSS amount (BEFORE discount)
            decimal taxAmount = 0m;
            if (item.TaxRate > 0)
            {
                if (!TryMultiply(grossAmount, (decimal)item.TaxRate / 100m, out taxAmount))
                {
                    AddError($"{prefix}Tax calculation overflow");
                    return;
                }
            }

            // ✅ Step 3: Calculate amount after adding tax
            if (!TryAdd(grossAmount, taxAmount, out decimal amountAfterTax))
            {
                AddError($"{prefix}Total calculation overflow");
                return;
            }

            // ✅ Step 4: Apply discount to amount after tax
            decimal discountAmount = item.Discount ?? 0m;

            // ✅ Step 5: Calculate final total
            decimal expectedTotal = Math.Round(Math.Max(0, amountAfterTax - discountAmount), 2);
            decimal actualTotal = Math.Round(item.TotalAmount ?? 0m, 2);

            // Allow 2 cent tolerance for rounding differences
            if (Math.Abs(expectedTotal - actualTotal) > 0.02m)
                AddError($"{prefix}Total mismatch. Expected {expectedTotal:F2}, got {actualTotal:F2}");

            // ✅ Validate tax charged matches calculated tax
            if (item.TaxCharged.HasValue)
            {
                decimal expectedTax = Math.Round(taxAmount, 2);
                decimal actualTax = Math.Round(item.TaxCharged.Value, 2);
                if (Math.Abs(expectedTax - actualTax) > 0.02m)
                    AddError($"{prefix}Tax mismatch. Expected {expectedTax:F2}, got {actualTax:F2}");
            }

            // ✅ Validate discount constraints
            if (discountAmount < 0)
                AddError($"{prefix}Discount cannot be negative");

            if (discountAmount > amountAfterTax)
                AddError($"{prefix}Discount cannot exceed total after tax");
        }

        /// <summary>
        /// Validates that invoice-level totals match the sum of all items
        /// </summary>
        private void ValidateInvoiceTotals(Invoice invoice, Action<string> AddError)
        {
            // ✅ Total Quantity = Sum of all item quantities
            decimal sumQty = invoice.Items.Sum(i => i.Quantity ?? 0m);
            if (Math.Abs((invoice.TotalQuantity ?? 0m) - sumQty) > 0.02m)
                AddError($"Total quantity mismatch: Invoice total {invoice.TotalQuantity:F2} vs Sum of items {sumQty:F2}");

            // ✅ Total Sale Value = Sum of (Quantity × Unit Price) for all items
            decimal sumSale = invoice.Items.Sum(i => (i.Quantity ?? 0m) * (i.SaleValue ?? 0m));
            if (Math.Abs((invoice.TotalSaleValue ?? 0m) - sumSale) > 0.02m)
                AddError($"Total sale value mismatch: Invoice total {invoice.TotalSaleValue:F2} vs Sum of items {sumSale:F2}");

            // ✅ Total Tax Charged = Sum of all item tax charges
            decimal sumTax = invoice.Items.Sum(i => i.TaxCharged ?? 0m);
            if (Math.Abs((invoice.TotalTaxCharged ?? 0m) - sumTax) > 0.02m)
                AddError($"Total tax mismatch: Invoice total {invoice.TotalTaxCharged:F2} vs Sum of items {sumTax:F2}");

            // ✅ Total Discount = Sum of all item discounts
            decimal sumDiscount = invoice.Items.Sum(i => i.Discount ?? 0m);
            if (Math.Abs((invoice.Discount ?? 0m) - sumDiscount) > 0.02m)
                AddError($"Total discount mismatch: Invoice total {invoice.Discount:F2} vs Sum of items {sumDiscount:F2}");

            // ✅ Total Bill Amount = Sum of all item totals
            //    (which equals: Gross + Tax - Discount for each item)
            decimal sumTotal = invoice.Items.Sum(i => i.TotalAmount ?? 0m);
            if (Math.Abs((invoice.TotalBillAmount ?? 0m) - sumTotal) > 0.02m)
                AddError($"Total bill amount mismatch: Invoice total {invoice.TotalBillAmount:F2} vs Sum of items {sumTotal:F2}");

            // ✅ ADDITIONAL VERIFICATION: Invoice total should equal (Sale Value + Tax - Discount)
            decimal calculatedInvoiceTotal = sumSale + sumTax - sumDiscount;
            if (Math.Abs((invoice.TotalBillAmount ?? 0m) - calculatedInvoiceTotal) > 0.02m)
                AddError($"Invoice total calculation error: Bill Amount {invoice.TotalBillAmount:F2} does not equal (Sale Value {sumSale:F2} + Tax {sumTax:F2} - Discount {sumDiscount:F2})");
        }

        /// <summary>
        /// Validates decimal field with overflow protection
        /// </summary>
        private bool ValidateSafeDecimal(decimal? value, string fieldName, Action<string> AddError,
            bool allowZero = false, decimal? customMax = null)
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
            decimal maxLimit = customMax ?? MAX_SAFE_VALUE;

            if (v > maxLimit)
            {
                AddError($"{fieldName} exceeds limit ({maxLimit:F2})");
                return false;
            }

            if (DecimalPlaces(v) > MAX_DECIMAL_PLACES)
            {
                AddError($"{fieldName} cannot have more than 2 decimal places");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Safe multiplication with overflow protection
        /// </summary>
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

        /// <summary>
        /// Safe addition with overflow protection
        /// </summary>
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

        /// <summary>
        /// Gets the number of decimal places in a decimal value
        /// </summary>
        private int DecimalPlaces(decimal value)
        {
            return BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        }
    }
}