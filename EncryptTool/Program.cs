using POSPRA.SecurityEncryption; // Your AES helper
using System.Text.Json;
using System.Xml.Linq;

namespace EncryptTool
{
    class Program
    {
        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== APP SETTINGS ENCRYPT / DECRYPT TOOL ===\n");
                Console.WriteLine("1 = Encrypt App.config/appsettings.json -> Encrypted Config File");
                Console.WriteLine("2 = Decrypt Blob/Config File -> Original appSettings XML/JSON");
                Console.WriteLine("3 = Encrypt All Pre-Decrypted Files → Replace Actual AppSettings");
                Console.WriteLine("0 = Exit");
                Console.Write("\nEnter choice: ");

                string choice = Console.ReadLine()?.Trim();
                if (choice == "0") break;

                if (choice == "1") EncryptMode();
                else if (choice == "2") DecryptMode();
                else if (choice == "3") EncryptAllDecryptedFiles();
                else
                {
                    Console.WriteLine("❌ Invalid choice.");
                    Console.ReadKey();
                }
            }
        }

        // ---------------------------------------------------
        // ENCRYPT MODE
        // ---------------------------------------------------
        private static void EncryptMode()
        {
            Console.WriteLine("\nEnter full path of App.config or appsettings.json:");
            string filePath = Console.ReadLine()?.Trim();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("❌ File not found.");
                Console.ReadKey();
                return;
            }

            try
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();
                string encryptedBlob;
                string outputContent;

                if (fileExtension == ".config")
                {
                    // Handle App.config
                    var xml = XDocument.Load(filePath);

                    // Extract all <appSettings><add> key/value pairs (exclude EncryptedSettings if exists)
                    var settings = xml.Descendants("appSettings")
                                      .Descendants("add")
                                      .Where(x => (string)x.Attribute("key") != "EncryptedSettings")
                                      .ToDictionary(
                                          x => (string)x.Attribute("key"),
                                          x => (string)x.Attribute("value")
                                      );

                    if (settings.Count == 0)
                    {
                        Console.WriteLine("❌ No app settings found to encrypt (or only EncryptedSettings exists).");
                        Console.ReadKey();
                        return;
                    }

                    // Convert to JSON
                    string json = JsonSerializer.Serialize(settings);

                    // Encrypt JSON
                    encryptedBlob = AesEncryptionHelper.Encrypt(json);

                    // Create full App.config structure with encrypted blob
                    var encryptedConfig = new XDocument(
                        new XDeclaration("1.0", "utf-8", null),
                        new XElement("configuration",
                            new XElement("appSettings",
                                new XElement("add",
                                    new XAttribute("key", "EncryptedSettings"),
                                    new XAttribute("value", encryptedBlob)
                                )
                            )
                        )
                    );

                    outputContent = encryptedConfig.ToString();
                }
                else if (fileExtension == ".json")
                {
                    // Handle appsettings.json
                    string jsonContent = File.ReadAllText(filePath);

                    // Parse the JSON to extract AppSettings section
                    using JsonDocument doc = JsonDocument.Parse(jsonContent);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("AppSettings", out JsonElement appSettingsElement))
                    {
                        // Serialize just the AppSettings section
                        string appSettingsJson = JsonSerializer.Serialize(appSettingsElement);
                        encryptedBlob = AesEncryptionHelper.Encrypt(appSettingsJson);

                        // Create new JSON structure with encrypted blob
                        var newJson = new Dictionary<string, object>
                        {
                            ["AppSettings"] = new Dictionary<string, string>
                            {
                                ["EncryptedSettings"] = encryptedBlob
                            }
                        };

                        // Copy other sections (like Logging) if they exist
                        foreach (var property in root.EnumerateObject())
                        {
                            if (property.Name != "AppSettings")
                            {
                                newJson[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                            }
                        }

                        outputContent = JsonSerializer.Serialize(newJson, new JsonSerializerOptions { WriteIndented = true });
                    }
                    else
                    {
                        throw new Exception("AppSettings section not found in JSON file.");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Unsupported file format. Only .config and .json files are supported.");
                    Console.ReadKey();
                    return;
                }

                // Output to console
                Console.WriteLine("\n========== ENCRYPTED CONFIG FILE CONTENT ==========\n");
                Console.WriteLine(outputContent);
                Console.WriteLine("\n====================================================");

                // Save the output to the same directory
                string dir = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string outputFile = Path.Combine(dir,
                    fileExtension == ".config" ? $"{fileName}.encrypted.config" : "encrypted_appsettings.json");
                File.WriteAllText(outputFile, outputContent);

                Console.WriteLine($"\n✅ Encrypted config saved to: {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        // ---------------------------------------------------
        // DECRYPT MODE - FIXED VERSION
        // ---------------------------------------------------
        private static void DecryptMode()
        {
            Console.WriteLine("\nEnter full path of the encrypted file:");
            string filePath = Console.ReadLine()?.Trim();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("❌ File not found.");
                Console.ReadKey();
                return;
            }

            try
            {
                string raw = File.ReadAllText(filePath).Trim();
                string blob = ExtractEncryptedBlob(raw);

                if (string.IsNullOrWhiteSpace(blob))
                    throw new Exception("Could not extract encrypted blob from the file.");

                // 🔑 Decrypt the blob to get the original JSON
                string decryptedJson = AesEncryptionHelper.Decrypt(blob);

                // Determine if the original was JSON or XML format
                bool isJsonFormat = raw.TrimStart().StartsWith("{");
                bool isFullAppConfig = raw.TrimStart().StartsWith("<?xml") || raw.TrimStart().StartsWith("<configuration");

                string output;
                string outputFileName;

                if (isJsonFormat)
                {
                    // For JSON files, reconstruct the full appsettings.json
                    output = ReconstructJsonConfig(raw, decryptedJson);
                    outputFileName = "decrypted_appsettings.json";
                    Console.WriteLine("\n========= DECRYPTED APPSETTINGS.JSON =========\n");
                }
                else
                {
                    // For XML files, build the appSettings section
                    output = ReconstructXmlConfig(decryptedJson, isFullAppConfig);
                    outputFileName = isFullAppConfig ? "decrypted_app.config" : "decrypted_appsettings.xml";
                    Console.WriteLine("\n========= DECRYPTED APP SETTINGS =========\n");
                }

                Console.WriteLine(output);
                Console.WriteLine("\n==============================================");

                // Save the decrypted file
                string dir = Path.GetDirectoryName(filePath);
                string outputFile = Path.Combine(dir, outputFileName);
                File.WriteAllText(outputFile, output);

                Console.WriteLine($"\n✅ Decrypted file saved to: {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static string ProjectRoot()
        {
            // bin/Debug/net9.0  → up 3 levels → project folder
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
        }

        // ---------------------------------------------------
        // NEW OPTION: Encrypt 4 Decrypted Files & Replace Originals
        // ---------------------------------------------------
        private static void EncryptAllDecryptedFiles()
        {
            Console.Clear();
            Console.WriteLine("=== BULK ENCRYPT: Using 4 Decrypted Inputs ===\n");

            try
            {
                // --------------------------------------------
                // DEFINE PATHS
                // --------------------------------------------
                string baseProjectPath = @"D:\Project\POS_2.0\";

                string root = ProjectRoot(); // EncryptTool project root

                string input_worker = Path.Combine(root, "decrypted_appsettings.worker.json");
                string input_sapp = Path.Combine(root, "decrypted_Sapp.config");
                string input_wapp = Path.Combine(root, "decrypted_Wapp.config");
                string input_api = Path.Combine(root, "decrypted_appsettings.json");


                string output_worker = Path.Combine(baseProjectPath, @"POSPRA.Worker\appsettings.worker.json");
                string output_sapp = Path.Combine(baseProjectPath, @"POSPRA.SetupUI\App.config");
                string output_wapp = Path.Combine(baseProjectPath, @"POSPRA-WinFormsUI\App.config");
                string output_api = Path.Combine(baseProjectPath, @"Pos.Api\appsettings.json");

                // Map paths for cleaner loop
                var pairs = new List<(string input, string output)>
                {
                    (input_worker, output_worker),
                    (input_sapp, output_sapp),
                    (input_wapp, output_wapp),
                    (input_api, output_api)
                };

                foreach (var (input, output) in pairs)
                {
                    if (!File.Exists(input))
                    {
                        Console.WriteLine($"❌ Missing input file: {input}");
                        continue;
                    }

                    Console.WriteLine($"\n→ Encrypting: {input}");

                    string extension = Path.GetExtension(input).ToLower();

                    string raw = File.ReadAllText(input);
                    string encryptedContent;

                    // --------------------------------------------
                    // HANDLE JSON
                    // --------------------------------------------
                    if (extension == ".json")
                    {
                        using var doc = JsonDocument.Parse(raw);

                        if (!doc.RootElement.TryGetProperty("AppSettings", out JsonElement appSettings))
                            throw new Exception($"AppSettings section NOT found in JSON: {input}");

                        // Encrypt AppSettings
                        string jsonToEncrypt = JsonSerializer.Serialize(appSettings);
                        string blob = AesEncryptionHelper.Encrypt(jsonToEncrypt);

                        var newAppSettings = new Dictionary<string, string>
                        {
                            ["EncryptedSettings"] = blob
                        };

                        // ✅ Add EC only for decrypted_appsettings.json
                        if (Path.GetFileName(input).Equals("decrypted_appsettings.json", StringComparison.OrdinalIgnoreCase))
                        {
                            newAppSettings["EC"] = "36b8dd382b014af3e053ecf1322f6db2";
                        }

                        // Build new JSON structure
                        var newJson = new Dictionary<string, object>
                        {
                            ["AppSettings"] = newAppSettings
                        };

                        // Copy other top-level sections
                        foreach (var property in doc.RootElement.EnumerateObject())
                        {
                            if (property.Name != "AppSettings")
                                newJson[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                        }

                        // Serialize back to string
                        encryptedContent = JsonSerializer.Serialize(newJson, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
                    }
                    // --------------------------------------------
                    // HANDLE APP.CONFIG (XML)
                    // --------------------------------------------
                    else if (extension == ".config")
                    {
                        var xml = XDocument.Parse(raw);

                        var settings = xml.Descendants("appSettings")
                                          .Descendants("add")
                                          .Where(x => (string)x.Attribute("key") != "EncryptedSettings")
                                          .ToDictionary(
                                              x => (string)x.Attribute("key"),
                                              x => (string)x.Attribute("value")
                                          );

                        string json = JsonSerializer.Serialize(settings);

                        string blob = AesEncryptionHelper.Encrypt(json);

                        var encryptedXml = new XDocument(
                            new XDeclaration("1.0", "utf-8", null),
                            new XElement("configuration",
                                new XElement("appSettings",
                                    new XElement("add",
                                        new XAttribute("key", "EncryptedSettings"),
                                        new XAttribute("value", blob)
                                    )
                                )
                            )
                        );

                        encryptedContent = encryptedXml.ToString();
                    }
                    else
                    {
                        Console.WriteLine($"❌ Skipped unsupported file: {input}");
                        continue;
                    }

                    // --------------------------------------------
                    // WRITE OUTPUT
                    // --------------------------------------------
                    File.WriteAllText(output, encryptedContent);

                    Console.WriteLine($"✅ Encrypted + Updated: {output}");
                }

                Console.WriteLine("\n🎉 ALL POSSIBLE FILES ENCRYPTED & UPDATED SUCCESSFULLY!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }


        private static string ExtractEncryptedBlob(string rawContent)
        {
            string trimmed = rawContent.Trim();

            // CASE 1: Full App.config with XML declaration
            if (trimmed.StartsWith("<?xml") || trimmed.StartsWith("<configuration"))
            {
                var xml = XDocument.Parse(trimmed);
                var node = xml.Descendants("add")
                              .FirstOrDefault(x => (string)x.Attribute("key") == "EncryptedSettings");
                return node?.Attribute("value")?.Value;
            }

            // CASE 2: Single <add> element
            else if (trimmed.StartsWith("<add"))
            {
                var element = XElement.Parse(trimmed);
                return element.Attribute("value")?.Value;
            }

            // CASE 3: App.config fragment
            else if (trimmed.Contains("EncryptedSettings") && trimmed.StartsWith("<"))
            {
                var xml = XDocument.Parse(trimmed);
                var node = xml.Descendants("add")
                              .FirstOrDefault(x => (string)x.Attribute("key") == "EncryptedSettings");
                return node?.Attribute("value")?.Value;
            }

            // CASE 4: JSON format
            else if (trimmed.StartsWith("{"))
            {
                using JsonDocument doc = JsonDocument.Parse(trimmed);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("AppSettings", out JsonElement appSettingsElement) &&
                    appSettingsElement.TryGetProperty("EncryptedSettings", out JsonElement encryptedElement))
                {
                    return encryptedElement.GetString();
                }
            }

            // CASE 5: Raw blob only
            else
            {
                return trimmed;
            }

            return null;
        }

        private static string ReconstructJsonConfig(string originalJson, string decryptedAppSettingsJson)
        {
            using JsonDocument originalDoc = JsonDocument.Parse(originalJson);
            JsonElement originalRoot = originalDoc.RootElement;

            var newJson = new Dictionary<string, object>();

            // Replace AppSettings with decrypted content
            var decryptedAppSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(decryptedAppSettingsJson);
            newJson["AppSettings"] = decryptedAppSettings;

            // Copy other sections (like Logging)
            foreach (var property in originalRoot.EnumerateObject())
            {
                if (property.Name != "AppSettings")
                {
                    newJson[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                }
            }

            return JsonSerializer.Serialize(newJson, new JsonSerializerOptions { WriteIndented = true });
        }

        private static string ReconstructXmlConfig(string decryptedJson, bool isFullConfig)
        {
            // Deserialize the decrypted JSON back to key-value pairs
            var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson);

            var appSettingsXml = new XElement("appSettings",
                settings.Select(kv =>
                    new XElement("add",
                        new XAttribute("key", kv.Key),
                        new XAttribute("value", kv.Value)
                    )
                )
            );

            if (isFullConfig)
            {
                // Reconstruct full App.config with decrypted settings
                var decryptedConfig = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement("configuration", appSettingsXml)
                );
                return decryptedConfig.ToString();
            }
            else
            {
                return appSettingsXml.ToString();
            }
        }
    }
}