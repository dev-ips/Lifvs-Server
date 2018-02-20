using Lifvs.Common.Utility.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Lifvs.Common.Utility
{
    public class CryptoGraphy : ICryptoGraphy
    {
        private readonly string _passphrase = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public string EncryptString(string message)
        {
            byte[] results;

            UTF8Encoding utf8 = new UTF8Encoding();
            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(_passphrase));

            TripleDESCryptoServiceProvider tdesAlgorithm = new TripleDESCryptoServiceProvider();

            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            byte[] dataToEncrypt = utf8.GetBytes(message);

            try
            {
                ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }
            return Convert.ToBase64String(results);
        }

        public string DecryptString(string message)
        {
            byte[] results;
            UTF8Encoding utf8 = new UTF8Encoding();

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(_passphrase));

            TripleDESCryptoServiceProvider tdesAlgorithm = new TripleDESCryptoServiceProvider();

            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            byte[] dataToDecrypt = Convert.FromBase64String(message);

            try
            {
                ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            return utf8.GetString(results);
        }
        public string GenerateCode()
        {
            const string numbers = "1234567890";

            const string characters = numbers;

            const int length = 6;
            var otp = string.Empty;
            for (var i = 0; i < length; i++)
            {
                string character;
                do
                {
                    var index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character, StringComparison.Ordinal) != -1);
                otp += character;
            }
            return otp;
        }
        public string GeneratePassword()
        {
            const string numbers = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            const string characters = numbers;

            const int length = 6;
            var otp = string.Empty;
            for (var i = 0; i < length; i++)
            {
                string character;
                do
                {
                    var index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character, StringComparison.Ordinal) != -1);
                otp += character;
            }
            return otp;
        }
    }
}
