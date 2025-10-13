using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.Application.Services.CloudSyncService.WorkerLogService
{
    public class WorkerLogService : IWorkerLogService
    {
        private readonly IServiceProvider _services;
        private readonly IMapper _mapper;

        public WorkerLogService(IServiceProvider services, IMapper mapper)
        {
            _services = services;
            _mapper = mapper;
        }

        public async Task LogAsync(string type, string message, string workerName, string workerId, string evt, int? statusCode = null, string? stackTrace = null)
        {
            using var scope = _services.CreateScope();
            var logSvc = scope.ServiceProvider.GetRequiredService<ILogService>();

            var log = new WorkerLogDto
            {
                Message = message,
                Type = type,
                WorkerName = workerName,
                WorkerInstanceId = workerId,
                WorkerEvent = evt,
                ResponseStatusCode = statusCode,
                StackTrace = stackTrace
            };

            await logSvc.CreateLogAsync(_mapper.Map<Logs>(log));
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