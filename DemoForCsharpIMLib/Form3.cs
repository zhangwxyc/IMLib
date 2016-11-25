using BA.Framework.IMLib;
using BA.Framework.IMLib.Message;
using System;
using System.Collections.Concurrent;
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

namespace DemoForCsharpIMLib
{
    public partial class Form3 : Form
    {
        IMServer m_server;
        public Form3()
        {
            InitializeComponent();
            m_server = new IMServer();
            m_server.Crypt_Version = "1.0";
            m_server.OnDisconnect += m_server_OnDisconnect;
            m_server.OnDownload += m_server_OnDownload;
            m_server.OnError += m_server_OnError;
            m_server.OnReceive += m_server_OnReceive;
            m_server.OnReConnected += m_server_OnReConnected;
            m_server.OnUpload += m_server_OnUpload;
            m_server.Log = new Logger();
            m_server.OnLog += m_server_OnLog;
            m_server.EnableEncrypt = true;
            m_server.ClientPingMode = BA.Framework.IMLib.Enums.PingMode.Passive;
            m_server.PingInterval = 10;
        }

        //RequestInfo EnData(RequestInfo info)
        //{
        //    if (m_server.Crypt_Version == "2.0")
        //    {
        //        info.Data = CommonCryptoService.Encrypt_DES(CommonExt.DynamicToJsonString(info.Data), info.MessageId);
        //        return info;
        //    }
        //    else
        //        return info;
        //}

        //dynamic DeData(BaseMessageInfo key)
        //{
        //    if (m_server.Crypt_Version == "2.0")
        //    {
        //        dynamic deString = CommonCryptoService.Decrypt_DES(key.Data.ToString(), key.MessageId);
        //        return CommonExt.JsonStringToDynamic(deString.ToString());
        //    }
        //    else
        //        return key.Data;
        //}

        void m_server_OnLog(string arg1, string arg2)
        {
            Log(string.Format("{0} -- {1}:{2}", DateTime.Now, arg1, arg2));
        }
        ConcurrentDictionary<string, long> uploadProcessList = new ConcurrentDictionary<string, long>();
        void m_server_OnUpload(string arg1, long arg2, long arg3)
        {
            //if (arg2 == arg3)
            //{
            //    Log(string.Format("File_Upload_{0}:{1} finish;", arg1, arg3));
            //}
            //else if (uploadProcessList.Keys.Contains(arg1))
            //{
            //    if (uploadProcessList[arg1] < arg3 / 2 && arg3 / 2 < arg2)
            //    {
            //        uploadProcessList[arg1] = arg2;
            //        Log(string.Format("File_Upload_{0}:{1}...;" , arg1, arg2));
            //    }
            //}
            //else
            //{  
            //    uploadProcessList.TryAdd(arg1, arg2);
            //    Log(string.Format("File_Upload_{0}:{1}...;", arg1, arg2));
            //}



            LogAdv(string.Format("File_Upload_{0}:(.*) current", arg1), string.Format("File_Upload_{0}:{1} current", arg1, arg2));

            if (arg2 == arg3)
            {
                //上传完成
                // Log(string.Format("File_Upload:{0} finish;", arg1));
                LogAdv(string.Format("File_Upload_{0}:(.*) current", arg1), string.Format("File_Upload_{0}:{1} finish", arg1, arg3));
            }

        }
        void m_server_OnDownload(string arg1, long arg2, long arg3)
        {
            LogAdv(string.Format("File_Download_{0}:(.*) current", arg1), string.Format("File_Download_{0}:{1} current", arg1, arg2));
            if (arg2 == arg3)
            {
                //完成
                LogAdv(string.Format("File_Download_{0}:(.*) current", arg1), string.Format("{2}_File_Download_{0}:{1} finish", arg1, arg3, DateTime.Now));
            }
        }
        void m_server_OnReConnected()
        {
            Log(string.Format("ReConnected success"));
        }

