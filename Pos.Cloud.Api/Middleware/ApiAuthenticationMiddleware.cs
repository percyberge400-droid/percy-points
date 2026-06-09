using Pos.Application.DTOs;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using Pos.Cloud.Api.Utility;
using System.Text.Json;

namespace Pos.Cloud.Api.Middleware;

public class ApiAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiAuthenticationMiddleware> _logger;

    // -----------------------------------------------
    // POSID from Query Params (GET endpoints)
    // -----------------------------------------------
    private static readonly HashSet<string> _queryParamRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ApiRoutes.IsServiceEnabled,
        ApiRoutes.IsLogEnabled,
        ApiRoutes.DisableLogBit,
        ApiRoutes.ProductCatalogueGetAll
    };

    // -----------------------------------------------
    // POSID from List<T> Body — first item
    // Environment always from query string for these
    // -----------------------------------------------
    private static readonly HashSet<string> _listBodyRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ApiRoutes.PostInvoice,
        ApiRoutes.CreateCloudLog
    };

    // -----------------------------------------------
    // POSID from Single Object Body
    // -----------------------------------------------
    private static readonly HashSet<string> _singleBodyRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ApiRoutes.ExportCsv,
        ApiRoutes.Authenticate,
        ApiRoutes.UpdateConfigFlag,
        ApiRoutes.IsCloudSyncEnabled,
        ApiRoutes.HeartBeat
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
        // Step 1: Skip ignored routes — no auth needed
        // -----------------------------------------------
        if (ApiRoutes.IgnoredRoutes.Contains(path))
        {
            _logger.LogInformation("Skipping auth for ignored route: {Path}", path);
            await _next(context);
            return;
        }

        // -----------------------------------------------
        // Step 2: Extract Bearer Token
        // -----------------------------------------------
        var bearerToken = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(bearerToken))
        {
            _logger.LogWarning("Missing bearer token: {Path}", path);
            await WriteUnauthorizedResponse(context);
            return;
        }

        // -----------------------------------------------
        // Step 3: Extract POSID + Environment
        // -----------------------------------------------
        string? posId = null;
        string? environment = null;

        if (_queryParamRoutes.Contains(path))
        {
            // GET endpoints — both posId and env from query string
            posId = context.Request.Query["posId"].FirstOrDefault();

            environment = context.Request.Query["env"].FirstOrDefault()
                       ?? context.Request.Query["environment"].FirstOrDefault();
        }
        else if (_listBodyRoutes.Contains(path))
        {
            context.Request.EnableBuffering();

            try
            {
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset for controller

                if (!string.IsNullOrEmpty(body))
                {
                    var list = JsonSerializer.Deserialize<List<JsonElement>>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    var first = list?.FirstOrDefault() ?? default;
                    posId = first.GetPropertyAsString("POSID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse list body: {Path}", path);
                await WriteUnauthorizedResponse(context);
                return;
            }

            // Environment always comes from query string for list endpoints
            environment = context.Request.Query["environment"].FirstOrDefault()
                       ?? context.Request.Query["env"].FirstOrDefault();
        }
        else if (_singleBodyRoutes.Contains(path))
        {
            context.Request.EnableBuffering();

            try
            {
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset for controller

                if (!string.IsNullOrEmpty(body))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dto = JsonSerializer.Deserialize<JsonElement>(body, options);

                    // Try all POSID key variations
                    posId = dto.GetPropertyAsString("POSID")
                         ?? dto.GetPropertyAsString("PosId")
                         ?? dto.GetPropertyAsString("posId");

                    // Try all environment key variations
                    environment = dto.GetPropertyAsString("Environment")
                               ?? dto.GetPropertyAsString("environment");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse single body: {Path}", path);
                await WriteUnauthorizedResponse(context);
                return;
            }

            // Fallback: environment from query string
            if (string.IsNullOrEmpty(environment))
            {
                environment = context.Request.Query["environment"].FirstOrDefault()
                           ?? context.Request.Query["env"].FirstOrDefault();
            }
        }

        // -----------------------------------------------
        // Step 4: Validate POSID extracted successfully
        // -----------------------------------------------
        if (string.IsNullOrEmpty(posId))
        {
            _logger.LogWarning("POSID could not be extracted: {Path}", path);
            await WriteUnauthorizedResponse(context);
            return;
        }

        // -----------------------------------------------
        // Step 5: Parse POSID string → long
        // -----------------------------------------------
        if (!long.TryParse(posId, out long posIdLong))
        {
            _logger.LogWarning("Invalid POSID format: {PosId}, Path: {Path}", posId, path);
            await WriteUnauthorizedResponse(context);
            return;
        }

        // -----------------------------------------------
        // Step 6: Validate Token Against DB
        // -----------------------------------------------
        try
        {
            using var scope = context.RequestServices.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPosClientRepository>();

            var posClient = await repo.GetByPosId(posIdLong, environment);

            if (posClient == null)
            {
                _logger.LogWarning("PosClient not found — POSID: {PosId}, Env: {Env}", posIdLong, environment);
                await WriteUnauthorizedResponse(context);
                return;
            }

            if (posClient.Token != bearerToken.Replace("Bearer ", "").Trim())
            {
                _logger.LogWarning("Token mismatch — POSID: {PosId}", posIdLong);
                await WriteUnauthorizedResponse(context);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auth — POSID: {PosId}", posIdLong);
            await WriteUnauthorizedResponse(context);
            return;
        }

        // -----------------------------------------------
        // Step 7: Authenticated — continue pipeline
        // -----------------------------------------------
        _logger.LogInformation("Auth success — POSID: {PosId}, Env: {Env}, Path: {Path}", posIdLong, environment, path);
        await _next(context);
    }

    // -----------------------------------------------
    // Unauthorized Response
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
// Extension: Safely read JsonElement as string
// Handles both Number (128762) and String ("128762")
// -----------------------------------------------
public static class JsonElementExtensions
{
    public static string? GetPropertyAsString(this JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return null;

        return prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetString(), 
            JsonValueKind.Number => prop.GetRawText(),
            _ => null
        };
    }
}