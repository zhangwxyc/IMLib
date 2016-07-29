using System;
namespace BA.Framework.IMLib
{
    interface IIMServer
    {
        bool Cancel(string msgId);
        bool Connect(string host, int port, string userType, string userName, string userAgent, string accessToken, int lastTick, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        int ConnectRetryTimes { get; set; }
        void Disconnect();
        string Download(string fileURL, string filePath, string msgId = "");
        string Download_BreakPoint(string fileURL, string filePath, string msgId = "");
        bool Invite(string to, string group, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool IsAvailable { get; }
        bool Join(string group, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool Leave(string group);
        ILogger Log { set; }
        event Action OnDisconnect;
        event Action<string, long, long> OnDownload;
        event Action<object, ErrorEventArgs> OnError;
        event Action<MessageType, string, string, string, int, object> OnReceive;
        event Action OnReConnected;
        event Action<string, long, long> OnUpload;
        bool Send(MessageType type, string to, string group, object data, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool SendFile(MessageType type, string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool SendImage(string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool SendText(string to, string group, string content, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool SendVideo(string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool SendVoice(string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool Transfer(string to, string group, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        bool Undo(string to, string group, string msg_id);
        bool Upload(string msg_id, string upload_url, string path);
    }
}
