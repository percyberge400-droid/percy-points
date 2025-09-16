using POSPRA.Domain.Entities;
using POSPRA.DTOs.LogDTOs;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.LogService
{
    /// <summary>
    /// Service contract for logging operations.
    /// Provides methods to persist log entries asynchronously.
    /// </summary>
    public interface ILogService
    {

        /// <summary>
        /// Persists a log entry to the **local SQLite** database.
        /// </summary>
        Task LogAsync(Logs model, int retry = 0);

        Logs BuildLog(string message, AlertType type, string? module = null, string? action = null, string? userId = null, string? userName = null);

        /// <summary>
        /// Persists an error log entry to the **SQL Server** database.
        /// </summary>
        /// 
        Task SaveErrorLogAsync(ErrorLogDto dto);
    }
}