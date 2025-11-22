using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using POSPRA.SecurityEncryption; // Your AES helper

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
                Console.WriteLine("1 = Encrypt App.config -> Encrypted Blob for ConfigManager");
                Console.WriteLine("2 = Decrypt Blob -> Original appSettings XML");
                Console.WriteLine("0 = Exit");
                Console.Write("\nEnter choice: ");

                string choice = Console.ReadLine()?.Trim();
                if (choice == "0") break;

                if (choice == "1") EncryptMode();
                else if (choice == "2") DecryptMode();
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
            Console.WriteLine("\nEnter full path of App.config:");
            string filePath = Console.ReadLine()?.Trim();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("❌ File not found.");
                Console.ReadKey();
                return;
            }

            try
            {
                var xml = XDocument.Load(filePath);

                // Extract all <appSettings><add> key/value pairs
                var settings = xml.Descendants("appSettings")
                                  .Descendants("add")
                                  .ToDictionary(
                                      x => (string)x.Attribute("key"),
                                      x => (string)x.Attribute("value")
                                  );

                // Convert to JSON
                string json = System.Text.Json.JsonSerializer.Serialize(settings);

                // Encrypt JSON
                string encryptedBlob = AesEncryptionHelper.Encrypt(json);

                // Prepare output line for ConfigurationManager
                string configLine = $"<add key=\"EncryptedSettings\" value=\"{encryptedBlob}\" />";

                // Output to console
                Console.WriteLine("\n========== COPY THIS LINE INTO App.config ==========\n");
                Console.WriteLine(configLine);
                Console.WriteLine("\n====================================================");

                // Save the full line to the same directory as App.config
                string dir = Path.GetDirectoryName(filePath);
                string encryptedFile = Path.Combine(dir, "encrypted_blob.txt");
                File.WriteAllText(encryptedFile, configLine); // <-- write full <add ... /> line

                Console.WriteLine($"\n✅ Encrypted blob saved to: {encryptedFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        // ---------------------------------------------------
        // DECRYPT MODE
        // ---------------------------------------------------
        private static void DecryptMode()
        {
            Console.WriteLine("\nEnter full path of the encrypted blob file:");
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
                string blob;

                // Case 1: File contains the full <add key=".." value=".."/>
                if (raw.StartsWith("<add"))
                {
                    // parse XML and extract value="..."
                    var xml = XElement.Parse(raw);
                    blob = xml.Attribute("value")?.Value;

                    if (string.IsNullOrWhiteSpace(blob))
                        throw new Exception("Cannot extract 'value' attribute.");
                }
                else
                {
                    // Case 2: File contains only the Base64 blob
                    blob = raw;
                }

                // 🔑 Now decrypt the blob safely
                string json = AesEncryptionHelper.Decrypt(blob);

                var settings = System.Text.Json.JsonSerializer
                                  .Deserialize<Dictionary<string, string>>(json);

                // Build XML
                var appSettingsXml = new XElement("appSettings",
                    settings.Select(kv =>
                        new XElement("add",
                            new XAttribute("key", kv.Key),
                            new XAttribute("value", kv.Value)
                        )
                    )
                );

                string xmlString = appSettingsXml.ToString();

                Console.WriteLine("\n========= DECRYPTED APP SETTINGS XML =========\n");
                Console.WriteLine(xmlString);
                Console.WriteLine("\n==============================================");

                string dir = Path.GetDirectoryName(filePath);
                string decryptedFile = Path.Combine(dir, "decrypted_appsettings.xml");
                File.WriteAllText(decryptedFile, xmlString);

                Console.WriteLine($"\n✅ Decrypted file saved to: {decryptedFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

    }
}
