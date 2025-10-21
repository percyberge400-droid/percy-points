using System.Diagnostics;
using System.ServiceProcess;
using Newtonsoft.Json;
using POSPRA.Domain.ValueObjects;

namespace POSPRA.Application.Utility
{
    public static class GlobalMethods
    {
        private static string IMS_Test_Url = "https://ims.fbr.gov.pk:443/SandBox/";
        private static string IMS_Live_Url = "https://ims.fbr.gov.pk/";

        //public static string InvoiceNumber(int POSID)
        //{
        //    string number = null;
        //    Random rand = new Random();



        //    if (GlobalVariables.IS_PRODUCTION == false)
        //        number = POSID.ToString() + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond.ToString().PadLeft(3, '0') + "*configTest*";
        //    else
        //        number = POSID.ToString() + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond.ToString().PadLeft(3, '0')+ "*config*";

        //    return number;
        //}

        /// <summary>
        /// TODO 
        /// </summary>
        /// <param name="POSID"></param>
        /// <returns></returns>
        public static string InvoiceNumber(long POSID)
        {
            string number = null;
            Random rand = new();
            string year = DateTime.Now.Year.ToString().Substring(2, 2);
            switch (DateTime.Now.Year.ToString().Substring(2, 2))
            {

                case "21":
                    year = "A";
                    break;

                case "22":
                    year = "B";
                    break;

                case "23":
                    year = "C";
                    break;

                case "24":
                    year = "D";
                    break;

                case "25":
                    year = "E";
                    break;

                case "26":
                    year = "F";
                    break;

                case "27":
                    year = "G";
                    break;

                case "28":
                    year = "H";
                    break;

                case "29":
                    year = "I";
                    break;

                case "30":
                    year = "J";
                    break;

                case "31":
                    year = "K";
                    break;

                case "32":
                    year = "L";
                    break;

                case "33":
                    year = "M";
                    break;

                case "34":
                    year = "N";
                    break;

                case "35":
                    year = "O";
                    break;

                case "36":
                    year = "P";
                    break;

                case "37":
                    year = "Q";
                    break;

                case "38":
                    year = "R";
                    break;

                case "39":
                    year = "S";
                    break;

                case "40":
                    year = "T";
                    break;

                case "41":
                    year = "U";
                    break;

                case "42":
                    year = "V";
                    break;

                case "43":
                    year = "W";
                    break;


                case "44":
                    year = "X";
                    break;

                case "45":
                    year = "Y";
                    break;

                case "46":
                    year = "Z";
                    break;

                default:
                    year = DateTime.Now.Year.ToString().Substring(2, 2);
                    break;
            }
            string month = "0";
            switch (DateTime.Now.Month)
            {

                case 1:
                    month = "A";
                    break;

                case 2:
                    month = "B";
                    break;

                case 3:
                    month = "C";
                    break;

                case 4:
                    month = "D";
                    break;

                case 5:
                    month = "E";
                    break;

                case 6:
                    month = "F";
                    break;

                case 7:
                    month = "G";
                    break;

                case 8:
                    month = "H";
                    break;

                case 9:
                    month = "I";
                    break;

                case 10:
                    month = "J";
                    break;

                case 11:
                    month = "K";
                    break;

                case 12:
                    month = "L";
                    break;

                default:
                    month = DateTime.Now.Month.ToString().PadLeft(2, '0');
                    break;
            }
            string day = "0";
            switch (DateTime.Now.Day)
            {

                case 1:
                    day = "1";
                    break;
                case 2:
                    day = "2";
                    break;
                case 3:
                    day = "3";
                    break;
                case 4:
                    day = "4";
                    break;
                case 5:
                    day = "5";
                    break;
                case 6:
                    day = "6";
                    break;

                case 7:
                    day = "A";
                    break;

                case 8:
                    day = "B";
                    break;

                case 9:
                    day = "C";
                    break;

                case 10:
                    day = "D";
                    break;

                case 11:
                    day = "E";
                    break;

                case 12:
                    day = "F";
                    break;

                case 13:
                    day = "G";
                    break;

                case 14:
                    day = "H";
                    break;

                case 15:
                    day = "I";
                    break;

                case 16:
                    day = "J";
                    break;

                case 17:
                    day = "K";
                    break;

                case 18:
                    day = "L";
                    break;

                case 19:
                    day = "M";
                    break;

                case 20:
                    day = "N";
                    break;

                case 21:
                    day = "O";
                    break;

                case 22:
                    day = "P";
                    break;

                case 23:
                    day = "Q";
                    break;

                case 24:
                    day = "R";
                    break;

                case 25:
                    day = "S";
                    break;

                case 26:
                    day = "T";
                    break;

                case 27:
                    day = "U";
                    break;

                case 28:
                    day = "V";
                    break;

                case 29:
                    day = "W";
                    break;


                case 30:
                    day = "X";
                    break;

                case 31:
                    day = "Y";
                    break;

                //case "46":
                //    day = "Z";
                //    break;

                default:
                    day = DateTime.Now.Day.ToString().PadLeft(2, '0');
                    break;
            }
            string hour = "0";
            switch (DateTime.Now.Hour)
            {

                case 1:
                    hour = "A";
                    break;

                case 2:
                    hour = "B";
                    break;

                case 3:
                    hour = "C";
                    break;

                case 4:
                    hour = "D";
                    break;

                case 5:
                    hour = "E";
                    break;

                case 6:
                    hour = "F";
                    break;

                case 7:
                    hour = "G";
                    break;

                case 8:
                    hour = "H";
                    break;

                case 9:
                    hour = "I";
                    break;

                case 10:
                    hour = "J";
                    break;

                case 11:
                    hour = "K";
                    break;

                case 12:
                    hour = "L";
                    break;

                case 13:
                    hour = "M";
                    break;

                case 14:
                    hour = "N";
                    break;

                case 15:
                    hour = "O";
                    break;

                case 16:
                    hour = "P";
                    break;

                case 17:
                    hour = "Q";
                    break;

                case 18:
                    hour = "R";
                    break;

                case 19:
                    hour = "S";
                    break;

                case 20:
                    hour = "T";
                    break;

                case 21:
                    hour = "U";
                    break;

                case 22:
                    hour = "V";
                    break;

                case 23:
                    hour = "W";
                    break;


                case 24:
                    hour = "X";
                    break;


                default:
                    hour = DateTime.Now.Hour.ToString();
                    break;
            }

            if (GlobalVariables.IS_PRODUCTION)
                number = POSID.ToString() + year + month + day + hour + DateTime.Now.Minute + DateTime.Now.Second + RandomKey.GenerateNumericKey(4);
            else
                number = POSID.ToString() + year + month + day + hour + DateTime.Now.Minute + DateTime.Now.Second + RandomKey.GenerateNumericKey(4) + "*test*";

            return number;
        }

