using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    /// <summary>
    /// 基础消息
    /// </summary>
    public class BaseMessageInfo
    {
        //[JsonConverter(typeof(StringEnumConverter))]
        /// <summary>
        /// 消息类型
        /// </summary>
        [JsonConverter(typeof(CustomEnumJsonConverter))]
        [JsonProperty("type")]
        public MessageType MsgType { get; set; }
        /// <summary>
        /// 消息ID
        /// </summary>
        [JsonProperty("msg_id")]
        public string MessageId { get; set; }
        /// <summary>
        /// 消息数据内容
        /// </summary>
        [JsonProperty("data")]
        public dynamic Data { get; set; }
        public BaseMessageInfo()
        {
            MessageId = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 文件类消息包含此属性
        /// </summary>
        [JsonIgnore]
        public FileMessageInfo RelateFileInfo { get; set; }
    }
}
