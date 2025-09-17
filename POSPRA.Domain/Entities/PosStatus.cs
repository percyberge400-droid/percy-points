namespace POSPRA.Domain.Entities
{
    public class PosStatus
    {
        public long Id { get; set; }          // Assuming there's an identity PK
        public long POSID { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
        public int? TypeId { get; set; }
    }
}
