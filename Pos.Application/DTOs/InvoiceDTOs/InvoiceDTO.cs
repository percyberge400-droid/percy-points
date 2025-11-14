using System.Text.Json.Serialization;

namespace Pos.Application.DTOs.InvoiceDtos
{
    public class InvoiceDto
    {
        public long POSID { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public string USIN { get; set; } = string.Empty;

        public byte InvoiceType { get; set; }

        public string? BuyerPNTN { get; set; }

        public string? BuyerCNIC { get; set; }

        public string? BuyerName { get; set; }

        public string? BuyerPhoneNumber { get; set; }

        public int PaymentMode { get; set; }
        public int SaleType { get; set; }

        public decimal TotalBillAmount { get; set; }

        public string? RefUSIN { get; set; }

        public decimal TotalQuantity { get; set; }

        public decimal TotalSaleValue { get; set; }

        public decimal TotalTaxCharged { get; set; }

        public decimal Discount { get; set; }

        public decimal FurtherTax { get; set; }

        public DateTime DateTime { get; set; }

        [JsonPropertyName("Items")]   // match the JSON array name
        public List<InvoiceItemDto>? InvoiceItemDto { get; set; }
    }
}
