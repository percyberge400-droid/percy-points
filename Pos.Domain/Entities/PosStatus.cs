namespace Pos.Domain.Entities
{
    public class PosStatus
    {
        public long Id { get; set; }              // Identity PK
        public long POSID { get; set; }           // Required
        public string Message { get; set; }       // nvarchar(2000), required
        public DateTime? DateCreated { get; set; } // Nullable
        public int? TypeId { get; set; }          // Nullable
    }
}
