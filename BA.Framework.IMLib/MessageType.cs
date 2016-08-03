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
        /// <summary>
        /// 纯文本消息，一般用于普通消息的发送
        /// </summary>
        Text,
        /// <summary>
        /// 图片消息 
        /// </summary>
        Image,
        /// <summary>
        /// 语音消息
        /// </summary>
        Voice,
        /// <summary>
        /// 视频消息
        /// </summary>
        Video,
        /// <summary>
        /// 用于构建发送文件的消息
        /// </summary>
        File,
        Invite,
        Join,
        Leave,
        undo,
        Transfer,
        //Location,
        //Vcard,
        Link,
        Custom,
        UnKnown
    }
}
