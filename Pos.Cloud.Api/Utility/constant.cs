namespace Pos.Cloud.Api.Utility
{
    public static class ApiRoutes
    {
        // -----------------------------------------------
        // Live Routes
        // -----------------------------------------------
        public const string PostInvoice = "/api/live/post-invoice";
        public const string ExportCsv = "/api/live/export-csv";
        public const string Authenticate = "/api/live/authenticate-by-mac";
        public const string UpdateConfigFlag = "/api/live/update-configuration-flag";
        public const string CreateCloudLog = "/api/live/create-cloud-log";
        public const string IsServiceEnabled = "/api/live/get-isservice-enable";
        public const string IsLogEnabled = "/api/live/get-islog-enable";
        public const string DisableLogBit = "/api/live/disbale-log-bit";

        // -----------------------------------------------
        // Configuration Routes (authenticated)
        // -----------------------------------------------
        public const string IsCloudSyncEnabled = "/api/configuration/iscloud-syncenabled";

        // POS Routes (authenticated)
        // -----------------------------------------------
        public const string HeartBeat = "/api/pos/heartbeat";

        // -----------------------------------------------
        // Product Catalogue Routes (authenticated)
        // -----------------------------------------------
        public const string ProductCatalogueGetAll = "/api/productcatalogue/getall";

        // -----------------------------------------------
        // Ignored Routes — No Auth Required
        // -----------------------------------------------
        public static readonly HashSet<string> IgnoredRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/configuration/get-update-version",
        "/api/configuration/get-updater-file",
        "/api/reference/get-all-payment-methods",
        "/api/reference/get-all-invoice-types",
        "/api/reference/get-all-services-rendered"
    };
    }
}
