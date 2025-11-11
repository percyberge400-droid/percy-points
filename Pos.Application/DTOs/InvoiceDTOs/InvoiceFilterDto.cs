namespace Pos.Application.DTOs.InvoiceDtos
{
    public class InvoiceFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PosId { get; set; } 
        public int? RegistrationNumber { get; set; } // optional extra filter
    }
}