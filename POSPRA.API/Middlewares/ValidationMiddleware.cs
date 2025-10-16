using Microsoft.Extensions.Options;
using POSPRA.API.Middlewares.Options;
using POSPRA.Application.Services.ClientService;
using POSPRA.Application.Utility;
using POSPRA.DTOs.ClientDtos;

namespace POSPRA.API.Middlewares
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ValidationMiddlewareOptions _options;

        public ValidationMiddleware(
            RequestDelegate next,
            IOptions<ValidationMiddlewareOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, IClientService clientService)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Skip validation if path matches any configured bypass url/prefix
            if (_options.BypassUrls.Any(p =>
                    path.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // ---------- Normal validation ----------
            var token = context.Request.Headers[RequestValidationDefaults.HeaderAuthorization]
                .FirstOrDefault()
                ?.Replace(RequestValidationDefaults.BearerPrefix, string.Empty,
                          StringComparison.OrdinalIgnoreCase);

            var mac = context.Request.Headers[RequestValidationDefaults.HeaderMacAddress].FirstOrDefault();
            var posHeader = context.Request.Headers[RequestValidationDefaults.HeaderPosId].FirstOrDefault();

            if (!long.TryParse(posHeader, out var posId))
                posId = 0;

            if (string.IsNullOrWhiteSpace(token) || posId == 0 || string.IsNullOrWhiteSpace(mac))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = RequestValidationDefaults.ApplicationType;
                await context.Response.WriteAsync(RequestValidationDefaults.MissingHeadersMessage);
                return;
            }

            var dto = new ClientValidationDto { Token = token, PosId = posId, MacAddress = mac };
            var validationResult = await clientService.GetByMacAsync(dto);

            if (validationResult.Data is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(validationResult);
                return;
            }

            await _next(context);
        }
    }
}
