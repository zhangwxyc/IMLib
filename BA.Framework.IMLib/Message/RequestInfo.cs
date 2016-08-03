using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    /// <summary>
    /// 上行消息
    /// </summary>
    public class RequestInfo : BaseMessageInfo
    {
        /// <summary>
        /// 目标用户ID
        /// </summary>
        [JsonProperty("to")]
        public string ToId { get; set; }
        /// <summary>
        /// 目标组ID
        /// </summary>
        [JsonProperty("group")]
        public string GroupId { get; set; }

        [JsonIgnore]
        public Action<RequestInfo, ResponseAckInfo> Callback { get; set; }

    }
}
