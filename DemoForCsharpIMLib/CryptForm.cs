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
    public partial class CryptForm : Form
    {
        public CryptForm()
        {
            InitializeComponent();
        }

        private void btnEncode_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tb_Data.Text)&&!string.IsNullOrWhiteSpace(tb_Key.Text))
            {
               tb_Data.Text= CommonCryptoService.Encrypt_DES(tb_Data.Text,tb_Key.Text);
            }
        }

        private void btnDecode_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tb_Data.Text)&&!string.IsNullOrWhiteSpace(tb_Key.Text))
            {
               tb_Data.Text= CommonCryptoService.Decrypt_DES(tb_Data.Text,tb_Key.Text);
            }
        }

    }
}
