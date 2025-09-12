using Microsoft.AspNetCore.Http;

namespace POSPRA.Application.Services.HelperService
{
    public class RequestHeaderService : IRequestHeaderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestHeaderService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long GetPosId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.TryGetValue("POS-ID", out var posIdValue))
            {
                if (long.TryParse(posIdValue, out var posId))
                {
                    return posId;
                }
            }

            throw new InvalidOperationException("POS-ID header is missing or invalid.");
        }

        public string? GetMacAddress()
        {
            var headers = _httpContextAccessor.HttpContext?.Request.Headers;
            return headers != null && headers.TryGetValue("MACADDRESS", out var value)
                ? value.FirstOrDefault()
                : null;
        }
    }
}