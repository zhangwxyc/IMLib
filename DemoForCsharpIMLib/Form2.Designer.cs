namespace DemoForCsharpIMLib
{
    partial class Form2
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tb_Ip = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_Port = new System.Windows.Forms.TextBox();
            this.btn_Connect = new System.Windows.Forms.Button();
            this.btn_close = new System.Windows.Forms.Button();
            this.tb_userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_Log = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_ToUser = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tb_Txt = new System.Windows.Forms.TextBox();
            this.btn_SendTxt = new System.Windows.Forms.Button();
            this.btn_SendFile = new System.Windows.Forms.Button();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_group = new System.Windows.Forms.TextBox();
            this.btn_clearLog = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_token = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tb_Ip
            // 
            this.tb_Ip.Location = new System.Drawing.Point(158, 46);
            this.tb_Ip.Name = "tb_Ip";
            this.tb_Ip.Size = new System.Drawing.Size(136, 21);
            this.tb_Ip.TabIndex = 0;
            this.tb_Ip.Text = "192.168.87.21";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(58, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "服务器信息：";
            // 
            // tb_Port
            // 
            this.tb_Port.Location = new System.Drawing.Point(300, 46);
            this.tb_Port.Name = "tb_Port";
            this.tb_Port.Size = new System.Drawing.Size(72, 21);
            this.tb_Port.TabIndex = 2;
            this.tb_Port.Text = "8282";
            // 
            // btn_Connect
            // 
            this.btn_Connect.Location = new System.Drawing.Point(483, 49);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(75, 56);
            this.btn_Connect.TabIndex = 3;
            this.btn_Connect.Text = "连接";
            this.btn_Connect.UseVisualStyleBackColor = true;
            this.btn_Connect.Click += new System.EventHandler(this.btn_Connect_Click);
            // 
            // btn_close
            // 
            this.btn_close.Location = new System.Drawing.Point(575, 49);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(75, 56);
            this.btn_close.TabIndex = 4;
            this.btn_close.Text = "断开";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // tb_userName
            // 
            this.tb_userName.Location = new System.Drawing.Point(158, 84);
            this.tb_userName.Name = "tb_userName";
            this.tb_userName.Size = new System.Drawing.Size(100, 21);
            this.tb_userName.TabIndex = 5;
            this.tb_userName.Text = "star";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(58, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "用户名：";
            // 
            // tb_Log
            // 
            this.tb_Log.Location = new System.Drawing.Point(60, 182);
            this.tb_Log.Multiline = true;
            this.tb_Log.Name = "tb_Log";
            this.tb_Log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_Log.Size = new System.Drawing.Size(720, 305);
            this.tb_Log.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(58, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "Log：";
            // 
            // tb_ToUser
            // 
            this.tb_ToUser.Location = new System.Drawing.Point(111, 515);
            this.tb_ToUser.Name = "tb_ToUser";
            this.tb_ToUser.Size = new System.Drawing.Size(129, 21);
            this.tb_ToUser.TabIndex = 9;
            this.tb_ToUser.Text = "s";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(58, 518);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "ToUser:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(58, 577);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "文本:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(58, 628);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "文件:";
            // 
            // tb_Txt
            // 
            this.tb_Txt.Location = new System.Drawing.Point(111, 574);
            this.tb_Txt.Name = "tb_Txt";
            this.tb_Txt.Size = new System.Drawing.Size(288, 21);
            this.tb_Txt.TabIndex = 12;
            // 
            // btn_SendTxt
            // 
            this.btn_SendTxt.Location = new System.Drawing.Point(483, 572);
            this.btn_SendTxt.Name = "btn_SendTxt";
            this.btn_SendTxt.Size = new System.Drawing.Size(75, 23);
            this.btn_SendTxt.TabIndex = 13;
            this.btn_SendTxt.Text = "发送";
            this.btn_SendTxt.UseVisualStyleBackColor = true;
            this.btn_SendTxt.Click += new System.EventHandler(this.btn_SendTxt_Click);
            // 
            // btn_SendFile
            // 
            this.btn_SendFile.Location = new System.Drawing.Point(483, 628);
            this.btn_SendFile.Name = "btn_SendFile";
            this.btn_SendFile.Size = new System.Drawing.Size(75, 23);
            this.btn_SendFile.TabIndex = 13;
            this.btn_SendFile.Text = "发送";
            this.btn_SendFile.UseVisualStyleBackColor = true;
            this.btn_SendFile.Click += new System.EventHandler(this.btn_SendFile_Click);
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(111, 628);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(288, 21);
            this.tbFilePath.TabIndex = 12;
            this.tbFilePath.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tbFilePath_MouseDoubleClick);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(288, 524);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 15;
            this.label8.Text = "Group:";
            // 
            // tb_group
            // 
            this.tb_group.Location = new System.Drawing.Point(348, 518);
            this.tb_group.Name = "tb_group";
            this.tb_group.Size = new System.Drawing.Size(129, 21);
            this.tb_group.TabIndex = 9;
            this.tb_group.Text = "a";
            // 
            // btn_clearLog
            // 
            this.btn_clearLog.Location = new System.Drawing.Point(705, 49);
            this.btn_clearLog.Name = "btn_clearLog";
            this.btn_clearLog.Size = new System.Drawing.Size(75, 56);
            this.btn_clearLog.TabIndex = 16;
            this.btn_clearLog.Text = "清空log";
            this.btn_clearLog.UseVisualStyleBackColor = true;
            this.btn_clearLog.Click += new System.EventHandler(this.btn_clearLog_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(58, 124);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "Token：";
            // 
            // tb_token
            // 
            this.tb_token.Location = new System.Drawing.Point(158, 122);
            this.tb_token.Name = "tb_token";
            this.tb_token.Size = new System.Drawing.Size(214, 21);
            this.tb_token.TabIndex = 5;
            this.tb_token.Text = "90f7b950-102b-4396-a131-7433c1d33aea";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 682);
            this.Controls.Add(this.btn_clearLog);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btn_SendFile);
            this.Controls.Add(this.btn_SendTxt);
            this.Controls.Add(this.tbFilePath);
            this.Controls.Add(this.tb_Txt);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tb_group);
            this.Controls.Add(this.tb_ToUser);
            this.Controls.Add(this.tb_Log);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_token);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_userName);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.btn_Connect);
            this.Controls.Add(this.tb_Port);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_Ip);
            this.Name = "Form2";
            this.Text = "DEMO";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_Ip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_Port;
        private System.Windows.Forms.Button btn_Connect;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.TextBox tb_userName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_Log;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_ToUser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tb_Txt;
        private System.Windows.Forms.Button btn_SendTxt;
        private System.Windows.Forms.Button btn_SendFile;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tb_group;
        private System.Windows.Forms.Button btn_clearLog;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_token;
    }
}

