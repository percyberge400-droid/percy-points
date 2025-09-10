using POSPRA.Domain.Entities;

namespace POSPRA.Application.Services.FiscalService
{
    public class InvoiceValidatorService
    {
        public bool InvoiceValidator(Invoice invoice, List<string> errors)
        {
            bool isValid = true;
            if (invoice.BPOSID == 0)
            {
                errors.Add("Invalid BPOSID!");
                isValid = false;
            }
            else if (invoice.BPOSID.ToString().Length != 6)
            {
                errors.Add("Invalid BPOSID should be of 6 digit!");
                isValid = false;
            }
            else
            {
                var posClient = invoice.BPOSID;
                if (posClient == null)
                {
                    errors.Add("Invalid BPOSID Not Found!");
                    isValid = false;
                }
            }
            if (invoice.InvoiceType < 1 || invoice.InvoiceType > 4)
            {
                errors.Add("Invalid Invoice Type!");
                isValid = false;
            }
            if (invoice.InvoiceDate == DateTime.MinValue)
            {
                errors.Add("Invalid Invoice Date!");
                isValid = false;
            }
            if (string.IsNullOrEmpty(invoice.NTN_CNIC.Trim()))
            {
                errors.Add("Invalid NTN/CNIC!");
                isValid = false;
            }
            else if (invoice.NTN_CNIC.Trim().Length > 7 || invoice.NTN_CNIC.Trim().Length > 13)
            {
                errors.Add("NTN/CNIC should be between 7 and 13");
                isValid = false;
            }
            if (string.IsNullOrEmpty(invoice.BuyerSellerName.Trim()))
            {
                errors.Add("Invalid Buyer/Seller Name!");
                isValid = false;
            }
            if (string.IsNullOrEmpty(invoice.DestinationAddress.Trim()))
            {
                errors.Add("Invalid Destination Address!");
                isValid = false;
            }
            if (invoice.SaleType <= 0)
            {
                errors.Add("Invalid Sale Type!");
                isValid = false;
            }

            if (invoice.TotalRetailPrice == 0)
            {
                errors.Add("Invalid Total Retail Price!");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(invoice.Distributor_NTN_CNIC))
            {
                if (invoice.Distributor_NTN_CNIC.Trim().Length < 7 || invoice.Distributor_NTN_CNIC.Trim().Length > 13)
                {
                    errors.Add("Invalid Distributor NTN/CNIC!");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(invoice.DistributorName))
                {
                    errors.Add("Invalid Distributor Name!");
                    isValid = false;
                }
            }

            if (invoice.InvoiceItemDetails.Count == 0)
            {
                errors.Add("Invoice Items Not Found!");
                isValid = false;
            }

            foreach (var item in invoice.InvoiceItemDetails)
            {
                if (string.IsNullOrEmpty(item.HSCode.Trim()))
                {
                    errors.Add("Invoice Item: HS Code Not Found!");
                    isValid = false;
                }
                else if (item.HSCode.Trim().Length != 8)
                {
                    errors.Add("Invoice Item: Invalid HS Code!");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(item.ProductCode.Trim()))
                {
                    errors.Add("Invoice Item: Product Code Not Found!");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(item.ProductDescription.Trim()))
                {
                    errors.Add("Invoice Item: Product Description Not Found!");
                    isValid = false;
                }

                if (item.Rate <= 0)
                {
                    errors.Add("Invoice Item: Rate Should be greater than zero!");
                    isValid = false;
                }

                if (item.UoM <= 0)
                {
                    errors.Add("Invoice Item: Invalid UoM!");
                    isValid = false;
                }

                if (item.Quantity <= 0)
                {
                    errors.Add("Invoice Item: Invalid Quantity!");
                    isValid = false;
                }

                if (item.ValueSalesExcludingST <= 0)
                {
                    errors.Add("Invoice Item: Invalid Value of Sales Excluding ST!");
                    isValid = false;
                }

                if (item.SalesTaxApplicable <= 0)
                {
                    errors.Add("Invoice Item: Invalid Value of Sales Tax Applicable!");
                    isValid = false;
                }

                if (item.RetailPrice <= 0)
                {
                    errors.Add("Invoice Item: Invalid Retail Price!");
                    isValid = false;
                }

                if (item.SalesTaxApplicable <= 0)
                {
                    errors.Add("Invoice Item: Invalid ST Withheld At Source!");
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}
