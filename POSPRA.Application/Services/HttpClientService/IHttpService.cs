namespace POSPRA.Application.Services.HttpClientService
{
    /// <summary>
    /// Service contract for sending HTTP requests.
    /// Provides methods for GET and POST operations with optional headers.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Sends a GET request to the specified URL.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="headers">Optional HTTP headers.</param>
        /// <returns>The response body as a string.</returns>
        Task<string> GetAsync(string url, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Sends a POST request with a JSON payload to the specified URL and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into.</typeparam>
        /// <param name="url">The target URL.</param>
        /// <param name="data">The request payload object to serialize to JSON.</param>
        /// <param name="headers">Optional HTTP headers.</param>
        /// <returns>The deserialized response object of type <typeparamref name="T"/>.</returns>
        Task<T> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null);
    }
}
