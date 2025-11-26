using System.Text.Json.Serialization;

namespace Pos.Application.DTOs.InvoiceDTOs
{
    public class InvoiceResponseDto
    {
        [JsonPropertyName("InvoiceNumber")]
        public string? InvoiceNumber { get; set; }

        [JsonPropertyName("Code")]
        public string? Code { get; set; }

        [JsonPropertyName("Response")]
        public string? Response { get; set; }

        [JsonPropertyName("Errors")]
        public string? Errors { get; set; }
    }
}