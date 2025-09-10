using System.Security.Cryptography;
using System.Text;

namespace POSPRA.Application.Utility
{
    public class ModernAESEncryption
    {
        public static (string cipherText, string nonce, string tag) Encrypt(string plainText, byte[] key)
        {
            using var aes = new AesGcm(key);

            byte[] nonce = RandomNumberGenerator.GetBytes(12); // 12 bytes is standard for GCM
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16]; // 128-bit tag

            aes.Encrypt(nonce, plaintextBytes, cipherBytes, tag);

            return (
                cipherText: Convert.ToBase64String(cipherBytes),
                nonce: Convert.ToBase64String(nonce),
                tag: Convert.ToBase64String(tag)
            );
        }

        public static string Decrypt(string cipherText, string nonceBase64, string tagBase64, byte[] key)
        {
            using var aes = new AesGcm(key);

            byte[] nonce = Convert.FromBase64String(nonceBase64);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] tag = Convert.FromBase64String(tagBase64);
            byte[] plainBytes = new byte[cipherBytes.Length];

            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}