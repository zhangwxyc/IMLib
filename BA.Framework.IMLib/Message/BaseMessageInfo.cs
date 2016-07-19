using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    public class BaseMessageInfo
    {
        [JsonProperty("type")]
        public MessageType MsgType { get; set; }
        [JsonProperty("msg_id")]
        public string MessageId { get; set; }
        [JsonProperty("data")]
        public dynamic Data { get; set; }
    }
}
