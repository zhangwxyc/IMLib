using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZLX.Framework.ToolKit.DP_Static;
namespace DataReaderAnysle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dig = new OpenFileDialog();
            if (dig.ShowDialog() == DialogResult.OK)
            {
                tb_filePath.Text = dig.FileName;
            }
        }

        private void btn_Create_Click(object sender, EventArgs e)
        {
            using (FileStream fs = new FileStream(tb_filePath.Text, FileMode.Open, FileAccess.Read))
            {
                XSSFWorkbook hssfworkbook = new XSSFWorkbook(fs);
                ISheet sheet = hssfworkbook.GetSheetAt(0);

                //表头  
                IRow header = sheet.GetRow(sheet.FirstRowNum);

                // Dictionary<Guid, decimal> updateDatas = new Dictionary<Guid, decimal>();
                List<dynamic> infos = new List<dynamic>();
                DataTable dt = new DataTable("s1");
                for (int index = 0; index < header.Cells.Count; index++)
                {
                    dt.Columns.Add(header.GetCell(index).ToString());
                }

                for (int rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    for (int colIndex = 0; colIndex < header.LastCellNum; colIndex++)
                    {
                        dr[colIndex] = sheet.GetRow(rowIndex).Cells[colIndex].ToString();
                        //string dsIdString = sheet.GetRow(rowIndex).Cells[colIndex].ToString();
                        //Guid dsId = Guid.Empty;
                        //bool isData = Guid.TryParse(dsIdString, out dsId);
                        //if (!isData)
                        //{
                        //    continue;
                        //}

                        //string valueString = sheet.GetRow(rowIndex).Cells[colIndex + 1].StringCellValue;
                        //decimal value = decimal.Parse(valueString == "" ? "0" : valueString);
                        //updateDatas.Add(dsId, value);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
                List<ItemInfo> itemInfos = ZLX.Framework.ToolKit.DP_Static.CustomProcess.DTToList<ItemInfo>(dt);

                var g = itemInfos.GroupBy(x => x.工号).Select(x => new { Key = x.Key, i = x.ToList(), l1 = x.Count(y => int.Parse(y.总分) > 85), l2 = x.Count(y => int.Parse(y.总分) <= 85 && int.Parse(y.总分) > 0), l3 = x.Count(y => int.Parse(y.总分) == 0) }).ToList();

            }



        }
    }
}
