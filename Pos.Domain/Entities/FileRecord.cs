using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Domain.Entities
{
    public class FileRecord
    {
        [Key]
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