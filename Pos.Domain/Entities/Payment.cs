namespace Pos.Domain.Entities
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        public int ID { get; set; }
        public string? NAME { get; set; }
    }
}
