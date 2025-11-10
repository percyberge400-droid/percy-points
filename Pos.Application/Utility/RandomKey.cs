using System.Security.Cryptography;
using System.Text;

namespace Pos.Application.Utility
{
    public static class RandomKey
    {
        public static string GenerateNumericKey(int length)
        {
            const string digits = "0123456789";
            var result = new StringBuilder(length);

            using var rng = RandomNumberGenerator.Create();
            var data = new byte[length];
            rng.GetBytes(data);

            foreach (var b in data)
            {
                result.Append(digits[b % digits.Length]);
            }

            return result.ToString();
        }
    }
}
