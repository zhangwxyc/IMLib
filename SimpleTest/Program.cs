using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BA.Framework.IMLib;
using BA.Framework.IMLib.Message;
namespace SimpleTest
{
    class Program
    {
        static void Main(string[] args)
        {
           // IMTest();
           List<UserPermission> list= BA.Framework.IMLib.Permission.GetUserPermission("3");
            //ImageThumbnail.Thumbnail.MakeThumbnailByRate(@"C:\Users\Public\Pictures\Sample Pictures\1.jpg", 0.2,"C:\\","C:\\123.jpg");
        }

        private static void IMTest()
        {
            IMServer server = new IMServer();
            bool isConnected = server.Connect("192.168.87.114", 8282, "new", "star", "pc", Guid.NewGuid().ToString(),1, Connected);
            server.OnReceive += server_OnReceive;
            server.OnUpload += server_OnUpload;
            server.OnError += server_OnError;
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
                        server.SendFile(MessageType.Image, "star", "", @"C:\Users\Public\Pictures\Sample Pictures\1.jpg",null);
                    }
                    else if (sendTxt == "down")
                    {
                        server.Download_BreakPoint("http://oa.bitauto.com/ImportFiles/UpLoad/0/0/0/929273d4-6ae5-47ae-9478-4ca4735d0a8a.xls", "C:\\123.xls");
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

        static void server_OnError(object arg1, ErrorEventArgs arg2)
        {
            Console.WriteLine("server_OnError:{0}", arg2.ExceptionInfo.Message);
        }

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
            Console.WriteLine("Connected:{0}", ackInfo.MessageId);
        }
    }
}
