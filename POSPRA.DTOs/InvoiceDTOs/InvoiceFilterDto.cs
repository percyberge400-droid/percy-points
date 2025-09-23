namespace POSPRA.DTOs.InvoiceDTOs
{
    public class InvoiceFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PosId { get; set; } // optional extra filter
        public int? RegistrationNumber { get; set; } // optional extra filter
    }
}