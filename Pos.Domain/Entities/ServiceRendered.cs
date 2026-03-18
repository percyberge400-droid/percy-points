using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Domain.Entities
{
    [Table("ServiceRendered")]
    public class ServiceRendered
    {
        [Key]
        public long ID { get; set; }
        public string? NAME { get; set; }
    }
}