        void m_server_OnReceive(MessageType arg1, string from, string group, string msg_id, int msg_time, object data)
        {
            // Log(string.Format("Server:{0}", new { type = arg1, from = from, group = group, msg_id = msg_id, msg_time = msg_time, data = data }.ToJsonString()));
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
            Log(string.Format("{0} -- {1}:{2}", DateTime.Now, arg2.MsgId, arg2.ExceptionInfo.Message));
            //Log(string.Format("MsgId:{0},Error:{1}", arg2.MsgId, arg2.ExceptionInfo.Message));
        }



        void m_server_OnDisconnect()
        {
            Log(string.Format("Disconnect"));
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            m_server.EnableEncrypt = cbEncode.Checked;
            if (cbEncode.Checked)
            {
                m_server.Crypt_Version = "2";
            }
            else
            {
                m_server.Crypt_Version = "1.0";
            }
            bool isConnected = m_server.Connect(tb_Ip.Text, int.Parse(tb_Port.Text), tbUtype.Text, tb_userName.Text, "DEVICEID:32443234234234;PUSHCODE:2342342342342", tb_token.Text, 0, Connected);
        }




        private void btn_close_Click(object sender, EventArgs e)
        {
            m_server.Disconnect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public void Connected(RequestInfo rqInfo, ResponseAckInfo ackInfo)
        {
            Log(string.Format("ConnectedCallBack:{0}", ackInfo.ToJsonString()));
        }

        void btn_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            MethodInfo mInfo = btn.Tag as MethodInfo;
            var parList = mInfo.GetParameters();
            Dictionary<string, object> pararms = new Dictionary<string, object>();
            var callback = new Action<RequestInfo, ResponseAckInfo>((rqInfo, ackInfo) =>
             {
                 // Log(string.Format("client:{0}\r\nserver:{1}", rqInfo.ToJsonString(), ackInfo.ToJsonString()));
             });
            foreach (var item in parList)
            {
                if (item.Name.ToLower().Equals("group"))
                {
                    pararms.Add(item.Name, tb_group.Text);
                }
                else if (item.Name.ToLower().Equals("to"))
                {
                    pararms.Add(item.Name, tb_ToUser.Text);
                }
                else if (item.Name.ToLower().Equals("callback"))
                {
                    pararms.Add(item.Name, callback);
                }
                else if (item.Name.ToLower().Equals("path"))
                {
                    pararms.Add(item.Name, tbFilePath.Text);
                }
                else
                {
                    pararms.Add(item.Name, "");
                }
            }
            //SelectParams selectDig = new SelectParams(pararms);
            //if (selectDig.ShowDialog() == DialogResult.OK)
            //{
            //    mInfo.Invoke(m_server, selectDig.Pararms.ToArray());
            //}

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
            if (cbIsLoop.Checked)
            {
                StartLoop(m_server.SendText, tb_ToUser.Text, tb_group.Text, tb_Txt.Text, (rqInfo, ackInfo) =>
            {
                Log(string.Format("SendTxtCallBack:{0}", ackInfo.ToJsonString()));
                // Log(string.Format("client:{0}\r\nserver:{1}", rqInfo.ToJsonString(), ackInfo.ToJsonString()));
            });
                return;
            }

            m_server.SendText(tb_ToUser.Text, tb_group.Text, tb_Txt.Text, (rqInfo, ackInfo) =>
            {
                Log(string.Format("SendTxtCallBack:{0}", ackInfo.ToJsonString()));
                // Log(string.Format("client:{0}\r\nserver:{1}", rqInfo.ToJsonString(), ackInfo.ToJsonString()));
            });
        }
        string filePath = "C:\\LoopTestLog.txt";
        private void StartLoop(Func<string, string, string, Action<RequestInfo, ResponseAckInfo>, string> func, string p1, string p2, string p3, Action<RequestInfo, ResponseAckInfo> p4)
        {
            int loopCount = 1000;
            if (!string.IsNullOrWhiteSpace(tb_Loop.Text))
            {
                int.TryParse(tb_Loop.Text, out loopCount);
            }

            int innveralTime = 0;
            if (!string.IsNullOrWhiteSpace(tb_Innerval.Text))
            {
                int.TryParse(tb_Innerval.Text, out innveralTime);
            }

            Log(string.Format("将执行 {0} 次测试", loopCount));
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception)
            {

            }

