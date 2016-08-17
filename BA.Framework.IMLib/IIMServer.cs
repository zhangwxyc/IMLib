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
        //string Download_BreakPoint(string fileURL, string filePath, string msgId = "");
        string Invite(string to, string group, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string Join(string group, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string Leave(string group);
        ILogger Log { set; }
        event Action OnDisconnect;
        event Action<string, long, long> OnDownload;
        event Action<object, ErrorEventArgs> OnError;
        event Action<MessageType, string, string, string, int, object> OnReceive;
        event Action OnReConnected;
        event Action<string, long, long> OnUpload;
        string Send(MessageType type, string to, string group, object data, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string SendFile(MessageType type, string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string SendImage(string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string SendText(string to, string group, string content, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string SendVideo(string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string SendVoice(string to, string group, string path, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string Transfer(string to, string group, Action<BA.Framework.IMLib.Message.RequestInfo, BA.Framework.IMLib.Message.ResponseAckInfo> callback);
        string Undo(string to, string group, string msg_id);
        void Upload(string msg_id, string upload_url, string path);
    }
}
