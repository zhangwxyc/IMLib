namespace BPointTokenGet
{
    partial class Form1
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
            this.btn_get = new System.Windows.Forms.Button();
            this.tb_Name = new System.Windows.Forms.TextBox();
            this.tb_pwd = new System.Windows.Forms.TextBox();
            this.tb_token = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_get
            // 
            this.btn_get.Location = new System.Drawing.Point(284, 37);
            this.btn_get.Name = "btn_get";
            this.btn_get.Size = new System.Drawing.Size(75, 54);
            this.btn_get.TabIndex = 0;
            this.btn_get.Text = "生成";
            this.btn_get.UseVisualStyleBackColor = true;
            this.btn_get.Click += new System.EventHandler(this.btn_get_Click);
            // 
            // tb_Name
            // 
            this.tb_Name.Location = new System.Drawing.Point(73, 37);
            this.tb_Name.Name = "tb_Name";
            this.tb_Name.Size = new System.Drawing.Size(141, 21);
            this.tb_Name.TabIndex = 1;
            this.tb_Name.Text = "zhangwx";
            // 
            // tb_pwd
            // 
            this.tb_pwd.Location = new System.Drawing.Point(73, 85);
            this.tb_pwd.Name = "tb_pwd";
            this.tb_pwd.Size = new System.Drawing.Size(141, 21);
            this.tb_pwd.TabIndex = 2;
            this.tb_pwd.Text = "207.XVm";
            this.tb_pwd.UseSystemPasswordChar = true;
            // 
            // tb_token
            // 
            this.tb_token.Location = new System.Drawing.Point(73, 142);
            this.tb_token.Multiline = true;
            this.tb_token.Name = "tb_token";
            this.tb_token.Size = new System.Drawing.Size(293, 50);
            this.tb_token.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 219);
            this.Controls.Add(this.tb_token);
            this.Controls.Add(this.tb_pwd);
            this.Controls.Add(this.tb_Name);
            this.Controls.Add(this.btn_get);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_get;
        private System.Windows.Forms.TextBox tb_Name;
        private System.Windows.Forms.TextBox tb_pwd;
        private System.Windows.Forms.TextBox tb_token;
    }
}