            Task.Factory.StartNew(() =>
                {
                    for (int index = 0; index < loopCount; index++)
                    {
                        if (!ReadCheckBox(cbIsLoop))
                        {
                            Log(string.Format("测试中断： {0}", index + 1));
                            break;
                        }
                        string sendData = p3 + (index + 1);
                        func(p1, p2, sendData, (rqInfo, ackInfo) =>
                        {
                            Log(string.Format("SendTxtCallBack:{0}", ackInfo.ToJsonString()));
                            LoopTestLogToFile(rqInfo, ackInfo, sendData);
                        });
                        if (innveralTime > 0)
                        {
                            System.Threading.Thread.Sleep(1000 * innveralTime);
                        }

                    }
                });
        }

        public bool ReadCheckBox(CheckBox control)
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<CheckBox, bool>(ReadCheckBox), control);
            }
            else
            {
                return control.Checked;
            }
        }
        public string ReadControlText(Control control)
        {
            if (this.InvokeRequired)
            {
                return (string)this.Invoke(new Func<Control, string>(ReadControlText), control);
            }
            else
            {
                return control.Text;
            }
        }

        private void LoopTestLogToFile(RequestInfo rqInfo, ResponseAckInfo ackInfo, string sendData)
        {
            System.IO.File.AppendAllText(filePath, string.Format("{0}|{1}|{2}|{3}\r\n", sendData, ackInfo.MessageId, ackInfo.Status, DateTime.Now.ToString()));
        }






        public void Log(string message)
        {
            tb_Log.CrossThreadCalls(() =>
                {
                    tb_Log.AppendText(string.Format("--------------------------------------------\r\n{0}\r\n--------------------------------------------\r\n\r\n", message));
                    tb_Log.ScrollToCaret();
                });
        }
        public void LogAdv(string key, string message)
        {
            tb_Log.CrossThreadCalls(() =>
                {
                    if (Regex.IsMatch(tb_Log.Text, key))
                    {
                        tb_Log.Text = Regex.Replace(tb_Log.Text, key, message);
                    }
                    else
                    {
                        tb_Log.AppendText(string.Format("--------------------------------------------\r\n{0}\r\n--------------------------------------------\r\n\r\n", message));
                    }
                    //
                    tb_Log.ScrollToCaret();
                });

        }

        private void btn_clearLog_Click(object sender, EventArgs e)
        {
            tb_Log.Text = "";
        }

        private void btn_Custom_Click(object sender, EventArgs e)
        {
            try
            {
                var data = tbCustom.Text.ToObject<Dictionary<string, object>>();
            }
            catch (Exception)
            {
                MessageBox.Show("自定义消息的data 不正确");
                return;
            }

            m_server.Send(MessageType.Custom, tb_ToUser.Text, tb_group.Text, tbCustom.Text.ToObject<Dictionary<string, object>>(), (rqInfo, ackInfo) =>
            {
                Log(string.Format("SendCustomCallBack:{0}", ackInfo.ToJsonString()));
            });
        }

       public  CryptForm childForm;

        private void btn_Crypt_Click(object sender, EventArgs e)
        {
            if (childForm==null)
            {
                childForm = new CryptForm();
            }
            childForm.Show();
            childForm.Activate();
        }


        /// <summary>
        /// 判断窗体是否已打开
        /// </summary>
        private bool IsExistForm(Form frm)
        {
            if (frm == null) return false;
            foreach (Form childFrm in this.MdiChildren)
            {
                //用子窗体的Name进行判断，如果存在则将他激活
                if (childFrm.Text == frm.Text)
                {
                    childFrm.Activate();
                    return true;
                }
            }
            return false;
        }
    }
}
