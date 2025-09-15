namespace POSPRA.DTOs.PosDTOs
{
    public class ResponseConfigurationDto
    {
        public string? LogInterval { get; set; }
        public string? RecordInterval { get; set; }
        public string? HeartbeatInterval { get; set; }
        public string? IMSUpdateInterval { get; set; }
        public string? RecordSyncLimit { get; set; }
        public string? LogSyncLimit { get; set; }
        public string? GatewayURL { get; set; }
        public string? FilePath { get; set; }
        public string? Version { get; set; }
        public string? FileSize { get; set; }
        public string? Token { get; set; }
    }
}
