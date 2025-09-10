using System.Security.Cryptography;
using System.Text;

namespace POSPRA.Application.Utility
{
    public static class DataSigning
    {
        /// <summary>
        /// Generates a new 2048-bit RSA key pair and returns the private key and public key PEM strings.
        /// </summary>
        public static (RSA PrivateKey, string PublicKeyPem) GenerateKeys()
        {
            var rsa = RSA.Create(2048);
            string publicKeyPem = ExportPublicKeyToPem(rsa);
            return (rsa, publicKeyPem);
        }

        /// <summary>
        /// Signs a payload using the provided RSA private key.
        /// </summary>
        public static string Sign(RSA privateKey, string payload)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(payload);
            byte[] signatureBytes = privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// Verifies a payload signature using a public key in PEM format.
        /// </summary>
        public static bool Verify(string publicKeyPem, string payload, string signatureBase64)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem.ToCharArray());

            byte[] dataBytes = Encoding.UTF8.GetBytes(payload);
            byte[] signatureBytes = Convert.FromBase64String(signatureBase64);

            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// Exports a public key from an RSA object to PEM format.
        /// </summary>
        private static string ExportPublicKeyToPem(RSA rsa)
        {
            var pubKey = rsa.ExportSubjectPublicKeyInfo();
            string base64 = Convert.ToBase64String(pubKey);
            StringBuilder sb = new();
            sb.AppendLine("-----BEGIN PUBLIC KEY-----");
            for (int i = 0; i < base64.Length; i += 64)
                sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
            sb.AppendLine("-----END PUBLIC KEY-----");
            return sb.ToString();
        }
    }
}