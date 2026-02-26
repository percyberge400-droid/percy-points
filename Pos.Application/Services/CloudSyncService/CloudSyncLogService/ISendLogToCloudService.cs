using Pos.Application.DTOs;

namespace Pos.Application.Services.CloudSyncService.CloudSyncLogService
{
    public interface ISendLogToCloudService
    {
        Task SyncLogAsync(string env, LogResponseDto? logResponse);
    }
}
