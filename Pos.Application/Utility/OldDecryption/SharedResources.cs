using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Utility.OldDecryption
{
    public static class SharedResources
    {
        public static string[] SplitString(string data, char delimiter)
        {
            return data.Split(delimiter);
        }

        public static DataTable ToDataTable<T>(IList<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
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

        public class Keys
        {
            public Keys(string privateKey, string publicKey, string encryptedKey, string token)
            {
                Public = publicKey;
                Private = privateKey;
                Encryption = encryptedKey;
                Token = token;
            }

            public string Public { get; set; }
            public string Private { get; set; }
            public string Encryption { get; set; }

            public string Token { get; set; }
        }

        public class POSConfigurations
        {
            public POSConfigurations(string LogInterval, string RecordInterval, string IMSUpdateInterval, string HeartbeatInterval, string RecordSyncLimit, string LogSyncLimit, string GatewayURL, string Version, string FilePath, string FileSize, string Token)
            {
                this.LogInterval = LogInterval;
                this.RecordInterval = RecordInterval;
                this.IMSUpdateInterval = IMSUpdateInterval;
                this.HeartbeatInterval = HeartbeatInterval;
                this.RecordSyncLimit = RecordSyncLimit;
                this.LogSyncLimit = LogSyncLimit;
                this.Version = Version;
                this.GatewayURL = GatewayURL;
                this.FilePath = FilePath;
                this.FileSize = FileSize;
                this.Token = Token;
            }

            public string LogInterval { get; set; }
            public string RecordInterval { get; set; }
            public string IMSUpdateInterval { get; set; }
            public string HeartbeatInterval { get; set; }
            public string RecordSyncLimit { get; set; }
            public string LogSyncLimit { get; set; }
            public string Version { get; set; }
            public string GatewayURL { get; set; }
            public string FilePath { get; set; }
            public string FileSize { get; set; }
            public string Token { get; set; }
        }
    }
}
