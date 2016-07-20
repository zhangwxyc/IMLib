using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    public class BaseMessageInfo
    {
        //[JsonConverter(typeof(StringEnumConverter))]
        [JsonConverter(typeof(CustomEnumJsonConverter))]
        [JsonProperty("type")]
        public MessageType MsgType { get; set; }
        [JsonProperty("msg_id")]
        public string MessageId { get; set; }
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
