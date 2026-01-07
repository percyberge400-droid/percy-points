using System.Security.Cryptography;
using System.Text;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.LogService;

namespace Pos.Application.Utility
{
    /// <summary>
    /// Provides methods to encrypt and decrypt data using AES-GCM (Galois/Counter Mode).
    /// </summary>
    public class AESEncryption
    {
        private readonly ICloudLogService _cloudLogService;
        public AESEncryption(ICloudLogService cloudLogService)
        {
            _cloudLogService = cloudLogService;
        }
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
        public (string cipherText, string nonce, string tag) Encrypt(string plaintext, byte[] key)
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

        public async Task<string> DecryptAsync(string encryptedPackage, string EC)
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
            catch (Exception ex)
            {
                var dto = SyncLogBuilder.Build(AlertType.Exception, ex.Message, posId: 0)
                    .WithExceptionInfo(ex)
                    .WithDomainInfo("SecurityEncryption", "Decrypt", null);

                await _cloudLogService.CreateCloudLog(new List<SyncLogDto> { dto }, "Production");

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
        private string DecryptFiscalInvoice(string cipherTextBase64, string nonceBase64, string tagBase64, byte[] key)
        {
            using var aes = new AesGcm(key);

            byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
            byte[] nonce = Convert.FromBase64String(nonceBase64);
            byte[] tag = Convert.FromBase64String(tagBase64);
            byte[] plaintextBytes = new byte[cipherBytes.Length];

            aes.Decrypt(nonce, cipherBytes, tag, plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        /// <summary>
        /// Decrypts a Base64-encoded string using AES-256-CBC encryption.
        /// This method is specifically designed for Windows 7 AES encryption format,
        /// where the IV (Initialization Vector) is prefixed to the cipher bytes.
        /// </summary>
        /// <param name="encryptedBase64">The encrypted invoice string in Base64 format.</param>
        /// <param name="EC">Encryption key string used for decryption.</param>
        /// <returns>
        /// Decrypted plaintext string if successful; otherwise, an empty string.
        /// Any exceptions are logged to the cloud logging service.
        /// </returns>
        public async Task<string> DecryptWindows7Async(string encryptedBase64, string EC)
        {
            try
            {
                // Convert Base64 string to raw byte array
                byte[] combinedBytes = Convert.FromBase64String(encryptedBase64);

                // Ensure the byte array contains at least the IV (16 bytes)
                if (combinedBytes.Length < 16)
                    return string.Empty;

                // Prepare AES key (32 bytes) from EC string
                byte[] aesKey = Encoding.UTF8.GetBytes(EC.PadRight(32).Substring(0, 32));

                using (var aes = new AesCryptoServiceProvider())
                {
                    // AES configuration
                    aes.KeySize = 256;           // 256-bit key
                    aes.BlockSize = 128;         // 128-bit block
                    aes.Mode = CipherMode.CBC;   // CBC mode
                    aes.Padding = PaddingMode.PKCS7; // PKCS7 padding
                    aes.Key = aesKey;

                    int ivSize = aes.BlockSize / 8; // IV length = 16 bytes

                    // Split combinedBytes into IV and ciphertext
                    byte[] iv = new byte[ivSize];
                    byte[] cipherBytes = new byte[combinedBytes.Length - ivSize];

                    Buffer.BlockCopy(combinedBytes, 0, iv, 0, ivSize);               // First 16 bytes = IV
                    Buffer.BlockCopy(combinedBytes, ivSize, cipherBytes, 0, cipherBytes.Length); // Remaining bytes = ciphertext

                    aes.IV = iv;

                    // Create decryptor and perform AES decryption
                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(plainBytes); // Convert decrypted bytes to string
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any exception to cloud logging service
                var dto = SyncLogBuilder.Build(AlertType.Exception, ex.Message, posId: 0)
                    .WithExceptionInfo(ex)
                    .WithDomainInfo("SecurityEncryption", "Decrypt", null);

                await _cloudLogService.CreateCloudLog(new List<SyncLogDto> { dto }, "Production");

                return string.Empty; // Return empty string on failure
            }
        }
    }
}