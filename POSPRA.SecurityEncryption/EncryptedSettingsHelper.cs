using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.Json;


namespace POSPRA.SecurityEncryption
{
    public static class EncryptedSettingsHelper
    {
        private static Dictionary<string, string> _settings;

        static EncryptedSettingsHelper()
        {
            string encryptedBlob = ConfigurationManager.AppSettings["EncryptedSettings"];
            if (!string.IsNullOrEmpty(encryptedBlob))
            {
                string json = AesEncryptionHelper.Decrypt(encryptedBlob);
                _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            else
            {
                _settings = new Dictionary<string, string>();
            }
        }

        public static string Get(string key)
        {
            return _settings.ContainsKey(key) ? _settings[key] : null;
        }
    }
}
