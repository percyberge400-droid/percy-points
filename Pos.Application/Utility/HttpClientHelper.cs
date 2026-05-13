using Pos.Application.DTOs;
using Pos.Application.DTOs.LogDTOs;
//using Pos.Application.DTOs.POS.LogDTOs;
using Pos.Application.Services.LogService;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Pos.Application.Utility
{
    public class HttpClientHelpers
    {
        public long PosId { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }

    public static class HttpClientHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        // ─────────────────────────────────────────────
        //  Authenticate by MAC
        // ─────────────────────────────────────────────
        public static async Task<ApiResponse<TResponse>> AuthenticateByMacAsync<TResponse>(
            string baseUrl,
            long posId,
            string macAddress,
            string bearerToken,
            string environment,
            ILogService logService,
            AppSettings appSettings)
        {
            const string endpoint = "api/Live/authenticate-by-mac";
            string url = $"{baseUrl.TrimEnd('/')}/{endpoint}";

            var payload = new List<HttpClientHelpers>
            {
                new()
                {
                    PosId       = posId,
                    MacAddress  = macAddress,
                    Token       = bearerToken,
                    Environment = environment
                }
            };

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(handler);

            try
            {
                var json = JsonSerializer.Serialize(payload);

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return await HandleError<TResponse>(
                        response, content, url, "POST",
                        logService, appSettings);

                var data = string.IsNullOrWhiteSpace(content)
                    ? default
                    : JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);

                return new ApiResponse<TResponse>(
                    ((int)response.StatusCode).ToString(),
                    "Success",
                    data);
            }
            catch (Exception ex)
            {
                return await HandleException<TResponse>(
                    ex, url, "POST", logService, appSettings);
            }
        }

        // ─────────────────────────────────────────────
        //  GET
        // ─────────────────────────────────────────────
        public static async Task<ApiResponse<List<T>>> GetAsync<T>(
      string endpoint,
      string? bearerToken,
      string? baseUrl,
      ILogService logService,
      AppSettings appSettings)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(handler);

            string url = $"{baseUrl?.TrimEnd('/')}/{endpoint.TrimStart('/')}";

            try
            {
                ApplyAuthHeaders(bearerToken!, httpClient);

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleError<List<T>>(
                        response, content, url, "GET",
                        logService, appSettings);
                }

                var apiResponse = string.IsNullOrWhiteSpace(content)
                    ? new ApiResponse<List<T>> { Data = new List<T>() }
                    : DeserializeFlexible<T>(content, _jsonOptions);

                return new ApiResponse<List<T>>(
                    apiResponse?.StatusCode ?? ApiStatusCode.Success.ToString(),
                    apiResponse?.Message ?? "Success",
                    apiResponse?.Data ?? new List<T>()
                );
            }
            catch (Exception ex)
            {
                return await HandleException<List<T>>(
                    ex, url, "GET", logService, appSettings);
            }
        }

        // ─────────────────────────────────────────────
        //  POST (generic)
        // ─────────────────────────────────────────────
        public static async Task<ApiResponse<TResponse>> PostAsync<TResponse>(
            string url,
            object body,
            string? bearerToken = null,
            Dictionary<string, string>? headers = null,
            ILogService? logService = null,
            AppSettings? appSettings = null)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(handler);

            try
            {
                var json = JsonSerializer.Serialize(body);

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrWhiteSpace(bearerToken))
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", bearerToken);

                if (headers != null)
                    foreach (var h in headers)
                        request.Headers.Add(h.Key, h.Value);

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return await HandleError<TResponse>(
                        response, content, url, "POST",
                        logService, appSettings);      //  removed ! — both are nullable

                if (string.IsNullOrWhiteSpace(content))
                    return new ApiResponse<TResponse>(
                        ((int)response.StatusCode).ToString(),
                        "Success",
                        default);

                // ✅ string type — return raw content directly
                // Prevents System.Text.Json vs Newtonsoft JObject conflict
                // Used by: AuthenticateAsync, SetEnvironmentAsync
                if (typeof(TResponse) == typeof(string))
                {
                    return new ApiResponse<TResponse>(
                        ((int)response.StatusCode).ToString(),
                        "Success",
                        (TResponse)(object)content);   // raw JSON — caller parses with Newtonsoft
                }

                // ✅ All other types — deserialize normally with System.Text.Json
                // Used by: all other callers — completely unaffected
                var data = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);

                return new ApiResponse<TResponse>(
                    ((int)response.StatusCode).ToString(),
                    "Success",
                    data);
            }
            catch (Exception ex)
            {
                return await HandleException<TResponse>(
                    ex, url, "POST", logService, appSettings);  //  removed ! — both are nullable
            }
        }

        // ─────────────────────────────────────────────
        //  POST — raw string response (CSV, plain text)
        // ─────────────────────────────────────────────

        // ─────────────────────────────────────────────
        //  POST — extracts "data" field as raw CSV string
        //  Use ONLY for APIs returning { statusCode, message, data: "csv..." }
        // ─────────────────────────────────────────────
        public static async Task<ApiResponse<string>> PostAsyncRawString(
            string url,
            object body,
            string? bearerToken = null,
            Dictionary<string, string>? headers = null,
            ILogService? logService = null,
            AppSettings? appSettings = null)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(handler);

            try
            {
                var json = JsonSerializer.Serialize(body);

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrWhiteSpace(bearerToken))
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", bearerToken);

                if (headers != null)
                    foreach (var h in headers)
                        request.Headers.Add(h.Key, h.Value);

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return await HandleError<string>(
                        response, content, url, "POST",
                        logService, appSettings);

                if (string.IsNullOrWhiteSpace(content))
                    return new ApiResponse<string>(
                        ((int)response.StatusCode).ToString(),
                        "Success",
                        string.Empty);


                // Correctly unescapes \r\n so CSV splits into proper lines
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                string? rawData = root.TryGetProperty("data", out var dataProp)
                    ? dataProp.GetString()
                    : content;

                return new ApiResponse<string>(
                    ((int)response.StatusCode).ToString(),
                    "Success",
                    rawData ?? string.Empty);
            }
            catch (Exception ex)
            {
                return await HandleException<string>(
                    ex, url, "POST", logService, appSettings);
            }
        }
        // ─────────────────────────────────────────────
        //  PRIVATE: Handle non-success HTTP response
        // ─────────────────────────────────────────────
        private static async Task<ApiResponse<T>> HandleError<T>(
            HttpResponseMessage response,
            string content,
            string url,
            string method,
            ILogService logService,
            AppSettings appSettings)
        {
            string errorMessage = ExtractErrorMessage(content)
                                  ?? response.ReasonPhrase
                                  ?? "API Error";

            var log = logService.BuildLog(
                message: errorMessage,

                type: AlertType.Error.ToString(),        //  enum → string
                module: "POS FBR",
                action: method
            //PosId: AesEncryptionHelper.Decrypt(appSettings.POS.ToString()!),
            //responseStatusCode: (int)response.StatusCode,
            //responseBody: content,
            //url: url,
            //timestamp: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            );
            // Manually populate the fields BuildLog doesn't cover
            log.ResponseStatusCode = (int)response.StatusCode;
            log.ResponseBody = content;
            log.Url = url;
            log.Timestamp = DateTime.Now;
            log.POSID = appSettings.POS;
            log.POSID = long.Parse(AesEncryptionHelper.Decrypt(appSettings.EC)); // EC holds the reg no



            await logService.CreateLogAsync(log);

            return new ApiResponse<T>(
                ((int)response.StatusCode).ToString(),
                errorMessage,
                default,
                content);
        }

        // ─────────────────────────────────────────────
        //  PRIVATE: Handle thrown exceptions
        // ─────────────────────────────────────────────
        private static async Task<ApiResponse<T>> HandleException<T>(
            Exception ex,
            string? url,
            string method,
            ILogService logService,
            AppSettings appSettings)
        {
            var log = new CreateLogDto
            {
                POSID = appSettings.POS,//AesEncryptionHelper.Decrypt(appSettings.POS.ToString()!),
                Message = ex.Message,
                Type = AlertType.Error.ToString(),          //  enum → string
                Url = url,
                Timestamp = DateTime.Now,
                HttpMethod = method,
                RequestPath = url,
                ExceptionType = ex.GetType().FullName,
                StackTrace = ex.StackTrace,
                Module = "POS FBR",
                ActionName = method,
                MachineName = Environment.MachineName,
                ApplicationName = AppDomain.CurrentDomain.FriendlyName,
                EnvironmentName = appSettings.Environment,
            };

            await logService.CreateLogAsync(log);

            return new ApiResponse<T>("0", "Request failed", default, ex.Message);
        }

        // ─────────────────────────────────────────────
        //  PRIVATE: Extract readable error from body
        // ─────────────────────────────────────────────
        private static string? ExtractErrorMessage(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            // Try JSON
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // WSO2 JSON fault
                if (root.TryGetProperty("fault", out var fault))
                {
                    var message = fault.TryGetProperty("message", out var msg) ? msg.GetString() : null;
                    var description = fault.TryGetProperty("description", out var desc) ? desc.GetString() : null;
                    return $"{message} {description}".Trim();
                }

                // Business validation error
                if (root.TryGetProperty("validationResponse", out var vr))
                {
                    var statusCode = vr.TryGetProperty("statusCode", out var sc) ? sc.GetString() : null;
                    if (statusCode != "00")
                        return vr.TryGetProperty("error", out var err)
                            ? err.GetString()
                            : "Validation Failed";
                }

                // Generic message field
                if (root.TryGetProperty("message", out var genericMessage))
                    return genericMessage.GetString();
            }
            catch { }

            // Try XML (WSO2 <ams:fault>)
            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(content);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ams", "http://wso2.org/apimanager/security");

                var messageNode = xml.SelectSingleNode("//ams:message", nsmgr);
                var descriptionNode = xml.SelectSingleNode("//ams:description", nsmgr);
                return $"{messageNode?.InnerText} {descriptionNode?.InnerText}".Trim();
            }
            catch { }

            return content;
        }

        // ─────────────────────────────────────────────
        //  PRIVATE: Apply Bearer + Accept headers
        // ─────────────────────────────────────────────
        private static void ApplyAuthHeaders(string bearerToken, HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);

            if (!httpClient.DefaultRequestHeaders.Accept.Any())
                httpClient.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static ApiResponse<List<T>> DeserializeFlexible<T>(string content, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                var data = ExtractList<T>(root, options);
                return new ApiResponse<List<T>>(
                    ApiStatusCode.Success.ToString(), "Success", data);
            }

            string statusCode = root.TryGetProperty("statusCode", out var sc)
                ? sc.GetString() ?? ApiStatusCode.Success.ToString()
                : ApiStatusCode.Success.ToString();

            string message = root.TryGetProperty("message", out var mg)
                ? mg.GetString() ?? "Success"
                : "Success";

            if (!root.TryGetProperty("data", out var dataProp))
                return new ApiResponse<List<T>>(statusCode, message, new List<T>());

            return new ApiResponse<List<T>>(statusCode, message,
                ExtractList<T>(dataProp, options));
        }

        private static List<T> ExtractList<T>(JsonElement element, JsonSerializerOptions options)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Array:
                    return JsonSerializer.Deserialize<List<T>>(element.GetRawText(), options)
                           ?? new List<T>();

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return new List<T>();

                default:
                    var single = JsonSerializer.Deserialize<T>(element.GetRawText(), options);
                    return single is not null ? new List<T> { single } : new List<T>();
            }
        }
    }
}