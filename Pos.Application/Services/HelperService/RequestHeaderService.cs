using Microsoft.AspNetCore.Http;

namespace Pos.Application.Services.HelperService
{
    /// <summary>
    /// Service implementation for retrieving POS-specific request headers.
    /// </summary>
    public class RequestHeaderService : IRequestHeaderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of <see cref="RequestHeaderService"/>.
        /// </summary>
        /// <param name="httpContextAccessor">Accessor to retrieve the current HTTP context.</param>
        public RequestHeaderService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc/>
        public int GetPosId()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context != null && context.Request.Headers.TryGetValue("POS-ID", out var posIdValue))
            {
                if (int.TryParse(posIdValue, out var posId))
                    return posId;
            }

            throw new InvalidOperationException("POS-ID header is missing or invalid.");
        }

        /// <inheritdoc/>
        public string? GetMacAddress()
        {
            var headers = _httpContextAccessor.HttpContext?.Request.Headers;

            return headers != null && headers.TryGetValue("MACADDRESS", out var value)
                ? value.FirstOrDefault()
                : null;
        }
    }
}
