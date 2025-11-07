using POSPRA.DTOs;
using POSPRA.DTOs.LogDtos;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.Application.Services.LogService
{
    public interface ICloudLogService
    {
        Task<ApiResponse<List<SyncLogDto>>> CreateCloudLog(List<SyncLogDto> dto);
        Task<ApiResponse<List<LogDto>>> GetAllCloudAsync();
        Task<ApiResponse<List<LogDto>>> UpdateLogAsync(List<LogDto> logDtos);
    }
}