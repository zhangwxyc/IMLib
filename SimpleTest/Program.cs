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
            IMServer server = new IMServer();
            bool isConnected = server.Connect("192.168.87.114", 8282, "new", "star", "pc", Guid.NewGuid().ToString(), Connected);
            server.OnReceive += server_OnReceive;
            if (isConnected)
            {
                
                while (true)
                {
                    string sendTxt=Console.ReadLine();
                    if (sendTxt=="q")
                    {
                        server.Disconnect();
                        break;
                    }
                    server.SendText("star", "111", sendTxt, SendCallback);
                }
            }
        }

        static void server_OnReceive(MessageType arg1, string arg2, string arg3, string arg4, int arg5, object arg6)
        {
            Console.WriteLine("server_OnReceive:{0}", arg4);
        }

        public static void SendCallback(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
             
        }

        public static void Connected(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            Console.WriteLine("Connected:{0}", ackInfo.MessageId);
        }
    }
}
