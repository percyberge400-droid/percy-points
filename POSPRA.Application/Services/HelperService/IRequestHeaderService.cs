namespace POSPRA.Application.Services.HelperService
{
    /// <summary>
    /// Service contract for retrieving request-specific header information.
    /// </summary>
    public interface IRequestHeaderService
    {
        /// <summary>
        /// Retrieves the POS (Point of Sale) ID from the request context or header.
        /// </summary>
        /// <returns>The POS ID as a long.</returns>
        long GetPosId();

        /// <summary>
        /// Retrieves the MAC address of the current machine or request context.
        /// </summary>
        /// <returns>The MAC address as a string, or null if unavailable.</returns>
        string? GetMacAddress();
    }
}
