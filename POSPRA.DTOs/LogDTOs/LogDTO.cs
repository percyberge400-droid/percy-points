namespace POSPRA.DTOs.LogDTOs
{
    public class LogDto
    {
        public long Id { get; set; }
        public string? Message { get; set; }

        public string? Type { get; set; }
        /// <summary>
        /// Indicates whether the log is synced (1) or not (0).
        /// </summary>
        public int IsSynced { get; set; } = 0; // default 0
    }
}
