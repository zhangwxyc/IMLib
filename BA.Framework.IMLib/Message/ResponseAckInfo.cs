using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    public class ResponseAckInfo:BaseMessageInfo
    {
        [JsonProperty("status")]
        public ResponseCode Status { get; set; }
    }
}
