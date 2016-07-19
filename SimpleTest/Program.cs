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
            IMServer server = new IMServer("192.168.87.114", 8282);
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

        public static void SendCallback(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            Console.WriteLine("SendCallback:{0}", ackInfo.MessageId);
        }

        static void server_OnReceive(object arg1, IMEventArgs arg2)
        {
            Console.WriteLine("server_OnReceive:{0}", arg2._ResponseInfo.MessageId);
        }

        public static void Connected(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            Console.WriteLine("Connected:{0}", ackInfo.MessageId);
        }
    }
}
