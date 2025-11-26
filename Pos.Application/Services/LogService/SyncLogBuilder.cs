using Pos.Application.DTOs.LogDTOs;

namespace Pos.Application.Services.LogService
{
    public static class SyncLogBuilder
    {
        // -------------------------------------------------------------------
        // USER / SECURITY
        // -------------------------------------------------------------------
        public static SyncLogDto WithUserInfo(
            this SyncLogDto log,
            string? userId,
            string? userName,
            string? userRole,
            string? sessionId)
        {
            log.UserId = userId;
            log.UserName = userName;
            log.UserRole = userRole;
            log.SessionId = sessionId;
            return log;
        }

        // -------------------------------------------------------------------
        // REQUEST CONTEXT
        // -------------------------------------------------------------------
        public static SyncLogDto WithRequestInfo(
            this SyncLogDto log,
            string? httpMethod,
            string? path,
            string? query,
            string? headers,
            string? body)
        {
            log.HttpMethod = httpMethod;
            log.RequestPath = path;
            log.QueryString = query;
            log.RequestHeaders = headers;
            log.RequestBody = body;
            return log;
        }

        // -------------------------------------------------------------------
        // RESPONSE CONTEXT
        // -------------------------------------------------------------------
        public static SyncLogDto WithResponseInfo(
            this SyncLogDto log,
            int? statusCode,
            string? responseBody)
        {
            log.ResponseStatusCode = statusCode;
            log.ResponseBody = responseBody;
            return log;
        }

        // -------------------------------------------------------------------
        // CLIENT / NETWORK
        // -------------------------------------------------------------------
        public static SyncLogDto WithClientInfo(
            this SyncLogDto log,
            string? ip,
            string? host,
            string? agent)
        {
            log.ClientIp = ip;
            log.ClientHost = host;
            log.UserAgent = agent;
            return log;
        }

        // -------------------------------------------------------------------
        // SERVER / ENVIRONMENT
        // -------------------------------------------------------------------
        public static SyncLogDto WithEnvironmentInfo(
            this SyncLogDto log,
            string? machineName,
            string? appName,
            string? environmentName,
            string? assemblyVersion)
        {
            log.MachineName = machineName;
            log.ApplicationName = appName;
            log.EnvironmentName = environmentName;
            log.AssemblyVersion = assemblyVersion;
            return log;
        }

        // -------------------------------------------------------------------
        // EXCEPTION DETAILS
        // -------------------------------------------------------------------
        public static SyncLogDto WithExceptionInfo(
            this SyncLogDto log,
            Exception? ex)
        {
            if (ex == null)
                return log;

            log.ExceptionType = ex.GetType()?.FullName?.Length>50 ? ex?.GetType()?.FullName?.Substring(0,50) : ex.GetType().FullName;
            log.ExceptionMessage = ex?.Message;
            log.StackTrace = ex?.StackTrace;
            log.InnerException = ex?.InnerException?.ToString();

            return log;
        }

        // -------------------------------------------------------------------
        // CUSTOM / DOMAIN
        // -------------------------------------------------------------------
        public static SyncLogDto WithDomainInfo(
            this SyncLogDto log,
            string? module,
            string? action,
            string? extraData)
        {
            log.Module = module;
            log.ActionName = action;
            log.AdditionalData = extraData;
            return log;
        }

        // -------------------------------------------------------------------
        // WORKER SERVICE
        // -------------------------------------------------------------------
        public static SyncLogDto WithWorkerInfo(
            this SyncLogDto log,
            string? workerName,
            string? instanceId,
            string? workerEvent,
            DateTime? startedAt,
            DateTime? stoppedAt,
            double? uptimeSeconds,
            string? workerHostVersion)
        {
            log.WorkerName = workerName;
            log.WorkerInstanceId = instanceId;
            log.WorkerEvent = workerEvent;
            log.WorkerStartedAtUtc = startedAt;
            log.WorkerStoppedAtUtc = stoppedAt;
            log.WorkerUptimeSeconds = uptimeSeconds;
            log.WorkerHostVersion = workerHostVersion;

            return log;
        }

        // -------------------------------------------------------------------
        // FINAL BUILDER METHOD
        // -------------------------------------------------------------------
        public static SyncLogDto Build(
            string type,
            string? message,
            long posId)
        {
            return new SyncLogDto
            {
                POSID = posId,
                Type = type,
                Message = message,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedAtPk = DateTime.Now
            };
        }
    }

}
