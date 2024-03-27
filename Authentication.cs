using System;
using System.Collections.Generic;
using System.Text;
using System.Web;


    public class Authentication
    {
        public const string AlphaNumericCharacters = "abcdefghijkmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$*!";
        System.Security.Cryptography.HashAlgorithm algorithm = new System.Security.Cryptography.SHA256Managed();

        public string GenerateSaltValue()
        {
            string chars = AlphaNumericCharacters;
            Random random = new Random();
            StringBuilder password = new StringBuilder(8);
            for (int i = 0; i < 8; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            return password.ToString();
        }
        public string GeneratePassword(int length)
        {
            string chars = AlphaNumericCharacters;
            Random random = new Random();
            StringBuilder password = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            return password.ToString();
        }
        public string Hash(string value,string salt)
        {   
            if (string.IsNullOrEmpty(salt))
            {
                salt = GenerateSaltValue();
            }
            value = value + salt;
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
