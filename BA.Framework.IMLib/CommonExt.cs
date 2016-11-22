using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public static class CommonExt
    {
        public static T ToObject<T>(this byte[] data)
        // where T : class
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data.ToJsonString());
        }
        public static T ToObject<T>(this string data)
        // where T : class
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }
        public static byte[] ToByte<T>(this T contentInfo)
        //  where T : class
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(contentInfo).ToByte();
        }

        public static string ToJsonString(this byte[] content)
        {
            return Encoding.UTF8.GetString(content);
        }

        public static string ToJsonString(this object contentInfo)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(contentInfo);
        }
        public static string DynamicToJsonString(dynamic contentInfo)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(contentInfo);
        }
        public static dynamic JsonStringToDynamic(string contentInfo)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(contentInfo);
        }

        public static byte[] ToByte(this string content)
        {
            return Encoding.UTF8.GetBytes(content+"\0");
        }
        public static byte[] ToByte_S(this string content)
        {
            return Encoding.UTF8.GetBytes(content);
        }
        public static string GetMD5HashFromFile(this string fileName)
        {
            using(FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                //file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
