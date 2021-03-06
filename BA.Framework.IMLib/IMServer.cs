﻿using BA.Framework.IMLib.Enums;
using BA.Framework.IMLib.Message;
using BA.Framework.IMLib.TransferEncrypt;
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
    /// <summary>
    /// 访问IM服务器的主要类
    /// </summary>
    public class IMServer : BA.Framework.IMLib.IIMServer, IDisposable
    {
        private DateTime _lastSendTime;

        private PingMode clientPingMode = PingMode.Passive;

        /// <summary>
        /// 客户端ping模式
        /// </summary>
        public PingMode ClientPingMode
        {
            get { return clientPingMode; }
            set { clientPingMode = value; }
        }

        private string pingData;
        /// <summary>
        /// 主动模式下可自定义ping包数据
        /// </summary>
        public string PingData
        {
            get { return pingData; }
            set
            {
                pingData = value;
            }
        }
        /// <summary>
        /// 主动模式下可自定义ping包间隔
        /// </summary>
        public int PingInterval { get; set; }

        public System.Timers.Timer PingTimer { get; set; }

        /// <summary>
        /// 默认初始1.0,可在连接前设置
        /// </summary>
        public string Encrypt_Version { get; set; }

        private bool enableEncrypt;
        /// <summary>
        /// 是否启用传输加密
        /// </summary>
        public bool EnableEncrypt
        {
            get { return enableEncrypt; }
            set { enableEncrypt = value; }
        }

        /// <summary>
        /// 启用传输加密时可定义加密适配器，默认使用系统
        /// </summary>
        public IMessageEncrypt EncryptAdapter;

        ///// <summary>
        ///// 消息加密处理
        ///// </summary>
        //public Func<RequestInfo, RequestInfo> EnCryptOp;

        ///// <summary>
        ///// 消息解密处理
        ///// </summary>
        //public Func<BaseMessageInfo, dynamic> DeCryptOp;

        /// <summary>
        /// 接收用户消息事件
        /// </summary>
        public event Action<MessageType, string, string, string, int, object> OnReceive;
        /// <summary>
        /// 上传文件事件
        /// </summary>
        public event Action<string, long, long> OnUpload;
        /// <summary>
        /// 下载文件事件
        /// </summary>
        public event Action<string, long, long> OnDownload;
        /// <summary>
        /// 异常事件
        /// </summary>
        public event Action<object, ErrorEventArgs> OnError;
        /// <summary>
        /// 断开连接事触发此事件
        /// </summary>
        public event Action OnDisconnect;
        /// <summary>
        /// 重新连接成功时触发此事件
        /// </summary>
        public event Action OnReConnected;
        /// <summary>
        /// 操作类型,日志
        /// </summary>
        public event Action<string, string> OnLog;

        private Socket m_Client;

        private DateTime m_LastPingTime;

        private int m_LastRecServerTime = 0;

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
        bool IsAvailable
        {
            get
            {
                if (m_Client != null && m_Client.Connected && m_User.IsAuthenticated)
                {
                    //被动心跳检测
                    if (m_LastPingTime.AddSeconds(HeartTimeOut) < DateTime.Now && clientPingMode == PingMode.Passive)
                    {
                        LogInfo(string.Format("上传收到心跳时间：{0}", m_LastPingTime), null);
                        Disconnect();
                        ReConnect();
                        if (m_Client == null || !m_Client.Connected || !m_User.IsAuthenticated)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }

        private ConcurrentDictionary<string, RequestInfo> m_SendBuffer;

        //private ConcurrentQueue<string> m_ReceiveBuffer;

        private ConcurrentQueue<List<byte>> m_ReceiveBufferBytes;
        /// <summary>
        /// 上传下载任务队列
        /// </summary>
        private ConcurrentBag<FileMessageInfo> m_FileMsgQueue;

        string m_syncObject = "";

        /// <summary>
        /// socket缓存区大小,1024字节
        /// </summary>
        private const int BufferSize = 102400;

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

        /// <summary>
        /// 无参构造
        /// </summary>
        public IMServer()
        {
            m_User = new UserIdentity();
            m_SendBuffer = new ConcurrentDictionary<string, RequestInfo>();
            //m_ReceiveBuffer = new ConcurrentQueue<string>();
            m_ReceiveBufferBytes = new ConcurrentQueue<List<byte>>();
            m_FileMsgQueue = new ConcurrentBag<FileMessageInfo>();
            InitEncrypt();
            InitPing();
        }

        private void InitEncrypt()
        {
            Encrypt_Version = "1.0";
            //设置默认加密方式
            EncryptAdapter = new MessageEncrypt();
        }

        private void InitPing()
        {
            PingData = new { type = "ping" }.ToJsonString();
            PingInterval = 5;
            PingTimer = new System.Timers.Timer(500);
            PingTimer.Elapsed += PingTimer_Elapsed;
        }

        void PingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendPing();
        }



        #region 连接

        /// <summary>
        /// 打开连接
        /// </summary>
        /// <param name="host">服务器地址</param>
        /// <param name="port">服务器端口号</param>
        /// <param name="userType">用户类型</param>
        /// <param name="userName">用户显示的名称</param>
        /// <param name="userAgent">用户登陆的终端信息</param>
        /// <param name="accessToken">访问令牌</param>
        /// <param name="lastTick">收到最后一条消息的UNIX时间戳</param>
        /// <param name="callback">回调函数</param>
        /// <returns>连接是否成功</returns>
        public bool Connect(string host, int port, string userType, string userName, string userAgent, string accessToken, int lastTick, Action<RequestInfo, ResponseAckInfo> callback)
        {
            var requestInfo = new Message.RequestInfo()
            {
                MsgType = MessageType.Connect,
                //MessageId = TimeStamp.Create(),
                Data = new
                {
                    userType = userType,
                    userName = userName,
                    userAgent = userAgent,
                    accessToken = accessToken,
                    lastTick = lastTick
                },
                Version = Encrypt_Version
            };
            var preRequestInfo = new Message.RequestInfo()
            {
                MsgType = MessageType.Connect,
                Data = requestInfo.Data,
                Version = Encrypt_Version
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

                if (EnableEncrypt)
                {
                    requestInfo.Data = EncryptAdapter.Encode(CommonExt.DynamicToJsonString(requestInfo.Data), requestInfo);
                }

                LogOpInfo("BeginConnect:", requestInfo.ToJsonString());
                m_Client.Connect(host, port);
                m_IpAddress = host;
                m_Port = port;
                if (m_Client.Connected)
                {
                    byte[] sendData = requestInfo.ToByte<Message.RequestInfo>();

                    m_Client.Send(sendData);
                    byte[] bufferData = new byte[BufferSize];
                    int receiveLen = m_Client.Receive(bufferData);
                    byte[] receivedData = bufferData.ToList().GetRange(0, receiveLen).ToArray();
                    var responseInfo = receivedData.ToObject<Message.ResponseAckInfo>();
                    if (EnableEncrypt)
                    {
                        dynamic deString = CommonCryptoService.Decrypt_DES(responseInfo.Data.ToString(), responseInfo.MessageId);
                        responseInfo.Data = CommonExt.JsonStringToDynamic(deString.ToString());
                    }
                    //LogOpInfo("ConnectOver:", responseInfo.ToJsonString());
                    if (responseInfo.Status == ResponseCode.OK)
                    {
                        m_User.IsAuthenticated = true;
                        m_User.Name = userName;
                        m_User.Token = accessToken;
                        m_User.UserAgent = userAgent;
                        m_User.UserType = userType;
                        m_User.AuthenticationType = responseInfo.Data.userPermission;
                        m_User.PermissionList = Permission.GetUserPermission(m_User.AuthenticationType);
                        //连接使用同步
                        // _sendBuffer.Add(requestInfo);
                        m_LastPingTime = DateTime.Now;

                        //启动线程接收
                        AfterConnectedServer();
                        result = true;
                        m_IsCanConnecting = true;

                        if (this.clientPingMode == PingMode.Initiative)
                        {
                            _lastSendTime = DateTime.Now;
                            PingTimer.Start();
                        }

                    }
                    else
                    {
                        m_Client.Disconnect(true);
                    }

                    RunUserCallback(callback, preRequestInfo, responseInfo);
                }
            }
            catch (Exception ex)
            {
                LogInfo("ConnectError", ex);
                RunUserCallback(callback, preRequestInfo, new ResponseAckInfo() { MsgType = MessageType.Ack, Status = ResponseCode.CLINET_ERR, Data = ex.Message });
            }

            return result;
        }

        #region AfterConnectedServer
        public static T[] Merge<T>(T[] arr, T[] other)
        {
            T[] buffer = new T[arr.Length + other.Length];
            arr.CopyTo(buffer, 0);
            other.CopyTo(buffer, arr.Length);
            return buffer;
        }
        #endregion

        #region AfterConnectedServer2
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
                            if (len == 0)
                            {
                                continue;
                            }

                            List<byte> bufferString = buffer.ToList().GetRange(0, len);
                            if (bufferString.Count == 0)
                            {
                                continue;
                            }
                            m_ReceiveBufferBytes.Enqueue(bufferString);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    ProcessError("", ex);
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
                    List<byte> leftBufferString = new List<byte>();
                    while (IsAvailable || m_ReceiveBufferBytes.Count != 0)
                    {
                        List<byte> bufferString = new List<byte>();
                        bool isSuccess = m_ReceiveBufferBytes.TryDequeue(out bufferString);
                        if (m_ReceiveBufferBytes.Count > 0)
                        {
                            LogOpInfo("QueueCount", m_ReceiveBufferBytes.Count.ToString());
                        }

                        if (isSuccess && bufferString.Count > 0)
                        {
                            //LogOpInfo("bf", bufferString);
                            //LogOpInfo("lf", leftBufferString);
                            if (leftBufferString.Count > 0)
                            {
                                //LogOpInfo("t_B", bufferString.Length.ToString());
                                bufferString = Merge<byte>(leftBufferString.ToArray(), bufferString.ToArray()).ToList();
                                //LogOpInfo("t_A", bufferString.Length.ToString());
                            }
                            //LogOpInfo("total", m_ReceiveBuffer.Count.ToString()+"$$$"+bufferString);

                            do
                            {
                                int index = bufferString.FindIndex(x => x == 0);
                                if (index == -1)
                                {
                                    break;
                                }
                                byte[] singleMsgData = bufferString.GetRange(0, index).ToArray();
                                Receive(singleMsgData.ToJsonString());
                                if (bufferString.Count <= index + 1)
                                {
                                    bufferString = new List<byte>();
                                    break;
                                }
                                bufferString = bufferString.GetRange(index + 1, bufferString.Count - index - 1);
                            } while (true);

                            leftBufferString = bufferString;
                        }
                        if (m_ReceiveBufferBytes.Count == 0)
                        {
                            Thread.Sleep(1);
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
                SendDisconnect();
                m_User.IsAuthenticated = false;
                m_Client.Disconnect(true);
                m_Client.Close(1000);
                PingTimer.Stop();
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

        private bool m_IsCanConnecting = true;
        /// <summary>
        /// 重连
        /// </summary>
        private void ReConnect()
        {
            LogOpInfo("开始重连", "..");
            lock (_reConnectSync)
            {
                if (IsAvailable || !m_IsCanConnecting)//防止多个线程引发的多次重连
                {
                    return;
                }
                for (int index = 0; index < ConnectRetryTimes; index++)
                {
                    if (Connect(m_IpAddress, m_Port, m_User.UserType, m_User.Name, m_User.UserAgent, m_User.Token, m_LastRecServerTime, null))
                    {

                        if (OnReConnected != null)
                        {
                            OnReConnected();
                        }

                        LogOpInfo(string.Format("第{0}次开始重连成功", index + 1), "==");

                        break;
                    }
                    else
                    {
                        LogOpInfo(string.Format("第{0}次开始重连失败", index + 1), "..");
                        if (index == ConnectRetryTimes - 1)
                        {
                            m_IsCanConnecting = false;
                            LogOpInfo("ReConnected fail", "连接断开");
                        }
                    }
                    Thread.Sleep(ConnectRetryInnerTime * 1000);
                }
            }
        }

        #endregion

        #region 发送

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="type">消息类型，参见枚举MessageType</param>
        /// <param name="to">要接收消息的用户</param>
        /// <param name="group">要接收消息的组</param>
        /// <param name="data">附加数据</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        public string Send(MessageType type, string to, string group, object data, Action<RequestInfo, ResponseAckInfo> callback)
        {
            var requestInfo = new Message.RequestInfo()
            {
                MsgType = type,
                //MessageId = TimeStamp.Create(),
                ToId = to,
                GroupId = group,
                Data = data,
                Callback = callback
            };
            var preRequestInfo = new Message.RequestInfo()
            {
                MsgType = type,
                //MessageId = TimeStamp.Create(),
                ToId = to,
                GroupId = group,
                Data = data,
                Callback = callback
            };

            LogOpInfo("Send", requestInfo.ToJsonString());

            //连接不可用
            if (!IsAvailable)
            {
                RunUserCallback(callback, requestInfo, new ResponseAckInfo() { MessageId = requestInfo.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.NO_AUTH });
                return requestInfo.MessageId;
            }

            if (!Permission.CheckPermission(m_User, type))
            {
                RunUserCallback(callback, requestInfo, new ResponseAckInfo() { MessageId = requestInfo.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.NO_PERMISSION });
                return requestInfo.MessageId;
            }

            //深拷贝到请求列表中
            m_SendBuffer.TryAdd(requestInfo.MessageId, new RequestInfo()
                {
                    MessageId = requestInfo.MessageId,
                    Callback = requestInfo.Callback,
                    Data = requestInfo.Data,
                    MsgType = requestInfo.MsgType,
                    RelateFileInfo = requestInfo.RelateFileInfo,
                    ToId = requestInfo.ToId,
                    Version = requestInfo.Version,
                    GroupId = requestInfo.GroupId
                });

            if (EnableEncrypt)
            {
                requestInfo.Data = EncryptAdapter.Encode(CommonExt.DynamicToJsonString(requestInfo.Data), requestInfo);
                LogOpInfo("Send--AfterEncode", requestInfo.ToJsonString());
            }
            try
            {
                byte[] sendData = requestInfo.ToByte<Message.RequestInfo>();

                m_Client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, m_Client);

                UpdateTimeForInitiativePing();
            }
            catch (Exception ex)
            {
                ProcessError(requestInfo.MessageId, ex);
                RunUserCallback(callback, preRequestInfo, new ResponseAckInfo() { MessageId = requestInfo.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });
            }

            return requestInfo.MessageId;

        }

        private void UpdateTimeForInitiativePing()
        {
            if (this.clientPingMode == PingMode.Initiative)
            {
                _lastSendTime = DateTime.Now;
            }
        }
        private void SendPing()
        {
            if (m_Client != null && m_Client.Connected && m_User.IsAuthenticated && ClientPingMode == PingMode.Initiative)
            {

                if (_lastSendTime.AddSeconds(PingInterval) > DateTime.Now)
                {
                    //未到时间
                    return;
                }

                try
                {
                    byte[] sendData = PingData.ToByte();
                    m_Client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, m_Client);

                    UpdateTimeForInitiativePing();
                    LogOpInfo("Send--Ping", pingData);

                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                PingTimer.Stop();
            }

        }
        private void LogOpInfo(string opType, string log)
        {
            if (OnLog != null)
            {
                OnLog(opType, log);
            }
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="to">要接收消息的用户</param>
        /// <param name="group">要接收消息的组</param>
        /// <param name="content">文本内容</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string SendText(string to, string group, string content, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Text, to, group, new { content = content }, callback);
        }

        /// <summary>
        /// 发送文件消息
        /// </summary>
        /// <param name="type">消息类型，参见枚举MessageType</param>
        /// <param name="to">要接收消息的用户</param>
        /// <param name="group">要接收消息的组</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string SendFile(MessageType type, string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            if (!File.Exists(path))
            {
                return "";
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

        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="to">要接收消息的用户</param>
        /// <param name="group">要接收消息的组</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string SendImage(string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return SendFile(MessageType.Image, to, group, path, callback);
        }

        /// <summary>
        /// 发送语音消息
        /// </summary>
        /// <param name="to">要接收消息的用户</param>
        /// <param name="group">要接收消息的组</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string SendVoice(string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return SendFile(MessageType.Voice, to, group, path, callback);
        }

        /// <summary>
        /// 发送视频消息
        /// </summary>
        /// <param name="to">要接收消息的用户</param>
        /// <param name="group">要接收消息的组</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string SendVideo(string to, string group, string path, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return SendFile(MessageType.Video, to, group, path, callback);
        }

        /// <summary>
        /// 邀请某人加入当前群组会话
        /// </summary>
        /// <param name="to">目标用户ID</param>
        /// <param name="group">群组ID</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string Invite(string to, string group, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Invite, to, group, "", callback);
        }

        /// <summary>
        /// 将当前会话转接给某人
        /// </summary>
        /// <param name="to">目标用户ID</param>
        /// <param name="group">群组ID</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string Transfer(string to, string group, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Transfer, to, group, "", callback);
        }

        /// <summary>
        /// 加入指定的群组会话
        /// </summary>
        /// <param name="group">群组ID</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>    
        [Debug]
        public string Join(string group, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.Join, "", group, "", callback);
        }

        /// <summary>
        /// 离开指定的群组会话
        /// </summary>
        /// <param name="group">群组ID</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string Leave(string group)
        {
            return Send(MessageType.Leave, "", group, "", null);
        }

        /// <summary>
        /// 撤销已发出的某条消息
        /// </summary>
        /// <param name="to">目标用户ID</param>
        /// <param name="group">群组ID</param>
        /// <param name="msg_id">需要撤销的消息ID</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string Undo(string to, string group, string msg_id)
        {
            return Send(MessageType.undo, to, group, new { msg_id = msg_id }, null);
        }

        /// <summary>
        /// 获取历史消息
        /// </summary>
        /// <param name="lastTick">收到最后一条消息的UNIX时间戳</param>
        /// <param name="callback">回调</param>
        /// <returns>消息ID</returns>
        [Debug]
        public string History(int lastTick, Action<RequestInfo, ResponseAckInfo> callback)
        {
            return Send(MessageType.History, "", "", new { lastTick = lastTick }, callback);
        }

        /// <summary>
        /// 发送结束消息，不管是否成功
        /// </summary>
        /// <returns></returns>
        private bool SendDisconnect()
        {
            try
            {
                var disconnectInfo = new { type = "disconnect", msg_id = Guid.NewGuid().ToString() };
                LogOpInfo("disconnect", disconnectInfo.ToJsonString());
                byte[] sendData = disconnectInfo.ToByte();
                m_Client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, m_Client);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region 接收
        private void Receive(string bufferMessage)
        {
            LogOpInfo("Receive", bufferMessage);

            m_LastPingTime = DateTime.Now;//采用客户端时间

            UpdateTimeForInitiativePing();

            var baseResponseInfo = bufferMessage.ToObject<Message.BaseMessageInfo>();

            switch (baseResponseInfo.MsgType)
            {
                case MessageType.Ack:
                    ProcessMessage_ACK(bufferMessage);
                    break;
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
                case MessageType.Kill:
                case MessageType.Custom:
                case MessageType.Vtext:
                    ProcessMessage_Response(bufferMessage);
                    break;
                case MessageType.UnKnown:
                    break;
                default:
                    break;
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
                m_LastPingTime = DateTime.Now;//采用客户端时间
            }
        }

        /// <summary>
        /// 处理普通的下行消息
        /// </summary>
        /// <param name="bufferMessage"></param>
        private void ProcessMessage_Response(string bufferMessage)
        {
            var responseInfo = bufferMessage.ToObject<Message.ResponseInfo>();
            if (EnableEncrypt && responseInfo.Data != null)
            {
                try
                {
                    dynamic deString = CommonCryptoService.Decrypt_DES(responseInfo.Data.ToString(), responseInfo.MessageId);
                    responseInfo.Data = CommonExt.JsonStringToDynamic(deString.ToString());
                    LogOpInfo("Receive--Decode", responseInfo.ToJsonString());
                }
                catch (Exception ex)
                {
                    LogInfo("用户：DeCryptOp异常", ex);
                    //throw ex;//抛出用户异常
                }
            }

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
            if (EnableEncrypt && responseAckInfo.Data != null)
            {
                try
                {
                    dynamic deString = CommonCryptoService.Decrypt_DES(responseAckInfo.Data.ToString(), responseAckInfo.MessageId);
                    responseAckInfo.Data = CommonExt.JsonStringToDynamic(deString.ToString());
                    LogOpInfo("Receive--Decode", responseAckInfo.ToJsonString());
                }
                catch (Exception ex)
                {
                    LogInfo("用户：DeCryptOp异常", ex);
                    //throw ex;//抛出用户异常
                }
            }

            var requestInfo = m_SendBuffer.FirstOrDefault(x => x.Key == responseAckInfo.MessageId).Value;
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
                m_SendBuffer.TryRemove(responseAckInfo.MessageId, out requestInfo);
            }
        }


        /// <summary>
        /// 对文件类型消息进行处理
        /// </summary>
        private void ProcessFileMessage(MessageContext info)
        {
            ResponseAckInfo responseAckInfo = info.IMResponse;
            if (responseAckInfo.Status == ResponseCode.OK)
            {
                if (responseAckInfo.Data == null)
                {
                    //已经有此文件
                    return;
                }

                if (responseAckInfo.DataDict.ContainsKey("upload_url"))
                {
                    object upload_url = responseAckInfo.DataDict["upload_url"];
                    if (upload_url != null
                        && !string.IsNullOrWhiteSpace(upload_url.ToString()))
                    {
                        Upload(info.IMRequest.MessageId, upload_url.ToString(), info.IMRequest.Data.path);
                        return;
                    }
                }


            }
        }

        #endregion

        #region 上传相关
        /// <summary>
        /// 上传或重传,不支持100MB以上文件
        /// </summary>
        /// <param name="msg_id">消息ID</param>
        /// <param name="upload_url">上传地址</param>
        /// <param name="path">本地路径</param>
        [Debug]
        public void Upload(string msg_id, string upload_url, string path)
        {

            FileMessageInfo info = FileMessageInfo.Create(upload_url);
            info.MessageId = msg_id;
            info.ProcessType = FileProcessType.UPLOAD;
            info.Client.UploadProgressChanged += UploadProgressChanged;
            info.Client.UploadDataCompleted += UploadDataCompleted;
            info.Status = 1;
            info.FilePath = path;
            m_FileMsgQueue.Add(info);

            var createBytes = new CreateBytes();
            string contentType = createBytes.ContentType;
            info.Client.Headers.Add("Content-Type", contentType);
            //new TaskFactory().StartNew(() =>
            //{

            //});

            new TaskFactory().StartNew(() =>
                {
                    // LogOpInfo("upload_begin" ,Thread.CurrentThread.ManagedThreadId.ToString());
                    try
                    {
                        int leftLength = (int)GetUploadStartPos(upload_url);
                        byte[] fileData = File.ReadAllBytes(path).Skip(leftLength).ToArray();
                        info.StartPos = leftLength;
                        ArrayList bytesArray = new ArrayList();
                        bytesArray.Add(createBytes.CreateFieldData("file", Path.GetFileName(path)
                                                            , "application/octet-stream", fileData));
                        byte[] postData = createBytes.JoinBytes(bytesArray);
                        info.TotalFileLength = postData.Length + leftLength;
                        info.Client.UploadDataAsync(new Uri(upload_url), "POST", postData, info);

                        //LogOpInfo("upload_end", Thread.CurrentThread.ManagedThreadId.ToString());
                    }
                    catch (Exception ex)
                    {
                        ProcessError(msg_id, ex);
                    }
                });
        }

        void UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var info = e.UserState as FileMessageInfo;
            if (e.BytesSent != e.TotalBytesToSend)
            {
                OnUpload(info.MessageId, e.BytesSent + info.StartPos, e.TotalBytesToSend + info.StartPos);
            }
            //发送完成触发多次
        }

        void UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            var fileMsgInfo = e.UserState as FileMessageInfo;
            if (e.Error != null)
            {
                fileMsgInfo.Status = 4;
                // contextInfo.IMRequest.Callback(contextInfo.IMRequest, new ResponseAckInfo() { Data = e.Error, MessageId = contextInfo.IMRequest.MessageId, MsgType = MessageType.Ack, Status = ResponseCode.TIMEOUT });

                //暂时不传递到异常事件中去
                //OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
                ProcessError(fileMsgInfo.MessageId, e.Error);
            }
            else
            {
                fileMsgInfo.Status = 4;
                try
                {
                    var recObj = e.Result.ToObject<HttpServerResultInfo>();
                    if (recObj.StatCode == 200)
                    {
                        //FileInfo info = new FileInfo(fileMsgInfo.FilePath);
                        //OnUpload(fileMsgInfo.MessageId, info.Length, info.Length);
                        OnUpload(fileMsgInfo.MessageId, fileMsgInfo.TotalFileLength, fileMsgInfo.TotalFileLength);
                        fileMsgInfo.Status = 2;
                    }
                    else
                    {
                        ProcessError(fileMsgInfo.MessageId, new Exception("服务器响应异常:" + recObj.StatCode + recObj.Message));
                    }
                }
                catch (Exception)
                {
                    ProcessError(fileMsgInfo.MessageId, new Exception("服务器响应异常"));
                }
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
                req.Timeout = 50;

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
        /// 异步下载（非断点）
        /// </summary>
        /// <param name="fileURL">下载文件的HTTP地址</param>
        /// <param name="filePath">保存的文件路径</param>
        /// <param name="msgId">消息唯一ID</param>
        /// <returns>如调用时不传msgId，则可通过返回值获取下载标示，用来获取下载进度或异常</returns>
        [Debug]
        public string Download(string fileURL, string filePath, string msgId = "")
        {
            if (string.IsNullOrWhiteSpace(msgId))
            {
                msgId = TimeStamp.Create();
            }
            FileMessageInfo info = FileMessageInfo.Create(fileURL);
            info.MessageId = msgId;
            info.ProcessType = FileProcessType.DOWNLOAD;
            info.FilePath = filePath;
            info.Status = 1;
            info.Client.DownloadProgressChanged += webClient_DownloadProgressChanged;
            info.Client.DownloadFileCompleted += Client_DownloadFileCompleted;
            new TaskFactory().StartNew(() =>
            {
                try
                {
                    info.Client.DownloadFileAsync(new Uri(fileURL), filePath, info);
                }
                catch (Exception ex)
                {
                    ProcessError(msgId, ex);
                }
            });
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
                // OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
                ProcessError(fileMsgInfo.MessageId, e.Error);
            }
            else
            {
                fileMsgInfo.Status = 2;
                FileInfo info = new FileInfo(fileMsgInfo.FilePath);
                OnDownload(fileMsgInfo.MessageId, info.Length, info.Length);
            }
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var fileMsgInfo = e.UserState as FileMessageInfo;
            if (e.BytesReceived != e.TotalBytesToReceive)
            {
                OnDownload(fileMsgInfo.MessageId, e.BytesReceived, e.TotalBytesToReceive);
            }
        }


        /// <summary>
        /// 断点下载，需服务器支持CustomRange
        /// </summary>
        /// <param name="fileURL"></param>
        /// <param name="filePath"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        private string Download_BreakPoint(string fileURL, string filePath, string msgId = "")
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
            info.Status = 1;
            m_FileMsgQueue.Add(info);
            info.Client.DownloadDataCompleted += Client_DownloadDataCompleted;
            info.Client.DownloadProgressChanged += webClient_DownloadProgressChanged;
            new TaskFactory().StartNew(() =>
            {
                info.Client.DownloadDataAsync(new Uri(fileURL), info);
            });
            return msgId;
        }

        void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var fileMsgInfo = e.UserState as FileMessageInfo;
            if (e.Error != null)
            {
                //传递到异常事件中去
                ProcessError(fileMsgInfo.MessageId, e.Error);
                //OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
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
                OnDownload(fileMsgInfo.MessageId, e.Result.Length, e.Result.Length);
            }
        }

        #endregion

        #region 取消

        /// <summary>
        /// 根据消息ID取消上传或下载的任务
        /// </summary>
        /// <param name="msgId">消息ID</param>
        /// <returns>是否取消成功</returns>
        [Debug]
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
                    //LogOpInfo("RunUserCallback:", responseInfo.ToJsonString());
                }
                catch (Exception ex)
                {
                    LogInfo("用户：回调异常", ex);
                    string runFunName = "未知方法";
                    try
                    {
                        runFunName = callback.Method.ReflectedType.Name + "." + callback.Method.Name;
                    }
                    catch
                    {

                    }
                    throw new Exception(string.Format("回调函数 {0} 异常", runFunName));//抛出用户异常
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
            if (OnError != null)
            {
                OnError(this, new ErrorEventArgs() { MsgId = msgId, ExceptionInfo = ex });
            }
            //收到网络异常且客户端已断开时触发重连
            if (ex is SocketException && !m_Client.Connected)
            {
                Disconnect();
                ReConnect();
            }
        }
        #endregion

        public void Dispose()
        {
            if (m_Client != null)
            {
                if (m_Client.Connected)
                {
                    // m_Client.Disconnect(false);
                    m_Client.Close();
                }
                m_Client.Dispose();
            }
        }
    }
}
