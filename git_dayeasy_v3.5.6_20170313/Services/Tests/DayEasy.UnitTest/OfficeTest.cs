using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DayEasy.Contracts;
using DayEasy.Core.Dependency;
using DayEasy.Office;
using DayEasy.Office.Models;
using DayEasy.UnitTest.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest
{
    [TestClass]
    public class OfficeTest : TestBase
    {
        [TestMethod]
        public void AnswerSheetTest()
        {
            var options = new Dictionary<string, int>();
            for (var i = 31; i < 81; i++)
            {
                options.Add(i.ToString(), 4);
            }
            var sheet = new AnswerSheetHelper(options);
            var bmp = sheet.GenerateBmp(4, AnswerSheetTips.TopLeft);
            bmp.Save("sheet.png", ImageFormat.Png);
        }

        [TestMethod]
        public void DcodeHelperTest()
        {
            var sheet = new DcodeHelper(zoom: 4);
            var bmp = sheet.Draw();
            bmp.Save("dcode.png", ImageFormat.Png);
        }

        [TestMethod]
        public void ExcelHelperTest()
        {
            var dt = new DataTable("sheet");
            dt.Columns.Add("row1", typeof(string));
            dt.Columns.Add("row2", typeof(string));
            dt.Columns.Add("row3", typeof(string));
            dt.Columns.Add("row4", typeof(string));
            dt.Columns.Add("row5", typeof(string));
            for (int i = 0; i < 50; i++)
            {
                dt.Rows.Add("得一科技有限公司" + i, "得一科技有限公司" + i, "得一科技有限公司" + i, "得一科技有限公司" + i, "得一科技有限公司" + i);
            }
            ExcelHelper.Export(new DataSet { Tables = { dt } }, "测试Excel", "excel");
        }

        [TestMethod]
        public void WordDw()
        {
            var str = "123                     456";
            Console.WriteLine(str);
            str = Regex.Replace(str, "\\s+", " ");
            Console.WriteLine(str);
        }



        [TestMethod]
        public void DownloadWordTest()
        {
            var paperResult = CurrentIocManager.Resolve<IPaperContract>().PaperDetailById("60be18c556194362a0460d962c44322f");
            var paper = paperResult.Data;
            paper.PaperSections = paper.PaperSections.Where(t => t.PaperSectionType == 1).ToList();
            paper.PaperBaseInfo.PaperTitle += "[A卷]";
            var word = paper.ToWdPaper();
            using (var helper = new WordHelper())
            {
                var zip = helper.DownLoadZip(new List<WdPaper> { word }, 8);
                using (var fs = new FileStream("card.zip", FileMode.OpenOrCreate))
                {
                    var bs = new BinaryWriter(fs);
                    bs.Write(zip.ToArray());
                    fs.Flush();
                    fs.Close();
                    zip.Close();
                }
            }
        }
    }
}
