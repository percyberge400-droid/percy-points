using System.Text;
using System.Text.Json;

namespace Pos.Application.Services.HttpClientService
{
    /// <summary>
    /// A simple HTTP client service for sending GET and POST requests.
    /// </summary>
    public class HttpService : HttpClient, IDisposable
    {
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of <see cref="HttpService"/>.
        /// </summary>
        /// <param name="httpClient">Injected HttpClient instance.</param>
        public HttpService(HttpClient httpClient)
        {
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Sends a GET request to the specified URL with optional headers.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <returns>The response body as a string.</returns>
        public async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add custom headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sends a POST request with JSON payload to the specified URL and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="data">The object to serialize as JSON and send in the request body.</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <returns>The deserialized response object of type <typeparamref name="T"/>.</returns>
        public async Task<T> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
        {
            var json = JsonSerializer.Serialize(data);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            // Add custom headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson)!;
        }

        /// <summary>
        /// Disposes the underlying HttpClient instance.
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