        public static byte[] ToByteArray(String HexString)
        {
            int NumberChars = HexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
            }
            return bytes;
        }

        //private static object PostData(object Model, string Endpoint, int POSID, string MAC_Address, out bool IsUnAuthorize, bool ImsUrl = false)
        //{
        //    object obj = null;
        //    IsUnAuthorize = false;
        //    if (!GlobalVariables.IsMACAddressNull)
        //    {


        //        try
        //        {
        //            using (HttpClient Client = new HttpClient())
        //            {
        //                Client.BaseAddress = !ImsUrl
        //                    ? new Uri(GlobalVariables.GATEWAY_URL)
        //                    : (GlobalVariables.IS_PRODUCTION ? new Uri(IMS_Live_Url) : new Uri(IMS_Test_Url));
        //                Client.DefaultRequestHeaders.Authorization =
        //                    new AuthenticationHeaderValue("Bearer", GlobalVariables.TOKEN);
        //                Client.DefaultRequestHeaders.Add("POSID", POSID.ToString());
        //                Client.DefaultRequestHeaders.Add("MACADDRESS", MAC_Address.ToString());
        //                var a = JsonConvert.SerializeObject(Model);
        //                StringContent content = new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8,
        //                    "application/json");

        //                if (!GlobalVariables.IS_PRODUCTION)
        //                {
        //                    //Code line to bypass SSL
        //                    System.Net.ServicePointManager.ServerCertificateValidationCallback =
        //                        delegate { return true; };
        //                }



