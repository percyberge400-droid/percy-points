namespace Pos.Application.DTOs
{
    public class LogResponseDto
    {
        public bool? IsLogSynced { get; set; }
        public DateTime? LogSyncedDateFrom { get; set; }
        public DateTime? LogSyncedDateTo { get; set; }
    }
}
