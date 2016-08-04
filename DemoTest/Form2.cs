using BA.Framework.IMLib;
using BA.Framework.IMLib.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoTest
{
    public partial class Form2 : Form
    {
        IMServer m_server;
        public Form2()
        {
            InitializeComponent();
            m_server = new IMServer();
            m_server.OnDisconnect += m_server_OnDisconnect;
            m_server.OnDownload += m_server_OnDownload;
            m_server.OnError += m_server_OnError;
            m_server.OnReceive += m_server_OnReceive;
            m_server.OnReConnected += m_server_OnReConnected;
            m_server.OnUpload += m_server_OnUpload;
        }
        private void btn_Connect_Click(object sender, EventArgs e)
        {
            bool isConnected = m_server.Connect(tb_Ip.Text, int.Parse(tb_Port.Text), "ut", tb_userName.Text, Environment.UserName, Guid.NewGuid().ToString(), 0, CallbackFun);
        }
        public void CallbackFun(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            Log(string.Format("client:{0}\r\nserver:{1}", rqInfo.ToJsonString(), ackInfo.ToJsonString()));
        }

        private void btn_SendTxt_Click(object sender, EventArgs e)
        {
            m_server.SendText(tb_ToUser.Text, tb_group.Text, tb_Txt.Text, CallbackFun);
        }

        private void btn_SendFile_Click(object sender, EventArgs e)
        {
            m_server.SendImage(tb_ToUser.Text, tb_group.Text, tbFilePath.Text, null);
        }
        void m_server_OnUpload(string arg1, long arg2, long arg3)
        {
            Log(string.Format("File_Upload_{0}:{1}/{2}", arg1,arg2,arg3));
        }
        void m_server_OnDownload(string arg1, long arg2, long arg3)
        {
            Log(string.Format("File_Download_{0}:{1}/{2}", arg1, arg2, arg3));
        }
        void m_server_OnReConnected()
        {
            Log(string.Format("ReConnected success"));
        }

        void m_server_OnReceive(MessageType arg1, string from, string group, string msg_id, int msg_time, object data)
        {
            Log(string.Format("Receive:{0}", new { type = arg1, from = from, group = group, msg_id = msg_id, msg_time = msg_time, data = data }.ToJsonString()));
        }

        void m_server_OnError(object arg1, ErrorEventArgs arg2)
        {
            //Log(string.Format("{0} -- {1}:{2}", DateTime.Now, arg2.MsgId, arg2.ExceptionInfo.Message));
            Log(string.Format("MsgId:{0},Error:{1}", arg2.MsgId, arg2.ExceptionInfo.Message));
        }

        void m_server_OnDisconnect()
        {
            Log(string.Format("Disconnect"));
        }


        private void btn_close_Click(object sender, EventArgs e)
        {
            m_server.Disconnect();
        }

        private void tbFilePath_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog dig = new OpenFileDialog();
            if (dig.ShowDialog() == DialogResult.OK)
            {
                tbFilePath.Text = dig.FileName;
            }
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
