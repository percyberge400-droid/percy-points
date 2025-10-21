namespace POSPRA.Application.Services.CloudSyncService.WorkerLogService
{
    public interface IWorkerLogService
    {
        Task LogAsync(string type, string message, string workerName, string workerId, string evt, int? statusCode = null, string? stackTrace = null);
        Task LogStartup(string workerName, string workerId);
        Task LogShutdown(string workerName, string workerId);
    }
}