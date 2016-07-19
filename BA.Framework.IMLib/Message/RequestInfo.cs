using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    public class RequestInfo : BaseMessageInfo
    {
        [JsonProperty("to")]
        public string ToId { get; set; }
        [JsonProperty("group")]
        public string GroupId { get; set; }

        [JsonIgnore]
        public Action<RequestInfo, ResponseAckInfo> Callback { get; set; }

    }
}
