using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using Pos.Application.Services.CloudSyncService.WorkerLogService;
using Pos.Application.Services.NetworkService;
using Pos.Application.Utility;
using Pos.Worker.Logging;

namespace Pos.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AppSettings _appSettings;

        public Worker(IServiceScopeFactory scopeFactory, IOptions<AppSettings> options)
        {
            _serviceScopeFactory = scopeFactory;
            _appSettings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var workerInstanceId = Guid.NewGuid().ToString();
            var workerName = nameof(Worker);

            // Write file log
            FileLogger.Log($"Worker started - InstanceId: {workerInstanceId}");

            // Also DB or custom log
            using (var startupScope = _serviceScopeFactory.CreateScope())
            {
                var logService = startupScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                await logService.LogStartup(workerName, workerInstanceId);
            }

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var networkService = scope.ServiceProvider.GetRequiredService<INetworkService>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                    bool internetAvailable = await networkService.IsInternetAvailableAsync();

                    if (!internetAvailable)
                    {
                        FileLogger.Log("Warning: Internet not available. Skipping sync.");
                        await LogWarningAsync("Internet not available. Skipping sync.", workerName, workerInstanceId);
                        await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                        continue;
                    }

                    bool enabledWorker = false;
                    var fullUrl = $"{_appSettings.BaseUrl}{Endpoints.IsServiceEnabled}?posId={_appSettings.POS}&?env={_appSettings.Environment}";

                    try
                    {
                        using var httpClient = httpClientFactory.CreateClient();
                        var response = await httpClient.GetAsync(fullUrl, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        var result = await response.Content.ReadAsStringAsync(cancellationToken);
                        enabledWorker = bool.TryParse(result, out bool parsed) && parsed;
                    }
                    catch (Exception ex)
                    {
                        FileLogger.LogException(ex, "Service Enabled Check");
                        await LogWarningAsync($"Error calling API: {ex.Message}", workerName, workerInstanceId);
                    }

                    if (!enabledWorker)
                    {
                        FileLogger.Log("Worker disabled via API.");
                        await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                        continue;
                    }

                    bool isCloudSyncEnabled = false;

                    try
                    {
                        using var httpClient = httpClientFactory.CreateClient();
                        var apiUrl = $"{_appSettings.BaseUrl}{Endpoints.IsCloudSyncEnabledAsync}";
                        var response = await httpClient.PostAsJsonAsync(apiUrl, new GetByPosIdDto { PosId = _appSettings.POS, Environment = _appSettings.Environment }, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync(cancellationToken);
                            isCloudSyncEnabled = JsonSerializer.Deserialize<bool>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        else
                        {
                            FileLogger.Log($"Cloud sync API failed: {response.StatusCode}");
                            await LogWarningAsync($"Cloud sync API call failed with status {response.StatusCode}", workerName, workerInstanceId);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        FileLogger.LogException(ex, "Cloud Sync Network Issue");
                        await LogWarningAsync($"Network issue while calling cloud sync API: {ex.Message}", workerName, workerInstanceId);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.LogException(ex, "Cloud Sync Unexpected Error");
                        await LogErrorAsync($"Unexpected error calling cloud sync API: {ex}", workerName, workerInstanceId);
                    }

                    if (isCloudSyncEnabled)
                    {
                        try
                        {
                            using var cloudScope = _serviceScopeFactory.CreateScope();
                            var invoiceCloudSyncService = cloudScope.ServiceProvider.GetRequiredService<ISendInvoiceToCloudService>();
                            await invoiceCloudSyncService.SyncInvoicesAsync(cancellationToken,_appSettings.Environment, workerInstanceId);
                        }
                        catch (Exception ex)
                        {
                            FileLogger.LogException(ex, "Invoice Sync");
                            await LogErrorAsync($"Error during invoice sync: {ex}", workerName, workerInstanceId);
                        }
                    }

                    await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException(ex, "ExecuteAsync Crash");
            }
            finally
            {
                // Shutdown logging
                using var shutdownScope = _serviceScopeFactory.CreateScope();
                var logService = shutdownScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                await logService.LogShutdown(workerName, workerInstanceId);

                FileLogger.Log($"Worker stopped - InstanceId: {workerInstanceId}");
            }
        }

        private async Task LogWarningAsync(string message, string workerName, string workerInstanceId)
        {
            FileLogger.Log("Warning: " + message);

            using var scope = _serviceScopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<IWorkerLogService>();
            await logService.LogAsync(AlertType.Warning, message, workerName, workerInstanceId, "NetworkIssue");
        }

        private async Task LogErrorAsync(string message, string workerName, string workerInstanceId)
        {
            FileLogger.Log("Error: " + message);

            using var scope = _serviceScopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<IWorkerLogService>();
            await logService.LogAsync(AlertType.Error, message, workerName, workerInstanceId, "WorkerError");
        }
    }
}
