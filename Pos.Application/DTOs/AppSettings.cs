namespace Pos.Application.DTOs
{
    public class AppSettings
    {
        public int LogInterval { get; set; }
        public int RecordInterval { get; set; }
        public int HeartbeatInterval { get; set; }
        public int RecordSyncLimit { get; set; }
        public int LogSyncLimit { get; set; }
        public string BaseUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int BackUpSize { get; set; }
        public string ClientSettingsProviderServiceUri { get; set; } = string.Empty;
        public string LocalURL { get; set; } = string.Empty;
        public string EC { get; set; } = string.Empty;
        public string PV { get; set; } = string.Empty;
        public string PB { get; set; } = string.Empty;
        public int POS { get; set; }
        public string FolderPath { get; set; } = string.Empty;
        public string LICENSEKEY { get; set; } = string.Empty;
        public string GatewayURL { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string IMSServicePath { get; set; } = string.Empty;
        public bool IsProduction { get; set; }
        public bool IsProductionWorker { get; set; }
        public int IMSUpdateInterval { get; set; }
        public int WorkerDelayTime { get; set; }
        public string? DefaultDBFilePath { get; set; }
        public bool IsBackupTestMode { get; set; }      
        public string? BackupDirectoryPath { get; set; }          // e.g., "C:\\POS_Backups"
    

    }
}
