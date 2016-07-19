using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public class ErrorEventArgs
    {
        public string MsgId { get; set; }

        public FileProcessType ProcessType { get; set; }
        public Exception ExceptionInfo { get; set; }
    }
}
