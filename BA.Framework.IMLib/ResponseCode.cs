using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public enum ResponseCode
    {
        OK = 200,
        BAD_REQUEST = 400,          //无效的请求
        NO_AUTH = 401,              //未登陆
        NO_PERMISSION = 402,        //无操作权限
        ACCESS_DENIED = 403,        //禁止访问 	请求被服务器拒绝
        USER_NOT_FOUND = 40401,     //目标用户不存在 	
        GROUP_NOT_FOUND = 40402,    //目标群组不存在 	
        TIMEOUT = 408,              //请求超时 	
        SERVER_ERR = 500,           //服务器内部错误 	
        SERVICE_UNAVAILABLE = 503   //服务不可用
    }
}
