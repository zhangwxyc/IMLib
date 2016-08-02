using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    /// <summary>
    /// OnError错误事件参数
    /// </summary>
    public class ErrorEventArgs
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 处理类型暂不使用
        /// </summary>
        public FileProcessType ProcessType { get; set; }
        /// <summary>
        /// 异常实体
        /// </summary>
        public Exception ExceptionInfo { get; set; }
    }
}
