using System.Text;
using System.Text.Json;

namespace POSPRA.Application.Services.HttpClientService
{
    public class HttpService(HttpClient httpClient) : IHttpService, IDisposable
    {
        private readonly HttpClient _client = httpClient;

        public async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add custom headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<T> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
        {
            var json = JsonSerializer.Serialize(data);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson)!;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
