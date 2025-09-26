using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.Application.Services.LogService
{
    /// <summary>
    /// Service contract for logging operations.
    /// Provides methods to persist log entries asynchronously.
    /// </summary>
    public interface ILogService
    {

        Task<ApiResponse<List<LogDto>>> GetAllAsync();

        /// <summary>
        /// Persists a log entry to the **local SQLite** database.
        /// </summary>
        Task LogAsync(Logs model);

        Logs BuildLog(
                string message,
                string type,
                string? module = null,
                string? action = null,
                string? userId = null,
                string? userName = null,
                string? clientIp = null,
                string? userAgent = null);
        /// <summary>
        /// Persists an error log entry to the **SQL Server** database.
        /// </summary>
        /// 
        Task SaveErrorLogAsync(ErrorLogDto dto);
    }
}