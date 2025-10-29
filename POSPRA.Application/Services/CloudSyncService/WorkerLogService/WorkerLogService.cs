using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Utility;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.Application.Services.CloudSyncService.WorkerLogService
{
    public class WorkerLogService : IWorkerLogService
    {
        private readonly HttpService _http;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public WorkerLogService(HttpService http, IOptions<AppSettings> options)
        {
            _http = http;
            _baseUrl = options.Value.BaseUrl;
        }

        public async Task LogAsync(
            string type,
            string message,
            string workerName,
            string workerId,
            string evt,
            int? statusCode = null,
            string? stackTrace = null)
        {
            try
            {
                var log = new CreateLogDto
                {
                    Message = message,
                    Type = type,
                    WorkerName = workerName,
                    WorkerInstanceId = workerId,
                    WorkerEvent = evt,
                    ResponseStatusCode = statusCode,
                    StackTrace = stackTrace
                };

                var jsonBody = JsonSerializer.Serialize(log, _jsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // ✅ your API endpoint for logs
                var url = $"{_baseUrl}{Endpoints.CreateLog}";

                var response = await _http.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"⚠️ [WorkerLogService] Failed to send log ({response.StatusCode}): {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [WorkerLogService] Exception while logging: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Task LogStartup(string workerName, string workerId)
        {
            await LogAsync(AlertType.Info, "POS service started.", workerName, workerId, AlertType.Startup);
        }

        public async Task LogShutdown(string workerName, string workerId)
        {
            await LogAsync(AlertType.Info, "POS service stopped.", workerName, workerId, AlertType.Shutdown);
        }
    }
}
