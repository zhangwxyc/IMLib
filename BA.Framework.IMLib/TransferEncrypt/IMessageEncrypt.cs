using BA.Framework.IMLib.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.TransferEncrypt
{
    public interface IMessageEncrypt
    {
        string Encode(string data,BaseMessageInfo info);
        string Decode(string data,BaseMessageInfo info);
    }
}
