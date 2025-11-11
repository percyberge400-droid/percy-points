namespace Pos.Application.Services.NetworkService
{
    /// <summary>
    /// Provides functionality to check current internet availability.
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        /// Returns true if an active internet connection is available,
        /// otherwise false.
        /// </summary>
        Task<bool> IsInternetAvailableAsync();
    }
}
