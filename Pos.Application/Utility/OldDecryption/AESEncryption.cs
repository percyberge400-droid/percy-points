using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Utility.OldDecryption
{
    public static class AESEncryption
    {
        private static byte[] DKey
        {
            get
            {
                return new byte[] { 0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6, 0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C };
            }
        }
        private static byte[] IV
        {
            get
            {
                return Convert.FromBase64String("wLqKPNViBADAuoo81WIEAA==");
            }
        }

        public static string Decrypt(string encryptedText, string encryptionKey)
        {
            byte[] bKey = encryptionKey == string.Empty ? DKey : SharedResources.ToByteArray(encryptionKey);
            byte[] cipherBytes = Convert.FromBase64String(encryptedText);
            string decodeAndDecrypt = AesDecrypt(cipherBytes, bKey);
            return decodeAndDecrypt;
        }

        private static string AesDecrypt(Byte[] inputBytes, byte[] Key)
        {
            Byte[] outputBytes = inputBytes;

            string plaintext = string.Empty;

            using (MemoryStream memoryStream = new MemoryStream(outputBytes))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, GetCryptoAlgorithm().CreateDecryptor(Key, IV), CryptoStreamMode.Read))
                {
                    using (BinaryReader srDecrypt = new BinaryReader(cryptoStream))
                    {
                        outputBytes = srDecrypt.ReadBytes(inputBytes.Length);
                        plaintext = Encoding.UTF8.GetString(outputBytes);
                    }
                }
            }
            return plaintext;
        }

        private static RijndaelManaged GetCryptoAlgorithm()
        {
            RijndaelManaged algorithm = new RijndaelManaged();
            algorithm.Padding = PaddingMode.None;
            algorithm.Mode = CipherMode.CBC;
            algorithm.KeySize = 128;
            algorithm.BlockSize = 128;
            return algorithm;
        }
    }
}
