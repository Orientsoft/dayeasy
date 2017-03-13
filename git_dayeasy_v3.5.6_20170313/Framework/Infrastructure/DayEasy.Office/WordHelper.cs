using DayEasy.Core;
using DayEasy.Office.Enum;
using DayEasy.Office.Models;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using Novacode;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DayEasy.Office
{
    public class WordHelper : IDisposable
    {
        private readonly ConcurrentDictionary<string, Stream> _streams = new ConcurrentDictionary<string, Stream>();
        private readonly ILogger _logger = LogManager.Logger<WordHelper>();
        //解析公式站点地址
        private readonly string _latexPath = Consts.Config.OpenSite + "mimetex/.tex?\\fs2 ";
        private MemoryStream _codeStream, //贴码区域图片
            _objectiveStream; //客观题图片

        /// <summary>
        /// 试卷下载(*.zip)
        /// </summary>
        /// <param name="type">下载类型：8答题卡、16原稿、64参考答案</param>
        /// <param name="papers">试卷列表</param>
        /// <returns>压缩文件流</returns>
        public MemoryStream DownLoadZip(List<WdPaper> papers, int type)
        {
            if (papers == null) return null;
            var files = new List<StreamFile>();
            papers.ForEach(paper =>
            {
                var tmp = DownLoad(paper, type);
                if (tmp != null && tmp.Any())
                    files.AddRange(tmp);
            });
            return !files.Any() ? null : ZipHelper.CreateZip(files);
        }

        /// <summary>
        /// 问题下载(*.zip)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public MemoryStream DownLoadZip(WdQuestionGroup group)
        {
            if (group == null || group.Subjects == null || !group.Subjects.Any()) return null;
            var error = false;
            group.Subjects.ForEach(s =>
            {
                if (s.Sections == null || !s.Sections.Any())
                {
                    error = true;
                    return;
                }
                s.Sections.ForEach(t =>
                {
                    if (t.Questions == null || !t.Questions.Any())
                    {
                        error = true;
                        return;
                    }
                    Init(t.Questions);
                });
            });
            if (error) return null;
            var stream = GetQuestions(group);
            if (stream == null) return null;
            var files = new List<StreamFile> { new StreamFile { FileName = group.Title + ".docx", Stream = stream } };
            var answerStream = GetAnswer(group);
            if (answerStream != null)
                files.Add(new StreamFile { FileName = group.Title + "-答案.docx", Stream = answerStream });

            return ZipHelper.CreateZip(files);
        }

        /// <summary>
        /// 试卷下载
        /// </summary>
        /// <param name="paper"></param>
        /// <param name="type"></param>
        private List<StreamFile> DownLoad(WdPaper paper, int type)
        {
            if (paper == null ||
                ((type & (byte)DowloadType.Card) == 0
                 && (type & (byte)DowloadType.Original) == 0
                 && (type & (byte)DowloadType.Answer) == 0))
                return null;

            paper.Score = paper.Sections.Sum(s => s.Questions.Sum(q => q.Score));
            paper.Sections.ForEach(s =>
            {
                if (s.Sort < 20)
                    s.Name = WordAssist.Chinese[s.Sort] + "." + s.Name;
                Init(s.Questions);
            });

            var files = new List<StreamFile>();
            _objectiveStream = null;

            if ((type & (byte)DowloadType.Card) > 0)
            {
                var st = GetCard(paper);
                if (st != null)
                    files.Add(new StreamFile { FileName = paper.Title + "-答题卡.docx", Stream = st });
            }
            if ((type & (byte)DowloadType.Original) > 0)
            {
                var st = GetManuscript(paper);
                if (st != null)
                    files.Add(new StreamFile { FileName = paper.Title + "-原卷.docx", Stream = st });
            }
            if ((type & (byte)DowloadType.Answer) > 0)
            {
                var st = GetAnswer(paper);
                if (st != null)
                    files.Add(new StreamFile { FileName = paper.Title + "-答案.docx", Stream = st });
            }
            return files;
        }

        /// <summary>
        /// 导出答题卡
        /// </summary>
        /// <returns></returns>
        private Stream GetCard(WdPaper paper)
        {
            try
            {
                var result = new MemoryStream();
                using (var doc = DocX.Create(result))
                {
                    var isEnglish = paper.SubjectId == 3;

                    doc.Init();
                    //                    QrCodeHeader(doc, paper);
                    DCodeHeader(doc, paper);
                    //客观题
                    if (_objectiveStream == null)
                        _objectiveStream = WordAssist.ObjectiveStream(paper);
                    if (_objectiveStream != null)
                    {
                        var tmpStream = new MemoryStream();
                        _objectiveStream.Seek(0, SeekOrigin.Begin);
                        _objectiveStream.CopyTo(tmpStream);
                        tmpStream.Seek(0, SeekOrigin.Begin);
                        var pic = doc.AddImage(tmpStream).CreatePicture();
                        pic.Width = pic.Width / 4;
                        pic.Height = pic.Height / 4;
                        doc.InsertParagraph().AppendLine().AppendPicture(pic);
                    }
                    var border = new Border(BorderStyle.Tcbs_single, BorderSize.five, 0, Color.Black);
                    //var borderBottom = new Border(BorderStyle.Tcbs_single, BorderSize.five, 0, Color.Black);

                    //填空题
                    var tkSections = paper.Sections.Where(s => s.Type == 7).ToList();
                    if (tkSections.Any())
                    {
                        tkSections.ForEach(s =>
                        {
                            doc.InsertParagraph().AppendLine().Append(s.Name).FontSize(11).Bold().AppendLine();
                            var tableTk = doc.InsertTable(1, 1);
                            tableTk.SetBorder(TableBorderType.Top, border);
                            tableTk.SetBorder(TableBorderType.Left, border);
                            tableTk.SetBorder(TableBorderType.Right, border);
                            tableTk.SetBorder(TableBorderType.Bottom, border);
                            tableTk.Design = TableDesign.TableNormal;
                            tableTk.AutoFit = AutoFit.Window;
                            var tkParagraph = tableTk.Rows[0].Cells[0].Paragraphs[0];
                            //                            tkParagraph.LineSpacingBefore = 1.8F;
                            s.Questions.OrderBy(q => q.Sort).ToList().ForEach(q =>
                            {
                                tkParagraph.Append(q.Sort + ".______________(" + q.Score.ToString("0.#") + "分)   ");
                            });
                        });
                    }

                    #region 主观题

                    paper.Sections.OrderBy(s => s.PSectionType).ThenBy(s => s.Sort).ToList().ForEach(s =>
                    {
                        if (s.Type == 7 || s.Questions.All(q => q.IsObjective))
                            return;
                        doc.InsertParagraph().AppendLine().Append(s.Name).FontSize(11).Bold().AppendLine();

                        //大题
                        s.Questions.OrderBy(q => q.Sort).ToList().ForEach(q =>
                        {
                            if (q.IsObjective)
                                return;
                            var tableQ = doc.InsertTable(1, 1);
                            tableQ.SetBorder(TableBorderType.Top, border);
                            tableQ.SetBorder(TableBorderType.Left, border);
                            tableQ.SetBorder(TableBorderType.Right, border);
                            tableQ.SetBorder(TableBorderType.Bottom, border);
                            tableQ.Design = TableDesign.TableNormal;
                            tableQ.AutoFit = AutoFit.Window;
                            var qParagraph = tableQ.Rows[0].Cells[0].Paragraphs[0];
                            var sortAndScore = string.Empty;
                            if (s.SmallRow && !q.SmallQuestions.IsNullOrEmpty())
                            {
                                sortAndScore += isEnglish ? ((char)(65 + q.Sort - 1)) + "." : string.Empty;
                            }
                            else
                            {
                                sortAndScore += q.Sort + ".";
                            }
                            sortAndScore += "(" + q.Score.ToString("0.#") + "分)";
                            qParagraph.Append(sortAndScore);

                            //有图片
                            if (q.ImgKeys != null && q.ImgKeys.Any())
                            {
                                qParagraph.AppendLine();
                                q.ImgKeys.ForEach(key =>
                                {
                                    var keyStream = _streams[key];
                                    if (keyStream != null)
                                    {
                                        var tmpStream = new MemoryStream();
                                        keyStream.Seek(0, SeekOrigin.Begin);
                                        keyStream.CopyTo(tmpStream);
                                        tmpStream.Seek(0, SeekOrigin.Begin);
                                        qParagraph.AppendPicture(doc.AddImage(tmpStream).CreatePicture().SetMaxWh()).Append(" ");
                                    }
                                });
                                qParagraph.AppendLine();
                            }

                            //有小问
                            if (q.SmallQuestions != null && q.SmallQuestions.Any())
                            {
                                if (s.SmallRow)
                                {
                                    q.SmallQuestions.ForEach(
                                        sq =>
                                            qParagraph.AppendLine()
                                                .Append(isEnglish ? "(" + sq.Sort + ")" : sq.Sort + "."));
                                }
                                else
                                {
                                    var sqSort = 1;
                                    q.SmallQuestions.ForEach(sq =>
                                    {
                                        qParagraph.AppendLine().Append("(" + (sqSort++) + ")");
                                        //小问图片
                                        if (sq.ImgKeys != null && sq.ImgKeys.Any())
                                        {
                                            qParagraph.AppendLine();
                                            sq.ImgKeys.ForEach(sk =>
                                            {
                                                var skStream = _streams[sk];
                                                if (skStream != null)
                                                {
                                                    var tmpSkStream = new MemoryStream();
                                                    skStream.Seek(0, SeekOrigin.Begin);
                                                    skStream.CopyTo(tmpSkStream);
                                                    tmpSkStream.Seek(0, SeekOrigin.Begin);
                                                    qParagraph.AppendPicture(
                                                        doc.AddImage(tmpSkStream).CreatePicture().SetMaxWh())
                                                        .Append(" ");
                                                }
                                            });
                                            qParagraph.AppendLine();
                                        }
                                    });
                                }
                            }
                            doc.InsertParagraph();
                        });
                    });

                    #endregion

                    doc.Save();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// 导出原稿
        /// </summary>
        /// <returns></returns>
        private Stream GetManuscript(WdPaper paper)
        {
            if (paper == null) return null;
            try
            {
                var result = new MemoryStream();
                using (var doc = DocX.Create(result))
                {
                    var isEnglish = paper.SubjectId == 3;

                    doc.Init();
                    //试卷基本信息
                    doc.InsertParagraph().Append("编号" + paper.Num).FontSize(10).Alignment = Alignment.right;
                    var pTitle = doc.InsertParagraph();
                    pTitle.Append(paper.Title).FontSize(12).Bold();
                    if (!paper.Simplify)
                        pTitle.Append(" (" + paper.Score.ToString("0.#") + "分)").FontSize(10).Color(Color.Red);
                    pTitle.Alignment = Alignment.center;
                    //版块
                    paper.Sections.OrderBy(u => u.PSectionType).ThenBy(u => u.Sort).ToList().ForEach(s =>
                    {
                        doc.InsertParagraph();
                        doc.InsertParagraph().Append(s.Name).FontSize(11).Bold();
                        //问题
                        s.Questions.OrderBy(q => q.Sort).ToList().ForEach(q =>
                        {
                            var showOption = q.ShowOption;
                            var tables = new List<Table>();
                            //题干
                            var p = doc.InsertParagraph().VerticalCenter();

                            var qSortStr = string.Empty;
                            if (s.SmallRow && !q.SmallQuestions.IsNullOrEmpty())
                            {
                                qSortStr += isEnglish ? ((char)(65 + q.Sort - 1)) + "." : string.Empty;
                            }
                            else
                            {
                                qSortStr += q.Sort + ".";
                            }
                            var qBody = paper.Simplify
                                ? qSortStr + q.Body
                                : qSortStr + q.Body + " (" + q.Score.ToString("0.#") + "分)";

                            tables.AddRange(PrintBody(qBody, doc, p, q.Keys, q.ImgKeys));
                            //小问
                            if (q.SmallQuestions != null && q.SmallQuestions.Any())
                            {
                                q.SmallQuestions.OrderBy(u => u.Sort).ToList().ForEach(sq =>
                                {
                                    var sortStr = s.SmallRow ? sq.Sort + ". " : string.Empty;

                                    tables.AddRange(PrintBody(("   " + sortStr + sq.Body), doc, p, sq.Keys, sq.ImgKeys));
                                    //小问选项
                                    if (sq.IsObjective && sq.Answers != null && sq.Answers.Any() && showOption)
                                    {
                                        sq.Answers.ForEach(sa =>
                                            tables.AddRange(PrintBody(("      " + sa.Tag + ". " + sa.Body), doc, p,
                                                sa.Keys, sa.ImgKeys)));
                                    }
                                });
                            }
                            else if (q.IsObjective && q.Answers != null && q.Answers.Any() && showOption)
                            {
                                //没有小问才显示题干选项
                                q.Answers.ForEach(a =>
                                    tables.AddRange(PrintBody(("   " + a.Tag + ". " + a.Body), doc, p, a.Keys, a.ImgKeys)));
                            }
                            //表格
                            if (tables.Any())
                                tables.ForEach(t => doc.InsertTable(t));
                            doc.InsertParagraph();
                        });
                    });
                    doc.Save();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// 导出参考答案
        /// </summary>
        /// <returns></returns>
        private Stream GetAnswer(WdPaper paper)
        {
            if (paper == null) return null;
            try
            {
                var result = new MemoryStream();
                using (var doc = DocX.Create(result))
                {
                    var isEnglish = paper.SubjectId == 3;

                    doc.Init();
                    doc.InsertParagraph().Append(paper.Title + "[参考答案]").FontSize(12).Bold();
                    paper.Sections.OrderBy(u => u.PSectionType).ThenBy(u => u.Sort).ToList().ForEach(s =>
                    {
                        doc.InsertParagraph().AppendLine().Append(s.Name).FontSize(11).Bold();
                        //问题
                        s.Questions.OrderBy(q => q.Sort).ToList().ForEach(q =>
                        {
                            var tables = new List<Table>();
                            var p = doc.InsertParagraph().VerticalCenter();
                            if (!q.IsObjective) p.AppendLine();
                            var qSortStr = string.Empty;
                            if (s.SmallRow && !q.SmallQuestions.IsNullOrEmpty())
                            {
                                qSortStr += isEnglish ? ((char)(65 + q.Sort - 1)) + "." : string.Empty;
                            }
                            else
                            {
                                qSortStr += q.Sort + ".";
                            }
                            p.Append(qSortStr);
                            //含小问的客观题才显示小问答案
                            if (q.SmallQuestions != null && q.SmallQuestions.Any() && q.IsObjective)
                            {
                                var sqSort = 1;
                                q.SmallQuestions.OrderBy(u => u.Sort).ToList().ForEach(sq =>
                                {
                                    p.Append("(" + (s.SmallRow ? sq.Sort : (sqSort++)) + "). ");
                                    sq.Answers.Where(sa => sa.IsCorrect).ToList()
                                        .Foreach(sa => p.Append(sa.Tag + "  "));
                                });
                            }
                            else
                            {
                                if (q.Answers == null || !q.Answers.Any())
                                    p.Append("略");
                                else
                                {
                                    if (q.IsObjective)
                                        q.Answers.Where(a => a.IsCorrect).ToList()
                                            .ForEach(a => p.Append(a.Tag + " "));
                                    else
                                    {
                                        var i = 0;
                                        q.Answers.ForEach(a =>
                                        {
                                            if (a.Body.IsNullOrEmpty())
                                                p.Append("略 ");
                                            else
                                            {
                                                tables.AddRange(PrintBody(a.Body, doc, p, a.Keys, a.ImgKeys));
                                                if (i++ > 0) p.AppendLine();
                                            }
                                        });
                                    }
                                }
                            }
                            if (!tables.Any()) return;
                            tables.ForEach(t => doc.InsertTable(t));
                            doc.InsertParagraph();
                        });
                    });
                    doc.Save();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Format(), ex);
                return null;
            }
        }

        /// <summary>
        /// 按科目、题型分组导出问题列表
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private Stream GetQuestions(WdQuestionGroup group)
        {
            if (group == null) return null;
            try
            {
                var result = new MemoryStream();
                using (var doc = DocX.Create(result))
                {
                    doc.Init();

                    doc.InsertParagraph().Append(group.Title).FontSize(12).Color(Color.Red).Bold().Alignment = Alignment.center;
                    group.Subjects.ForEach(s =>
                    {
                        if (s.SubjectName.IsNotNullOrEmpty())
                        {
                            doc.InsertParagraph();
                            doc.InsertParagraph().Append(s.SubjectName).FontSize(12).Color(Color.CornflowerBlue).Bold();
                        }
                        s.Sections.OrderBy(t => t.Type).ThenBy(t => t.Sort).ToList().ForEach(t =>
                        {
                            if (t.Name.IsNotNullOrEmpty())
                            {
                                doc.InsertParagraph();
                                doc.InsertParagraph().Append(t.Name).Color(Color.LightSkyBlue).Bold();
                            }
                            t.Questions.ForEach(q =>
                            {
                                doc.InsertParagraph();
                                var tables = new List<Table>();
                                var p = doc.InsertParagraph().VerticalCenter();
                                tables.AddRange(PrintBody((q.Sort + ". " + q.Body), doc, p, q.Keys, q.ImgKeys));
                                //小问
                                if (q.SmallQuestions != null && q.SmallQuestions.Any())
                                {
                                    q.SmallQuestions.ForEach(sq =>
                                    {
                                        tables.AddRange(PrintBody(("   " + sq.Body), doc, p, sq.Keys, sq.ImgKeys));
                                        //小问选项
                                        if (q.ShowOption && sq.IsObjective && sq.Answers != null && sq.Answers.Any())
                                        {
                                            sq.Answers.ForEach(sa =>
                                                tables.AddRange(PrintBody(("      " + sa.Tag + ". " + sa.Body), doc, p,
                                                    sa.Keys, sa.ImgKeys)));
                                        }
                                    });
                                }
                                else if (q.ShowOption && q.IsObjective && q.Answers != null && q.Answers.Any())
                                {
                                    //没有小问才显示题干选项
                                    q.Answers.ForEach(a =>
                                        tables.AddRange(PrintBody(("   " + a.Tag + ". " + a.Body), doc, p, a.Keys,
                                            a.ImgKeys)));
                                }
                                //表格
                                if (tables.Any())
                                    tables.ForEach(tb => doc.InsertTable(tb));
                            });
                        });
                    });
                    doc.Save();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Format(), ex);
                return null;
            }
        }

        /// <summary>
        /// 按科目、题型分组到处问题答案
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private Stream GetAnswer(WdQuestionGroup group)
        {
            if (group == null) return null;
            try
            {
                var result = new MemoryStream();
                using (var doc = DocX.Create(result))
                {
                    doc.Init();
                    doc.InsertParagraph().Append(group.Title + "[参考答案]").FontSize(12).Color(Color.Red).Bold().Alignment = Alignment.center;
                    group.Subjects.ForEach(s =>
                    {
                        if (s.SubjectName.IsNotNullOrEmpty())
                        {
                            doc.InsertParagraph();
                            doc.InsertParagraph().Append(s.SubjectName).FontSize(12).Color(Color.CornflowerBlue).Bold();
                        }
                        s.Sections.OrderBy(t => t.Type).ThenBy(t => t.Sort).ToList().ForEach(t =>
                        {
                            if (t.Name.IsNotNullOrEmpty())
                            {
                                doc.InsertParagraph();
                                doc.InsertParagraph().Append(t.Name).Color(Color.LightSkyBlue).Bold();
                            }
                            t.Questions.ForEach(q =>
                            {
                                var tables = new List<Table>();
                                var p = doc.InsertParagraph().VerticalCenter();
                                if (!q.IsObjective) p.AppendLine();
                                p.Append(q.Sort + ". ");
                                if (q.Answers == null || !q.Answers.Any())
                                    p.Append("略");
                                else
                                {
                                    if (q.IsObjective)
                                    {
                                        q.Answers.Where(a => a.IsCorrect).ToList().ForEach(a => p.Append(a.Tag + " "));
                                    }
                                    else
                                    {
                                        var i = 0;
                                        q.Answers.ForEach(a =>
                                        {
                                            if (a.Body.IsNullOrEmpty())
                                                p.Append("略 ");
                                            else
                                            {
                                                tables.AddRange(PrintBody(a.Body, doc, p, a.Keys, a.ImgKeys));
                                                if (i++ > 0) p.AppendLine();
                                            }
                                        });
                                    }
                                }
                                if (q.SmallQuestions != null && q.SmallQuestions.All(sq => sq.IsObjective))
                                {
                                    var sqSort = 1;
                                    q.SmallQuestions.ForEach(sq =>
                                    {
                                        p.Append("(" + sqSort++ + "). ");
                                        if (sq.Answers == null || !sq.Answers.Any())
                                            p.Append("略");
                                        else
                                        {
                                            if (sq.IsObjective)
                                            {
                                                sq.Answers.Where(sa => sa.IsCorrect)
                                                    .ToList()
                                                    .ForEach(sa => p.Append(sa.Tag + "  "));
                                            }
                                            else
                                            {
                                                var i = 0;
                                                sq.Answers.ForEach(sa =>
                                                {
                                                    if (sa.Body.IsNullOrEmpty()) p.Append("略 ");
                                                    else
                                                    {
                                                        tables.AddRange(PrintBody(sa.Body, doc, p, sa.Keys, sa.ImgKeys));
                                                        if (i++ > 0) p.AppendLine();
                                                    }
                                                });
                                            }
                                        }
                                    });
                                }
                                if (!tables.Any()) return;
                                tables.ForEach(tb => doc.InsertTable(tb));
                                doc.InsertParagraph();
                            });
                        });
                    });
                    doc.Save();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Format(), ex);
                return null;
            }
        }


        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="questions"></param>
        private void Init(List<WdQuestion> questions)
        {
            questions.ForEach(q =>
            {
                #region 题干图片及公式

                List<string> keys;
                q.Body = GetStream(q.Body, out keys);
                q.Keys = keys;

                //题干字段图片
                if (q.Images != null && q.Images.Any())
                {
                    q.ImgKeys = new List<string>();
                    foreach (var src in q.Images)
                    {
                        if (!_streams.Keys.Contains(src))
                            _streams.TryAdd(src, src.GetMemoryStream());
                        q.ImgKeys.Add(src);
                    }
                }

                #endregion

                #region 答案图片

                if (q.Answers != null && q.Answers.Any())
                {
                    if (q.IsObjective)
                        q.Answers = q.Answers.OrderBy(a => a.Sort).ToList();
                    var i = 0;
                    q.Answers.ForEach(a =>
                    {
                        if (q.IsObjective)
                            a.Tag = Convert.ToChar((i++) + 65).ToString(CultureInfo.InvariantCulture);

                        List<string> aKeys;
                        a.Body = GetStream(a.Body, out aKeys);
                        a.Keys = aKeys;
                        //答案字段图片
                        if (a.Images == null || !a.Images.Any()) return;
                        a.ImgKeys = new List<string>();
                        foreach (var src in a.Images)
                        {
                            if (!_streams.Keys.Contains(src))
                                _streams.TryAdd(src, src.GetMemoryStream());
                            a.ImgKeys.Add(src);
                        }
                    });
                }

                #endregion

                #region 小问图片

                if (q.SmallQuestions != null && q.SmallQuestions.Any())
                {
                    q.SmallQuestions.ForEach(sq =>
                    {
                        List<string> sqKeys;
                        sq.Body = GetStream(sq.Body, out sqKeys);
                        sq.Keys = sqKeys;
                        //小问字段图片
                        if (sq.Images != null && sq.Images.Any())
                        {
                            sq.ImgKeys = new List<string>();
                            foreach (var src in sq.Images)
                            {
                                if (!_streams.Keys.Contains(src))
                                    _streams.TryAdd(src, src.GetMemoryStream());
                                sq.ImgKeys.Add(src);
                            }
                        }

                        #region 小问答案图片

                        if (sq.Answers != null && sq.Answers.Any())
                        {
                            if (sq.IsObjective)
                                sq.Answers = sq.Answers.OrderBy(sa => sa.Sort).ToList();
                            var i = 0;
                            sq.Answers.ForEach(sa =>
                            {
                                if (sq.IsObjective)
                                    sa.Tag = Convert.ToChar((i++) + 65).ToString(CultureInfo.InvariantCulture);

                                List<string> saKeys;
                                sa.Body = GetStream(sa.Body, out saKeys);
                                sa.Keys = saKeys;
                                //小问答案字段图片
                                if (sa.Images == null || !sa.Images.Any()) return;
                                sa.ImgKeys = new List<string>();
                                foreach (var src in sa.Images)
                                {
                                    if (!_streams.Keys.Contains(src))
                                        _streams.TryAdd(src, src.GetMemoryStream());
                                    sa.ImgKeys.Add(src);
                                }
                            });
                        }

                        #endregion
                    });
                }

                #endregion
            });
        }

        /// <summary>
        /// 解析body图片转化为流存储
        /// </summary>
        /// <param name="body"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private string GetStream(string body, out List<string> keys)
        {
            keys = new List<string>();
            if (body.IsNullOrEmpty())
                return string.Empty;

            body = body.StrFilter(); //过滤特殊符号
            var regImg = new Regex("<img[^>]*src=['\"]?(?<src>[^'\"]+)['\"]?[^>]*>"); //标签图片
            var regLatex = new Regex(@"(\\\[([\s\S]*?)\\\])|(\\\(([\s\S]*?)\\\))|(\$([\s\S]*?)\$)"); //公式图片

            var mMatchs = regImg.Matches(body);
            if (mMatchs.Count > 0)
            {
                foreach (Match m in mMatchs)
                {
                    var src = m.Groups["src"].Value;
                    if (m.Value.IsNullOrEmpty() || src.IsNullOrEmpty()) continue;
                    var key = "{" + IdHelper.Instance.GetGuid32() + "}";
                    body = body.ReplaceSingle(m.Value, key);
                    keys.Add(key);
                    _streams.TryAdd(key, src.GetMemoryStream());
                }
            }
            var fMatchs = regLatex.Matches(body);
            if (fMatchs.Count < 1) return body;
            foreach (Match m in fMatchs)
            {
                var latex = m.Value.LatexFilter();
                if (latex.IsNullOrEmpty()) continue;
                var key = "{" + IdHelper.Instance.GetGuid32() + "}";
                body = body.ReplaceSingle(m.Value, key);
                keys.Add(key);
                _streams.TryAdd(key, (_latexPath + latex).GetMemoryStream());
            }
            return body;
        }

        /// <summary> 二维码格式头部 </summary>
        /// <param name="doc"></param>
        /// <param name="paper"></param>
        private void QrCodeHeader(DocX doc, WdPaper paper)
        {
            var tableTop = doc.InsertTable(1, 3);
            tableTop.Design = TableDesign.TableNormal;
            tableTop.AutoFit = AutoFit.Window;

            //列1 - 试卷资料
            tableTop.Rows[0].Cells[0].VerticalAlignment = VerticalAlignment.Center;
            var lParagraph = tableTop.Rows[0].Cells[0].Paragraphs[0];
            lParagraph.Alignment = Alignment.left;
            lParagraph.SetLineSpacing(LineSpacingType.Line, 1.3f);
            lParagraph.Append(paper.Title).FontSize(12).Bold()
                .AppendLine().Append("总分：" + paper.Score.ToString("0.#") + "分").FontSize(10)
                .AppendLine().Append("编号：" + paper.Num).FontSize(10);

            //列2 - 贴码区
            tableTop.Rows[0].Cells[1].Width = 80;
            var cParagraph = tableTop.Rows[0].Cells[1].Paragraphs[0];
            cParagraph.Alignment = Alignment.right;
            if (_codeStream == null)
                _codeStream = WordAssist.CodeStream();
            if (_codeStream == null)
                cParagraph.AppendLine().Append("贴码区");
            else
            {
                var tmpStream = new MemoryStream();
                _codeStream.Seek(0, SeekOrigin.Begin);
                _codeStream.CopyTo(tmpStream);
                tmpStream.Seek(0, SeekOrigin.Begin);
                cParagraph.AppendPicture(doc.AddImage(tmpStream).CreatePicture());
            }

            //列3 - 姓名..
            tableTop.Rows[0].Cells[2].Width = 80;
            tableTop.Rows[0].Cells[2].VerticalAlignment = VerticalAlignment.Center;
            var rParagraph = tableTop.Rows[0].Cells[2].Paragraphs[0];
            rParagraph.Alignment = Alignment.left;
            rParagraph.SetLineSpacing(LineSpacingType.Line, 1.4f);
            rParagraph.Append("班级：").FontSize(10)
                .AppendLine().Append("学号：").FontSize(10)
                .AppendLine().Append("姓名：").FontSize(10);
        }

        /// <summary> 得一号格式头部 </summary>
        /// <param name="doc"></param>
        /// <param name="paper"></param>
        private void DCodeHeader(DocX doc, WdPaper paper)
        {
            var topTable = doc.InsertTable(1, 2);
            topTable.Design = TableDesign.TableNormal;
            topTable.AutoFit = AutoFit.Window;
            var titleCell = topTable.Rows[0].Cells[0];
            titleCell.VerticalAlignment = VerticalAlignment.Center;
            var title = titleCell.Paragraphs[0];
            title.Alignment = Alignment.left;
            title.SetLineSpacing(LineSpacingType.Line, 1.3f);
            //标题
            title.Append(paper.Title).FontSize(12).Bold();

            var tb = titleCell.InsertTable(1, 2);
            tb.Rows[0].Cells[0].Width = 180;
            tb.Rows[0].Cells[0].Paragraphs[0].SetLineSpacing(LineSpacingType.Line, 1.3f);
            tb.Rows[0].Cells[0].Paragraphs[0].Append("编号：" + paper.Num).FontSize(10);
            tb.Rows[0].Cells[1].Paragraphs[0].Append("总分：" + paper.Score.ToString("0.#") + "分").FontSize(10);

            tb = tb.InsertTableAfterSelf(1, 2);
            tb.Rows[0].Cells[0].Paragraphs[0].Append("班级：").FontSize(10);
            //            tb.Rows[0].Cells[1].Width = 200;
            //            tb.Rows[0].Cells[1].Paragraphs[0].Append("学号：").FontSize(10);

            tb.Rows[0].Cells[1].Paragraphs[0].Append("姓名：").FontSize(10);

            //得一号
            var dcodeCell = topTable.Rows[0].Cells[1];
            dcodeCell.Width = 400;
            var dcode = dcodeCell.Paragraphs[0];
            dcode.Alignment = Alignment.right;
            var codeStream = WordAssist.DcodeStream();
            var tmpStream = new MemoryStream();
            codeStream.CopyTo(tmpStream);
            tmpStream.Seek(0, SeekOrigin.Begin);
            var codePic = doc.AddImage(tmpStream).CreatePicture();
            codePic.Width = codePic.Width / 4;
            codePic.Height = codePic.Height / 4;
            dcode.AppendPicture(codePic);
        }


        /// <summary>
        /// 解析body table 标签 转化为word表格
        /// </summary>
        /// <param name="body"></param>
        /// <param name="doc"></param>
        /// <param name="latexKeys"></param>
        /// <returns></returns>
        private WdBodyTable GetTable(string body, DocX doc, List<string> latexKeys)
        {
            var tables = new List<Table>();
            if (body.IsNullOrEmpty())
                return null;

            var regTb = new Regex(@"(?:<table[^>]*>([\w\W]*?)</table>)");
            var regTr = new Regex(@"(?:<t[rh][^>]*>([\w\W]*?)</t[rh]>)");
            var regTd = new Regex(@"(?:<td[^>]*>([\w\W]*?)</td>)");
            var tMatchs = regTb.Matches(body);
            if (tMatchs.Count == 0)
                return null;
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
                table.SetBorder(TableBorderType.InsideH, WordAssist.TableBorder);
                table.SetBorder(TableBorderType.InsideV, WordAssist.TableBorder);
                table.Design = TableDesign.TableNormal;
                table.AutoFit = AutoFit.Contents;
                var i = 0;
                trList.ForEach(tds =>
                {
                    var j = 0;
                    tds.ForEach(td =>
                    {
                        var p = table.Rows[i].Cells[j++].Paragraphs[0];
                        p.AppendHtml(td);
                        //表格中的图片
                        PrintLatex(latexKeys, doc, p);
                    });
                    i++;
                });
                tables.Add(table);
                body = body.Replace(mt.Value, string.Empty);
            }
            return new WdBodyTable { Body = body, Tables = tables };
        }

        /// <summary>
        /// 写Body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="doc"></param>
        /// <param name="p"></param>
        /// <param name="latexKeys"></param>
        /// <param name="imgKeys"></param>
        /// <returns></returns>
        private IEnumerable<Table> PrintBody(string body, DocX doc, Paragraph p, List<string> latexKeys, List<string> imgKeys)
        {
            body = body.Replace("&nbsp;", " ");
            body = Regex.Replace(body, "&amp;", "&");
            body = Regex.Replace(body, "&lt;", "<");
            body = Regex.Replace(body, "&gt;", ">");
            body = Regex.Replace(body, "&quot;", "\"");
            body = Regex.Replace(body, "&#39;", "′");
            body = Regex.Replace(body, "\\s+", " ");
            body = ReplaceLowOrderASCIICharacters(body);
            var tables = new List<Table>();
            var table = GetTable(body, doc, latexKeys);
            if (table != null)
            {
                body = table.Body;
                tables = table.Tables;
            }
            p.AppendHtml(body).AppendLine();
            //公式图片
            PrintLatex(latexKeys, doc, p);
            //字段图片
            PrintPicture(imgKeys, doc, p);
            return tables;
        }
        /// <summary>
        /// 过滤非打印字符
        /// </summary>
        /// <param name="tmp">待过滤</param>
        /// <returns>过滤好的</returns>
        private string ReplaceLowOrderASCIICharacters(string tmp)
        {
            StringBuilder info = new StringBuilder();
            foreach (char cc in tmp)
            {
                int ss = (int)cc;
                if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss <= 32)))
                    info.AppendFormat(" ", ss);
                else info.Append(cc);
            }
            return info.ToString();
        }

        /// <summary>
        /// 替换公式图片
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="doc"></param>
        /// <param name="p"></param>
        private void PrintLatex(List<string> keys, DocX doc, Paragraph p)
        {
            if (keys == null || !keys.Any()) return;
            keys.ForEach(k =>
            {
                var keyStream = _streams[k];
                if (keyStream == null) return;
                var tmpStream = new MemoryStream();
                keyStream.Seek(0, SeekOrigin.Begin);
                keyStream.CopyTo(tmpStream);
                tmpStream.Seek(0, SeekOrigin.Begin);
                var picture = doc.AddImage(tmpStream).CreatePicture().SetMaxWh();

                var idx = p.Text.IndexOf(k, StringComparison.Ordinal);
                if (idx > -1)
                {
                    p.InsertPicture(picture, idx);
                    p.RemoveText(idx, k.Length);
                }
                //else
                //{
                //    p.AppendPicture(picture);
                //}
            });
        }

        /// <summary>
        /// 字段存储的图片
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="doc"></param>
        /// <param name="p"></param>
        private void PrintPicture(List<string> keys, DocX doc, Paragraph p)
        {
            if (keys == null || !keys.Any()) return;
            keys.ForEach(k =>
            {
                var keyStream = _streams[k];
                if (keyStream == null) return;
                var tmpStream = new MemoryStream();
                keyStream.Seek(0, SeekOrigin.Begin);
                keyStream.CopyTo(tmpStream);
                tmpStream.Seek(0, SeekOrigin.Begin);
                p.AppendPicture(doc.AddImage(tmpStream).CreatePicture().SetMaxWh());
            });
            p.AppendLine();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_codeStream != null)
            {
                _codeStream.Close();
                _codeStream.Dispose();
            }
            if (_objectiveStream != null)
            {
                _objectiveStream.Close();
                _objectiveStream.Dispose();
            }
            if (_streams == null || !_streams.Any())
                return;
            foreach (var st in _streams.Keys.Select(k => _streams[k]).Where(st => st != null))
            {
                st.Close();
                st.Dispose();
            }
        }
    }
}