using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UsersStorageService.Utils
{
    public static class ShaHelper
    {
        public static string GetSHA256String(string str)
        {
            var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(str);
            var hash = sha256.ComputeHash(bytes);

            return GetStringFromHash(hash);
        }

        public static string GetSHA512String(string str)
        {
            var sha512 = SHA512.Create();
            var bytes = Encoding.UTF8.GetBytes(str);
            var hash = sha512.ComputeHash(bytes);

            return GetStringFromHash(hash);
        }

        private static string GetStringFromHash(IEnumerable<byte> hash)
        {
            var builder = new StringBuilder();

            foreach (var theByte in hash)
            {
                builder.Append(theByte.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
