using POSPRA.Domain.Entities;

namespace POSPRA.Application.Services.FiscalService
{
    /// <summary>
    /// Simple DTO to return overall validity and all validation messages.
    /// </summary>
    public record ValidationResult(bool IsValid, string ErrorMessages);

    public class InvoiceValidatorService
    {
        /// <summary>
        /// Validates an invoice and returns a single string with all errors separated by new lines.
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

            // -------- BPOSID --------
            if (invoice.POSID == 0)
                AddError("Invalid POSID!");
            else if (invoice.POSID.ToString().Length != 6)
                AddError("Invalid POSID should be of 6 digits!");

            // -------- InvoiceType ---
            if (invoice.InvoiceType < 1 || invoice.InvoiceType > 4)
                AddError("Invalid Invoice Type!");

            //--------Date----------
            if (invoice.EntryDate == DateTime.MinValue)
                AddError("Invalid Invoice Date!");

            //--------NTN / CNIC----
            var ntn = TrimSafe(invoice.BuyerCNIC);
            if (ntn.Length == 0)
                AddError("Invalid NTN/CNIC!");
            else if (ntn.Length < 7 || ntn.Length > 13)
                AddError("NTN/CNIC should be between 7 and 13 characters.");

            //--------Buyer / Seller--
            if (IsEmpty(invoice.BuyerName))
                AddError("Invalid Buyer/Seller Name!");

            //if (IsEmpty(invoice.DestinationAddress))
            //    AddError("Invalid Destination Address!");

            //--------Sale Type & Price
            if (invoice.InvoiceType <= 0)
                AddError("Invalid Sale Type!");

            if (invoice.TotalBillAmount == 0)
                AddError("Invalid Total Retail Price!");

            //--------Distributor----
            //var distNtn = TrimSafe(invoice.Distributor_NTN_CNIC);
            //if (!IsEmpty(distNtn))
            //{
            //    if (distNtn.Length < 7 || distNtn.Length > 13)
            //        AddError("Invalid Distributor NTN/CNIC!");
            //    if (IsEmpty(invoice.DistributorName))
            //        AddError("Invalid Distributor Name!");
            //}

            //--------Items-------- -
            if (invoice.InvoiceItems.Count == 0)
            {
                AddError("Invoice Items Not Found!");
            }
            else
            {
                //foreach (var item in invoice.InvoiceItems)
                //{
                //    var hs = TrimSafe(item.HSCode);
                //    if (hs.Length == 0)
                //        AddError("Invoice Item: HS Code Not Found!");
                //    else if (hs.Length != 8)
                //        AddError("Invoice Item: Invalid HS Code!");

                //    if (IsEmpty(item.ProductCode))
                //        AddError("Invoice Item: Product Code Not Found!");

                //    if (IsEmpty(item.ProductDescription))
                //        AddError("Invoice Item: Product Description Not Found!");

                //    if (item.Rate <= 0)
                //        AddError("Invoice Item: Rate should be greater than zero!");
                //    if (item.UoM <= 0)
                //        AddError("Invoice Item: Invalid UoM!");
                //    if (item.Quantity <= 0)
                //        AddError("Invoice Item: Invalid Quantity!");
                //    if (item.ValueSalesExcludingST <= 0)
                //        AddError("Invoice Item: Invalid Value of Sales Excluding ST!");
                //    if (item.SalesTaxApplicable <= 0)
                //    {
                //        AddError("Invoice Item: Invalid Value of Sales Tax Applicable!");
                //        AddError("Invoice Item: Invalid ST Withheld At Source!");
                //    }
                //    if (item.RetailPrice <= 0)
                //        AddError("Invoice Item: Invalid Retail Price!");
                //}
            }

            // -------- Final result --
            string message = errors.Count > 0
                ? string.Join(Environment.NewLine, errors)
                : string.Empty;

            return new ValidationResult(isValid, message);
        }
    }
}
