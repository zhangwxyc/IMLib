using BA.Framework.IMLib;
using BA.Framework.IMLib.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoForCsharpIMLib
{
    public partial class Form1 : Form
    {
        IMServer m_server;
        public Form1()
        {
            InitializeComponent();
            m_server = new IMServer();
            m_server.OnDisconnect += m_server_OnDisconnect;
            m_server.OnDownload += m_server_OnDownload;
            m_server.OnError += m_server_OnError;
            m_server.OnReceive += m_server_OnReceive;
            m_server.OnReConnected += m_server_OnReConnected;
            m_server.OnUpload += m_server_OnUpload;
            m_server.Log = new Logger();
        }

        void m_server_OnUpload(string arg1, long arg2, long arg3)
        {
            if (arg2 == arg3)
            {
                //上传完成
                Log(string.Format("File_Upload:{0} finish", arg1));
            }
        }
        void m_server_OnDownload(string arg1, long arg2, long arg3)
        {
            if (arg2 == arg3)
            {
                //完成
                Log(string.Format("File_Download:{0} finish", arg1));
            }
        }
        void m_server_OnReConnected()
        {
            Log(string.Format("ReConnected success"));
        }

        void m_server_OnReceive(MessageType arg1, string from, string group, string msg_id, int msg_time, object data)
        {
            Log(string.Format("Server:{0}", new { type = arg1, from = from, group = group, msg_id = msg_id, msg_time = msg_time, data = data }.ToJsonString()));
            switch (arg1)
            {
                case MessageType.Ack:
                    break;
                case MessageType.Text:
                    break;
                case MessageType.Image:
                    break;
                case MessageType.Voice:
                    break;
                case MessageType.Video:
                    break;
                case MessageType.File:
                    break;
                case MessageType.Invite:
                    break;
                case MessageType.Join:
                    break;
                case MessageType.Leave:
                    break;
                case MessageType.Transfer:
                    break;
                case MessageType.Link:
                    break;
                case MessageType.Custom:
                    break;
                case MessageType.UnKnown:
                    break;
                default:
                    break;
            }
        }

        void m_server_OnError(object arg1, ErrorEventArgs arg2)
        {
            Log(string.Format("MsgId:{0},Error:{1}", arg2.MsgId, arg2.ExceptionInfo.Message));
        }



        void m_server_OnDisconnect()
        {
            Log(string.Format("Disconnect"));
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            bool isConnected = m_server.Connect(tb_Ip.Text, int.Parse(tb_Port.Text), "ut", tb_userName.Text, Environment.UserName, Guid.NewGuid().ToString(), Connected);
        }


        public void Connected(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            Log(string.Format("client:{0}\r\nserver:{1}", rqInfo.ToJsonString(), ackInfo.ToJsonString()));
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            m_server.Disconnect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void tbFilePath_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog dig = new OpenFileDialog();
            if (dig.ShowDialog() == DialogResult.OK)
            {
                tbFilePath.Text = dig.FileName;
            }
        }

        private void btn_SendFile_Click(object sender, EventArgs e)
        {
            m_server.SendImage(tb_ToUser.Text, tb_group.Text, tbFilePath.Text, null);

        }

        private void btn_SendTxt_Click(object sender, EventArgs e)
        {
            m_server.SendText(tb_ToUser.Text, tb_group.Text, tb_Txt.Text, (rqInfo, ackInfo) =>
            {
                Log(string.Format("client:{0}\r\nserver:{1}", rqInfo.ToJsonString(), ackInfo.ToJsonString()));
            });
        }

        public void Log(string message)
        {
            tb_Log.CrossThreadCalls(() =>
                {
                    tb_Log.AppendText(string.Format("--------------------------------------------\r\n{0}\r\n--------------------------------------------\r\n\r\n", message));
                    tb_Log.ScrollToCaret();
                });
        }


    }
}
