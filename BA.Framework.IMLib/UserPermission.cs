using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public enum UserPermission
    {
        /// <summary>
        /// 发送文本消息 
        /// </summary>
        SEND_TEXT = 1,
        /// <summary>
        /// 发送图片消息 
        /// </summary>
        SEND_IMAGE = 2,
        /// <summary>
        /// 发送语音消息
        /// </summary>
        SEND_VOICE = 4,
        /// <summary>
        /// 发送视频消息
        /// </summary>
        SEND_VIDEO = 8,
        /// <summary>
        /// 发送文件消息
        /// </summary>
        SEND_FILE = 16,
        /// <summary>
        /// 发送链接消息
        /// </summary>
        SEND_LINK = 32,
        /// <summary>
        /// 发送自定义内容消息
        /// </summary>
        SEND_CUSTOM = 64,
        /// <summary>
        /// 邀请某人加入群组对话
        /// </summary>
        INVITE = 128,
        /// <summary>
        /// 加入群组
        /// </summary>
        JOIN = 256,
        /// <summary>
        /// 将会话转接给某人
        /// </summary>
        TRANSFER = 512,
        /// <summary>
        /// 取消
        /// </summary>
        UNDO = 1024,
        /// <summary>
        /// 取消
        /// </summary>
        NO_PERMISSION = 2048
    }
}
