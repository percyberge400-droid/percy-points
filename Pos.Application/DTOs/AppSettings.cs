namespace Pos.Application.DTOs
{
    public class AppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int WorkerDelayTime { get; set; }
        public string? DefaultDBFilePath { get; set; }
        public string? DefaultDBPassword { get; set; }
        public bool IsBackupTestMode { get; set; }
        public string? BackupDirectoryPath { get; set; }          // e.g., "C:\\POS_Backups"
        public string? Environment { get; set; }          // e.g., "C:\\POS_Backups"
        public string? Password { get; set; }          // e.g., "C:\\POS_Backups"
        public string EC { get; set; } = string.Empty;
        public int POS { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
