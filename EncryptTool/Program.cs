using System;
using POSPRA.SecurityEncryption; // ✅ Reference your helper library

namespace EncryptTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AES Encryption Helper Tool\n");

            Console.Write("Enter text to encrypt (e.g. Username): ");
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("❌ No input provided.");
                return;
            }

            try
            {
                string encrypted = AesEncryptionHelper.Encrypt(input);
                Console.WriteLine($"\n✅ Encrypted value (copy this into app.config):\n{encrypted}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
