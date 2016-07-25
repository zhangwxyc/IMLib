using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    /// <summary>
    /// 即时消息类型
    /// </summary>
    public enum MessageType
    {
        //命令消息
        Ack,
        Connect,
        Disconnect,
        Ping,
        //应答消息
        //文本
        Text,
        Image,
        Voice,
        Video,
        File,
        Invite,
        Join,
        Leave,
        Transfer,
        //Location,
        //Vcard,
        Link,
        Custom,
        UnKnown
    }
}
