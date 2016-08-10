using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace BA.Framework.IMLib.Message
{
    public class FileMessageInfo
    {
        public string MessageId { get; set; }

        /// <summary>
        /// 处理类型
        /// </summary>
        public FileProcessType ProcessType { get; set; }

        /// <summary>
        /// 状态
        /// 0未开始，1进行中，2完成，3用户终止，4服务端异常
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 相关client
        /// </summary>
        public WebClientV2 Client { get; set; }

        public static FileMessageInfo Create(string serverHttpUrl)
        {
            var webClient = new WebClientV2();
            webClient.BaseAddress = serverHttpUrl;
            webClient.Credentials = CredentialCache.DefaultCredentials;
            return new FileMessageInfo()
            {
                Client = webClient,
                Status = 0
            };
        }

        /// <summary>
        /// 文件本地路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 上传或下载断点位置
        /// </summary>
        public int StartPos { get; set; }

        /// <summary>
        /// 发送数据总大小
        /// </summary>
        public int TotalFileLength { get; set; }
    }
}
