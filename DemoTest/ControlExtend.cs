using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoTest
{
    public static class ControlExtend
    {
        public static void CrossThreadCalls(this Control ctl, Action del)
        {
            ctl.FindForm().BeginInvoke(new Action(del));
        }
    }
}
