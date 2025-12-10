using Pos.SecurityEncryption; // Your AES helper
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
                Console.WriteLine("4 = Encrypt The Common Pre-Decrypted File → Replace Common AppSettings");
                Console.WriteLine("0 = Exit");
                Console.Write("\nEnter choice: ");

                string choice = Console.ReadLine()?.Trim();
                if (choice == "0") break;

                if (choice == "1") EncryptMode();
                else if (choice == "2") DecryptMode();
                else if (choice == "3") EncryptAllDecryptedFiles();
                else if (choice == "4") EncryptCommmnonDecryptedFiles();
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
        // NEW OPTION: Encrypt 5 Decrypted Files & Replace Originals
        // ---------------------------------------------------
        private static void EncryptAllDecryptedFiles()
        {
            Console.Clear();
            Console.WriteLine("=== BULK ENCRYPT: Using Decrypted Inputs ===\n");

            try
            {
                // --------------------------------------------
                // DEFINE PATHS
                // --------------------------------------------
                string root = ProjectRoot(); // EncryptTool project root

                // Go up one level from EncryptTool to get the solution root (POS_2.0 or POS-PRA)
                string baseProjectPath = Path.GetFullPath(Path.Combine(root, ".."));

                Console.WriteLine($"📁 Project Root: {baseProjectPath}");
                Console.WriteLine($"📁 EncryptTool Root: {root}\n");

                // Input files (decrypted versions in EncryptTool project)
                string input_appsettings = Path.Combine(root, "appSettings/decrypted_appsettings.json");
                string input_worker = Path.Combine(root, "appSettings/decrypted_appsettings.worker.json");
                string input_cloud = Path.Combine(root, "appSettings/decrypted_cloud_appsettings.json");
                string input_sapp = Path.Combine(root, "appSettings/decrypted_Sapp.config");
                string input_wapp = Path.Combine(root, "appSettings/decrypted_Wapp.config");

                // Output files (encrypted versions in their respective projects)
                string output_appsettings = Path.Combine(baseProjectPath, @"Pos.Local.Api\appsettings.json");
                string output_worker = Path.Combine(baseProjectPath, @"Pos.Worker\appsettings.worker.json");
                string output_cloud = Path.Combine(baseProjectPath, @"Pos.Cloud.Api\cloud.appsettings.json");
                string output_sapp = Path.Combine(baseProjectPath, @"Pos.SetupUI\App.config");
                string output_wapp = Path.Combine(baseProjectPath, @"Pos.WinFormsUI\App.config");

                // Map paths for cleaner loop
                var pairs = new List<(string input, string output, string displayName)>
                {
                    (input_appsettings, output_appsettings, "Pos.Local.Api/appsettings.json"),
                    (input_worker, output_worker, "Pos.Worker/appsettings.worker.json"),
                    (input_cloud, output_cloud, "Pos.Cloud.Api/cloud.appsettings.json"),
                    (input_sapp, output_sapp, "Pos.SetupUI/App.config"),
                    (input_wapp, output_wapp, "Pos.WinFormsUI/App.config")
                };

                int successCount = 0;
                int skippedCount = 0;

                foreach (var (input, output, displayName) in pairs)
                {
                    if (!File.Exists(input))
                    {
                        Console.WriteLine($"⚠️  Missing input file: {Path.GetFileName(input)}");
                        skippedCount++;
                        continue;
                    }

                    Console.WriteLine($"\n→ Processing: {displayName}");
                    Console.WriteLine($"  Source: {Path.GetFileName(input)}");

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

                        // ✅ Add EC only for Pos.Local.Api appsettings.json
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
                        skippedCount++;
                        continue;
                    }

                    // --------------------------------------------
                    // WRITE OUTPUT
                    // --------------------------------------------
                    Directory.CreateDirectory(Path.GetDirectoryName(output));
                    File.WriteAllText(output, encryptedContent);

                    Console.WriteLine($"✅ Encrypted → {output}");
                    successCount++;
                }

                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine($"🎉 ENCRYPTION COMPLETE!");
                Console.WriteLine($"   ✅ Successful: {successCount}");
                if (skippedCount > 0)
                    Console.WriteLine($"   ⚠️  Skipped: {skippedCount}");
                Console.WriteLine(new string('=', 60));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        // ---------------------------------------------------
        // UPDATE COMMON SETTINGS & ENCRYPT ALL
        // ---------------------------------------------------
        private static void EncryptCommmnonDecryptedFiles()
        {
            Console.Clear();
            Console.WriteLine("=== UPDATE COMMON SETTINGS & ENCRYPT ===\n");
            try
            {
                string root = ProjectRoot(); // EncryptTool project root
                string commonSettingsPath = Path.Combine(root, "common-settings.json");

                if (!File.Exists(commonSettingsPath))
                {
                    Console.WriteLine($"❌ Common settings file not found: {commonSettingsPath}");
                    Console.WriteLine("\nPress any key to return...");
                    Console.ReadKey();
                    return;
                }

                // Load common settings
                Console.WriteLine($"📖 Loading common settings from: {Path.GetFileName(commonSettingsPath)}");
                string commonJson = File.ReadAllText(commonSettingsPath);
                using JsonDocument commonDoc = JsonDocument.Parse(commonJson);

                if (!commonDoc.RootElement.TryGetProperty("AppSettings", out JsonElement commonAppSettings))
                {
                    Console.WriteLine("❌ AppSettings section NOT found in common-settings.json");
                    Console.WriteLine("\nPress any key to return...");
                    Console.ReadKey();
                    return;
                }

                // Extract common settings as dictionary
                var commonSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(commonAppSettings.GetRawText());

                Console.WriteLine($"\n✅ Found {commonSettings.Count} common setting(s) to apply:");
                foreach (var setting in commonSettings)
                {
                    Console.WriteLine($"   • {setting.Key} = {setting.Value}");
                }

                // Define all decrypted files
                var decryptedFiles = new List<(string path, string type, string name)>
                {
                    (Path.Combine(root, "appSettings/decrypted_appsettings.json"), "json", "decrypted_appsettings.json"),
                    (Path.Combine(root, "appSettings/decrypted_appsettings.worker.json"), "json", "decrypted_appsettings.worker.json"),
                    (Path.Combine(root, "appSettings/decrypted_cloud_appsettings.json"), "json", "decrypted_cloud_appsettings.json"),
                    (Path.Combine(root, "appSettings/decrypted_Sapp.config"), "config", "decrypted_Sapp.config"),
                    (Path.Combine(root, "appSettings/decrypted_Wapp.config"), "config", "decrypted_Wapp.config")
                };

                Console.WriteLine($"\n{new string('=', 60)}\n");
                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var (filePath, fileType, fileName) in decryptedFiles)
                {
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"⚠️  File not found, skipping: {fileName}");
                        skippedCount++;
                        continue;
                    }

                    Console.WriteLine($"→ Updating: {fileName}");
                    string rawContent = File.ReadAllText(filePath);
                    string updatedContent;
                    int settingsApplied = 0;

                    // --------------------------------------------
                    // UPDATE JSON FILES
                    // --------------------------------------------
                    if (fileType == "json")
                    {
                        using var doc = JsonDocument.Parse(rawContent);

                        if (!doc.RootElement.TryGetProperty("AppSettings", out JsonElement existingAppSettings))
                        {
                            Console.WriteLine($"  ⚠️  No AppSettings section found in {fileName}, skipping...");
                            skippedCount++;
                            continue;
                        }

                        // Deserialize existing AppSettings as JsonElement dictionary first
                        var existingSettingsDict = new Dictionary<string, JsonElement>();
                        foreach (var prop in existingAppSettings.EnumerateObject())
                        {
                            existingSettingsDict[prop.Name] = prop.Value;
                        }

                        // Now create a mutable dictionary with actual values
                        var updatedSettings = new Dictionary<string, object>();

                        // Copy all existing settings
                        foreach (var kvp in existingSettingsDict)
                        {
                            // Convert JsonElement to appropriate type
                            object value;
                            switch (kvp.Value.ValueKind)
                            {
                                case JsonValueKind.String:
                                    value = kvp.Value.GetString();
                                    break;
                                case JsonValueKind.Number:
                                    value = kvp.Value.GetInt32();
                                    break;
                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    value = kvp.Value.GetBoolean();
                                    break;
                                default:
                                    value = kvp.Value.GetRawText();
                                    break;
                            }
                            updatedSettings[kvp.Key] = value;
                        }

                        // Apply common settings
                        foreach (var commonSetting in commonSettings)
                        {
                            if (updatedSettings.ContainsKey(commonSetting.Key))
                            {
                                var oldValue = updatedSettings[commonSetting.Key];
                                updatedSettings[commonSetting.Key] = commonSetting.Value;
                                settingsApplied++;
                                Console.WriteLine($"  ✓ Updated: {commonSetting.Key}");
                                Console.WriteLine($"    Old: {oldValue}");
                                Console.WriteLine($"    New: {commonSetting.Value}");
                            }
                        }

                        if (settingsApplied == 0)
                        {
                            Console.WriteLine($"  ℹ️  No matching keys found in {fileName}");
                            Console.WriteLine($"  Available keys: {string.Join(", ", updatedSettings.Keys)}");
                        }

                        // Rebuild JSON structure
                        var newJson = new Dictionary<string, object>
                        {
                            ["AppSettings"] = updatedSettings
                        };

                        // Copy other top-level sections
                        foreach (var property in doc.RootElement.EnumerateObject())
                        {
                            if (property.Name != "AppSettings")
                                newJson[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                        }

                        updatedContent = JsonSerializer.Serialize(newJson, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
                    }
                    // --------------------------------------------
                    // UPDATE CONFIG FILES (XML)
                    // --------------------------------------------
                    else if (fileType == "config")
                    {
                        var xml = XDocument.Parse(rawContent);
                        var appSettingsNode = xml.Descendants("appSettings").FirstOrDefault();

                        if (appSettingsNode == null)
                        {
                            Console.WriteLine($"  ⚠️  No appSettings section found in {fileName}, skipping...");
                            skippedCount++;
                            continue;
                        }

                        // Apply common settings to XML
                        foreach (var commonSetting in commonSettings)
                        {
                            var existingNode = appSettingsNode.Descendants("add")
                                .FirstOrDefault(x => (string)x.Attribute("key") == commonSetting.Key);

                            if (existingNode != null)
                            {
                                existingNode.Attribute("value").Value = commonSetting.Value;
                                settingsApplied++;
                                Console.WriteLine($"  ✓ Updated: {commonSetting.Key}");
                            }
                        }

                        if (settingsApplied == 0)
                        {
                            Console.WriteLine($"  ℹ️  No matching keys found in {fileName}");
                        }

                        updatedContent = xml.ToString();
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ Unsupported file type: {fileName}");
                        skippedCount++;
                        continue;
                    }

                    // Write updated content back to file
                    try
                    {
                        File.WriteAllText(filePath, updatedContent);

                        // Verify the file was written
                        string verification = File.ReadAllText(filePath);
                        if (verification != updatedContent)
                        {
                            Console.WriteLine($"  ⚠️ WARNING: File verification failed for {fileName}");
                        }
                        else
                        {
                            updatedCount++;
                            Console.WriteLine($"  ✅ Saved and verified: {fileName}");

                            // Show a snippet of what was saved
                            if (fileType == "json")
                            {
                                using var verifyDoc = JsonDocument.Parse(verification);
                                if (verifyDoc.RootElement.TryGetProperty("AppSettings", out JsonElement verifyAppSettings))
                                {
                                    Console.WriteLine($"  📄 Verified AppSettings in file:");
                                    foreach (var commonSetting in commonSettings)
                                    {
                                        if (verifyAppSettings.TryGetProperty(commonSetting.Key, out JsonElement val))
                                        {
                                            Console.WriteLine($"     {commonSetting.Key} = {val.GetString()}");
                                        }
                                    }
                                }
                            }
                        }
                        Console.WriteLine();
                    }
                    catch (Exception writeEx)
                    {
                        Console.WriteLine($"  ❌ Failed to write {fileName}: {writeEx.Message}");
                        skippedCount++;
                    }
                }

                Console.WriteLine($"{new string('=', 60)}");
                Console.WriteLine($"📊 Update Summary:");
                Console.WriteLine($"   ✅ Files Updated: {updatedCount}");
                if (skippedCount > 0)
                    Console.WriteLine($"   ⚠️  Files Skipped: {skippedCount}");
                Console.WriteLine($"{new string('=', 60)}\n");

                // Now encrypt all updated decrypted files
                if (updatedCount > 0)
                {
                    Console.WriteLine("🔐 Now encrypting all updated files...\n");
                    Console.WriteLine("⏱️  Waiting 1 second to ensure file writes are complete...");
                    System.Threading.Thread.Sleep(1000); // Give file system time to flush
                    Console.WriteLine("Press any key to continue to encryption...");
                    Console.ReadKey();
                    EncryptAllDecryptedFiles();
                }
                else
                {
                    Console.WriteLine("⚠️  No files were updated. Encryption skipped.");
                    Console.WriteLine("\nPress any key to return...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
            }
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