using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.Application.Services.CloudSyncService.WorkerLogService
{
    public class WorkerLogService : IWorkerLogService
    {
        private readonly ILogService _logService;
        public WorkerLogService(ILogService logService)
        {
            _logService = logService;
        }

        public async Task LogAsync(
            string type,
            string message,
            string workerName,
            string workerId,
            string evt,
            int? statusCode = null,
            string? stackTrace = null)
        {
            try
            {
                var log = new CreateLogDto
                {
                    Message = message,
                    Type = type,
                    WorkerName = workerName,
                    WorkerInstanceId = workerId,
                    WorkerEvent = evt,
                    ResponseStatusCode = statusCode,
                    StackTrace = stackTrace
                };

                await _logService.CreateLogAsync(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [WorkerLogService] Exception while logging: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Task LogStartup(string workerName, string workerId)
        {
            await LogAsync(AlertType.Info, "POS service started.", workerName, workerId, AlertType.Startup);
        }

        public async Task LogShutdown(string workerName, string workerId)
        {
            await LogAsync(AlertType.Info, "POS service stopped.", workerName, workerId, AlertType.Shutdown);
        }
    }
}
