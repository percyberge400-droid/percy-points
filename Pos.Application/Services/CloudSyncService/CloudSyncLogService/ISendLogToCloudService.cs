namespace Pos.Application.Services.CloudSyncService.CloudSyncLogService
{
    public interface ISendLogToCloudService
    {
        Task SyncLogAsync();
    }
}
