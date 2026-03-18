namespace Pos.Domain.Entities
{
    [Table("ServiceRendered")]
    public class ServiceRendered
    {
        [Key]
        public int ID { get; set; }
        public string? NAME { get; set; }
    }
}
