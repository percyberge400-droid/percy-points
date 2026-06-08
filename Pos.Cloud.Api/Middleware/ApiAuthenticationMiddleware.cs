using Pos.Application.DTOs;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using System.Text.Json;

namespace Pos.Cloud.Api.Middleware;

public class ApiAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiAuthenticationMiddleware> _logger;

    // -----------------------------------------------
    // Endpoints where POSID comes from Query Params
    // -----------------------------------------------
    private static readonly HashSet<string> _queryParamRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/live/get-isservice-enable",
        "/api/live/get-islog-enable",
        "/api/live/disbale-log-bit"
    };

    // -----------------------------------------------
    // Endpoints where POSID comes from Request Body
    // -----------------------------------------------
    private static readonly HashSet<string> _bodyRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/live/decrypt-save",
        "/api/live/export-csv",
        "/api/live/authenticate-by-mac",
        "/api/live/create-cloud-log"
    };

    public ApiAuthenticationMiddleware(RequestDelegate next, ILogger<ApiAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // -----------------------------------------------
        // Step 1: Extract Bearer Token
        // -----------------------------------------------
        var bearerToken = context.Request.Headers.Authorization.ToString();

        var userName = context.Request.Headers["Username"].ToString();
        var password = context.Request.Headers["Password"].ToString();

        if (string.IsNullOrEmpty(bearerToken))
        {
            _logger.LogWarning("Missing bearer token: {Path}", path);
            await WriteUnauthorizedResponse(context);
            return;
        }

        // -----------------------------------------------
        // Step 2: Extract POSID based on route type
        // -----------------------------------------------
        string? posId = null;
        string? environment = null;

        if (_queryParamRoutes.Contains(path))
        {
            // GET endpoints — posId and env from query string
            // /api/live/get-isservice-enable?posId=123&env=live
            // /api/live/get-islog-enable?posId=123&env=live
            // /api/live/disbale-log-bit?posId=123&env=live
            posId = context.Request.Query["posId"].ToString();
            environment = context.Request.Query["env"].ToString();
        }
        else if (_bodyRoutes.Contains(path))
        {
            // POST endpoints — posId from body
            context.Request.EnableBuffering();

            try
            {
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset for controller

                if (!string.IsNullOrEmpty(body))
                {
                    (posId, environment) = ExtractPosIdFromBody(path, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse request body at: {Path}", path);
                await WriteUnauthorizedResponse(context);
                return;
            }

            // environment from query string for POST endpoints
            if (string.IsNullOrEmpty(environment))
                environment = context.Request.Query["environment"].ToString();
        }

        if (string.IsNullOrEmpty(posId))
        {
            _logger.LogWarning("POSID could not be extracted from request: {Path}", path);
            await WriteUnauthorizedResponse(context);
            return;
        }

        // -----------------------------------------------
        // Step 3: Validate Token Against DB
        // -----------------------------------------------
        try
        {
            if (!long.TryParse(posId, out long posIdLong))
            {
                _logger.LogWarning("Invalid POSID format: {PosId}, Path: {Path}", posId, path);
                await WriteUnauthorizedResponse(context);
                return;
            }

            using var scope = context.RequestServices.CreateScope();
            var posClientRepository = scope.ServiceProvider.GetRequiredService<IPosClientRepository>();

            var posClient = await posClientRepository.GetByPosId(posIdLong, environment);

            if (posClient == null || posClient.Token != bearerToken.Replace("Bearer ", "").Trim())
            {
                _logger.LogWarning("Token mismatch for POSID: {PosId}, Path: {Path}", posId, path);
                await WriteUnauthorizedResponse(context);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation for POSID: {PosId}", posId);
            await WriteUnauthorizedResponse(context);
            return;
        }

        // -----------------------------------------------
        // Step 4: Authenticated — continue pipeline
        // -----------------------------------------------
        _logger.LogInformation("Auth success — POSID: {PosId}, Path: {Path}", posId, path);
        await _next(context);
    }

    // -----------------------------------------------
    // Extract POSID from different DTO body shapes
    // -----------------------------------------------
    private static (string? posId, string? environment) ExtractPosIdFromBody(string path, string body)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // POST /api/live/decrypt-save       → List<FileRecordDto>       → POSID
        // POST /api/live/create-cloud-log   → List<SyncLogDto>          → POSID
        if (path.Contains("decrypt-save", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("create-cloud-log", StringComparison.OrdinalIgnoreCase))
        {
            var dtos = JsonSerializer.Deserialize<List<JsonElement>>(body, options);
            var posId = dtos?.FirstOrDefault().GetPropertyOrDefault("POSID");
            return (posId, null);
        }

        // POST /api/live/export-csv         → InvoiceFilterDto          → POSID
        // POST /api/live/authenticate-by-mac → ClientValidationDto      → POSID
        if (path.Contains("export-csv", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("authenticate-by-mac", StringComparison.OrdinalIgnoreCase))
        {
            var dto = JsonSerializer.Deserialize<JsonElement>(body, options);
            var posId = dto.GetPropertyOrDefault("POSID");
            return (posId, null);
        }

        return (null, null);
    }

    // -----------------------------------------------
    // Unified Unauthorized Response
    // -----------------------------------------------
    private static async Task WriteUnauthorizedResponse(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>(
            ApiStatusCode.Error.ToString(),
            ResponseMessages.InvalidBearerToken,
            null
        );

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

// -----------------------------------------------
// Extension: Safely get property from JsonElement
// -----------------------------------------------
public static class JsonElementExtensions
{
    public static string? GetPropertyOrDefault(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }
}