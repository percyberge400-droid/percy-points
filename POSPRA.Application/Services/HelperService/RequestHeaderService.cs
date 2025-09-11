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

        public int? GetPosId()
        {
            var headers = _httpContextAccessor.HttpContext?.Request.Headers;
            if (headers != null && headers.TryGetValue("POSID", out var value)
                && int.TryParse(value.FirstOrDefault(), out var posId))
            {
                return posId;
            }
            return null;
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