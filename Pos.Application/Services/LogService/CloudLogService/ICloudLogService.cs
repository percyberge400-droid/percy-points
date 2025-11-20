using Pos.Application.DTOs;
using Pos.Application.DTOs.LogDtos;
using Pos.Application.DTOs.LogDTOs;

namespace Pos.Application.Services.LogService
{
    public interface ICloudLogService
    {
        Task<ApiResponse<List<SyncLogDto>>> CreateCloudLog(List<SyncLogDto> dto, string environment);
        Task<ApiResponse<List<LogDto>>> GetAllCloudAsync(long posId, string env);
        Task<ApiResponse<List<LogDto>>> UpdateLogAsync(List<LogDto> logDtos);
    }
}