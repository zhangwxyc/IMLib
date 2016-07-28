using BA.Framework.IMLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoForCsharpIMLib
{
    public class Logger:ILogger
    {
        public void Log(string description, Exception ex)
        {
            File.AppendAllText(Path.Combine(Environment.CurrentDirectory,"IMsdkLog.txt"),string.Format("{0}\r\n{1}\r\n{2}\r\n",description,ex==null?"":ex.Message,ex==null?"":ex.StackTrace));
        }
    }
}
