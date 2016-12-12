using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace BA.Framework.IMLib.TransferEncrypt
{
    public class CommonCryptoService
    {

        private static byte[] IV_KEY = new byte[] { 18, 52, 86, 120, 144, 171, 205, 239 };

        public static string[] chars = new string[]
{
"A",
"B",
"C",
"D",
"E",
"F",
"G",
"H",
"I",
"J",
"K",
"L",
"M",
"N",
"O",
"P",
"Q",
"R",
"S",
"T",
"U",
"V",
"W",
"X",
"Y",
"Z",
"0",
"1",
"2",
"3",
"4",
"5",
"6",
"7",
"8",
"9",
"a",
"b",
"c",
"d",
"e",
"f",
"g",
"h",
"i",
"j",
"k",
"l",
"m",
"n",
"o",
"p",
"q",
"r",
"s",
"t",
"u",
"v",
"w",
"x",
"y",
"z",
"+",
"="
};


        public static string Encrypt_DES(string str, string key)
        {
            string result;
            using (DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider())
            {
                key = getKey(key);
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                dESCryptoServiceProvider.Key = Encoding.ASCII.GetBytes(key.Substring(0, 8));
                dESCryptoServiceProvider.IV = CommonCryptoService.IV_KEY;
                dESCryptoServiceProvider.Mode = CipherMode.ECB;
                dESCryptoServiceProvider.Padding = PaddingMode.Zeros;
                MemoryStream memoryStream = new MemoryStream();
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cryptoStream.Close();
                }
                string text = Convert.ToBase64String(memoryStream.ToArray());
                memoryStream.Close();
                result = text;
            }
            return result;
        }

        public static string Decrypt_DES(string str, string key)
        {
            byte[] array = Convert.FromBase64String(str);
            string result;
            using (DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider())
            {
                key = getKey(key);
                dESCryptoServiceProvider.Key = Encoding.ASCII.GetBytes(key.Substring(0, 8));
                dESCryptoServiceProvider.IV = CommonCryptoService.IV_KEY;
                dESCryptoServiceProvider.Mode = CipherMode.ECB;
                dESCryptoServiceProvider.Padding = PaddingMode.Zeros;
                MemoryStream memoryStream = new MemoryStream();
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(array, 0, array.Length);
                    cryptoStream.FlushFinalBlock();
                    cryptoStream.Close();
                }
                string text = Encoding.UTF8.GetString(memoryStream.ToArray());
                string arg_A7_0 = text;
                char[] trimChars = new char[1];
                text = arg_A7_0.TrimEnd(trimChars);
                string @string = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text));
                memoryStream.Close();
                result = @string;
            }
            return result;
        }

        public static string getKey(string sKey)
        {
            string text = sKey + getStringHashCode(sKey);
            text = Create(text, KeyFormat.None);
            StringBuilder stringBuilder = new StringBuilder();
            int num = chars.Length;
            for (int i = 0; i < 8; i++)
            {
                string value = text.Substring(i * 4, 4);
                int num2 = Convert.ToInt32(value, 16);
                stringBuilder.Append(chars[num2 % num]);
            }
            return stringBuilder.ToString();
        }
        private static int getStringHashCode(string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            int num = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                num += getHashCode(bytes[i]);
            }
            return num;
        }

        private static int getHashCode(byte b)
        {
            return (int)b << 2 ^ 37;
        }
        public static string Create(string key, KeyFormat formater = KeyFormat.ToUpper)
        {
            string result;
            switch (formater)
            {
                case KeyFormat.None:
                    result = FormsAuthentication.HashPasswordForStoringInConfigFile(key, "MD5");
                    break;
                case KeyFormat.ToUpper:
                    result = FormsAuthentication.HashPasswordForStoringInConfigFile(key.ToUpper(), "MD5");
                    break;
                case KeyFormat.ToLower:
                    result = FormsAuthentication.HashPasswordForStoringInConfigFile(key.ToLower(), "MD5");
                    break;
                default:
                    result = null;
                    break;
            }
            return result;
        }
        public enum KeyFormat
        {
            None,
            ToUpper,
            ToLower
        }
    }
}
