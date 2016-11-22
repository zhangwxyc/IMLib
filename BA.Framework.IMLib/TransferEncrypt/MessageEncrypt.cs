using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.TransferEncrypt
{
    public class MessageEncrypt : IMessageEncrypt
    {
        public string Encode(string data, Message.BaseMessageInfo info)
        {
            if (string.IsNullOrWhiteSpace(data)||string.IsNullOrWhiteSpace(info.MessageId))
            {
                return data;
            }
            return CommonCryptoService.Encrypt_DES(CommonExt.DynamicToJsonString(info.Data), info.MessageId);
        }

        public string Decode(string data, Message.BaseMessageInfo info)
        {
            if (string.IsNullOrWhiteSpace(data)||string.IsNullOrWhiteSpace(info.MessageId))
            {
                return data;
            }
            dynamic deString = CommonCryptoService.Decrypt_DES(data, info.MessageId);
            return CommonExt.JsonStringToDynamic(deString.ToString());
        }
    }
}
