namespace POSPRA.DTOs.LogDTOs
{
    public class SyncLogDto
    {
        public long Id { get; set; }
        public long POSID { get; set; }
        public string? Message { get; set; }
        public string Type { get; set; }
        public bool IsSynced { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAtPk { get; set; } = DateTime.Now;
        // ---------- User / Security ----------
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public string? SessionId { get; set; }

        // ---------- Request Context ----------
        public string? HttpMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? QueryString { get; set; }
        public string? RequestHeaders { get; set; }
        public string? RequestBody { get; set; }

        // ---------- Response Context ----------
        public int? ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }

        // ---------- Client / Network ----------
        public string? ClientIp { get; set; }
        public string? ClientHost { get; set; }
        public string? UserAgent { get; set; }

        // ---------- Server / Environment ----------
        public string? MachineName { get; set; }
        public string? ApplicationName { get; set; }
        public string? EnvironmentName { get; set; }
        public string? AssemblyVersion { get; set; }

        // ---------- Exception Details ----------
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }

        // ---------- Custom / Domain ----------
        public string? Module { get; set; }
        public string? ActionName { get; set; }
        public string? AdditionalData { get; set; }

        // ---------- Worker Service ----------
        /// <summary>
        /// Logical name of the background/worker service (e.g. "POSPRA.Worker").
        /// </summary>
        public string? WorkerName { get; set; }

        /// <summary>
        /// Unique identifier for this worker instance (GUID or hostname+pid).
        /// </summary>
        public string? WorkerInstanceId { get; set; }

        /// <summary>
        /// Lifecycle event: Started, Stopping, Stopped, Restarted, Crashed, etc.
        /// </summary>
        public string? WorkerEvent { get; set; }

        /// <summary>UTC time when the worker started.</summary>
        public DateTime? WorkerStartedAtUtc { get; set; }

        /// <summary>UTC time when the worker stopped or crashed.</summary>
        public DateTime? WorkerStoppedAtUtc { get; set; }

        /// <summary>Total uptime (seconds) when stop/crash was logged.</summary>
        public double? WorkerUptimeSeconds { get; set; }

        /// <summary>Executable/build version of the worker host.</summary>
        public string? WorkerHostVersion { get; set; }
    }
}