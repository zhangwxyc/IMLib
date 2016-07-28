namespace NetworkChange
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
            this.btn_Network = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_Network
            // 
            this.btn_Network.Location = new System.Drawing.Point(74, 44);
            this.btn_Network.Name = "btn_Network";
            this.btn_Network.Size = new System.Drawing.Size(101, 73);
            this.btn_Network.TabIndex = 4;
            this.btn_Network.Text = "网络";
            this.btn_Network.UseVisualStyleBackColor = true;
            this.btn_Network.Click += new System.EventHandler(this.btn_Network_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 162);
            this.Controls.Add(this.btn_Network);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_Network;

    }
}

