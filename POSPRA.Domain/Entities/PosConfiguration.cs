namespace POSPRA.Domain.Entities
{
    public class PosConfiguration
    {
        public int Id { get; set; }
        public int? LogInterval { get; set; }
        public int? RecordInterval { get; set; }
        public int? HeartbeatInterval { get; set; }
        public int? IMSUpdateInterval { get; set; }
        public int? RecordSyncLimit { get; set; }
        public int? LogSyncLimit { get; set; }
        public string? GatewayURL { get; set; }
        public string? FilePath { get; set; }
        public string? Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public long? POSID { get; set; }
        public long? FileSize { get; set; }
        public string? Token { get; set; }
    }
}
