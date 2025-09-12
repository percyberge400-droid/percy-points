using POSPRA.Domain.Entities;

namespace POSPRA.Application.Services.LogService
{
    /// <summary>
    /// Service contract for logging operations.
    /// Provides methods to persist log entries asynchronously.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Logs the specified <see cref="Logs"/> model asynchronously.
        /// </summary>
        /// <param name="model">The log entry to persist.</param>
        /// <param name="retry">
        /// Optional retry count in case of failure. Defaults to 0 (no retries).
        /// </param>
        Task LogAsync(Logs model, int retry = 0);
    }
}
