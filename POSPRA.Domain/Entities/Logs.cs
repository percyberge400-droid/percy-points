namespace POSPRA.Domain.Entities
{
    public class Logs
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public int TypeId { get; set; }
        public bool IsSynced { get; set; }

        public Logs(string message, int typeId, bool isSynced)
        {
            Message = message;
            TypeId = typeId;
            IsSynced = isSynced;
        }
    }
}
