namespace POSPRA.DTOs.InvoiceDTOs
{
    public class FileRecordDTO
    {
        public int ID { get; set; }
        public int POSID { get; set; }
        public string? InvoiceData { get; set; }
        public string? InvoiceNumber { get; set; }
        public int IsSynced { get; set; }
        public int AttemptCount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
