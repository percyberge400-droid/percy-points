namespace POSPRA.DTOs.InvoiceDTOs
{
    public class InvoiceItemDto
    {
        public long InvoiceItemId { get; set; }

        public long InvoiceID { get; set; }

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? PCTCode { get; set; }

        public decimal? Quantity { get; set; }

        public double TaxRate { get; set; }

        public decimal? SaleValue { get; set; }

        public decimal? TaxCharged { get; set; }

        public decimal? TotalAmount { get; set; }

        public bool IsActive { get; set; }

        public DateTime EntryDate { get; set; }

        public byte? InvoiceType { get; set; }

        public string? RefUSIN { get; set; }

        public decimal? Discount { get; set; }

        public decimal? FurtherTax { get; set; }
    }
}
