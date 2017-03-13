using System;
using System.Data;
using System.Globalization;
using System.IO;
using DayEasy.Utility.Extend;
using org.in2bits.MyXls;

namespace DayEasy.Office
{
    public static class ExcelHelper
    {
        /// <summary>
        /// 导出Excel - 支持多Sheet
        /// DataTable = Sheet
        /// DataTable.TableName = Sheet.Name
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="fileName">文件名.xls</param>
        /// <param name="savePath">保存路径，为空则使用流的方式下载</param>
        public static void Export(DataSet ds, string fileName, string savePath = "")
        {
            if (ds == null || ds.Tables.Count == 0 || fileName.IsNullOrEmpty())
                return;
            var xls = new XlsDocument
            {
                FileName = fileName
            };
            xls.SummaryInformation.Author = "DayEasy"; //文件作者
            xls.SummaryInformation.Subject = "DayEasy-DataExport"; //文件主题
            xls.DocumentSummaryInformation.Company = "www.dayeasy.net"; //文件公司信息

            //Excel首行样式
            var xf = xls.NewXF();
            xf.HorizontalAlignment = HorizontalAlignments.Centered; //居中
            xf.Font.ColorIndex = 0;
            xf.Font.FontName = "宋体";
            xf.Font.Bold = true;
            xf.Font.Height = 13 * 20; //大小
            xf.Pattern = 1; //背景 0(无色) 1(没有间隙的实色)
            xf.UseBorder = true;
            xf.TopLineStyle = xf.LeftLineStyle = xf.RightLineStyle = xf.BottomLineStyle = 1;
            xf.TopLineColor = xf.LeftLineColor = xf.RightLineColor = xf.BottomLineColor = Colors.Black;
            xf.PatternBackgroundColor = Colors.Default29;
            xf.PatternColor = Colors.Default29;

            var normalXf = xls.NewXF();
            normalXf.HorizontalAlignment = HorizontalAlignments.Centered; //居中
            normalXf.Font.ColorIndex = 0;
            normalXf.Font.FontName = "宋体";
            normalXf.Font.Height = 13 * 20; //大小
            normalXf.Pattern = 0; //背景 0(无色) 1(没有间隙的实色)
            normalXf.UseBorder = true;
            normalXf.TopLineStyle = normalXf.LeftLineStyle = normalXf.RightLineStyle = normalXf.BottomLineStyle = 1;
            normalXf.TopLineColor = normalXf.LeftLineColor = normalXf.RightLineColor = normalXf.BottomLineColor = Colors.Black;
            for (var i = 0; i < ds.Tables.Count; i++)
            {
                var dt = ds.Tables[i];
                if (dt == null || dt.Rows.Count == 0)
                    continue;
                var sheetName = dt.TableName.IsNotNullOrEmpty() ? dt.TableName : "sheet" + (i + 1);
                var sheet = xls.Workbook.Worksheets.Add(sheetName); //sheet 页
                var cells = sheet.Cells;
                for (var j = 0; j < dt.Rows.Count; j++)
                {
                    var drCells = dt.Rows[j].ItemArray;
                    for (var k = 0; k < drCells.Length; k++)
                    {
                        cells.Add(j + 1, k + 1, drCells[k] ?? string.Empty, j == 0 ? xf : normalXf);
                    }
                }
            }
            if (savePath.IsNotNullOrEmpty())
            {
                xls.Save(savePath);
                return;
            }
            xls.Send();
        }

        /// <summary>
        /// 解析Excel - 仅支持Excel2003.xls格式
        /// </summary>
        /// <param name="xls"></param>
        /// <returns></returns>
        public static DataSet Read(XlsDocument xls)
        {
            try
            {
                var ds = new DataSet { DataSetName = xls.FileName };

                foreach (var sheet in xls.Workbook.Worksheets)
                {
                    var dt = new DataTable(sheet.Name);

                    //当前Sheet的最大列
                    var cellLen = 0;
                    for (var i = 0; i < sheet.Rows.Count; i++)
                    {
                        if (!sheet.Rows.RowExists(ushort.Parse(i.ToString(CultureInfo.InvariantCulture))))
                            break;
                        var len = sheet.Rows[ushort.Parse(i.ToString(CultureInfo.InvariantCulture))].CellCount;
                        if (len > cellLen) 
                            cellLen = len;
                    }
                    for (var t = 0; t < cellLen; t++)
                    {
                        dt.Columns.Add(new DataColumn("column" + t, typeof(string)));
                    }

                    for (var j = 0; j < sheet.Rows.Count; j++)
                    {
                        if (!sheet.Rows.RowExists(ushort.Parse(j.ToString(CultureInfo.InvariantCulture))))
                            break;
                        var row = sheet.Rows[ushort.Parse(j.ToString(CultureInfo.InvariantCulture))];
                        var dr = dt.NewRow();
                        for (var k = 0; k < cellLen; k++)
                        {
                            if (row.CellCount > k)
                                dr[k] = row.GetCell(ushort.Parse((k + 1).ToString(CultureInfo.InvariantCulture))).Value;
                            else
                                dr[k] = string.Empty;
                        }
                        dt.Rows.Add(dr);
                    }
                    ds.Tables.Add(dt);
                }

                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static DataSet Read(string path)
        {
            try
            {
                var xls = new XlsDocument(path);
                return Read(xls);
            }
            catch
            {
                return null;
            }
        }

        public static DataSet Read(Stream stream)
        {
            try
            {
                var xls = new XlsDocument(stream);
                return Read(xls);
            }
            catch
            {
                return null;
            }
        }
    }
}
