using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    /// <summary>
    /// 应答消息
    /// </summary>
    public class ResponseAckInfo:BaseMessageInfo
    {
        /// <summary>
        /// 服务器应答状态码
        /// </summary>
        [JsonProperty("status")]
        public ResponseCode Status { get; set; }

        [JsonProperty("msg_time")]
        public int MsgTime { get; set; }
    }
}
