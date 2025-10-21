namespace POSPRA.Domain.Entities
{
    public class FileRecord
    {
        public int ID { get; set; }
        public long POSID { get; set; }
        public string? InvoiceData { get; set; }
        public string? InvoiceNumber { get; set; }
        public int IsSynced { get; set; }
        //public bool IsValid { get; set; }
        public int AttemptCount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}