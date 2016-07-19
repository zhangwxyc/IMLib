using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public class IMEventArgs : EventArgs
    {
        //public string MessageId { get; set; }
        //public string ResponseType { get; set; }
        public Message.ResponseInfo _ResponseInfo { get; set; }
    }
}
