namespace POSPRA.DTOs.LogDtos
{
    public class WorkerLogDto
    {
        // ---------- Worker Service ----------
        /// <summary>
        /// Logical name of the background/worker service (e.g. "POSPRA.Worker").
        /// </summary>
        public string? WorkerName { get; set; }
        public string? Message { get; set; }

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
        public string? Type { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string? StackTrace { get; set; }
    }
}
