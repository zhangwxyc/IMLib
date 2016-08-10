using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    public class ResponseInfo:BaseMessageInfo
    {
        [JsonProperty("from")]
        public string FromId { get; set; }
        [JsonProperty("from_name")]
        public string FromName { get; set; }
        [JsonProperty("group")]
        public string GroupId { get; set; }
        [JsonProperty("msg_time")]
        public int MsgTime { get; set; }

    }
}
