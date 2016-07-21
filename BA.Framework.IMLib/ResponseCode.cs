using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public enum ResponseCode
    {
        /// <summary>
        /// 客户端执行错误
        /// </summary>
        CLINET_ERR = 103,
        /// <summary>
        /// 正常
        /// </summary>
        OK = 200,
        /// <summary>
        /// 无效的请求
        /// </summary>
        BAD_REQUEST = 400,
        /// <summary>
        /// 未登陆
        /// </summary>
        NO_AUTH = 401,
        /// <summary>
        /// 无操作权限
        /// </summary>
        NO_PERMISSION = 402,
        /// <summary>
        /// 禁止访问 	请求被服务器拒绝
        /// </summary>
        ACCESS_DENIED = 403,
        /// <summary>
        /// 目标用户不存在
        /// </summary>
        USER_NOT_FOUND = 40401,
        /// <summary>
        /// 目标群组不存在
        /// </summary>
        GROUP_NOT_FOUND = 40402,
        /// <summary>
        /// 请求超时
        /// </summary>
        TIMEOUT = 408,
        /// <summary>
        /// 服务器内部错误
        /// </summary>
        SERVER_ERR = 500,
        /// <summary>
        /// 服务不可用
        /// </summary>
        SERVICE_UNAVAILABLE = 503
    }
}
