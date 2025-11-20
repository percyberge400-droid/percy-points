using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.Services.CloudSyncService.WorkerLogService;
using Pos.Application.Services.FileRecordService;
using Pos.Application.Services.HttpClientService;
using Pos.Application.Utility;
using System;
using System.Text;
using System.Text.Json;

namespace Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService
{
    public class SendInvoiceToCloudService : ISendInvoiceToCloudService
    {
        private readonly HttpService _http;
        private readonly string _baseUrl;
        private readonly IWorkerLogService _workerLogService;
        private readonly IFileRecordService _fileRecordService;


        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public SendInvoiceToCloudService(
            IServiceProvider services,
            HttpService http,
            IOptions<AppSettings> options,
            IWorkerLogService workerLogService,
            IFileRecordService fileRecordService)
        {
            _http = http;
            _baseUrl = options.Value.BaseUrl;
            _workerLogService = workerLogService;
            _fileRecordService = fileRecordService;
        }

        public async Task SyncInvoicesAsync(CancellationToken token, string workerId, string evn)
        {
            await ProcessHealthCheck(workerId, token, evn);
        }

        private async Task ProcessHealthCheck(string id, CancellationToken token, string env)
        {
            try
            {
                var response = await _fileRecordService.GetAllUnsyncedAsync();
                if (response.StatusCode != ApiStatusCode.Success)
                    return;


                // ✅ Send to cloud (now passing List<FileRecordDto>)
                var resp = await PostEncryptedDataAsync(id, response.Data, token, env);
                if (resp is null || !resp.IsSuccessStatusCode)
                    return;

                var postJson = await resp.Content.ReadAsStringAsync(token);
                var apiResp = JsonSerializer.Deserialize<ApiResponse<List<FileRecordDto>>>(postJson, JsonOpts);
                var syncedFiles = apiResp?.Data ?? new();

                if (syncedFiles.Count > 0)
                {
                    var updateResponse = await _fileRecordService.UpdateFileRecordsAsync(syncedFiles);
                    if (updateResponse.StatusCode == ApiStatusCode.Success)
                        foreach (var file in syncedFiles)
                        {
                            await _workerLogService.LogAsync(
                                AlertType.Info,
                                $"Invoice '{file.InvoiceNumber}' synced successfully.",
                                nameof(SendInvoiceToCloudService),
                                id,
                                "InvoiceSynced");
                        }
                }
            }
            catch (Exception ex)
            {
                await _workerLogService.LogAsync(
                    AlertType.Exception,
                    ex.Message,
                    nameof(SendInvoiceToCloudService),
                    id,
                    "SyncException",
                    null,
                    ex.ToString());
            }
        }

        private async Task<HttpResponseMessage?> PostEncryptedDataAsync(string id, List<FileRecordDto> fileRecordDtos, CancellationToken token, string env)
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

                // Add environment as a query parameter
                var url = $"{_baseUrl}{Endpoints.DecryptSave}?environment={env}";

                var resp = await _http.PostAsync(url, content, token);
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync(token);
                    await _workerLogService.LogAsync(AlertType.Warning, $"POST failed {resp.StatusCode}: {err}", nameof(SendInvoiceToCloudService), id, "PostFailed", (int)resp.StatusCode);
                }
                return resp;
            }
            catch (Exception ex)
            {
                await _workerLogService.LogAsync(AlertType.Exception, ex.Message, nameof(SendInvoiceToCloudService), id, "PostException", null, ex.ToString());
            }
            return null;
        }
    }

}
