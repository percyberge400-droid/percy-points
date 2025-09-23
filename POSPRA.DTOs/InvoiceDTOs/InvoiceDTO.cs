namespace POSPRA.DTOs.InvoiceDTOs
{
    public class InvoiceDto
    {
        public long InvoiceID { get; set; }

        public string FBRInvoiceNumber { get; set; } = string.Empty;

        public long POSID { get; set; }

        public string USIN { get; set; } = string.Empty;

        public DateTime DateTime { get; set; }

        public string? BuyerName { get; set; }

        public string? BuyerPhoneNumber { get; set; }

        public decimal? TotalSaleValue { get; set; }

        public decimal? TotalQuantity { get; set; }

        public decimal? TotalTaxCharged { get; set; }

        public decimal? Discount { get; set; }

        public decimal? TotalBillAmount { get; set; }

        public int PaymentMode { get; set; }

        public DateTime EntryDate { get; set; }

        public bool IsActive { get; set; }

        public byte? RequestFrom { get; set; }

        public byte? InvoiceType { get; set; }

        public string? RefUSIN { get; set; }

        public bool? IsValidCalculation { get; set; }

        public bool? IsImport { get; set; }

        public string? Version { get; set; }

        public string? BuyerNTN { get; set; }

        public decimal? FurtherTax { get; set; }

        public string? BuyerCNIC { get; set; }

        public short? StatusID { get; set; }

        public string? Remarks { get; set; }

        public List<InvoiceItemDto>? InvoiceItemDto { get; set; }
    }
}
