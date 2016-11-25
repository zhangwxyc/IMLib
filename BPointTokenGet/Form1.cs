using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BPointTokenGet
{
    public partial class Form1 : Form
    {
        private string BaseUrl = "http://crm.maiche.biz/Ocim/Api";
        public Form1()
        {
            InitializeComponent();
            SecurityValue = "7DD6A2462AA641B6844CBFB00D6DC8A4";
        }

        private void btn_get_Click(object sender, EventArgs e)
        {
            string timespan = ParasAuthentication.ToUnixTime(DateTime.Now).ToString();


            Dictionary<string, string> data = new Dictionary<string, string>
	        {
		        {
			        "UserName",
				    tb_Name.Text
		        },
		        {
			        "SecurePassword",
			        DesHelper.DES3Encrypt(tb_pwd.Text, "")
		        }
	        };
            string body = JsonConvert.SerializeObject(data);

            Dictionary<string, string> parameters = new Dictionary<string, string>
		    {
			    {
				    "[Body]",
				    body
			    },
			    {
				    "Timespan",
				    timespan
			    }
		    };
            string sign = ParasAuthentication.CreateSign(parameters, this.SecurityValue);

            Dictionary<string, string> headers = new Dictionary<string, string>
		    {
			    {
				    "AppVersion",
				    "1.0.0.10"
			    },
			    {
				    "Signature",
				    sign
			    },
			    {
				    "Timespan",
				    timespan
			    }
		    };

            string resultStr = HttpHelper.HttpPost(string.Format("{0}/login/", this.BaseUrl), body
            ,headers);
            tb_token.Text = resultStr;
        }

        public string SecurityValue { get; set; }
    }
}
