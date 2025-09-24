using System.Net.NetworkInformation;

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
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 3000); // Google DNS
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}