        //                var response = Client.PostAsync(Endpoint, content).Result;
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    obj = response.Content.ReadAsAsync<object>().Result;
        //                }
        //                else if (response.StatusCode == HttpStatusCode.Unauthorized)
        //                {
        //                    GlobalVariables.IsMACAddressNull = true;
        //                    IsUnAuthorize = true;

        //                }
        //                else if (response.StatusCode == HttpStatusCode.InternalServerError)
        //                {
        //                    if (ImsUrl == false)
        //                        obj = PostData(Model, Endpoint, POSID, MAC_Address, out IsUnAuthorize, true);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            var errorObj = new
        //            {
        //                ID = 0,
        //                Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
        //                Code = 404
        //            };
        //            obj = null;
        //            if (ImsUrl == false)
        //                obj = PostData(Model, Endpoint, POSID, MAC_Address, out IsUnAuthorize, true);
        //        }
        //    }

        //    return obj;
        //}

        //private static ConfigurationDTO GetData(string Endpoint, int POSID, string MAC_Address, out bool IsUnAuthorize, bool ImsUrl = false)
        //{
        //    IsUnAuthorize = false;
        //    ConfigurationDTO obj = null;
        //    if (!GlobalVariables.IsMACAddressNull)
        //    {
        //        try
        //        {

        //            using (HttpClient Client = new HttpClient())
        //            {

        //                //Client.BaseAddress = !ImsUrl ? new Uri(GlobalVariables.GATEWAY_URL) : new Uri("https://ims.fbr.gov.pk:443/SandBox/");
        //                //Client.BaseAddress = !ImsUrl ? new Uri(GlobalVariables.GATEWAY_URL) : new Uri("http://localhost:48697/");
        //                Client.BaseAddress = !ImsUrl
        //                    ? new Uri(GlobalVariables.GATEWAY_URL)
        //                    : (GlobalVariables.IS_PRODUCTION ? new Uri(IMS_Live_Url) : new Uri(IMS_Test_Url));
        //                Client.DefaultRequestHeaders.Authorization =
        //                    new AuthenticationHeaderValue("Bearer", GlobalVariables.TOKEN);
        //                Client.DefaultRequestHeaders.Add("POSID", POSID.ToString());
        //                Client.DefaultRequestHeaders.Add("MACADDRESS", MAC_Address.ToString());

        //                if (!GlobalVariables.IS_PRODUCTION)
        //                {
        //                    //Code line to bypass SSL
        //                    System.Net.ServicePointManager.ServerCertificateValidationCallback =
        //                        delegate { return true; };
        //                }

        //                var response = Client.GetAsync(Endpoint).Result;
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    obj = response.Content.ReadAsAsync<ConfigurationDTO>().Result;
        //                }
        //                else if (response.StatusCode == HttpStatusCode.Unauthorized)
        //                {
        //                    GlobalVariables.IsMACAddressNull = true;
        //                    IsUnAuthorize = true;

        //                }
        //                else if (response.StatusCode == HttpStatusCode.InternalServerError)
        //                {
        //                    if (ImsUrl == false)
        //                        obj = GetData(Endpoint, POSID, MAC_Address, out IsUnAuthorize, true);
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            obj = null;
        //            if (ImsUrl == false)
        //                obj = GetData(Endpoint, POSID, MAC_Address, out IsUnAuthorize, true);
        //        }
        //    }

        //    return obj;
        //}

        public static string ToHex(long number)
        {
            return number.ToString("X");
        }

