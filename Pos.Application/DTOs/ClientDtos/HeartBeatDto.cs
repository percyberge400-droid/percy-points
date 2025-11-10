namespace Pos.Application.DTOs.ClientDtos
{
    public class HeartBeatDto
    {
        public bool IsConnected { get; set; }
        public DateTime? HeartbeatUpdatedOn { get; set; }
        public string? StoreStatus { get; set; }
    }
}
