namespace POSPRA.Application.Utility
{
    public class Status
    {
        public const string RECEIVED = "Received";
        public const string SYNCED = "Synced";

        public const string CONNECTED = "Connected";
        public const string DISCONNECTED = "DisConnected";
    }

    public class Endpoints
    {
        public const string POSStatus = "api/POS/Status";
        public const string POSInvoice = "api/POS/Invoice";
        public const string POSBulkInvoice = "api/POS/BulkInvoice";
        public const string Configuration = "api/POS/Configuration";
        public const string HeartBeat = "api/POS/HeartBeat";
        public const string DownloadFile = "api/POS/DownloadFile";
    }

    public class Messages
    {
        public const string COMPONENT_START = "IMS Component has been started";
        public const string COMPONENT_STOPPED_USER = "IMS Component has been stopped by user";
        public const string COMPONENT_STOPPED_SHUTDOWN = "IMS Component has been stopped";
        public const string INVALID_MODEL = "Invoice Model not valid";
        public const string ERROR_API_RESPONSE = "Either Internet is not connected or api is not responding";
        public const string ERROR_API_UNAUTHORIZED = "API Returned 401";
        public static string INVOICE_NOT_AVAILABLE = "Invoice Number Not available {0}";
        public static string SUCCESS = "Invoice Number {0} generated successfully {1}";
        public static string BEFORE_EXTENSION = "Data prior to {0} is no more acceptable";
        public static string VERSION = "Current Version running on clients system";
        public static string INVALID_POS = "POS ID entered is invalid.";
        public static string HEARTBEAT_UPDATED = "Heartbeat updated successfully.";
    }

    public static class ResponseMessages
    {
        public const string ConfigurationsFound = "Success...Configuration(s) found.";
        public const string ConfigurationsNotFound = "No configuration(s) found.";
        public const string ConfigurationsFetchError = "Error occurred while fetching configurations: ";
        public const string HeartbeatUpdated = "Heartbeat updated";
        public const string ErrorUpdatingHeartbeat = "Error updating heartbeat";
    }

    public enum ApiStatusCode
    {
        Success = 200,
        Error = 500
    }

    public static class StoredProcedures
    {
        public const string sp_UpdatePOSHeartbeat = "sp_UpdatePOSHeartbeat";
        public const string sp_GetConfigurations = "sp_GetConfigurations";
        public const string sp_InsertInvoice = "sp_InsertInvoice";
        public const string sp_UpdateStatus = "sp_UpdateStatus";
        // Add more stored procedures as needed
    }
}
