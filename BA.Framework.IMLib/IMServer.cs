using BA.Framework.IMLib.Message;
using System;
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

        private Socket _client;

        private DateTime _lastPingTime;

        /// <summary>
        /// 当前用户验证信息
        /// </summary>
        private UserIdentity _user;

        /// <summary>
        /// 当前连接是否可用
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                if (_client.Connected && _user.IsAuthenticated)
                {
                    //被动心跳检测
                    if (_lastPingTime.AddSeconds(HeartTimeOut) < DateTime.Now)
                    {
                        Disconnect();
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        private ConcurrentBag<RequestInfo> _sendBuffer;

        private ConcurrentQueue<string> _receiveBuffer;

        private ConcurrentBag<FileMessageInfo> _fileMsgQueue;

        string _syncObject = "";

        //socket缓存区大小,1024字节
        private const int BufferSize = 1024;

        //心跳检测超时时间，30s
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
            _client = new Socket(SocketType.Stream, ProtocolType.Tcp);

            _user = new UserIdentity();
            _sendBuffer = new ConcurrentBag<RequestInfo>();
            _receiveBuffer = new ConcurrentQueue<string>();
            _fileMsgQueue = new ConcurrentBag<FileMessageInfo>();
        }

        private string IpAddress { get; set; }
        private int Port { get; set; }

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

            //开始连接
            try
            {
                _client.Connect(host, port);
                IpAddress = IpAddress;
                Port = port;
                if (_client.Connected)
                {
                    byte[] sendData = requestInfo.ToByte<Message.RequestInfo>();
                    _client.Send(sendData);
                    byte[] bufferData = new byte[1024];
                    int receiveLen = _client.Receive(bufferData);
                    byte[] receivedData = bufferData.ToList().GetRange(0, receiveLen).ToArray();
                    var responseInfo = receivedData.ToObject<Message.ResponseAckInfo>();
                    if (responseInfo.Status == ResponseCode.OK)
                    {
                        _user.IsAuthenticated = true;
                        _user.Name = userName;
                        _user.Token = accessToken;
                        _user.UserAgent = userAgent;
                        _user.UserType = userType;

                        //连接使用同步
                        // _sendBuffer.Add(requestInfo);
                        _lastPingTime = DateTime.Now;
                        if (callback != null)
                        {
                            callback(requestInfo, responseInfo);
                        }
                        //启动线程接收
                        AfterConnectedServer();
                        return true;
                    }
                    else
                    {
                        _client.Disconnect(true);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (callback != null)
                {
                    callback(requestInfo, new ResponseAckInfo() { MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT, Data = ex });
                }
            }
            return false;
        }


        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Disconnect()
        {
            if (_client.Connected)
            {
                //close
                _user.IsAuthenticated = false;
                _client.Disconnect(false);
            }
            if (OnDisconnect != null)
            {
                OnDisconnect();
            }
        }
         
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
                    if (Connect(IpAddress, Port, _user.AuthenticationType, _user.Name, _user.UserAgent, _user.Token, null))
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

        #region AfterConnectedServer
        private void AfterConnectedServer()
        {
            ///接收buffer
            new TaskFactory().StartNew(() =>
            {
                byte[] buffer = new byte[BufferSize];
                while (IsAvailable)
                {
                    int len = _client.Receive(buffer);
                    lock (_syncObject)
                    {
                        _receiveBuffer.Enqueue(buffer.ToList().GetRange(0, len).ToArray().ToJsonString());
                    }
                }
            });

            //解析buffer
            new TaskFactory().StartNew(() =>
            {
                string leftBufferString = "";
                while (IsAvailable)
                {
                    string bufferString = "";
                    bool isSuccess = _receiveBuffer.TryDequeue(out bufferString);
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
            });
        }

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
            if (baseResponseInfo.MsgType == MessageType.Ack)
            {
                ProcessMessage_ACK(bufferMessage);
            }
            else
            {
                ProcessMessage_Response(bufferMessage);
            }
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
                _lastPingTime = TimeStamp.UnixTimestampToDateTime(DateTime.Now, responseInfo.MsgTime);
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
                OnReceive(responseInfo.MsgType, responseInfo.FromId, responseInfo.GroupId, responseInfo.MessageId, responseInfo.MsgTime, responseInfo.Data);
            }
        }

        /// <summary>
        /// 处理ACK消息
        /// </summary>
        /// <param name="bufferMessage"></param>
        private void ProcessMessage_ACK(string bufferMessage)
        {
            var responseAckInfo = bufferMessage.ToObject<Message.ResponseAckInfo>();
            var requestInfo = _sendBuffer.FirstOrDefault(x => x.MessageId == responseAckInfo.MessageId);
            if (requestInfo != null && requestInfo.Callback != null)
            {
                MessageContext context = new MessageContext() { IMRequest = requestInfo, IMResponse = responseAckInfo };
                if (IsFileMessage(requestInfo.MsgType))
                {//文件类型消息延迟回调
                    ProcessFileMessage(context);
                }
                else
                {//非文件类消息收到ack直接回调
                    requestInfo.Callback(requestInfo, responseAckInfo);
                }
            }
        }

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

        #endregion


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
                    Upload(info);
                    return;
                }
            }
            info.IMRequest.Callback(info.IMRequest, info.IMResponse);
        }

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
                if (callback != null)
                {
                    callback(requestInfo, new ResponseAckInfo() { MsgType = MessageType.Ack, Status = ResponseCode.NO_AUTH });
                }
                return false;
            }

            try
            {
                byte[] sendData = requestInfo.ToByte<Message.RequestInfo>();

                _sendBuffer.Add(requestInfo);

                _client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, _client);

                return true;
            }
            catch (Exception ex)
            {
                callback(requestInfo, new ResponseAckInfo() { Data = ex, MessageId = requestInfo.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });
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

        public bool Invite(string to, string group)
        {
            return Send(MessageType.Invite, to, group, "", null);
        }
        public bool Transfer(string to, string group)
        {
            return Send(MessageType.Transfer, to, group, "", null);
        }
        public bool Join(string group)
        {
            return Send(MessageType.Join, "", group, "", null);
        }
        public bool Leave(string group)
        {
            return Send(MessageType.Leave, "", group, "", null);
        }

        //public string ServerHttpUrl { get; set; }

        private bool Upload(MessageContext contextInfo)
        {
            FileMessageInfo info = FileMessageInfo.Create(contextInfo.IMResponse.Data.upload_url.ToString());
            info.MessageId = contextInfo.IMRequest.MessageId;
            info.ProcessType = FileProcessType.UPLOAD;
            string path = contextInfo.IMRequest.Data.path;
            info.Client.UploadProgressChanged += webClient_UploadProgressChanged;
            info.Client.UploadFileCompleted += Client_UploadFileCompleted;
            contextInfo.IMRequest.RelateFileInfo = info;
            info.Client.UploadFileAsync(new Uri(contextInfo.IMResponse.Data.upload_url.ToString() + "123"), "POST", path, contextInfo);
            info.Status = 1;
            _fileMsgQueue.Add(info);
            return true;

        }


        void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            var contextInfo = e.UserState as MessageContext;
            var fileMsgInfo = contextInfo.IMRequest.RelateFileInfo;
            if (e.Error != null)
            {
                fileMsgInfo.Status = 4;
                contextInfo.IMRequest.Callback(contextInfo.IMRequest, new ResponseAckInfo() { Data = e.Error, MessageId = contextInfo.IMRequest.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });

                //暂时不传递到异常事件中去
                //OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
            }
            else
            {
                fileMsgInfo.Status = 2;
                contextInfo.IMRequest.Callback(contextInfo.IMRequest, contextInfo.IMResponse);
            }
            //fileMsgInfo.Client.Dispose();
        }

        void webClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var contextInfo = e.UserState as MessageContext;
            OnUpload(contextInfo.IMRequest.MessageId, e.BytesSent, e.TotalBytesToSend);
        }

        //public string DownloadPath { get; set; }


        /// <summary>
        /// 直接下载，返回下载标示，用来获取下载进度或异常
        /// </summary>
        /// <param name="fileURL"></param>
        /// <returns></returns>
        public string Download(string fileURL, string filePath, string msgId = "")
        {
            if (string.IsNullOrWhiteSpace(msgId))
            {
                msgId = Guid.NewGuid().ToString();
            }
            FileMessageInfo info = FileMessageInfo.Create(fileURL);
            info.MessageId = msgId;
            info.ProcessType = FileProcessType.DOWNLOAD;
            info.Client.DownloadProgressChanged += webClient_DownloadProgressChanged;
            info.Client.DownloadFileCompleted += Client_DownloadFileCompleted;
            info.Client.DownloadFileAsync(new Uri(fileURL), filePath, info);
            //info.Client.DownloadDataAsync(new Uri(fileURL), info);
            _fileMsgQueue.Add(info);
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


        public bool Cancel(string msgId)
        {
            var fileMsgInfo = _fileMsgQueue.FirstOrDefault(x => x.MessageId == msgId);
            if (fileMsgInfo != null && fileMsgInfo.Status == 1)
            {
                fileMsgInfo.Client.CancelAsync();
                fileMsgInfo.Status = 3;
            }
            return true;
        }

        //public async Task<bool> Upload(string path, Action<object, UploadProgressChangedEventArgs> uploadProgressChanged, Action<object, UploadFileCompletedEventArgs> uploadFileCompleted)
        //{
        //    WebClient webClient = new WebClient();
        //    webClient.Credentials = CredentialCache.DefaultCredentials;
        //    await webClient.UploadFileTaskAsync(new Uri(path), "POST", path);
        //    webClient.UploadProgressChanged += webClient_UploadProgressChanged;
        //    return true;

        //}


        //暂时不实现
        //join
        //transfer


        public void ProcessError(string msgId, Exception ex)
        {
            //收到网络异常且客户端已断开时触发重连
            if (ex is SocketException && !_client.Connected)
            {
                Disconnect();
                ReConnect();
            }
        }
    }
}
