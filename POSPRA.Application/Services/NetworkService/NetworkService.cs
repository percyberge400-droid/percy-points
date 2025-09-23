namespace POSPRA.Application.Services.NetworkService
{
    /// <summary>
    /// Default implementation that checks internet availability
    /// by testing general network connectivity.
    /// </summary>
    /// <inheritdoc />
    public class NetworkService : INetworkService
    {
        /// <summary>
        /// Checks if the machine has active internet by trying to reach a reliable external host.
        /// </summary>
        public async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                // You can replace with your own endpoint
                using var response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}