using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Utilities
{
    public static class Crypto
    {
        private static readonly Random _random = new Random();

        public static string GetHashString(string target, string salt)
        {
            using var sha = SHA512.Create();
            var hashedTarget = sha.ComputeHash(Encoding.UTF8.GetBytes(target));
            var saltedTarget = new byte[hashedTarget.Length + salt.Length];
            for (var i = 0; i < hashedTarget.Length; i++)
            {
                saltedTarget[i] = hashedTarget[i];
            }

            for (var i = 0; i < salt.Length; i++)
            {
                saltedTarget[hashedTarget.Length + i] = (byte) salt[i];
            }

            return string.Join(string.Empty, saltedTarget.Select(i => i.ToString("X2")).ToArray());
        }

        public static bool Verify(string input, string hash, string salt)
        {
            var inputHash = GetHashString(input, salt);
            return StringComparer.OrdinalIgnoreCase.Compare(inputHash, hash) == 0;
        }

        public static string GetRandomSalt(int characterLength)
        {
            var alphabet = Enumerable.Range('A', 'Z' - 'A' + 1).ToArray();

            return string.Join(
                string.Empty,
                Enumerable.Range(0, characterLength)
                    .Select(c => alphabet
                        .Select(b => (char) b).ToArray()[_random.Next(alphabet.Length)]));
        }
    }
}