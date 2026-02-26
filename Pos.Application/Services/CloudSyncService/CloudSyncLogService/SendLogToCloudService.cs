using AutoMapper;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.HttpClientService;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using System.Net.Http.Headers; // CHANGED
using System.Text;
using System.Text.Json;

namespace Pos.Application.Services.CloudSyncService.CloudSyncLogService
{
    public class SendLogToCloudService : ISendLogToCloudService
    {
        private readonly HttpService _http;
        private readonly string _baseUrl;
        private readonly ILogService _logService;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings; // CHANGED: access token

        public SendLogToCloudService(
            HttpService http,
            IOptions<AppSettings> options,
            ILogService logService,
            IMapper mapper)
        {
            _http = http;
            _baseUrl = options.Value.BaseUrl;
            _logService = logService;
            _mapper = mapper;
            _appSettings = options.Value; // CHANGED
        }

        public async Task SyncLogAsync(string env, LogResponseDto? logResponse)
        {
            await ProcessHealthCheck(env, logResponse);
        }

        private async Task ProcessHealthCheck(string env, LogResponseDto? logResponse)
        {
            try
            {
                var response = await _logService.GetAllUnsyncLogs(logResponse);

                if (response.StatusCode == ApiStatusCode.Success)
                {
                    var resp = await PostDataAsync(response.Data, env);
                    if (!resp.IsSuccessStatusCode)
                        return;

                    var logs = _mapper.Map<List<Logs>>(response?.Data);
                    await _logService.UpdateLog(logs);

                }
                else if (response.StatusCode != ApiStatusCode.NotFound)
                {
                    await _logService.CreateLogAsync(new CreateLogDto(ResponseMessages.DataNotFound, AlertType.Exception, false));
                }
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{GlobalVariables.DATE} InsertInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.CreateLogAsync(new CreateLogDto(errorMessage, AlertType.Error, false));
            }
        }

        private async Task<HttpResponseMessage?> PostDataAsync(List<SyncLogDto> logDtos, string env)
        {
            try
            {
                if (logDtos.Count == 0)
                {
                    await _logService.CreateLogAsync(new CreateLogDto(ResponseMessages.DataNotFound, AlertType.Exception, false));
                    return null;
                }

                var jsonBody = JsonSerializer.Serialize(logDtos);
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // Construct URL
                var url = $"{_baseUrl}{Endpoints.CreateCloudLog}?environment={env}";

                // CHANGED: Use HttpRequestMessage to add headers
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                // CHANGED: Add Authorization header from appsettings
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _appSettings.Token);

                // CHANGED: Use SendAsync
                var resp = await _http.SendAsync(request);

                if (!resp.IsSuccessStatusCode)
                {
                    await _logService.CreateLogAsync(new CreateLogDto(ResponseMessages.DatabaseError, AlertType.Warning, false));
                }

                return resp;
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{GlobalVariables.DATE} PostException: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.CreateLogAsync(new CreateLogDto(errorMessage, AlertType.Exception, false));
            }

            return null;
        }
    }
}
