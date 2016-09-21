using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BA.Framework.IMLib;
using BA.Framework.IMLib.Message;
using System.Threading;
using System.IO;
namespace SimpleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //DynamicTest();
          // byte[] aa= Encoding.UTF8.GetBytes("\0");

            //TestFileUpload();
            //IMTest();
            // List<UserPermission> list= BA.Framework.IMLib.Permission.GetUserPermission("3");
            // ImageThumbnail.Thumbnail.MakeThumbnailByRate(@"E:\文件测试\2.gif", 0.5);
            string testData = "烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病 烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病 烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病 烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病 烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生虫病烦恼寄生";
            byte[] datas = Encoding.UTF8.GetBytes(testData);

            List<byte[]> items = new List<byte[]>();
            List<string> si = new List<string>();
            int len = 1024;
            for (int index = 0; index < datas.Length; index+=len)
            {
                int leftLen=datas.Length-index;
                int nextLen=leftLen>len?len:leftLen;
                byte[] curData=new byte[nextLen];
                Array.Copy(datas, index, curData, 0, nextLen);
                items.Add(curData);
                si.Add(Encoding.UTF8.GetString(curData));

                Console.WriteLine(Encoding.UTF8.GetChars(curData));
            }

            List<byte[]> items1 = new List<byte[]>();
            foreach (var item in si)
            {
                items1.Add(Encoding.UTF8.GetBytes(item));
            }


        }

        private static void TestFileUpload()
        {
            WebClientV2 wb = new WebClientV2();
            string aa = Console.ReadLine();
            wb.UploadProgressChanged += wb_UploadProgressChanged;
            wb.UploadFileCompleted += wb_UploadFileCompleted;
            while (aa != "q")
            {
                FileInfo info = new FileInfo(aa);
                Console.WriteLine("Size:" + info.Length);
                wb.UploadFileAsync(new Uri("http://localhost:46916/upload"), aa);
                Thread.Sleep(5000);
                aa = Console.ReadLine();
            }
        }

        private static void DynamicTest()
        {
            List<dynamic> cc = new List<dynamic>();
            cc.Add(1);
            cc.Add("123");
            cc.Add(new { a = 1, b = "123", c = new object() });

            foreach (var data in cc)
            {
                Dictionary<string, object> ss = new Dictionary<string, object>();

                if (data != null)
                {
                    Type dType = data.GetType();
                    if (dType.IsClass && dType.Name != "String")
                    {
                        var propArray = dType.GetProperties();
                        foreach (var propItem in propArray)
                        {
                            ss.Add(propItem.Name, propItem.GetValue(data));
                        }
                    }
                    else
                        ss.Add("data", data);
                }

            }
        }

        static void wb_UploadFileCompleted(object sender, System.Net.UploadFileCompletedEventArgs e)
        {
            // Console.WriteLine("ok" + e.Error != null ? e.Error.Message : "");
        }

        static void wb_UploadProgressChanged(object sender, System.Net.UploadProgressChangedEventArgs e)
        {
            Console.WriteLine("{0} / {1}", e.BytesSent, e.TotalBytesToSend);
        }

        private static void IMTest()
        {
            IMServer server = new IMServer();
            bool isConnected = server.Connect("192.168.87.21", 8282, "new", "star", "pc", Guid.NewGuid().ToString(), 1, Connected);
            server.OnReceive += server_OnReceive;
            server.OnUpload += server_OnUpload;
            //server.OnError += server_OnError;
            server.OnDownload += server_OnDownload;

            if (isConnected)
            {

                while (true)
                {
                    string sendTxt = Console.ReadLine();
                    if (sendTxt == "q")
                    {
                        server.Disconnect();
                        break;
                    }
                    if (sendTxt == "file_test1")
                    {
                        server.SendFile(MessageType.Image, "star", "", @"C:\Users\Public\Pictures\Sample Pictures\1.jpg", SendCallback);
                    }
                    if (sendTxt == "file_test2")
                    {
                        server.SendFile(MessageType.Image, "star", "", @"C:\Users\Public\Pictures\Sample Pictures\1.jpg", null);
                    }
                    else if (sendTxt == "down")
                    {
                        //server.Download_BreakPoint("http://oa.bitauto.com/ImportFiles/UpLoad/0/0/0/929273d4-6ae5-47ae-9478-4ca4735d0a8a.xls", "C:\\123.xls");
                    }
                    else

                        server.SendText("star", "111", sendTxt, SendCallback);
                }
            }
        }

        static void server_OnDownload(string arg1, long arg2, long arg3)
        {
            Console.WriteLine("server_OnDownload:{0},{1}/{2}", arg1, arg2, arg3);
        }

        //static void server_OnError(object arg1, ErrorEventArgs arg2)
        //{
        //    Console.WriteLine("server_OnError:{0}", arg2.ExceptionInfo.Message);
        //}

        static void server_OnUpload(string arg1, long arg2, long arg3)
        {
            Console.WriteLine("server_OnUpload:{0},{1}/{2}", arg1, arg2, arg3);
        }

        static void server_OnReceive(MessageType arg1, string arg2, string arg3, string arg4, int arg5, object arg6)
        {
            Console.WriteLine("server_OnReceive:{0}", arg4);
        }

        public static void SendCallback(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            Console.WriteLine("SendCallback:{0}", ackInfo.MessageId);
        }

        public static void Connected(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            int a = ackInfo.Data["11"];
            Console.WriteLine("Connected:{0}", ackInfo.MessageId);
        }
    }
}
