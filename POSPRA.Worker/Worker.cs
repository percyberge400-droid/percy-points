using System.Text;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly HttpService _httpService;
        private readonly string _baseUrl;

        public Worker(
            IServiceProvider serviceProvider,
            IMapper mapper,
            HttpService httpService,
            IOptions<AppSettings> options)
        {
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _httpService = httpService;
            _baseUrl = options.Value.BaseUrl;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workerInstanceId = Guid.NewGuid().ToString();

            // Log service start
            await LogAsync(AlertType.Information, "Worker service started.", workerInstanceId, AlertType.Startup);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessHealthCheck(workerInstanceId, stoppingToken);
                    await Task.Delay(1000, stoppingToken);
                }
            }
            finally
            {
                // Log service stop
                await LogAsync(AlertType.Information, "Worker service stopped.", workerInstanceId, AlertType.Shutdown);
            }
        }

        private async Task ProcessHealthCheck(string workerInstanceId, CancellationToken token)
        {
            try
            {
                var response = await _httpService.GetAsync($"{_baseUrl}{Endpoints.GetAll}", token);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Health response: {content}");

                    // ✅ Call POST with encrypted data
                    // Call POST and check result
                    bool postSuccess = await PostEncryptedDataAsync(workerInstanceId, content, token);
                    if (postSuccess)
                    {
                        // ✅ Call your service method on POST success
                        //using var scope = _serviceProvider.CreateScope();
                        //var myService = scope.ServiceProvider.GetRequiredService<IMyService>();
                        //await myService.OnPostSuccessAsync(content, token);
                    }
                }
                else
                {
                    await LogAsync(AlertType.Warning, $"Failed health check with status {response.StatusCode}", workerInstanceId, "HealthCheckFailed", (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                await LogAsync(AlertType.Exception, ex.Message, workerInstanceId, "LoopException", null, ex.StackTrace);
            }
        }

        private async Task<bool> PostEncryptedDataAsync(
      string workerInstanceId,
      string rawJson,
      CancellationToken token)
        {
            try
            {
                // Deserialize the envelope and extract only the data list
                var envelope = JsonSerializer.Deserialize<ApiResponse<List<FileRecordDto>>>(
                    rawJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (envelope?.Data == null || envelope.Data.Count == 0)
                {
                    await LogAsync(AlertType.Warning,
                                   "No records found in provided JSON.",
                                   workerInstanceId,
                                   "NoData");
                    return false;
                }

                // Serialize the list back to a plain JSON array for the API
                var jsonBody = JsonSerializer.Serialize(envelope.Data);
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // Build endpoint URL
                var url = $"{_baseUrl.TrimEnd('/')}/{Endpoints.SaveData.TrimStart('/')}";

                using var postResponse = await _httpService.PostAsync(url, content, token);

                if (postResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("POST request successful.");
                    return true;
                }

                // Read response body safely on all .NET versions
                var errorBody = await postResponse.Content.ReadAsStringAsync();
                await LogAsync(AlertType.Warning,
                               $"POST failed {postResponse.StatusCode}: {errorBody}",
                               workerInstanceId,
                               "PostFailed",
                               (int)postResponse.StatusCode);

                return false;
            }
            catch (OperationCanceledException)
            {
                await LogAsync(AlertType.Warning,
                               "POST request was canceled.",
                               workerInstanceId,
                               "PostCanceled");
                return false;
            }
            catch (Exception ex)
            {
                await LogAsync(AlertType.Exception,
                               ex.Message,
                               workerInstanceId,
                               "PostException",
                               null,
                               ex.StackTrace);
                return false;
            }
        }


        private async Task LogAsync(string type, string message, string workerInstanceId, string workerEvent, int? statusCode = null, string? stackTrace = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

            var logDto = new WorkerLogDto
            {
                Message = message,
                Type = type,
                WorkerName = "POSPRA.Worker",
                WorkerInstanceId = workerInstanceId,
                WorkerEvent = workerEvent,
                ResponseStatusCode = statusCode,
                StackTrace = stackTrace,
                WorkerStartedAtUtc = type == AlertType.Information && workerEvent.Equals(AlertType.Startup) ? DateTime.Now : null,
                WorkerStoppedAtUtc = type == AlertType.Information && workerEvent.Equals(AlertType.Shutdown) ? DateTime.Now : null
            };

            await logService.LogAsync(_mapper.Map<Logs>(logDto));
        }
    }
}
