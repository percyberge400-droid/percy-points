namespace Pos.Domain.Entities
{
    [Table("InvoiceType")]
    public class InvoiceType
    {
        [Key]
        public int ID { get; set; }
        public string? NAME { get; set; }
    }
}
