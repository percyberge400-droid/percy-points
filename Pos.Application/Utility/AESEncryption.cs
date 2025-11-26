using System.Security.Cryptography;
using System.Text;

namespace Pos.Application.Utility
{
    /// <summary>
    /// Provides methods to encrypt and decrypt data using AES-GCM (Galois/Counter Mode).
    /// </summary>
    public static class ModernAESEncryption
    {
        /// <summary>
        /// Encrypts the specified plaintext using AES-GCM with the given key.
        /// </summary>
        /// <param name="plaintext">The plaintext string to encrypt.</param>
        /// <param name="key">The 256-bit key to use for encryption (32 bytes).</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><c>cipherText</c> – Base64-encoded ciphertext</item>
        /// <item><c>nonce</c> – Base64-encoded random nonce used for encryption</item>
        /// <item><c>tag</c> – Base64-encoded authentication tag</item>
        /// </list>
        /// </returns>
        public static (string cipherText, string nonce, string tag) Encrypt(string plaintext, byte[] key)
        {
            using var aes = new AesGcm(key);

            byte[] nonce = RandomNumberGenerator.GetBytes(12); // 96-bit nonce
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] cipherBytes = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16]; // 128-bit authentication tag

            aes.Encrypt(nonce, plaintextBytes, cipherBytes, tag);

            return (Convert.ToBase64String(cipherBytes),
                    Convert.ToBase64String(nonce),
                    Convert.ToBase64String(tag));
        }

        public static string Decrypt(string encryptedPackage, string EC)
        {
            try
            {
                // Split cipherText, nonce, tag
                var parts = encryptedPackage.Split(':');
                if (parts.Length != 3)
                    throw new InvalidOperationException("Invalid encrypted package format.");

                string cipherText = parts[0];
                string nonce = parts[1];
                string tag = parts[2];

                // Recreate AES key (must match CreateFiscalInvoiceAsync)
                byte[] aesKey = Encoding.UTF8.GetBytes(EC.PadRight(32).Substring(0, 32));

                // Decrypt
                string decryptedText = DecryptFiscalInvoice(cipherText, nonce, tag, aesKey);

                return decryptedText; // "{invoiceJson}|false|Latest|{signatureBase64}"
            }
            catch(Exception){
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts the specified ciphertext using AES-GCM with the given key, nonce, and authentication tag.
        /// </summary>
        /// <param name="cipherTextBase64">The Base64-encoded ciphertext.</param>
        /// <param name="nonceBase64">The Base64-encoded nonce used during encryption.</param>
        /// <param name="tagBase64">The Base64-encoded authentication tag generated during encryption.</param>
        /// <param name="key">The 256-bit key used for decryption (32 bytes).</param>
        /// <returns>The decrypted plaintext string.</returns>
        /// <exception cref="CryptographicException">Thrown if authentication fails or decryption fails.</exception>
        private static string DecryptFiscalInvoice(string cipherTextBase64, string nonceBase64, string tagBase64, byte[] key)
        {
            using var aes = new AesGcm(key);

            byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
            byte[] nonce = Convert.FromBase64String(nonceBase64);
            byte[] tag = Convert.FromBase64String(tagBase64);
            byte[] plaintextBytes = new byte[cipherBytes.Length];

            aes.Decrypt(nonce, cipherBytes, tag, plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}