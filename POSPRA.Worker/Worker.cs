using Microsoft.Extensions.Options;
using POSPRA.Application.Services.ClientService;
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

            // Startup log
            using (var startupScope = _serviceScopeFactory.CreateScope())
            {
                var logService = startupScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                await logService.LogStartup(workerName, workerInstanceId);
            }

            try
            {
                //int serviceDisbaledLogCount = 0;
                while (!cancellationToken.IsCancellationRequested)
                {


                    //serviceDisbaledLogCount = 0;
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

                        // Don't kill worker, just wait and retry
                        await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                        continue;
                    }

                    using (var workerScope = _serviceScopeFactory.CreateScope())
                    {
                        var clientService = workerScope.ServiceProvider.GetRequiredService<IClientService>();
                        bool EnabledWorker = false;


                        var fullUrl = $"{_appSettings.BaseUrl}{Endpoints.IsServiceEnabled}?posId={_appSettings.POS}";

                        using (var httpClient = new HttpClient())
                        {
                            try
                            {
                                var response = await httpClient.GetAsync(fullUrl);
                                response.EnsureSuccessStatusCode();

                                string result = await response.Content.ReadAsStringAsync();

                                // Parse string "true"/"false" to bool
                                EnabledWorker = bool.TryParse(result, out bool parsedValue) && parsedValue;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error calling API: {ex.Message}");
                            }
                        }

                        if (!EnabledWorker)
                        {
                            await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                            continue;
                        }
                    }

                    using (var workerScope = _serviceScopeFactory.CreateScope())
                    {
                        var configurationService = workerScope.ServiceProvider.GetRequiredService<IConfigurationService>();
                        var invoiceCloudSyncService = workerScope.ServiceProvider.GetRequiredService<ISendInvoiceToCloudService>();
                        var logCloudSyncService = workerScope.ServiceProvider.GetRequiredService<ISendLogToCloudService>();

                        // Check if Cloud Sync is enabled
                        bool isCloudSyncEnabled = false;

                        try
                        {
                            isCloudSyncEnabled = await configurationService.IsCloudSyncEnabledAsync(
                                new GetByPosIdDto { PosId = _appSettings.POS }
                            );
                        }
                        catch (HttpRequestException ex)
                        {
                            await LogWarningAsync($"Network issue while checking cloud sync status: {ex.Message}", workerName, workerInstanceId);
                        }
                        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            // Worker stopped gracefully
                            break;
                        }
                        catch (Exception ex)
                        {
                            await LogErrorAsync($"Unexpected error while checking cloud sync status: {ex}", workerName, workerInstanceId);
                        }

                        // Continue only if sync is enabled and network still okay
                        if (isCloudSyncEnabled)
                        {
                            try
                            {
                                await invoiceCloudSyncService.SyncInvoicesAsync(cancellationToken, workerInstanceId);
                                await logCloudSyncService.IsLogSyncEnable();
                                //await logCloudSyncService.SyncLogAsync();
                            }
                            catch (HttpRequestException ex)
                            {
                                await LogWarningAsync($"Network issue during invoice sync: {ex.Message}", workerName, workerInstanceId);
                            }
                            catch (Exception ex)
                            {
                                await LogErrorAsync($"Unexpected error during invoice sync: {ex}", workerName, workerInstanceId);
                            }
                        }
                    }
                    // delay before next iteration
                    await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                }
            }
            finally
            {
                // Shutdown log
                using (var shutdownScope = _serviceScopeFactory.CreateScope())
                {
                    var logService = shutdownScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                    await logService.LogShutdown(workerName, workerInstanceId);
                }
            }
        }

        private async Task LogWarningAsync(string message, string workerName, string workerInstanceId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<IWorkerLogService>();
            await logService.LogAsync(AlertType.Warning, message, workerName, workerInstanceId, "NetworkIssue");
        }

        private async Task LogErrorAsync(string message, string workerName, string workerInstanceId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<IWorkerLogService>();
            await logService.LogAsync(AlertType.Error, message, workerName, workerInstanceId, "WorkerError");
        }

    }
}
