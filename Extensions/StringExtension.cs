using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace AspNetCoreSamplesJwt.Extensions
{
    public static class StringExtension
    {
        public static string GetKeyDerivationHash(this string plainText, byte[] salt = null)
        {
            if (salt is null)
            {
                salt = new byte[128 / 8];
                using var random = RandomNumberGenerator.Create();
                random.GetBytes(salt);
            }

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(password: plainText, salt: salt, KeyDerivationPrf.HMACSHA1, 10000, numBytesRequested: 256 / 8));
        }
    }
}
