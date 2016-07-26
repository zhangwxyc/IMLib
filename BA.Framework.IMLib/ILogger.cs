using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public interface ILogger
    {
        void Log(string description, Exception ex);
    }
}
