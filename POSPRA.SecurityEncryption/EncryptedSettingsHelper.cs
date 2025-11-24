using System.Configuration;
using System.Text.Json;

namespace POSPRA.SecurityEncryption
{
    public static class EncryptedSettingsHelper
    {
        private static Dictionary<string, string> _settings;
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        static EncryptedSettingsHelper()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // Decrypt multiple appsettings files
                DecryptAppSettingsFile("appsettings.json");
                DecryptAppSettingsFile("appsettings.worker.json");

                _initialized = true;
            }
        }

        private static void DecryptAppSettingsFile(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    using JsonDocument doc = JsonDocument.Parse(jsonContent);

                    string encryptedBlob = FindEncryptedBlob(doc.RootElement);
                    if (!string.IsNullOrEmpty(encryptedBlob))
                    {
                        string decryptedJson = AesEncryptionHelper.Decrypt(encryptedBlob);
                        var dict = ConvertJsonToDictionary(decryptedJson);

                        if (dict != null)
                        {
                            foreach (var kvp in dict)
                            {
                                _settings[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to decrypt {fileName}: {ex.Message}");
                }
            }
        }

        // Public method to decrypt specific JSON files and return dictionary
        public static Dictionary<string, string> DecryptSettingsFiles(params string[] fileNames)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileName in fileNames)
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (File.Exists(filePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(filePath);
                        using JsonDocument doc = JsonDocument.Parse(jsonContent);

                        string encryptedBlob = FindEncryptedBlob(doc.RootElement);
                        if (!string.IsNullOrEmpty(encryptedBlob))
                        {
                            string decryptedJson = AesEncryptionHelper.Decrypt(encryptedBlob);
                            var dict = ConvertJsonToDictionary(decryptedJson);

                            foreach (var kvp in dict)
                            {
                                result[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to decrypt {fileName}: {ex.Message}");
                    }
                }
            }

            return result;
        }

        // Public method to decrypt a single JSON file
        public static Dictionary<string, string> DecryptSettingsFile(string fileName)
        {
            return DecryptSettingsFiles(fileName);
        }

        // NEW: Public method to decrypt App.config file
        public static Dictionary<string, string> DecryptAppConfigFile()
        {
            var decryptedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Check if there's an EncryptedSettings blob in app.config
                var encryptedBlob = ConfigurationManager.AppSettings["EncryptedSettings"];

                if (!string.IsNullOrEmpty(encryptedBlob))
                {
                    System.Diagnostics.Debug.WriteLine("Found EncryptedSettings blob in app.config, decrypting...");

                    // Decrypt the blob to get JSON
                    string decryptedJson = AesEncryptionHelper.Decrypt(encryptedBlob);

                    // Parse the JSON to get key-value pairs
                    var settingsDict = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson);

                    if (settingsDict != null)
                    {
                        foreach (var kvp in settingsDict)
                        {
                            // Add with AppSettings: prefix for configuration consistency
                            var configKey = kvp.Key.StartsWith("AppSettings:") ? kvp.Key : $"AppSettings:{kvp.Key}";
                            decryptedValues[configKey] = kvp.Value;
                        }

                        System.Diagnostics.Debug.WriteLine($"Successfully decrypted {decryptedValues.Count} settings from app.config blob");
                        return decryptedValues;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No EncryptedSettings blob found, trying individual key decryption...");

                    // Fallback: Try individual key decryption (for backward compatibility)
                    var allKeys = ConfigurationManager.AppSettings.AllKeys;

                    foreach (var key in allKeys)
                    {
                        var value = ConfigurationManager.AppSettings[key];

                        if (!string.IsNullOrEmpty(value))
                        {
                            // Try to decrypt if it looks encrypted, otherwise use as-is
                            string finalValue = IsLikelyEncrypted(value) ? DecryptSingleValue(value) : value;

                            var configKey = key.StartsWith("AppSettings:") ? key : $"AppSettings:{key}";
                            decryptedValues[configKey] = finalValue;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Processed {decryptedValues.Count} individual values from app.config");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error decrypting app.config: {ex.Message}");

                // Fallback to original values
                FallbackToOriginalAppConfig(decryptedValues);
            }

            return decryptedValues;
        }

        private static string DecryptSingleValue(string encryptedValue)
        {
            try
            {
                var decrypted = AesEncryptionHelper.Decrypt(encryptedValue);
                System.Diagnostics.Debug.WriteLine($"Successfully decrypted value: {encryptedValue.Substring(0, Math.Min(10, encryptedValue.Length))}...");
                return decrypted;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to decrypt value, using original: {ex.Message}");
                return encryptedValue;
            }
        }

        private static bool IsLikelyEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 20)
                return false;

            // Check if it's a valid base64 string (encrypted values usually are)
            if (value.Length % 4 == 0 && System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9\+/]*={0,3}$"))
            {
                try
                {
                    var bytes = Convert.FromBase64String(value);
                    // Encrypted values are typically longer than plain text
                    return bytes.Length > 16;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private static void FallbackToOriginalAppConfig(Dictionary<string, string> decryptedValues)
        {
            try
            {
                var allKeys = ConfigurationManager.AppSettings.AllKeys;
                foreach (var key in allKeys)
                {
                    if (key == "EncryptedSettings") continue; // Skip the encrypted blob itself

                    var value = ConfigurationManager.AppSettings[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        var configKey = key.StartsWith("AppSettings:") ? key : $"AppSettings:{key}";
                        decryptedValues[configKey] = value;
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Fallback: Using {decryptedValues.Count} original values from app.config");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in fallback: {ex.Message}");
            }
        }

        private static Dictionary<string, string> ConvertJsonToDictionary(string json)
        {
            var result = new Dictionary<string, string>();
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                FlattenJsonElement(doc.RootElement, "", result);
            }
            return result;
        }

        private static void FlattenJsonElement(JsonElement element, string prefix, Dictionary<string, string> dict)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var newPrefix = string.IsNullOrEmpty(prefix)
                            ? property.Name
                            : $"{prefix}:{property.Name}";
                        FlattenJsonElement(property.Value, newPrefix, dict);
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        var newPrefix = $"{prefix}[{index}]";
                        FlattenJsonElement(item, newPrefix, dict);
                        index++;
                    }
                    break;

                default:
                    dict[prefix] = element.ValueKind switch
                    {
                        JsonValueKind.String => element.GetString(),
                        JsonValueKind.Number => element.GetRawText(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => null,
                        _ => element.ToString()
                    };
                    break;
            }
        }

        private static string FindEncryptedBlob(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("EncryptedSettings", out JsonElement encryptedElement) &&
                    encryptedElement.ValueKind == JsonValueKind.String)
                {
                    return encryptedElement.GetString();
                }

                foreach (var property in element.EnumerateObject())
                {
                    var result = FindEncryptedBlob(property.Value);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        // Existing public methods
        public static string Get(string key)
        {
            if (!_initialized) Initialize();
            return _settings.ContainsKey(key) ? _settings[key] : null;
        }

        public static string Get(string key, string defaultValue)
        {
            if (!_initialized) Initialize();
            return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
        }

        public static T Get<T>(string key, T defaultValue = default(T))
        {
            if (!_initialized) Initialize();

            if (_settings.ContainsKey(key))
            {
                try
                {
                    string value = _settings[key];

                    // Handle common types
                    if (typeof(T) == typeof(string))
                        return (T)(object)value;
                    if (typeof(T) == typeof(int) && int.TryParse(value, out int intValue))
                        return (T)(object)intValue;
                    if (typeof(T) == typeof(bool) && bool.TryParse(value, out bool boolValue))
                        return (T)(object)boolValue;
                    if (typeof(T) == typeof(double) && double.TryParse(value, out double doubleValue))
                        return (T)(object)doubleValue;
                    if (typeof(T) == typeof(decimal) && decimal.TryParse(value, out decimal decimalValue))
                        return (T)(object)decimalValue;

                    // For other types, try JSON deserialization
                    return JsonSerializer.Deserialize<T>(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        public static bool TryGetValue(string key, out string value)
        {
            if (!_initialized) Initialize();
            return _settings.TryGetValue(key, out value);
        }

        public static IReadOnlyDictionary<string, string> GetAllSettings()
        {
            if (!_initialized) Initialize();
            return _settings;
        }

        public static void Reload()
        {
            lock (_lock)
            {
                _initialized = false;
                _settings = null;
                Initialize();
            }
        }
    }
}