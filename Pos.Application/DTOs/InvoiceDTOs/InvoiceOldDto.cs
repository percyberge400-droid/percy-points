namespace Pos.Application.DTOs.InvoiceDTOs
{
    public class OldInvoiceDto
    {
        public string? InvoiceNumber { get; set; }
        public long POSID { get; set; }
        public string? USIN { get; set; }
        public DateTime DateTime { get; set; }
        public string? BuyerNTN { get; set; }
        public string? BuyerCNIC { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerPhoneNumber { get; set; }
        public decimal TotalBillAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalSaleValue { get; set; }
        public decimal TotalTaxCharged { get; set; }
        public decimal Discount { get; set; }
        public decimal FurtherTax { get; set; }
        public int PaymentMode { get; set; }
        public List<OldInvoiceItemDto>? Items { get; set; }
        public string? RefUSIN { get; set; }
        public int InvoiceType { get; set; }
    }

    public class OldInvoiceItemDto
    {
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string? PCTCode { get; set; }
        public decimal TaxRate { get; set; }
        public decimal SaleValue { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxCharged { get; set; }
        public decimal Discount { get; set; }
        public decimal FurtherTax { get; set; }
        public int InvoiceType { get; set; }
        public string? RefUSIN { get; set; }
    }
}
