using System.Configuration;

namespace POSPRA.Domain.ValueObjects
{
    public static class GlobalVariables
    {
        private const string _RestartService = "net stop IMS_Fiscalization && net start IMS_Fiscalization";
        public static int POS_ID
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["POS"]); }
        }

        public static bool IS_PRODUCTION
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["isProduction"]); }
        }

        public static string FOLDER_PATH
        {
            get { return ConfigurationManager.AppSettings["FolderPath"]; }
        }

        public static string FILE_NAME
        {
            get { return ConfigurationManager.AppSettings["FolderPath"] + ConfigurationManager.AppSettings["POS"] + ".ims"; }
        }

        public static string INSTALLATION_DIRECTORY
        {
            get { return @"C:\\Pral\\FiscalSoftware\\"; }
        }

        public static string INSTALLATION_DIRECTORY_RELEASE
        {
            get { return @"C:\\Pral\\FiscalSoftware\\Release\\"; }
        }

        public static string DATE
        {
            get { return DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> "; }
        }

        public static string GATEWAY_URL
        {
            get { return ConfigurationManager.AppSettings["GatewayURL"]; }
        }

        public static string LOCAL_URL
        {
            get { return ConfigurationManager.AppSettings["LocalURL"]; }
        }

        public static int RECORD_SYNC_LIMIT
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["RecordSyncLimit"]); }
        }

        public static int LOG_SYNC_LIMIT
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["LogSyncLimit"]); }
        }

        public static int TIME_RECORD
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["RecordInterval"].ToString()); }
        }

        public static int TIME_LOG
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["LogInterval"].ToString()); }
        }

        public static int TIME_HEARTBEAT
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["HeartbeatInterval"].ToString()); }
        }

        public static int TIME_IMS_UPDATE
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["IMSUpdateInterval"].ToString()); }
        }

        public static string ENCRYPTION_KEY
        {
            get { return ConfigurationManager.AppSettings["EC"].ToString(); }
        }

        public static string PRIVATE_KEY
        {
            get { return ConfigurationManager.AppSettings["PV"].ToString(); }
        }

        public static string LICENSE_KEY
        {
            get { return ConfigurationManager.AppSettings["Key"].ToString(); }
        }

        public static string TOKEN
        {
            get { return ConfigurationManager.AppSettings["Token"].ToString(); }
        }

        public static string VERSION
        {
            get { return ConfigurationManager.AppSettings["Version"].ToString(); }
        }

        public static int BACKUP_SIZE
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["BackUpSize"]); }
        }

        public static string IMS_SERVICE_PATH
        {
            get { return ConfigurationManager.AppSettings["IMSServicePath"].ToString(); }
        }

        public static string RESTART_SERVICE
        {
            get { return _RestartService; }
        }

        public static bool IsMACAddressNull { get; set; } = false;
    }
}
