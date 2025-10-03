using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.ConfigurationService;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;

public class SendInvoiceToCloudService : ISendInvoiceToCloudService
{
    private readonly HttpService _http;
    private readonly string _baseUrl;
    private readonly INetworkService _networkService;
    private readonly AppSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfigurationService _configurationService;
    private readonly IWorkerLogService _workerLogService;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public SendInvoiceToCloudService(
        IServiceProvider services,
        HttpService http,
        INetworkService networkService,
        IOptions<AppSettings> options,
        IServiceScopeFactory scopeFactory,
        IConfigurationService configurationService,
        IWorkerLogService workerLogService)
    {
        _http = http;
        _baseUrl = options.Value.BaseUrl;
        _networkService = networkService;
        _settings = options.Value;
        _scopeFactory = scopeFactory;
        _configurationService = configurationService;
        _workerLogService = workerLogService;
    }

    public async Task SyncInvoicesAsync(CancellationToken token, string workerId)
    {
        await ProcessHealthCheck(workerId, token);
    }

    private async Task ProcessHealthCheck(string id, CancellationToken token)
    {
        try
        {
            using var readScope = _scopeFactory.CreateScope();
            var fiscalService = readScope.ServiceProvider.GetRequiredService<IFileRecordService>();
            var response = await fiscalService.GetAllUnsyncedAsync();

            if (response.StatusCode == ApiStatusCode.Success)
            {
                var resp = await PostEncryptedDataAsync(id, response.Data, token);
                if (resp is null || !resp.IsSuccessStatusCode)
                    return;

                var postJson = await resp.Content.ReadAsStringAsync(token);
                var apiResp = JsonSerializer.Deserialize<ApiResponse<List<FileRecordDto>>>(postJson, JsonOpts);
                var files = apiResp?.Data ?? new();

                if (files.Count > 0)
                {
                    using var updateScope = _scopeFactory.CreateScope();
                    var fiscal = updateScope.ServiceProvider.GetRequiredService<IFileRecordService>();
                    await fiscal.UpdateFileRecordsAsync(files, false);
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

    private async Task<HttpResponseMessage?> PostEncryptedDataAsync(string id, List<FileRecordDto> fileRecordDtos, CancellationToken token)
    {
        try
        {
            if (fileRecordDtos.Count == 0)
            {
                await _workerLogService.LogAsync(AlertType.Warning, "No records to send.", nameof(SendInvoiceToCloudService), id, "NoData");
                return null;
            }

            var jsonBody = JsonSerializer.Serialize(fileRecordDtos);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var url = $"{_baseUrl}{Endpoints.DecryptSave.TrimStart('/')}";

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
