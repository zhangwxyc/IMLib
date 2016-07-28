using BA.Framework.IMLib.Message;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public class IMServer : IIMServer
    {
        /// <summary>
        /// 接收用户消息事件
        /// </summary>
        public event Action<MessageType, string, string, string, int, object> OnReceive;
        public event Action<string, long, long> OnUpload;
        public event Action<string, long, long> OnDownload;
        public event Action<object, ErrorEventArgs> OnError;
        public event Action OnDisconnect;
        public event Action OnReConnected;

        private Socket m_Client;

        private DateTime m_LastPingTime;

        private ILogger _logger;

        /// <summary>
        /// 需要输出日志时设置
        /// </summary>
        public ILogger Log
        {
            set { _logger = value; }
        }

        /// <summary>
        /// 当前用户验证信息
        /// </summary>
        private UserIdentity m_User;

        private string m_IpAddress;
        private int m_Port { get; set; }

        /// <summary>
        /// 当前连接是否可用
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                if (m_Client != null && m_Client.Connected && m_User.IsAuthenticated)
                {
                    //被动心跳检测
                    if (m_LastPingTime.AddSeconds(HeartTimeOut) < DateTime.Now)
                    {
                        Disconnect();
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        private ConcurrentBag<RequestInfo> m_SendBuffer;

        private ConcurrentQueue<string> m_ReceiveBuffer;

        /// <summary>
        /// 上传下载任务队列
        /// </summary>
        private ConcurrentBag<FileMessageInfo> m_FileMsgQueue;

        string m_syncObject = "";

        /// <summary>
        /// socket缓存区大小,1024字节
        /// </summary>
        private const int BufferSize = 1024;

        /// <summary>
        /// 心跳检测超时时间，30s
        /// </summary>
        private const int HeartTimeOut = 300;

        /// <summary>
        /// 断线重连间隔,5s
        /// </summary>
        private const int ConnectRetryInnerTime = 5;

        /// <summary>
        /// 断线重连次数，默认5次
        /// </summary>
        private int _connectRetryTimes = 5;

        /// <summary>
        /// 断线重连次数，可设置
        /// </summary>
        public int ConnectRetryTimes
        {
            get { return _connectRetryTimes; }
            set { _connectRetryTimes = value; }
        }


        public IMServer()
        {
            m_User = new UserIdentity();
            m_SendBuffer = new ConcurrentBag<RequestInfo>();
            m_ReceiveBuffer = new ConcurrentQueue<string>();
            m_FileMsgQueue = new ConcurrentBag<FileMessageInfo>();
        }



        #region 连接
        /// <summary>
        /// 打开连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userType"></param>
        /// <param name="userName"></param>
        /// <param name="userAgent"></param>
        /// <param name="accessToken"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool Connect(string host, int port, string userType, string userName, string userAgent, string accessToken, Action<RequestInfo, ResponseAckInfo> callback)
        {
            var requestInfo = new Message.RequestInfo()
            {
                MsgType = MessageType.Connect,
                MessageId = TimeStamp.Create(),
                Data = new
                {
                    userType = userType,
                    userName = userName,
                    userAgent = userAgent,
                    accessToken = accessToken
                }
                // ,Callback = callback
            };

            if (IsAvailable)
            {
                //已经连接成功
                if (callback != null)
                {
                    callback(requestInfo, new ResponseAckInfo() { MsgType = MessageType.Ack, Status = ResponseCode.OK });
                }
                return true;
            }
            bool result = false;
            //开始连接
            try
            {
                m_Client = new Socket(SocketType.Stream, ProtocolType.Tcp);

                m_Client.Connect(host, port);
                m_IpAddress = m_IpAddress;
                m_Port = port;
                if (m_Client.Connected)
                {
                    byte[] sendData = requestInfo.ToByte<Message.RequestInfo>();
                    m_Client.Send(sendData);
                    byte[] bufferData = new byte[BufferSize];
                    int receiveLen = m_Client.Receive(bufferData);
                    byte[] receivedData = bufferData.ToList().GetRange(0, receiveLen).ToArray();
                    var responseInfo = receivedData.ToObject<Message.ResponseAckInfo>();
                    if (responseInfo.Status == ResponseCode.OK)
                    {
                        m_User.IsAuthenticated = true;
                        m_User.Name = userName;
                        m_User.Token = accessToken;
                        m_User.UserAgent = userAgent;
                        m_User.UserType = userType;

                        //连接使用同步
                        // _sendBuffer.Add(requestInfo);
                        m_LastPingTime = DateTime.Now;

                        //启动线程接收
                        AfterConnectedServer();
                        result = true;
                    }
                    else
                    {
                        m_Client.Disconnect(true);
                    }

                    RunUserCallback(callback, requestInfo, responseInfo);
                }
            }
            catch (Exception ex)
            {
                LogInfo("ConnectError", ex);

                RunUserCallback(callback, requestInfo, new ResponseAckInfo() { MsgType = MessageType.Ack, Status = ResponseCode.CLINET_ERR, Data = ex });
            }

            return result;
        }

        #region AfterConnectedServer
        private void AfterConnectedServer()
        {
            ///接收buffer
            new TaskFactory().StartNew(() =>
            {
                try
                {
                    byte[] buffer = new byte[BufferSize];
                    while (IsAvailable)
                    {
                        int len = m_Client.Receive(buffer);
                        lock (m_syncObject)
                        {
                            m_ReceiveBuffer.Enqueue(buffer.ToList().GetRange(0, len).ToArray().ToJsonString());
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Disconnect();
                    LogInfo("接收网络异常", ex);
                }
                catch (Exception ex)
                {
                    LogInfo("接收其他异常", ex);
                }
            });

            //解析buffer
            new TaskFactory().StartNew(() =>
            {
                try
                {
                    string leftBufferString = "";
                    while (IsAvailable)
                    {
                        string bufferString = "";
                        bool isSuccess = m_ReceiveBuffer.TryDequeue(out bufferString);
                        if (isSuccess && !string.IsNullOrWhiteSpace(bufferString))
                        {
                            string[] bufferMessages = bufferString.Split('\0');
                            leftBufferString = bufferMessages[bufferMessages.Length - 1];
                            for (int index = 0; index < bufferMessages.Length - 1; index++)
                            {
                                Receive(bufferMessages[index]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogInfo("接收处理异常", ex);
                }
            });
        }

        #endregion

        #endregion

        #region 关闭连接
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Disconnect()
        {
            if (m_Client.Connected)
            {
                //close
                m_User.IsAuthenticated = false;
                m_Client.Disconnect(true);
                m_Client.Close(1000);
            }
            if (OnDisconnect != null)
            {
                OnDisconnect();
            }

            LogInfo("断开连接", null);
        }

        #endregion

        #region 重新连接

        string _reConnectSync = string.Empty;

        /// <summary>
        /// 重连
        /// </summary>
        private void ReConnect()
        {

            lock (_reConnectSync)
            {
                if (IsAvailable)
                {
                    return;
                }
                for (int index = 0; index < ConnectRetryTimes; index++)
                {
                    if (Connect(m_IpAddress, m_Port, m_User.AuthenticationType, m_User.Name, m_User.UserAgent, m_User.Token, null))
                    {
                        if (OnReConnected != null)
                        {
                            OnReConnected();
                        }
                        break;
                    }
                    Thread.Sleep(ConnectRetryInnerTime);
                }
            }
        }

        #endregion

        #region 发送

        public bool Send(MessageType type, string to, string group, object data, Action<RequestInfo, ResponseAckInfo> callback)
        {
            var requestInfo = new Message.RequestInfo()
            {
                MsgType = type,
                MessageId = TimeStamp.Create(),
                ToId = to,
                GroupId = group,
                Data = data,
                Callback = callback
            };

            //连接不可用
            if (!IsAvailable)
            {
                RunUserCallback(callback, requestInfo, new ResponseAckInfo() { MessageId = requestInfo.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.NO_AUTH });

                return false;
            }

            try
            {
                byte[] sendData = requestInfo.ToByte<Message.RequestInfo>();

                m_SendBuffer.Add(requestInfo);

                m_Client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, m_Client);

                return true;
            }
            catch (Exception ex)
            {
                RunUserCallback(callback, requestInfo, new ResponseAckInfo() { MessageId = requestInfo.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });
                return false;
            }
        }

        public bool SendText(string to, string group, string content, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Text, to, group, new { content = content }, callback);
        }

        public bool SendFile(MessageType type, string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            FileInfo fileInfo = new FileInfo(path);

            return Send(type, to, group, new
            {
                path = path, // 文件路径
                name = Path.GetFileName(path), // 文件名称
                size = fileInfo.Length, // 文件大小
                md5 = path.GetMD5HashFromFile(), // 文件MD5值
                comment = "", // 备注内容，可选项
            }, callback);
        }

        //image
        public bool SendImage(string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return SendFile(MessageType.Image, to, group, path, callback);
        }
        //voice
        public bool SendVoice(string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return SendFile(MessageType.Voice, to, group, path, callback);
        }
        public bool SendVideo(string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return SendFile(MessageType.Video, to, group, path, callback);
        }

        public bool Invite(string to, string group, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Invite, to, group, "", callback);
        }
        public bool Transfer(string to, string group, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Transfer, to, group, "", callback);
        }
        public bool Join(string group, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Join, "", group, "", callback);
        }
        public bool Leave(string group)
        {
            return Send(MessageType.Leave, "", group, "", null);
        }

        #endregion

        #region 接收
        private void Receive(string bufferMessage)
        {
            var baseResponseInfo = bufferMessage.ToObject<Message.BaseMessageInfo>();
            switch (baseResponseInfo.MsgType)
            {
                case MessageType.Ack:
                    ProcessMessage_ACK(bufferMessage);
                    break;
                //case MessageType.Connect:
                //    break;
                //case MessageType.Disconnect:
                //    break;
                case MessageType.Ping:
                    ProcessMessage_Ping(bufferMessage);
                    break;

                case MessageType.Text:
                case MessageType.Image:
                case MessageType.Voice:
                case MessageType.Video:
                case MessageType.File:
                case MessageType.Invite:
                case MessageType.Join:
                case MessageType.Leave:
                case MessageType.Transfer:
                case MessageType.Link:
                case MessageType.Custom:
                    ProcessMessage_Response(bufferMessage);
                    break;
                case MessageType.UnKnown:
                    break;
                default:
                    break;
            }
            //if (baseResponseInfo.MsgType == MessageType.Ack)
            //{
            //    ProcessMessage_ACK(bufferMessage);
            //}
            //else
            //{
            //    ProcessMessage_Response(bufferMessage);
            //}
        }

        /// <summary>
        /// 处理PING消息
        /// </summary>
        /// <param name="bufferMessage"></param>
        private void ProcessMessage_Ping(string bufferMessage)
        {
            var responseInfo = bufferMessage.ToObject<Message.ResponseInfo>();
            if (IsAvailable)
            {
                m_LastPingTime = DateTime.Now;//采用客户端时间
                // TimeStamp.UnixTimestampToDateTime(DateTime.Now, responseInfo.MsgTime);
            }
        }

        /// <summary>
        /// 处理普通的下行消息
        /// </summary>
        /// <param name="bufferMessage"></param>
        private void ProcessMessage_Response(string bufferMessage)
        {
            var responseInfo = bufferMessage.ToObject<Message.ResponseInfo>();
            if (IsFileMessage(responseInfo.MsgType))
            {
                //用户自己根据url下载，无需处理
            }

            if (OnReceive != null)
            {
                try
                {
                    OnReceive(responseInfo.MsgType, responseInfo.FromId, responseInfo.GroupId, responseInfo.MessageId, responseInfo.MsgTime, responseInfo.Data);
                }
                catch (Exception ex)
                {
                    LogInfo("用户：OnReceive异常", ex);
                    throw ex;//抛出用户异常
                }
            }
        }

        /// <summary>
        /// 处理ACK消息
        /// </summary>
        /// <param name="bufferMessage"></param>
        private void ProcessMessage_ACK(string bufferMessage)
        {
            var responseAckInfo = bufferMessage.ToObject<Message.ResponseAckInfo>();
            var requestInfo = m_SendBuffer.FirstOrDefault(x => x.MessageId == responseAckInfo.MessageId);
            if (requestInfo != null)
            {
                MessageContext context = new MessageContext() { IMRequest = requestInfo, IMResponse = responseAckInfo };
                if (IsFileMessage(requestInfo.MsgType) && requestInfo.Callback == null)
                {//文件类型消息且用户不提供回调自动下载文件
                    ProcessFileMessage(context);
                }
                else if (requestInfo.Callback != null)
                {//非文件类消息收到ack直接回调
                    requestInfo.Callback(requestInfo, responseAckInfo);
                }
            }
        }



        /// <summary>
        /// 对文件类型消息进行处理
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <param name="responseAckInfo"></param>
        private void ProcessFileMessage(MessageContext info)
        {
            ResponseAckInfo responseAckInfo = info.IMResponse;
            if (responseAckInfo.Status == ResponseCode.OK)
            {
                object upload_url = ((Newtonsoft.Json.Linq.JObject)(responseAckInfo.Data)).GetValue("upload_url");
                if (upload_url != null
                    && !string.IsNullOrWhiteSpace(upload_url.ToString()))
                {
                    //Upload(info);
                    Upload(info.IMRequest.MessageId, upload_url.ToString(), info.IMRequest.Data.path);
                    //UploadFile(info.IMRequest.MessageId, upload_url.ToString(), info.IMRequest.Data.path);
                    return;
                }
            }
            //info.IMRequest.Callback(info.IMRequest, info.IMResponse);
        }

        #endregion

        #region 上传相关
        /// <summary>
        /// 上传或重传
        /// </summary>
        /// <param name="msg_id"></param>
        /// <param name="upload_url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Upload(string msg_id, string upload_url, string path)
        {
            FileMessageInfo info = FileMessageInfo.Create(upload_url);
            info.MessageId = msg_id;
            info.ProcessType = FileProcessType.UPLOAD;
            int leftLength = (int)GetUploadStartPos(upload_url);
            byte[] fileData = File.ReadAllBytes(path).Skip(leftLength).ToArray();



            var createBytes = new CreateBytes();
            string contentType = createBytes.ContentType;
            info.Client.Headers.Add("Content-Type", contentType);
            ArrayList bytesArray = new ArrayList();
            bytesArray.Add(createBytes.CreateFieldData("file", Path.GetFileName(path)
                                                , "application/octet-stream", fileData));
            byte[] postData = createBytes.JoinBytes(bytesArray);

            info.Client.UploadProgressChanged += UploadProgressChanged;
            info.Client.UploadDataCompleted += UploadDataCompleted;
            info.Client.UploadDataAsync(new Uri(upload_url), "POST", postData, info);
            info.Status = 1;
            m_FileMsgQueue.Add(info);
            return true;
        }

        void UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var info = e.UserState as FileMessageInfo;
            OnUpload(info.MessageId, e.BytesSent, e.TotalBytesToSend);
        }

        void UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            var fileMsgInfo = e.UserState as FileMessageInfo;
            if (e.Error != null)
            {
                fileMsgInfo.Status = 4;
                // contextInfo.IMRequest.Callback(contextInfo.IMRequest, new ResponseAckInfo() { Data = e.Error, MessageId = contextInfo.IMRequest.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });

                //暂时不传递到异常事件中去
                OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
            }
            else
            {
                fileMsgInfo.Status = 2;
                //  contextInfo.IMRequest.Callback(contextInfo.IMRequest, contextInfo.IMResponse);
            }
        }

        /// <summary>
        /// 断点上传前获取上传开始位置
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private long GetUploadStartPos(string url)
        {
            long length = 0;
            try
            {
                var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "HEAD";
                req.Timeout = 5000;

                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    string lengthString = res.Headers["startPos"];
                    if (!string.IsNullOrWhiteSpace(lengthString))
                    {
                        long.TryParse(lengthString, out length);
                    }
                }
            }
            catch (Exception)
            {
                //log
            }
            return length;
        }

        #endregion

        #region 下载

        /// <summary>
        /// 异步下载（非断点），返回下载标示，用来获取下载进度或异常
        /// </summary>
        /// <param name="fileURL"></param>
        /// <returns></returns>
        public string Download(string fileURL, string filePath, string msgId = "")
        {
            if (string.IsNullOrWhiteSpace(msgId))
            {
                msgId = TimeStamp.Create();
            }
            FileMessageInfo info = FileMessageInfo.Create(fileURL);
            info.MessageId = msgId;
            info.ProcessType = FileProcessType.DOWNLOAD;
            info.Client.DownloadProgressChanged += webClient_DownloadProgressChanged;
            info.Client.DownloadFileCompleted += Client_DownloadFileCompleted;
            info.Client.DownloadFileAsync(new Uri(fileURL), filePath, info);
            m_FileMsgQueue.Add(info);
            return msgId;
        }
        void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var fileMsgInfo = e.UserState as FileMessageInfo;
            if (e.Error != null)
            {
                fileMsgInfo.Status = 4;
                ///
                // contextInfo.IMRequest.Callback(contextInfo.IMRequest, new ResponseAckInfo() { Data = e.Error, MessageId = contextInfo.IMRequest.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });

                //传递到异常事件中去
                OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
            }
            else
            {
                fileMsgInfo.Status = 2;
            }
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var fileMsgInfo = e.UserState as FileMessageInfo;
            OnDownload(fileMsgInfo.MessageId, e.BytesReceived, e.TotalBytesToReceive);
        }

        /// <summary>
        /// 断点下载，需服务器支持CustomRange
        /// </summary>
        /// <param name="fileURL"></param>
        /// <param name="filePath"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public string Download_BreakPoint(string fileURL, string filePath, string msgId = "")
        {
            if (string.IsNullOrWhiteSpace(msgId))
            {
                msgId = TimeStamp.Create();
            }
            long currentLength = 0;
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                currentLength = fileInfo.Length;
            }
            FileMessageInfo info = FileMessageInfo.Create(fileURL);
            info.Client.Headers.Add("CustomRange", string.Format("bytes={0}-", currentLength));
            info.MessageId = msgId;
            info.FilePath = filePath;
            info.ProcessType = FileProcessType.DOWNLOAD;
            m_FileMsgQueue.Add(info);
            info.Status = 1;
            info.Client.DownloadDataAsync(new Uri(fileURL), info);
            info.Client.DownloadDataCompleted += Client_DownloadDataCompleted;
            info.Client.DownloadProgressChanged += webClient_DownloadProgressChanged;
            return msgId;
        }

        void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var fileMsgInfo = e.UserState as FileMessageInfo;
            if (e.Error != null)
            {
                //传递到异常事件中去
                OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
            }
            else
            {
                string contentRange = fileMsgInfo.Client.ResponseHeaders["Content-Range"];
                if (string.IsNullOrEmpty(contentRange))
                {
                    //不支持range
                    File.WriteAllBytes(fileMsgInfo.FilePath, e.Result);
                }
                else
                {
                    //追加
                    using (FileStream sw = new FileStream(fileMsgInfo.FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        sw.Write(e.Result, 0, e.Result.Length);
                    }
                }
            }
        }

        #endregion

        #region 取消
        /// <summary>
        /// 根据消息ID取消上传或下载的任务
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public bool Cancel(string msgId)
        {
            var fileMsgInfo = m_FileMsgQueue.FirstOrDefault(x => x.MessageId == msgId);
            if (fileMsgInfo != null && fileMsgInfo.Status == 1)
            {
                fileMsgInfo.Client.CancelAsync();
                fileMsgInfo.Status = 3;
            }
            return true;
        }

        #endregion

        #region Common
        /// <summary>
        /// 是否是文件类型消息
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        private bool IsFileMessage(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Image:
                case MessageType.Voice:
                case MessageType.Video:
                    return true;
                default:
                    break;
            }
            return false;
        }

        /// <summary>
        /// 统一处理用户回调函数
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="requestInfo"></param>
        /// <param name="responseInfo"></param>
        private void RunUserCallback(Action<RequestInfo, ResponseAckInfo> callback, RequestInfo requestInfo, ResponseAckInfo responseInfo)
        {
            if (callback != null)
            {
                try
                {
                    callback(requestInfo, responseInfo);
                }
                catch (Exception ex)
                {
                    LogInfo("用户：回调异常", ex);
                    throw ex;//抛出用户异常
                }
            }
        }

        /// <summary>
        /// 记录sdk内部日志
        /// </summary>
        /// <param name="description"></param>
        /// <param name="ex"></param>
        private void LogInfo(string description, Exception ex)
        {
            if (_logger != null)
            {
                _logger.Log(description, ex);
            }
        }

        /// <summary>
        /// 出现网络异常自动处理（暂时不用）
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="ex"></param>
        private void ProcessError(string msgId, Exception ex)
        {
            //收到网络异常且客户端已断开时触发重连
            if (ex is SocketException && !m_Client.Connected)
            {
                Disconnect();
                ReConnect();
            }
        }
        #endregion

        //public string ServerHttpUrl { get; set; }

        //private bool UploadFile(string msg_id, string upload_url, string path)
        //{
        //    FileMessageInfo info = FileMessageInfo.Create(upload_url);
        //    info.MessageId = msg_id;
        //    info.ProcessType = FileProcessType.UPLOAD;
        //    info.Client.UploadProgressChanged += UploadProgressChanged;
        //    //info.Client.UploadDataCompleted += UploadDataCompleted;
        //    info.Client.UploadFileAsync(new Uri(upload_url), path);
        //    info.Status = 1;
        //    _fileMsgQueue.Add(info);
        //    return true;
        //}

        //private bool Upload_Data(MessageContext contextInfo)
        //{
        //    FileMessageInfo info = FileMessageInfo.Create(contextInfo.IMResponse.Data.upload_url.ToString());
        //    info.MessageId = contextInfo.IMRequest.MessageId;
        //    info.ProcessType = FileProcessType.UPLOAD;
        //    string path = contextInfo.IMRequest.Data.path;
        //    int leftLength = 100;
        //    byte[] fileData = File.ReadAllBytes(path).Skip(leftLength).ToArray();
        //    info.Client.UploadProgressChanged += webClient_UploadProgressChanged;
        //    info.Client.UploadDataCompleted += Client_UploadDataCompleted;
        //    contextInfo.IMRequest.RelateFileInfo = info;
        //    info.Client.UploadDataAsync(new Uri(contextInfo.IMResponse.Data.upload_url.ToString()), "POST", fileData, contextInfo);
        //    info.Status = 1;
        //    _fileMsgQueue.Add(info);
        //    return true;
        //}
        //void Client_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        //{
        //    var contextInfo = e.UserState as MessageContext;
        //    var fileMsgInfo = contextInfo.IMRequest.RelateFileInfo;
        //    if (e.Error != null)
        //    {
        //        fileMsgInfo.Status = 4;
        //        contextInfo.IMRequest.Callback(contextInfo.IMRequest, new ResponseAckInfo() { Data = e.Error, MessageId = contextInfo.IMRequest.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });

        //        //暂时不传递到异常事件中去
        //        //OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
        //    }
        //    else
        //    {
        //        fileMsgInfo.Status = 2;
        //        contextInfo.IMRequest.Callback(contextInfo.IMRequest, contextInfo.IMResponse);
        //    }
        //}


        //void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        //{
        //    var contextInfo = e.UserState as MessageContext;
        //    var fileMsgInfo = contextInfo.IMRequest.RelateFileInfo;
        //    if (e.Error != null)
        //    {
        //        fileMsgInfo.Status = 4;
        //        contextInfo.IMRequest.Callback(contextInfo.IMRequest, new ResponseAckInfo() { Data = e.Error, MessageId = contextInfo.IMRequest.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });

        //        //暂时不传递到异常事件中去
        //        //OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
        //    }
        //    else
        //    {
        //        fileMsgInfo.Status = 2;
        //        contextInfo.IMRequest.Callback(contextInfo.IMRequest, contextInfo.IMResponse);
        //    }
        //    //fileMsgInfo.Client.Dispose();
        //}

        //void webClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        //{
        //    var contextInfo = e.UserState as MessageContext;
        //    OnUpload(contextInfo.IMRequest.MessageId, e.BytesSent, e.TotalBytesToSend);
        //}

        //public string DownloadPath { get; set; }

        //public async Task<bool> Upload(string path, Action<object, UploadProgressChangedEventArgs> uploadProgressChanged, Action<object, UploadFileCompletedEventArgs> uploadFileCompleted)
        //{
        //    WebClient webClient = new WebClient();
        //    webClient.Credentials = CredentialCache.DefaultCredentials;
        //    await webClient.UploadFileTaskAsync(new Uri(path), "POST", path);
        //    webClient.UploadProgressChanged += webClient_UploadProgressChanged;
        //    return true;

        //}
        //public IMServer(string ipAddress, int port)
        //{
        //    this.IpAddress = ipAddress;
        //    this.Port = port;
        //    _client = new Socket(SocketType.Stream, ProtocolType.Tcp);

        //    _user = new UserIdentity();
        //    _sendBuffer = new ConcurrentBag<RequestInfo>();
        //    _receiveBuffer = new ConcurrentQueue<string>();
        //    _fileMsgQueue = new ConcurrentBag<FileMessageInfo>();
        //}
    }
}
