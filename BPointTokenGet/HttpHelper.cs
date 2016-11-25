using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BPointTokenGet
{
    public class HttpHelper
    {
        public static string HttpGet(string url, string postDataStr = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + ((postDataStr == "") ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "application/json;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string HttpPost(string url, string postDataStr ,Dictionary<string,string> headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            foreach (var item in headers)
            {
                request.Headers.Add(item.Key, item.Value);
            }
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            Encoding encoding = Encoding.UTF8;
            bool flag = !string.IsNullOrEmpty(postDataStr);
            if (flag)
            {
                byte[] buffer = encoding.GetBytes(postDataStr);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
    }
}
