using AutoMapper;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
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
        private readonly ILogService _logService;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
        private readonly IMapper _mapper;

        public SendLogToCloudService(
            HttpService http,
            IOptions<AppSettings> options
,
            ILogService logService,
            IMapper mapper)
        {
            _http = http;
            _baseUrl = options.Value.BaseUrl;
            _logService = logService;
            _mapper = mapper;
        }

        public async Task SyncLogAsync()
        {
            await ProcessHealthCheck();
        }

        private async Task ProcessHealthCheck()
        {
            try
            {
                var response = await _logService.GetAllUnsyncLogs();

                if (response.StatusCode == ApiStatusCode.Success)
                {
                    var resp = await PostDataAsync(response.Data);
                    if (resp is null || !resp.IsSuccessStatusCode)
                        return;

                    var postJson = await resp.Content.ReadAsStringAsync();
                    var apiResp = JsonSerializer.Deserialize<ApiResponse<List<SyncLogDto>>>(postJson, JsonOpts);
                    var logDtos = apiResp?.Data ?? new();
                    if (logDtos.Count > 0)
                    {
                        var logs = _mapper.Map<List<Logs>>(logDtos);
                        await _logService.UpdateLog(logs);
                    }
                }
                else if (response.StatusCode != ApiStatusCode.NotFound)
                {
                    await _logService.CreateLogAsync(new Logs(ResponseMessages.DataNotFound, AlertType.Exception, false));
                }
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{GlobalVariables.DATE} InsertInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.CreateLogAsync(new Logs(errorMessage, AlertType.Error, false));
            }
        }

        private async Task<HttpResponseMessage?> PostDataAsync(List<SyncLogDto> logDtos)
        {
            try
            {
                if (logDtos.Count == 0)
                {
                    await _logService.CreateLogAsync(new Logs(ResponseMessages.DataNotFound, AlertType.Exception, false));
                    return null;
                }

                var jsonBody = JsonSerializer.Serialize(logDtos);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}{Endpoints.CreateCloudLog}";

                var resp = await _http.PostAsync(url, content);
                if (!resp.IsSuccessStatusCode)
                {
                    await _logService.CreateLogAsync(new Logs(ResponseMessages.DatabaseError, AlertType.Warning, false));
                }
                return resp;
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{GlobalVariables.DATE} PostException: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.CreateLogAsync(new Logs(errorMessage, AlertType.Exception, false));
            }
            return null;
        }
    }
}
