using Microsoft.Extensions.Options;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.CloudSyncLogService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.ConfigurationService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.DTOs;
using POSPRA.DTOs.CommanDtos;

namespace POSPRA.Worker
{
    public class Worker(
        IServiceScopeFactory scopeFactory,
        IOptions<AppSettings> options,
        INetworkService networkService) : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = scopeFactory;
        private readonly AppSettings _appSettings = options.Value;
        private readonly INetworkService _networkService = networkService;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var workerInstanceId = Guid.NewGuid().ToString();
            var workerName = nameof(Worker);

            // ✅ Startup log
            using (var startupScope = _serviceScopeFactory.CreateScope())
            {
                var logService = startupScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                await logService.LogStartup(workerName, workerInstanceId);
            }

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool internetAvailable = await _networkService.IsInternetAvailableAsync();

                    if (!internetAvailable)
                    {
                        using (var warningScope = _serviceScopeFactory.CreateScope())
                        {
                            var logService = warningScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                            await logService.LogAsync(
                                AlertType.Warning,
                                "Internet not available. Skipping sync.",
                                workerName,
                                workerInstanceId,
                                "NoInternet"
                            );
                        }

                        // ❌ Don't kill worker, just wait and retry
                        await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                        continue;
                    }

                    using (var workerScope = _serviceScopeFactory.CreateScope())
                    {
                        var configurationService = workerScope.ServiceProvider.GetRequiredService<IConfigurationService>();
                        var invoiceCloudSyncService = workerScope.ServiceProvider.GetRequiredService<ISendInvoiceToCloudService>();
                        var logCloudSyncService = workerScope.ServiceProvider.GetRequiredService<ISendLogToCloudService>();

                        // ✅ Check if Cloud Sync is enabled
                        if (await configurationService.IsCloudSyncEnabledAsync(new GetByPosIdDto { PosId = 110050 }))
                        {
                            // 🔹 Sync invoices
                            await invoiceCloudSyncService.SyncInvoicesAsync(cancellationToken, workerInstanceId);

                            // 🔹 Sync logs
                            await logCloudSyncService.SyncLogAsync(cancellationToken, workerInstanceId);
                        }
                    }

                    // ⏳ delay before next iteration
                    await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                }
            }
            finally
            {
                // ✅ Shutdown log
                using (var shutdownScope = _serviceScopeFactory.CreateScope())
                {
                    var logService = shutdownScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                    await logService.LogShutdown(workerName, workerInstanceId);
                }
            }
        }
    }
}
