using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.InvoiceDTOs;

namespace Pos.Application.AutoMapperProfile
{
    public static class InvoiceMapper
    {
        public static InvoiceDto ToNewInvoiceDto(this OldInvoiceDto old)
        {
            return new InvoiceDto
            {
                POSID = old.POSID,
                InvoiceNumber = old.InvoiceNumber,
                USIN = old.USIN,
                InvoiceType = (byte)old.InvoiceType,   // <-- FIX

                BuyerPNTN = old.BuyerNTN,
                BuyerCNIC = old.BuyerCNIC,
                BuyerName = old.BuyerName,
                BuyerPhoneNumber = old.BuyerPhoneNumber,

                PaymentMode = old.PaymentMode,
                SaleType = 0, // assign default if needed

                TotalBillAmount = old.TotalBillAmount,
                RefUSIN = old.RefUSIN,
                TotalQuantity = old.TotalQuantity,
                TotalSaleValue = old.TotalSaleValue,
                TotalTaxCharged = old.TotalTaxCharged,
                Discount = old.Discount,
                FurtherTax = old.FurtherTax,
                DateTime = old.DateTime,

                Items = old.Items?.Select(x => new InvoiceItemDto
                {
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    Quantity = x.Quantity, // If your new DTO uses decimal -> OK.
                                           // If it uses double -> Quantity = (double)x.Quantity,

                    PCTCode = x.PCTCode,
                    TaxRate = Convert.ToDouble(x.TaxRate),
                    SaleValue = x.SaleValue,
                    TotalAmount = x.TotalAmount,
                    TaxCharged = x.TaxCharged,
                    Discount = x.Discount,
                    FurtherTax = x.FurtherTax,
                    RefUSIN = x.RefUSIN
                }).ToList()
            };
        }
    }
}
