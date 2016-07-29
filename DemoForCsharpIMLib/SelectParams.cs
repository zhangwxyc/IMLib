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
    public partial class SelectParams : Form
    {
        public SelectParams(Dictionary<string, string> paramList)
        {
            InitializeComponent();
            ParamList = paramList;
        }
        public Dictionary<string,string> ParamList { get; set; }
        private void btnOk_Click(object sender, EventArgs e)
        {

            int rowCount = ParamList.Count;


            panel_table.RowCount = rowCount;    //设置分成几行  

            panel_table.ColumnCount = colCount;    //设置分成几列  
            //for (int i = 0; i < colCount; i++)
            //{
            //    panel_table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            //}  

            //panel_table.AutoSize = true;
            panel_table.Margin = new Padding(5);
            foreach (var item in list)
            {
                var btn = new Button();
                btn.Click += btn_Click;
                btn.Dock = DockStyle.Fill;
                btn.Text = item.Name;
                btn.Tag = item;
                panel_table.Controls.Add(btn);
            }
        }
    }
}
