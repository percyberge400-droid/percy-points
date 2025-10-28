using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDtos;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.Application.Services.LogService
{
    /// <summary>
    /// Service contract for logging operations.
    /// Provides methods to persist log entries asynchronously.
    /// </summary>
    public interface ILogService
    {
        Task<ApiResponse<List<LogDto>>> GetAllCloudAsync();
        Task<ApiResponse<List<SyncLogDto>>> GetAllUnsyncLogs();

        Task<ApiResponse<List<LogDto>>> GetAllAsync();

        Task<ApiResponse<bool>> UpdateLog(List<Logs> dtos);

        Task<ApiResponse<List<LogDto>>> UpdateLogAsync(List<LogDto> logDtos);

        Task<ApiResponse<CreateLogDto>> CreateLogAsync(CreateLogDto dto);

        Task<ApiResponse<List<SyncLogDto>>> CreateCloudLog(List<SyncLogDto> dto);
        
        CreateLogDto BuildLog(
                string message,
                string type,
                string? module = null,
                string? action = null,
                string? userId = null,
                string? userName = null,
                string? clientIp = null,
                string? userAgent = null);
    }
}