namespace POSPRA.Domain.Entities
{
    /// <summary>
    /// Master application log for PRAL/FBR systems (Pakistan).
    /// Stores detailed request, response, environment and error data.
    /// </summary>
    public class Logs
    {
        // ---------- Primary ----------
        public int Id { get; set; }

        /// <summary>Main descriptive message or error text.</summary>
        public string? Message { get; set; }

        /// <summary>Severity/category: 1=Info, 2=Warning, 3=Error, 4=Audit, etc.</summary>
        public int TypeId { get; set; }

        /// <summary>True if this log was synced to the central server.</summary>
        public bool IsSynced { get; set; }

        // ---------- Date/Time ----------
        /// <summary>UTC timestamp (always recorded).</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Pakistan Standard Time (Asia/Karachi) for direct reporting.</summary>
        public DateTime CreatedAtPk { get; set; } =
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi"));

        // ---------- User / Security ----------
        public string? UserId { get; set; }          // Application user id
        public string? UserName { get; set; }        // Friendly user name
        public string? UserRole { get; set; }        // Role(s)
        public string? SessionId { get; set; }       // Web session or token id

        // ---------- Request Context ----------
        public string? HttpMethod { get; set; }      // GET, POST, PUT, etc.
        public string? RequestPath { get; set; }     // /api/Invoice/Save
        public string? QueryString { get; set; }
        public string? RequestHeaders { get; set; }  // Serialized JSON or key=value
        public string? RequestBody { get; set; }

        // ---------- Response Context ----------
        public int? ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }

        // ---------- Client / Network ----------
        public string? ClientIp { get; set; }
        public string? ClientHost { get; set; }
        public string? UserAgent { get; set; }       // Browser / device

        // ---------- Server / Environment ----------
        public string? MachineName { get; set; }     // Server machine name
        public string? ApplicationName { get; set; } // e.g. POSPRA
        public string? EnvironmentName { get; set; } // Development/Staging/Production
        public string? AssemblyVersion { get; set; } // App build version

        // ---------- Exception Details ----------
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }

        // ---------- Custom / Domain ----------
        public string? Module { get; set; }          // e.g. “MoneyExchange”
        public string? ActionName { get; set; }      // Controller/Method
        public string? AdditionalData { get; set; }  // JSON blob for anything else

        // ---------- Constructors ----------
        public Logs() { }

        public Logs(string message, int typeId, bool isSynced)
        {
            Message = message;
            TypeId = typeId;
            IsSynced = isSynced;
            CreatedAtUtc = DateTime.UtcNow;
            CreatedAtPk = TimeZoneInfo.ConvertTimeFromUtc(
                               DateTime.UtcNow,
                               TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi"));
        }
    }
}
