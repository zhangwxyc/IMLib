using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BPointTokenGet
{
    public class ParasAuthentication
    {
        public static string CreateSign(IDictionary<string, string> parameters, string secret)
        {
            parameters.Remove("sign");
            IDictionary<string, string> dictionary = new SortedDictionary<string, string>(parameters);
            IEnumerator<KeyValuePair<string, string>> enumerator = dictionary.GetEnumerator();
            StringBuilder stringBuilder = new StringBuilder(secret);
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, string> current = enumerator.Current;
                string key = current.Key;
                current = enumerator.Current;
                string value = current.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    stringBuilder.Append(key).Append(value);
                }
            }
            stringBuilder.Append(secret);
            MD5 mD = MD5.Create();
            byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            StringBuilder stringBuilder2 = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                string text = array[i].ToString("X");
                if (text.Length == 1)
                {
                    stringBuilder2.Append("0");
                }
                stringBuilder2.Append(text);
            }
            return stringBuilder2.ToString();
        }

        public static long ToUnixTime(DateTime dt)
        {
            return (dt.ToUniversalTime().Ticks - 621355968000000000L) / 10000000L;
        }

        public static DateTime FromUnixTime(long timeSpan)
        {
            return new DateTime((timeSpan + 28800L) * 10000000L + 621355968000000000L);
        }
    }
}