        public static long ToDecimal(string hex)
        {
            return long.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        //public static StatusDTO PostData(object model, string endpoint, out bool IsUnAuthorize)
        //{
        //    var res = PostData(model, endpoint, GlobalVariables.POS_ID, GlobalVariables.LICENSE_KEY, out IsUnAuthorize);
        //    if (res != null)
        //    {
        //        var deserializedRes = JsonConvert.DeserializeObject<StatusDTO>(res.ToString());
        //        return deserializedRes;
        //    }

        //    return null;
        //}
        //public static List<StatusDTO> PostBulkData(object model, string endpoint, out bool IsUnAuthorize)
        //{
        //    //var st = model.ToString();
        //    var response = PostData(model, endpoint, GlobalVariables.POS_ID, GlobalVariables.LICENSE_KEY, out IsUnAuthorize);

        //    if (response != null)
        //    {
        //        return JsonConvert.DeserializeObject<List<StatusDTO>>(response.ToString());
        //    }

        //    return new List<StatusDTO>();
        //}
        //public static ConfigurationDTO GetData(string endpoint, out bool IsUnAuthorize)
        //{
        //    return GetData(endpoint, GlobalVariables.POS_ID, GlobalVariables.LICENSE_KEY, out IsUnAuthorize);
        //}

        public static void StartService(string serviceName)
        {
            try
            {
                //Kepp Updater Service Up and running
                ServiceController controller = new("ServiceUpdater");
                if (controller.Status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();
                }
            }
            catch
            {
            }
        }

        public static void ExecuteCommand(string command)
        {
            try
            {

                ProcessStartInfo procStartInfo = new("cmd", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process proc = new()
                {
                    StartInfo = procStartInfo
                };
                proc.Start();
            }
            catch
            {
            }
        }

        public static bool IsValidJson<T>(this string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<T>(strInput);
                    return true;
                }
                catch // not valid
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }


        public static void ClearReadOnlyFolder()
        {
            try
            {
                DirectoryInfo parentDirectoryInfo = new(@GlobalVariables.FOLDER_PATH);
                ClearReadOnly(parentDirectoryInfo);
            }
            catch (Exception) { }
        }
        //public static void ClearReadOnly(DirectoryInfo parentDirectory)
        //{
        //    if (parentDirectory != null)
        //    {
        //        parentDirectory.Attributes = FileAttributes.Normal;
        //        foreach (FileInfo fi in parentDirectory.GetFiles())
        //        {
        //            fi.Attributes = FileAttributes.Normal;
        //        }
        //        foreach (DirectoryInfo di in parentDirectory.GetDirectories())
        //        {
        //            ClearReadOnly(di);
        //        }
        //    }
        //}
        public static void ClearReadOnly(DirectoryInfo parentDirectory)
        {
            if (parentDirectory != null)
            {
                parentDirectory.Attributes = FileAttributes.Normal;
                var fii = parentDirectory.GetFiles(GlobalVariables.POS_ID.ToString() + ".ims", SearchOption.TopDirectoryOnly);
                foreach (FileInfo fi in fii)
                {
                    fi.Attributes = FileAttributes.Normal;
                }

                DeleteLogFile();

                //foreach (DirectoryInfo di in parentDirectory.GetDirectories())
                //{
                //    ClearReadOnly(di);
                //}
            }
        }

        public static void DeleteLogFile()
        {
            DirectoryInfo parentDirectory = new(@GlobalVariables.FOLDER_PATH);
            if (parentDirectory != null)
            {
                parentDirectory.Attributes = FileAttributes.Normal;

                var fii_log = parentDirectory.GetFiles(GlobalVariables.POS_ID.ToString() + "-log.ims", SearchOption.TopDirectoryOnly);
                foreach (FileInfo fi in fii_log)
                {
                    fi.Attributes = FileAttributes.Normal;
                    File.Delete(@GlobalVariables.FOLDER_PATH + GlobalVariables.POS_ID.ToString() + "-log.ims");

                }


            }
        }

    }
}
