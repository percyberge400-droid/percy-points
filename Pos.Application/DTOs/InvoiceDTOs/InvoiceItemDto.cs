namespace Pos.Application.DTOs.InvoiceDtos
{
    public class InvoiceItemDto
    {
        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? PCTCode { get; set; }

        public decimal Quantity { get; set; }

        public decimal SaleValue { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal TaxCharged { get; set; }

        public double TaxRate { get; set; }

        public decimal Discount { get; set; }

        public decimal FurtherTax { get; set; }

        public byte InvoiceType { get; set; }

        public string? RefUSIN { get; set; }
    }
}
