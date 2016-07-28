using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkChange
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btn_Network.BackColor = Color.LightGreen;

        }

        private void btn_Network_Click(object sender, EventArgs e)
        {
            if (btn_Network.BackColor != Color.LightGreen)
            {
                NetworkManager.Control(true);
                btn_Network.BackColor = Color.LightGreen;
            }
            else
            {
                NetworkManager.Control(false);
                btn_Network.BackColor = Color.Red;
            }

        }
    }
}
