namespace DemoForCsharpIMLib
{
    partial class CryptForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tb_Data = new System.Windows.Forms.TextBox();
            this.btnEncode = new System.Windows.Forms.Button();
            this.btnDecode = new System.Windows.Forms.Button();
            this.tb_Key = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tb_Data
            // 
            this.tb_Data.Location = new System.Drawing.Point(25, 58);
            this.tb_Data.Multiline = true;
            this.tb_Data.Name = "tb_Data";
            this.tb_Data.Size = new System.Drawing.Size(444, 295);
            this.tb_Data.TabIndex = 0;
            // 
            // btnEncode
            // 
            this.btnEncode.Location = new System.Drawing.Point(511, 36);
            this.btnEncode.Name = "btnEncode";
            this.btnEncode.Size = new System.Drawing.Size(75, 66);
            this.btnEncode.TabIndex = 1;
            this.btnEncode.Text = "加密";
            this.btnEncode.UseVisualStyleBackColor = true;
            this.btnEncode.Click += new System.EventHandler(this.btnEncode_Click);
            // 
            // btnDecode
            // 
            this.btnDecode.Location = new System.Drawing.Point(511, 153);
            this.btnDecode.Name = "btnDecode";
            this.btnDecode.Size = new System.Drawing.Size(75, 66);
            this.btnDecode.TabIndex = 2;
            this.btnDecode.Text = "解密";
            this.btnDecode.UseVisualStyleBackColor = true;
            this.btnDecode.Click += new System.EventHandler(this.btnDecode_Click);
            // 
            // tb_Key
            // 
            this.tb_Key.Location = new System.Drawing.Point(25, 22);
            this.tb_Key.Name = "tb_Key";
            this.tb_Key.Size = new System.Drawing.Size(444, 21);
            this.tb_Key.TabIndex = 3;
            // 
            // CryptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 378);
            this.Controls.Add(this.tb_Key);
            this.Controls.Add(this.btnDecode);
            this.Controls.Add(this.btnEncode);
            this.Controls.Add(this.tb_Data);
            this.Name = "CryptForm";
            this.Text = "CryptForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_Data;
        private System.Windows.Forms.Button btnEncode;
        private System.Windows.Forms.Button btnDecode;
        private System.Windows.Forms.TextBox tb_Key;
    }
}