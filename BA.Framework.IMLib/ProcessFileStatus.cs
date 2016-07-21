using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public enum ProcessFileStatus
    {
        // 0未开始，1进行中，2完成，3用户终止，4服务端异常

        Ready=0,
        Running,
        Finished,
        Suspend,
        Abort
    }
}
