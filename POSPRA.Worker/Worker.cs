using System.Text;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.Worker
{
    /// <summary>
    /// Background service that periodically checks a remote endpoint for unsynced file records,
    /// posts them to a decrypt/save API, and updates the local database.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IMapper _mapper;
        private readonly HttpService _http;
        private readonly string _baseUrl;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
        private readonly INetworkService _networkService;
        private readonly AppSettings _settings;
        public Worker(IServiceProvider services,
            IMapper mapper,
            HttpService http,
            IOptions<AppSettings> opts,
            INetworkService networkService,
            IOptions<AppSettings> options)
        {
            _services = services;
            _mapper = mapper;
            _http = http;
            _baseUrl = opts.Value.BaseUrl;
            _networkService = networkService;
            _settings = options.Value;
        }

        /// <summary>
        /// Main entry point for the background worker.
        /// Starts the loop that performs periodic health checks until the service is stopped.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var id = Guid.NewGuid().ToString();
            await LogAsync(AlertType.Information, "Worker service started.", id, AlertType.Startup);

            bool wasInternetAvailable = true; // Track previous state to reduce repeated logs

            try
            {
                while (!token.IsCancellationRequested)
                {
                    bool isInternetAvailable = await _networkService.IsInternetAvailableAsync();

                    if (isInternetAvailable)
                    {
                        // Internet is back or still available
                        if (!wasInternetAvailable)
                        {
                            await LogAsync(AlertType.Information, "Internet connection restored.", id, "InternetRestored");
                        }

                        wasInternetAvailable = true;

                        await ProcessHealthCheck(id, token);

                        // Normal loop delay
                        await Task.Delay(_settings.WorkerDelayTime, token);
                    }
                    else
                    {
                        // Internet is down
                        if (wasInternetAvailable)
                        {
                            // Log only when state changes
                            await LogAsync(AlertType.Warning, "Internet not available. Skipping health check.", id, "NoInternet");
                        }

                        wasInternetAvailable = false;

                        // Longer delay when offline to avoid busy loop
                        await Task.Delay(1000, token);
                    }
                }
            }
            finally
            {
                await LogAsync(AlertType.Information, "Worker service stopped.", id, AlertType.Shutdown);
            }
        }


        /// <summary>
        /// Performs a single health check:  
        /// 1) Calls the GetAllUnsynced endpoint.  
        /// 2) Posts retrieved data to the decrypt/save endpoint.  
        /// 3) Updates the local database if records are returned.
        /// </summary>
        private async Task ProcessHealthCheck(string id, CancellationToken token)
        {
            try
            {
                var resp = await _http.GetAsync($"{_baseUrl}{Endpoints.GetAllUnsyncedAsync}", token);
                if (!resp.IsSuccessStatusCode)
                {
                    await LogAsync(AlertType.Warning, $"Health check failed: {resp.StatusCode}", id, "HealthCheckFailed", (int)resp.StatusCode);
                    return;
                }

                var raw = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<object>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result!.StatusCode == ApiStatusCode.Success)
                {
                    using var post = await PostEncryptedDataAsync(id, raw, token);
                    if (post is null || !post.IsSuccessStatusCode) return;

                    var postJson = await post.Content.ReadAsStringAsync();
                    var apiResp = JsonSerializer.Deserialize<ApiResponse<List<FileRecordDto>>>(postJson, JsonOpts);
                    var files = apiResp?.Data ?? new();

                    if (files.Count > 0)
                    {
                        using var scope = _services.CreateScope();
                        var fiscal = scope.ServiceProvider.GetRequiredService<IFiscalService>();
                        await fiscal.UpdateFileRecordsAsync(files, false);
                    }
                }
            }
            catch (Exception ex)
            {
                await LogAsync(AlertType.Exception, ex.Message, id, "LoopException", null, ex.StackTrace);
            }
        }

        /// <summary>
        /// Sends encrypted data to the DecryptSave API endpoint.
        /// Returns the HTTP response so the caller can inspect status and content.
        /// Logs warnings or exceptions when the operation fails.
        /// </summary>
        private async Task<HttpResponseMessage?> PostEncryptedDataAsync(string id, string rawJson, CancellationToken token)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<ApiResponse<List<FileRecordDto>>>(rawJson, JsonOpts);
                if (envelope?.Data is null || envelope.Data.Count == 0)
                {
                    await LogAsync(AlertType.Warning, "No records in JSON.", id, "NoData");
                    return null;
                }

                var jsonBody = JsonSerializer.Serialize(envelope.Data);
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
            catch (OperationCanceledException)
            {
                await LogAsync(AlertType.Warning, "POST request canceled.", id, "PostCanceled");
            }
            catch (Exception ex)
            {
                await LogAsync(AlertType.Exception, ex.Message, id, "PostException", null, ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Writes a log entry to the database using the ILogService.
        /// Supports information, warning, and exception logs with optional status code and stack trace.
        /// </summary>
        private async Task LogAsync(
            string type,
            string message,
            string workerId,
            string evt,
            int? statusCode = null,
            string? stackTrace = null)
        {
            using var scope = _services.CreateScope();
            var logSvc = scope.ServiceProvider.GetRequiredService<ILogService>();

            var log = new WorkerLogDto
            {
                Message = message,
                Type = type,
                WorkerName = nameof(Worker),
                WorkerInstanceId = workerId,
                WorkerEvent = evt,
                ResponseStatusCode = statusCode,
                StackTrace = stackTrace,
                WorkerStartedAtUtc = type == AlertType.Information && evt == AlertType.Startup ? DateTime.Now : null,
                WorkerStoppedAtUtc = type == AlertType.Information && evt == AlertType.Shutdown ? DateTime.Now : null
            };

            await logSvc.LogAsync(_mapper.Map<Logs>(log));
        }
    }
}