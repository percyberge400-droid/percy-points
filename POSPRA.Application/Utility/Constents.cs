namespace POSPRA.Application.Utility
{
    public class Endpoints
    {
        public const string POSStatus = "api/POS/Status";
        public const string Configuration = "api/POS/Configuration";
        public const string HeartBeat = "api/POS/HeartBeat";

        // Fiscal API
        public const string GetAllUnsyncedAsync = "api/fiscal/getallunsynced";
        public const string GetAll = "api/fiscal/getall";
        public const string Create = "api/fiscal/create";

        // Live API
        public const string DecryptSave = "api/live/decrypt-save";
        public const string ExportCSV = "api/live/export-csv";
        public const string Authenticate = "api/Live/authenticate-by-mac";
    }

    public static class ResponseMessages
    {
        public const string ConfigurationsFound = "Success...Configuration(s) found.";
        public const string ConfigurationsNotFound = "No configuration(s) found.";
        public const string ConfigurationsFetchError = "Error occurred while fetching configurations: ";
        public const string HeartbeatUpdated = "Heartbeat updated";
        public const string ErrorUpdatingHeartbeat = "Error updating heartbeat";

        // ===== Generic Success =====
        public const string OperationSuccess = "Operation completed successfully.";
        public const string RecordSaved = "Record saved successfully.";
        public const string RecordUpdated = "Record updated successfully.";
        public const string RecordDeleted = "Record deleted successfully.";
        public const string DataSynchronized = "Data synchronized successfully.";
        public const string BackupCompleted = "Backup completed successfully.";
        public const string RecordFound = "Record Found.";

        // ===== Generic Error =====
        public const string UnknownError = "An unexpected error occurred. Please try again.";
        public const string DatabaseError = "Database error occurred while processing your request.";
        public const string DataNotFound = "Requested data not found.";
        public const string DuplicateRecord = "Duplicate record detected.";
        public const string InvalidInput = "Invalid input provided.";
        public const string UnauthorizedAccess = "You are not authorized to perform this action.";
        public const string SessionExpired = "Session has expired. Please log in again.";
    }

    public static class AlertType
    {
        // Generic/common alerts
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Error = "Error";
        public const string Success = "Success";
        public const string Update = "Update";
        public const string Critical = "Critical";
        public const string Exception = "Exception";
        public const string Startup = "Startup";
        public const string Shutdown = "Shutdown";
        public const string ConfigurationChange = "ConfigurationChange";
        public const string VersionUpdate = "VersionUpdate";
        public const string DatabaseEvent = "DatabaseEvent";
        public const string SecurityEvent = "SecurityEvent";

        // Specific alerts from your original enum
        public const string ComponentStarted = "Started";
        public const string ComponentStopped = "Stopped";
        public const string Version = "Version";
        public const string DatabaseInfo = "Database Information";
        public const string Configuration = "Configuration Information";
        public const string InvalidInvoiceModel = "Invalid model received";
    }

    public static class AlertMessages
    {
        public const string Forminitialized = "Form initialized";
        public const string Error = "Error";

    }

    public static class ApiStatusCode
    {
        public const string Success = "200";
        public const string Error = "500";
        public const string NotFound = "404";
        public const string Unauthorized = "401";
    }

    public enum InvoiceStatus
    {
        NotSynced = 0,
        Synced = 1,
        Faulty = 2
    }

    public static class RequestValidationDefaults
    {
        // AppSettings
        public const string ConfigSection = "ValidationMiddleware";

        // Header names
        public const string HeaderAuthorization = "Authorization";
        public const string HeaderMacAddress = "MAC-ADDRESS";
        public const string HeaderPosId = "POS-ID";
        public const string ApplicationType = "application/json";

        // Text tokens
        public const string BearerPrefix = "Bearer ";

        // Error messages
        public const string MissingHeadersMessage =
            "Missing token, POSID or MAC address.";
    }
}