using System.Configuration;

namespace Pos.Domain.ValueObjects
{
    public static class GlobalVariables
    {
        private const string _RestartService = "net stop IMS_Fiscalization && net start IMS_Fiscalization";
        public static int POS_ID
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["POS"]); }
        }

        public static string FOLDER_PATH
        {
            get { return ConfigurationManager.AppSettings["FolderPath"]; }
        }

        public static string DATE
        {
            get { return DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> "; }
        }
    }
}
