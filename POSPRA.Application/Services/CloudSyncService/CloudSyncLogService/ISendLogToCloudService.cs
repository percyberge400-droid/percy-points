namespace POSPRA.Application.Services.CloudSyncService.CloudSyncLogService
{
    public interface ISendLogToCloudService
    {
        Task SyncLogAsync();
        Task IsLogSyncEnable();
    }
}
