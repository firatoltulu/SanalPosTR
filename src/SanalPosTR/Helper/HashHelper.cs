using System;
using System.Security.Cryptography;
using System.Text;

namespace SanalPosTR
{
    public class HashHelper
    {
        public HashHelper()
        {
        }

        public enum HashType : int
        {
            MD5,
            SHA1,
            SHA256,
            SHA512
        }

        public static string GetHash(string text, HashType hashType)
        {
            string hashString;
            switch (hashType)
            {
                case HashType.MD5:
                    hashString = GetMD5(text);
                    break;

                case HashType.SHA1:
                    hashString = GetSHA1(text);
                    break;

                case HashType.SHA256:
                    hashString = GetSHA256(text);
                    break;

                case HashType.SHA512:
                    hashString = GetSHA512(text);
                    break;

                default:
                    hashString = "Invalid Hash Type";
                    break;
            }
            return hashString;
        }

        public static bool CheckHash(string original, string hashString, HashType hashType)
        {
            string originalHash = GetHash(original, hashType);
            return (originalHash == hashString);
        }

        public static string GetMD5(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            MD5 hashString = new MD5CryptoServiceProvider();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        private static byte[] GetSHA1Bytes(string text)
        {
            byte[] message = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(text);
            SHA1 sha = new SHA1CryptoServiceProvider();
            var hashValue = sha.ComputeHash(message);
            return hashValue;
        }

        public static string GetSHA1(string text)
        {
            return Convert.ToBase64String(GetSHA1Bytes(text));
        }

        public static string GetSHA1WithHexaDecimal(string text)
        {
            return GetHexaDecimal(GetSHA1Bytes(text));
        }

        public static string GetSHA1WithUTF8(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);

            SHA1 sha = new SHA1CryptoServiceProvider();
            var hashValue = sha.ComputeHash(message);
            return Convert.ToBase64String(hashValue);
        }

        private static string GetHexaDecimal(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();
            int length = bytes.Length;
            for (int n = 0; n <= length - 1; n++)
            {
                s.Append(String.Format("{0,2:x}", bytes[n]).Replace(" ", "0"));
            }
            return s.ToString();
        }

        public static string GetSHA256(string originalString)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(originalString));
                return Convert.ToBase64String(bytes);
            }
        }

        public static string GetSHA512(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            SHA512Managed hashString = new SHA512Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }
    }
}