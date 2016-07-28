using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoForCsharpIMLib
{
    public static class ControlExtend
    {
        //public static void CrossThreadCalls(this Control ctl, ThreadStart del)
        //{
        //    if (del == null) return;
        //    if (ctl.InvokeRequired)
        //        ctl.Invoke(del, null);
        //    else
        //        del();
        //}
        public static void CrossThreadCalls(this Control ctl, Action del)
        {
            ctl.FindForm().BeginInvoke(new Action(del));
        }
    }
}
