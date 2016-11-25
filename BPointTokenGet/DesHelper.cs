using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BPointTokenGet
{
    public class DesHelper
    {
        /// <summary>
        /// 密钥
        /// </summary>
        private static readonly string Des3Key = "CBXYhS6EL8yMUoprdtZePG72D3iKzR9q";

        /// <summary>
        /// 矢量,矢量可以为空
        /// </summary>
        private static readonly string Des3IV = "rAD2gyhZpfS=";

        /// <summary>
        /// DES加密16位
        /// </summary>
        /// <param name="inputString">加密字符串</param>
        /// <param name="key">加密Key</param>
        /// <returns>返回结果</returns>
        public static string DesEncrypt16(string inputString, string key)
        {
            StringBuilder strRetValue = new StringBuilder();
            string result;
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                byte[] keyIV = keyBytes;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(inputString);
                using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
                {
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        using (CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(keyBytes, keyIV), CryptoStreamMode.Write))
                        {
                            cStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cStream.FlushFinalBlock();
                            byte[] array = mStream.ToArray();
                            for (int i = 0; i < array.Length; i++)
                            {
                                byte b = array[i];
                                strRetValue.AppendFormat("{0:x2}", b);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                result = strRetValue.ToString();
                return result;
            }
            result = strRetValue.ToString();
            return result;
        }

        /// <summary>
        /// 3DES 加密
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>加密数据</returns>
        public static string DES3Encrypt(string data, string key = "")
        {
            bool flag = string.IsNullOrEmpty(key);
            if (flag)
            {
                key = DesHelper.Des3Key;
            }
            string result;
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
            {
                des.Key = Convert.FromBase64String(key);
                des.IV = Convert.FromBase64String(DesHelper.Des3IV);
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform desEncrypt = des.CreateEncryptor(des.Key, des.IV))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(data);
                    result = Convert.ToBase64String(desEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));
                }
            }
            return result;
        }

        /// <summary>
        /// 3DES 解密
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>解密数据</returns>
        public static string DES3Decrypt(string data, string key = "")
        {
            bool flag = string.IsNullOrEmpty(key);
            if (flag)
            {
                key = DesHelper.Des3Key;
            }
            string result2;
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
            {
                des.Key = Convert.FromBase64String(key);
                des.IV = Convert.FromBase64String(DesHelper.Des3IV);
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform desDecrypt = des.CreateDecryptor(des.Key, des.IV))
                {
                    string result = "";
                    try
                    {
                        byte[] buffer = Convert.FromBase64String(data);
                        result = Encoding.UTF8.GetString(desDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
                    }
                    catch (Exception)
                    {
                        result2 = data;
                        return result2;
                    }
                    result2 = result;
                }
            }
            return result2;
        }

        /// <summary>
        /// 解密获取使用3DES加密的文件数据
        /// </summary>
        /// <param name="path">加密文件</param>
        /// <param name="key">密钥</param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>解密数据</returns>
        public static T GetDataWithDes3<T>(string path, string key = "")
        {
            bool flag = !File.Exists(path);
            T result2;
            if (flag)
            {
                result2 = default(T);
            }
            else
            {
                bool flag2 = string.IsNullOrEmpty(key);
                if (flag2)
                {
                    key = DesHelper.Des3Key;
                }
                string des3Data = File.ReadAllText(path, Encoding.UTF8);
                string data = DesHelper.DES3Decrypt(des3Data, key);
                T result = JsonConvert.DeserializeObject<T>(data);
                result2 = result;
            }
            return result2;
        }

        /// <summary>
        /// 使用3DES加密保存文件数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="path">文件</param>
        /// <param name="key">密钥</param>
        /// <typeparam name="T">数据类型</typeparam>
        public static void SaveDataWithDes3<T>(T data, string path, string key = "")
        {
            bool flag = string.IsNullOrEmpty(key);
            if (flag)
            {
                key = DesHelper.Des3Key;
            }
            string contents = JsonConvert.SerializeObject(data);
            string des3Data = DesHelper.DES3Encrypt(contents, key);
            File.WriteAllText(path, des3Data, Encoding.UTF8);
        }
    }
}
