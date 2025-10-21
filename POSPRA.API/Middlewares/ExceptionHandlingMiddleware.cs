using Newtonsoft.Json;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using System.Net;

namespace POSPRA.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");

                // ✅ Create scope manually
                using var scope = _scopeFactory.CreateScope();

                var unitOfWork = scope.ServiceProvider.GetRequiredService<ISqlServerUnitOfWork>();
                var logRepo = scope.ServiceProvider.GetRequiredService<ILogSQLServerRepository>();

                var log = new Logs
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    //CreatedAt = DateTime.UtcNow
                };

                await logRepo.AddAsync(log);
                await unitOfWork.SaveChangesAsync();

                // ✅ Return clean API response
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<object>(
                    statusCode: "500",
                    message: "An unexpected error occurred. Please contact support.",
                    data: null,
                    errors: ex.Message
                );

                var result = JsonConvert.SerializeObject(response);
                await context.Response.WriteAsync(result);
            }
        }
    }
}
