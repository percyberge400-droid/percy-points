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

        Task<ApiResponse<List<LogDto>>> GetAllCloudAsync();
        Task<ApiResponse<List<LogDto>>> GetAllUnsyncCloudAsync();

        Task<ApiResponse<List<LogDto>>> GetAllAsync();

        Task<ApiResponse<bool>> UpdateLog(List<Logs> dtos);
        Task<ApiResponse<List<LogDto>>> UpdateLogAsync(List<LogDto> logDtos);

        Task CreateLogAsync(Logs model);

        Task<ApiResponse<List<Logs>>> CreateCloudLog(List<SyncLogDto> dto);

        Logs BuildLog(
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