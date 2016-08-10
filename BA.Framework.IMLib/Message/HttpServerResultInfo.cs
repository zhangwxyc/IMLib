using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    public class HttpServerResultInfo
    {
        [JsonProperty("statcode")]
        public int StatCode { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
