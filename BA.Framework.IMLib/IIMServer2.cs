﻿using BA.Framework.IMLib.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    interface IIMServer2
    {
        bool Connect(string host, int port, string userType, string userName, string userAgent, string accessToken, Action<RequestInfo, ResponseAckInfo> callback);

        bool Send(MessageType type, string to, string group, object data, Action<RequestInfo, ResponseAckInfo> callback);

        event Action<MessageType, string, string, string, int, object> OnReceive;
    }
}
