using System.Security.Cryptography;
using System.Text;

namespace POSPRA.Application.Utility
{
    public class DataSigning
    {
        public static string Sign(string privateKeyXml, string data)
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKeyXml); // load private key in XML format

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        public static bool Verify(string publicKeyXml, string data, string signature)
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(publicKeyXml); // load public key

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
