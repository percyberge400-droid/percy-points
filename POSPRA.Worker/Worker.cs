using Microsoft.Extensions.Options;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.DTOs;
using POSPRA.DTOs.CommanDtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace POSPRA.Worker
{
    public class Worker(
        IServiceScopeFactory scopeFactory,
        IOptions<AppSettings> options,
        INetworkService networkService,
        IHttpClientFactory httpClientFactory) : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = scopeFactory;
        private readonly AppSettings _appSettings = options.Value;
        private readonly INetworkService _networkService = networkService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var workerInstanceId = Guid.NewGuid().ToString();
            var workerName = nameof(Worker);

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
                        await LogWarningAsync("Internet not available. Skipping sync.", workerName, workerInstanceId);
                        await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                        continue;
                    }

                    using (var workerScope = _serviceScopeFactory.CreateScope())
                    {
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

                    bool isCloudSyncEnabled = false;

                    try
                    {
                        // ✅ Replace service call with HTTP POST
                        using var httpClient = _httpClientFactory.CreateClient();
                        var apiUrl = $"{_appSettings.BaseUrl}{Endpoints.IsCloudSyncEnabledAsync}";

                        var response = await httpClient.PostAsJsonAsync(apiUrl, new GetByPosIdDto { PosId = _appSettings.POS }, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync(cancellationToken);
                            isCloudSyncEnabled = JsonSerializer.Deserialize<bool>(content, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        else
                        {
                            await LogWarningAsync($"Cloud sync API call failed with status {response.StatusCode}", workerName, workerInstanceId);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        await LogWarningAsync($"Network issue while calling cloud sync API: {ex.Message}", workerName, workerInstanceId);
                    }
                    catch (Exception ex)
                    {
                        await LogErrorAsync($"Unexpected error calling cloud sync API: {ex}", workerName, workerInstanceId);
                    }

                    if (isCloudSyncEnabled)
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var invoiceCloudSyncService = scope.ServiceProvider.GetRequiredService<ISendInvoiceToCloudService>();
                        //var logCloudSyncService = scope.ServiceProvider.GetRequiredService<ISendLogToCloudService>();
                        //var apiUrlSyncInvoicesAsync = $"{_appSettings.BaseUrl}{Endpoints.SyncInvoicesAsync}?workerId={workerInstanceId}";
                        //var apiUrlIsLogSyncEnable = $"{_appSettings.BaseUrl}{Endpoints.IsLogSyncEnable}";

                        try
                        {
                            await invoiceCloudSyncService.SyncInvoicesAsync(cancellationToken, workerInstanceId);
                            //using var client = _httpClientFactory.CreateClient();

                            //var response = await client.PostAsync(apiUrlSyncInvoicesAsync, null, cancellationToken);
                            //var response2 = await client.GetAsync(apiUrlIsLogSyncEnable);

                            //await logCloudSyncService.IsLogSyncEnable();
                        }
                        catch (Exception ex)
                        {
                            await LogErrorAsync($"Error during invoice sync: {ex}", workerName, workerInstanceId);
                        }
                    }

                    await Task.Delay(_appSettings.WorkerDelayTime, cancellationToken);
                }
            }
            finally
            {
                using var shutdownScope = _serviceScopeFactory.CreateScope();
                var logService = shutdownScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                await logService.LogShutdown(workerName, workerInstanceId);
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
