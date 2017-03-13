using DayEasy.Office.Models;
using DayEasy.Utility.Extend;
using Novacode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DayEasy.Office
{
    public static class WordAssist
    {
        public static readonly Border TableBorder = new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black);

        /// <summary>
        /// 数字0~20的中文字符
        /// </summary>
        public static List<string> Chinese = new List<string> { "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十" };

        /// <summary>
        /// 公式过滤
        /// </summary>
        /// <param name="latex"></param>
        /// <returns></returns>
        public static string LatexFilter(this string latex)
        {
            if (latex.IsNullOrEmpty())
                return string.Empty;
            latex = Regex.Replace(latex, "%08", "\b");
            latex = Regex.Replace(latex, "%0D", "\r");
            latex = Regex.Replace(latex, "\\s+", " ");
            latex = System.Web.HttpUtility.HtmlDecode(latex);

            var reg = new Regex("(\\\\\\[)|(\\\\\\])|(\\\\\\()|(\\\\\\))|(\\$)|(\\\\mathop)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            latex = reg.Replace(latex, string.Empty);

            latex = Regex.Replace(latex, "\\\\begin\\{matrix\\}\\{", "\\begin{matrix}\\\\{");

            // \centerdot => .
            latex = Regex.Replace(latex, "centerdot", "cdot", RegexOptions.IgnoreCase);
            //特殊符号
            latex = Regex.Replace(latex, "＜", "<");
            latex = Regex.Replace(latex, "≤", "\\le");
            latex = Regex.Replace(latex, "＞", ">");
            latex = Regex.Replace(latex, "≥", "\\ge");
            return latex;
        }

        /// <summary> 贴码区图片 </summary>
        public static MemoryStream CodeStream()
        {
            var ms = new MemoryStream();
            var image = new Bitmap(70, 70);
            var g = Graphics.FromImage(image);
            var penDash = new Pen(Color.Black, 1)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Custom,
                DashPattern = new float[] { 2, 2 }
            };
            try
            {
                g.Clear(Color.White);
                g.DrawString("贴码区", new Font("微软雅黑", 10), new SolidBrush(Color.Black), 14, 28);
                g.DrawRectangle(penDash, 1, 1, 68, 68); //外边框
                image.Save(ms, ImageFormat.Png);
                image.Dispose();
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary> 得一号 图片 </summary>
        public static MemoryStream DcodeStream()
        {
            var ms = new MemoryStream();
            var helper = new DcodeHelper(5, 4);
            var image = helper.Draw();
            image.Save(ms, ImageFormat.Png);
            image.Dispose();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        /// <summary>
        /// 客观题图片
        /// </summary>
        /// <param name="paper"></param>
        /// <returns></returns>
        public static MemoryStream ObjectiveStream(WdPaper paper)
        {
            if (paper == null || paper.Sections == null || !paper.Sections.Any())
                return null;
            var data = new Dictionary<string, int>();

            foreach (var section in paper.Sections)
            {
                var questions = section.Questions.Where(q => q.IsObjective).ToList();
                if (!questions.Any())
                    continue;
                foreach (var question in questions)
                {
                    if (question.SmallQuestions == null || !question.SmallQuestions.Any())
                    {
                        //无小问
                        data.Add(question.Sort.ToString(CultureInfo.InvariantCulture), question.Answers.Count);
                    }
                    else
                    {
                        //小问
                        foreach (var small in question.SmallQuestions)
                        {
                            if (section.SmallRow)
                            {
                                data.Add(small.Sort.ToString(CultureInfo.InvariantCulture), small.Answers.Count);
                            }
                            else
                            {
                                data.Add(string.Format("{0}({1})", question.Sort, small.Sort), small.Answers.Count);
                            }
                        }
                    }
                }
            }
            if (data.Count == 0)
                return null;

            var helper = new AnswerSheetHelper(data);
            var ms = new MemoryStream();
            using (var bmp = helper.GenerateBmp(4, AnswerSheetTips.TopLeft))
            {
                bmp.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
            }
            return ms;
        }

        /// <summary>
        /// 初始化文档 - 边距、页脚
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static void Init(this DocX doc)
        {
            //边距
            doc.MarginTop = 30f;
            doc.MarginBottom = 30f;
            doc.MarginLeft = 35f;
            doc.MarginRight = 35f;
            //页脚
            doc.AddFooters();
            doc.Footers.even.InsertParagraph().Append("得一基础教育信息化云平台[www.dayeasy.net]").FontSize(9).Alignment
                = Alignment.right;
            doc.Footers.odd.InsertParagraph().Append("得一基础教育信息化云平台[www.dayeasy.net]").FontSize(9).Alignment
                = Alignment.right;
        }

        /// <summary>
        /// 设置表格边框
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Table SetBorder(this Table table)
        {
            table.SetBorder(TableBorderType.Left, TableBorder);
            table.SetBorder(TableBorderType.Top, TableBorder);
            table.SetBorder(TableBorderType.Right, TableBorder);
            table.SetBorder(TableBorderType.Bottom, TableBorder);
            return table;
        }

        /// <summary>
        /// 设置图片最大宽度
        /// </summary>
        /// <param name="pic"></param>
        /// <returns></returns>
        public static Picture SetMaxWh(this Picture pic)
        {
            if (pic == null || pic.Width <= 600) return pic;
            pic.Height = (600 * pic.Height) / pic.Width;
            pic.Width = 600;
            return pic;
        }

        /// <summary>
        /// 字符串替换
        /// </summary>
        /// <param name="str"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static string ReplaceSingle(this string str, string oldValue, string newValue)
        {
            var index = str.IndexOf(oldValue, StringComparison.Ordinal);
            return index <= -1 ? str : str.Remove(index, oldValue.Length).Insert(index, newValue);
        }

        /// <summary>
        /// 字符过滤
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StrFilter(this string str)
        {
            str = Regex.Replace(str, "<br[^>]*>", "\r\n", RegexOptions.IgnoreCase); //换行
            str = Regex.Replace(str, "\\\\vartriangle", "\\triangle"); //三角形
            str = Regex.Replace(str, "\\\\square", "\\boxempty"); //四边形
            return str;
        }

        /// <summary>
        /// 将物理文件转换为内存流
        /// </summary>
        /// <param name="path">物理文件路径</param>
        /// <returns></returns>
        public static MemoryStream ConvertToMemoryStream(this string path)
        {
            if (path.IsNullOrEmpty()) return null;
            try
            {
                return new MemoryStream(File.ReadAllBytes(path));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取网络图片流
        /// </summary>
        /// <param name="url">网络图片地址</param>
        /// <returns></returns>
        public static MemoryStream GetMemoryStream(this string url)
        {
            try
            {
                var response = WebRequest.Create(url).GetResponse();
                var stream = response.GetResponseStream();
                if (stream == null)
                    return null;
                var mStream = new MemoryStream();
                var bStream = new BufferedStream(stream);
                int len;
                var buffer = new byte[4096];
                do
                {
                    len = bStream.Read(buffer, 0, buffer.Length);
                    if (len > 0)
                        mStream.Write(buffer, 0, len);
                } while (len > 0);
                bStream.Close();
                mStream.Seek(0, SeekOrigin.Begin);
                return mStream;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解析body table 标签 转化为word表格
        /// </summary>
        /// <param name="body"></param>
        /// <param name="doc"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static string GetTable(this string body, DocX doc, out List<Table> tables)
        {
            tables = new List<Table>();
            if (body.IsNullOrEmpty())
                return string.Empty;

            var regTb = new Regex(@"(?:<table[^>]*>([\w\W]*?)</table>)");
            var regTr = new Regex(@"(?:<t[rh][^>]*>([\w\W]*?)</t[rh]>)");
            var regTd = new Regex(@"(?:<td[^>]*>([\w\W]*?)</td>)");
            var tMatchs = regTb.Matches(body);
            if (tMatchs.Count == 0)
                return body;
            foreach (Match mt in tMatchs)
            {
                if (!mt.Success) continue;
                var trList = new List<List<string>>();
                var rMatchs = regTr.Matches(mt.Value);
                if (rMatchs.Count > 0)
                {
                    foreach (Match mr in regTr.Matches(mt.Value))
                    {
                        if (!mr.Success) continue;
                        var tdList = new List<string>();
                        var dMatchs = regTd.Matches(mr.Value);
                        if (dMatchs.Count > 0)
                        {
                            tdList.AddRange(from Match md in dMatchs where md.Success select md.Value);
                        }
                        if (tdList.Any()) trList.Add(tdList);
                    }
                }
                if (!trList.Any()) continue;
                int r = trList.Count, c = trList.Max(d => d.Count);
                var table = doc.AddTable(r, c).SetBorder();
                table.SetBorder(TableBorderType.InsideH, TableBorder);
                table.SetBorder(TableBorderType.InsideV, TableBorder);
                table.Design = TableDesign.TableNormal;
                table.AutoFit = AutoFit.Contents;
                var i = 0;
                trList.ForEach(tds =>
                {
                    var j = 0;
                    tds.ForEach(td => table.Rows[i].Cells[j++].Paragraphs[0].AppendHtml(td));
                    i++;
                });
                tables.Add(table);
                body = body.Replace(mt.Value, string.Empty);
            }
            return body;
        }

        public static Bitmap PaperSheet(string sheets)
        {
            var dict = new Dictionary<string, int>();
            if (!string.IsNullOrWhiteSpace(sheets))
            {
                sheets = sheets.UrlDecode().Trim();
                var list = sheets.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                var reg = new Regex("^(\\d+)-(\\d+),(\\d+)$");
                foreach (var item in list)
                {
                    var arr = item.Split(',');
                    if (arr.Length != 2)
                        continue;
                    int count;
                    if (reg.IsMatch(item))
                    {
                        var match = reg.Match(item);
                        int start = match.Groups[1].Value.To(0),
                            end = match.Groups[2].Value.To(0);
                        count = match.Groups[3].Value.To(0);
                        if (count <= 0)
                            continue;
                        if (start <= end)
                        {
                            for (var i = start; i <= end; i++)
                            {
                                dict.Add(i.ToString(), count);
                            }
                        }
                        else
                        {
                            for (var i = start; i >= end; i--)
                            {
                                dict.Add(i.ToString(), count);
                            }
                        }
                    }
                    else
                    {
                        count = arr[1].To(0);
                        if (count <= 0) continue;
                        dict.Add(arr[0], count);
                    }
                }
            }
            var helper = new AnswerSheetHelper(dict);
            return helper.GenerateBmp(4, AnswerSheetTips.TopLeft);
        }

        public static MemoryStream DownLoadPaperSheet(string sheets)
        {
            var stream = new MemoryStream();
            using (var doc = DocX.Create(stream))
            using (var bmp = PaperSheet(sheets))
            {
                doc.Init();
                using (var tmpStream = new MemoryStream())
                {
                    bmp.Save(tmpStream, ImageFormat.Png);
                    tmpStream.Seek(0, SeekOrigin.Begin);
                    var pic = doc.AddImage(tmpStream).CreatePicture();
                    pic.Width = pic.Width / 4;
                    pic.Height = pic.Height / 4;
                    doc.InsertParagraph().AppendLine().AppendPicture(pic);
                    doc.Save();
                }
            }
            return stream;
        }
    }
}
