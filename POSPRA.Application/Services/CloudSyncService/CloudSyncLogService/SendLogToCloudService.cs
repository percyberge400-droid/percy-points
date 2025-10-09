using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDTOs;
using System.Text;
using System.Text.Json;

namespace POSPRA.Application.Services.CloudSyncService.CloudSyncLogService
{
    public class SendLogToCloudService : ISendLogToCloudService
    {
        private readonly HttpService _http;
        private readonly string _baseUrl;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWorkerLogService _workerLogService;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public SendLogToCloudService(IServiceScopeFactory scopeFactory,
            HttpService http,
            IOptions<AppSettings> options
,
            IWorkerLogService workerLogService)
        {
            _scopeFactory = scopeFactory;
            _http = http;
            _baseUrl = options.Value.BaseUrl;
            _workerLogService = workerLogService;
        }

        public async Task SyncLogAsync(CancellationToken token, string workerId)
        {
            await ProcessHealthCheck(workerId, token);
        }

        private async Task ProcessHealthCheck(string id, CancellationToken token)
        {
            try
            {
                using var readScope = _scopeFactory.CreateScope();
                var logService = readScope.ServiceProvider.GetRequiredService<ILogService>();
                var response = await logService.GetAllUnsyncLogs();

                if (response.StatusCode == ApiStatusCode.Success)
                {
                    var resp = await PostDataAsync(id, response.Data, token);
                    if (resp is null || !resp.IsSuccessStatusCode)
                        return;

                    var postJson = await resp.Content.ReadAsStringAsync(token);
                    var apiResp = JsonSerializer.Deserialize<ApiResponse<List<Logs>>>(postJson, JsonOpts);
                    var logs = apiResp?.Data ?? new();
                    if (logs.Count > 0)
                    {
                        using var updateScope = _scopeFactory.CreateScope();
                        var log = updateScope.ServiceProvider.GetRequiredService<ILogService>();
                        await log.UpdateLog(logs);
                    }
                }
                else if (response.StatusCode != ApiStatusCode.NotFound)
                {
                    await _workerLogService.LogAsync(AlertType.Warning, $"Sync failed: {response.StatusCode}", nameof(SendInvoiceToCloudService), id, "SyncFailed");
                }
            }
            catch (Exception ex)
            {
                await _workerLogService.LogAsync(AlertType.Exception, ex.Message, nameof(SendInvoiceToCloudService), id, "SyncException", null, ex.ToString());
            }
        }

        private async Task<HttpResponseMessage?> PostDataAsync(string id, List<SyncLogDto> logDtos, CancellationToken token)
        {
            try
            {
                if (logDtos.Count == 0)
                {
                    await _workerLogService.LogAsync(AlertType.Warning, "No records to send.", nameof(SendInvoiceToCloudService), id, "NoData");
                    return null;
                }

                var jsonBody = JsonSerializer.Serialize(logDtos);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}{Endpoints.CreateCloudLog}";

                var resp = await _http.PostAsync(url, content, token);
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    await _workerLogService.LogAsync(AlertType.Warning, $"POST failed {resp.StatusCode}: {err}", nameof(SendInvoiceToCloudService), id, "PostFailed", (int)resp.StatusCode);
                }
                return resp;
            }
            catch (Exception ex)
            {
                await _workerLogService.LogAsync(AlertType.Exception, ex.Message, nameof(SendInvoiceToCloudService), id, "PostException", null, ex.StackTrace);
            }
            return null;
        }
    }
}
