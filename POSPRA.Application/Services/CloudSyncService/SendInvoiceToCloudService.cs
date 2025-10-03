using System.Text;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.ConfigurationService;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.Application.Services.CloudSyncService
{
    public class SendInvoiceToCloudService(
        IServiceProvider services,
        IMapper mapper,
        HttpService http,
        IOptions<AppSettings> opts,
        INetworkService networkService,
        IOptions<AppSettings> options,
        IServiceScopeFactory scopeFactory,
        IConfigurationService configurationService) : ISendInvoiceToCloudService
    {
        private readonly IMapper _mapper = mapper;
        private readonly HttpService _http = http;
        private readonly string _baseUrl = opts.Value.BaseUrl;
        private readonly INetworkService _networkService = networkService;
        private readonly AppSettings _settings = options.Value;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IConfigurationService _configurationService = configurationService;
        private readonly IServiceProvider _services = services;

        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public async Task SyncInvoicesAsync(CancellationToken token, string workerId)
        {
            bool isInternetAvailable = await _networkService.IsInternetAvailableAsync();

            if (!isInternetAvailable)
            {
                await LogAsync(AlertType.Warning, "Internet not available. Skipping sync.", workerId, "NoInternet");
                return;
            }

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
                    await LogAsync(AlertType.Warning, $"Sync failed: {response.StatusCode}", id, "SyncFailed");
                }
            }
            catch (Exception ex)
            {
                await LogAsync(AlertType.Exception, ex.Message, id, "SyncException", null, ex.ToString());
            }
        }

        private async Task<HttpResponseMessage?> PostEncryptedDataAsync(string id, List<FileRecordDto> fileRecordDtos, CancellationToken token)
        {
            try
            {
                if (fileRecordDtos.Count == 0)
                {
                    await LogAsync(AlertType.Warning, "No records to send.", id, "NoData");
                    return null;
                }

                var jsonBody = JsonSerializer.Serialize(fileRecordDtos);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}{Endpoints.DecryptSave.TrimStart('/')}";

                var resp = await _http.PostAsync(url, content, token);
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    await LogAsync(AlertType.Warning, $"POST failed {resp.StatusCode}: {err}", id, "PostFailed", (int)resp.StatusCode);
                }
                return resp;
            }
            catch (Exception ex)
            {
                await LogAsync(AlertType.Exception, ex.Message, id, "PostException", null, ex.StackTrace);
            }
            return null;
        }

        private async Task LogAsync(string type, string message, string workerId, string evt, int? statusCode = null, string? stackTrace = null)
        {
            using var scope = _services.CreateScope();
            var logSvc = scope.ServiceProvider.GetRequiredService<ILogService>();

            var log = new WorkerLogDto
            {
                Message = message,
                Type = type,
                WorkerName = nameof(SendInvoiceToCloudService),
                WorkerInstanceId = workerId,
                WorkerEvent = evt,
                ResponseStatusCode = statusCode,
                StackTrace = stackTrace
            };

            await logSvc.LogAsync(_mapper.Map<Logs>(log));
        }

        public async Task LogStartup(string workerId)
        {
            await LogAsync(
                AlertType.Info,
                "Worker service started.",
                workerId,
                AlertType.Startup
            );
        }

        public async Task LogShutdown(string workerId)
        {
            await LogAsync(
                AlertType.Info,
                "Worker service stopped.",
                workerId,
                AlertType.Shutdown
            );
        }
    }
}
