using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.Services.CloudSyncService.CloudSyncLogService;
using Pos.Application.Services.CloudSyncService.WorkerLogService;
using Pos.Application.Services.NetworkService;
using Pos.Application.Utility;
using Pos.Worker.Configurations;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pos.Worker.Services
{
    public class LogWorkerService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LogWorkerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _appSettings;

        public LogWorkerService(
            IHttpClientFactory httpClientFactory,
            ILogger<LogWorkerService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<AppSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _appSettings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workerInstanceId = Guid.NewGuid().ToString();
            var workerName = nameof(LogWorkerService);

            _logger.LogInformation($"{workerName} started.");
            // Also DB or custom log
            using (var startupScope = _scopeFactory.CreateScope())
            {
                var logService = startupScope.ServiceProvider.GetRequiredService<IWorkerLogService>();
                await logService.LogStartup(workerName, workerInstanceId);
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var networkService = scope.ServiceProvider.GetRequiredService<INetworkService>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                    bool internetAvailable = await networkService.IsInternetAvailableAsync();

                    if (!internetAvailable)
                    {
                        FileLogger.Log("Warning: Internet not available. Skipping sync.");
                        await LogWarningAsync("Internet not available. Skipping sync.", workerName, workerInstanceId);
                        await Task.Delay(_appSettings.WorkerDelayTime, stoppingToken);
                        continue;
                    }

                    var query = new Dictionary<string, string?>
                    {
                        ["posId"] = _appSettings.POS.ToString(),
                        ["env"] = _appSettings.Environment
                    };

                    var fullUrlIsLogEnabled = QueryHelpers.AddQueryString($"{_appSettings.BaseUrl}{Endpoints.IsLogEnabled}", query);
                    var disableLogBitUrl = QueryHelpers.AddQueryString($"{_appSettings.BaseUrl}{Endpoints.DisableLogBit}", query);

                    var logResponse = await CheckLogStatusAsync(fullUrlIsLogEnabled, stoppingToken);

                    if (logResponse?.IsLogSynced == true)
                    {
                        await ProcessLogSyncAsync(disableLogBitUrl, stoppingToken, logResponse);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in worker loop");
                    await LogErrorAsync(ex.Message, workerName, Guid.NewGuid().ToString());
                }

                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }

        private async Task<LogResponseDto?> CheckLogStatusAsync(string url, CancellationToken token)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _appSettings.Token);

                var response = await httpClient.GetAsync(url, token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<LogResponseDto>(cancellationToken: token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check IsLogEnabled API");
                await LogWarningAsync(ex.Message, nameof(LogWorkerService), Guid.NewGuid().ToString());
                return null;
            }
        }


        private async Task ProcessLogSyncAsync(string disableLogUrl, CancellationToken token, LogResponseDto logResponse)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var logCloudSyncService = scope.ServiceProvider.GetRequiredService<ISendLogToCloudService>();

                await logCloudSyncService.SyncLogAsync(_appSettings.Environment, logResponse);

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appSettings.Token);

                var response = await httpClient.GetAsync(disableLogUrl, token);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing logs or disabling log bit.");
                await LogErrorAsync(ex.Message, nameof(LogWorkerService), Guid.NewGuid().ToString());
            }
        }

        private async Task LogWarningAsync(string message, string workerName, string workerInstanceId)
        {
            FileLogger.Log("Warning: " + message);

            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<IWorkerLogService>();
            await logService.LogAsync(AlertType.Warning, message, workerName, workerInstanceId, "NetworkIssue");
        }

        private async Task LogErrorAsync(string message, string workerName, string workerInstanceId)
        {
            FileLogger.Log("Error: " + message);

            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<IWorkerLogService>();
            await logService.LogAsync(AlertType.Error, message, workerName, workerInstanceId, "Invoice Worker ServiceError");
        }
    }
}
