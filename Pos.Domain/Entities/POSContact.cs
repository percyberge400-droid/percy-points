using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Domain.Entities
{
    [Table("POSContact")]
    public class POSContact
    {
        [Key]
        public long POSContactID { get; set; }

        public long? POSMASTERID { get; set; }

        [MaxLength(100)]
        public string? Person { get; set; }

        [MaxLength(20)]
        public string? Mobile { get; set; }

        [MaxLength(20)]
        public string? LandLine { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public int? ContactType { get; set; }

        public DateTime? EntryDate { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? UpdateDate { get; set; }

        public long? Computer_No { get; set; }
    }
}
