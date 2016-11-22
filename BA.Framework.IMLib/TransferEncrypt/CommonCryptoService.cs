using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace BA.Framework.IMLib.TransferEncrypt
{
    internal class CommonCryptoService
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
    }
}
