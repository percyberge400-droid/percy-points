using System;
using System.IO;
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
                Console.WriteLine("1 = Encrypt App.config");
                Console.WriteLine("2 = Decrypt encrypted blob file");
                Console.WriteLine("0 = Exit");
                Console.Write("\nEnter choice: ");

                string choice = Console.ReadLine()?.Trim();
                if (choice == "0") break;

                switch (choice)
                {
                    case "1":
                        EncryptFileMode();
                        break;
                    case "2":
                        DecryptFileMode();
                        break;
                    default:
                        Console.WriteLine("❌ Invalid selection. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void EncryptFileMode()
        {
            Console.WriteLine("\nEnter full path of the App.config file to encrypt:");
            string filePath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("❌ Invalid file path.");
                Console.ReadKey();
                return;
            }

            try
            {
                string content = File.ReadAllText(filePath);

                // Encrypt entire content
                string encryptedBlob = AesEncryptionHelper.Encrypt(content);

                // Save encrypted blob to file
                string output = Path.Combine(Path.GetDirectoryName(filePath), "appsettings.encrypted.txt");
                File.WriteAllText(output, encryptedBlob);

                Console.WriteLine("\n✅ Encryption successful!");
                Console.WriteLine($"Encrypted blob saved to: {output}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private static void DecryptFileMode()
        {
            Console.WriteLine("\nEnter full path of the encrypted file to decrypt (default: appsettings.encrypted.txt):");
            string filePath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = "appsettings.encrypted.txt";
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine("❌ File does not exist.");
                Console.ReadKey();
                return;
            }

            try
            {
                string encryptedBlob = File.ReadAllText(filePath);

                // Decrypt
                string decryptedContent = AesEncryptionHelper.Decrypt(encryptedBlob);

                // Save decrypted content to file
                string output = Path.Combine(Path.GetDirectoryName(filePath), "appsettings.decrypted.xml");
                File.WriteAllText(output, decryptedContent);

                Console.WriteLine("\n✅ Decryption successful!");
                Console.WriteLine($"Decrypted file saved to: {output}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Decryption error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }
}
