using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    //处理发送和应答的上下文
    public class MessageContext
    {
        public RequestInfo IMRequest { get; set; }
        public ResponseAckInfo IMResponse { get; set; }
    }
}
