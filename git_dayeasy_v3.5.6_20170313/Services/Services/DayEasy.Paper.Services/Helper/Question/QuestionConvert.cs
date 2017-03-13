using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Paper.Services.Model;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.Paper.Services.Helper.Question
{
    /// <summary> 题目转换类 </summary>
    internal static class QuestionConvert
    {
        #region 保存图片

        /// <summary>
        /// 取得HTML中所有图片的 URL
        /// </summary>
        /// <param name="sHtmlText">HTML代码</param>
        /// <returns>图片的URL列表</returns>
        private static string[] GetHtmlImageUrlList(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签
            var regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串
            var matches = regImg.Matches(sHtmlText);

            var i = 0;
            var sUrlList = new string[matches.Count];

            // 取得匹配项列表
            foreach (Match match in matches)
                sUrlList[i++] = match.Groups["imgUrl"].Value;

            return sUrlList;
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="qContent"></param>
        /// <returns></returns>
        internal static string ConvertImgs(this string qContent)
        {
            if (string.IsNullOrWhiteSpace(qContent))
                return qContent;
            var imgs = GetHtmlImageUrlList(qContent);
            if (imgs != null && imgs.Any())
            {
                imgs.ToList().ForEach(u =>
                {
                    if (!u.Contains("data:image/png;base64,"))
                        return;
                    var imgBytes = Convert.FromBase64String(u.Replace("data:image/png;base64,", ""));
                    if (!imgBytes.Any())
                        return;
                    using (
                        var helper =
                            new HttpHelper(
                                Consts.Config.FileSite + "/uploader?type=" + (byte)UploadType.Image, "POST",
                                Encoding.UTF8, null))
                    {
                        var dic = new Dictionary<string, Stream>
                        {
                            {
                                Guid.NewGuid().ToString("N") + ".jpg",
                                new MemoryStream(imgBytes)
                            }
                        };
                        helper.AddFiles(dic);

                        var result = helper.GetHtml();
                        if (string.IsNullOrEmpty(result)) return;
                        var results = JsonHelper.Json<UploadResult>(result);
                        if (results != null)
                        {
                            qContent = qContent.Replace(u, results.urls.FirstOrDefault());
                        }
                    }
                });
            }
            return qContent;
        }

        #endregion
        /// <summary> 重置问题 </summary>
        /// <param name="question"></param>
        /// <param name="userId"></param>
        public static void Reset(this TQ_Question question, long userId)
        {
            question.Id = IdHelper.Instance.Guid32;
            question.AddedAt = Clock.Now;
            question.AddedBy = userId;
            question.AddedIP = Utils.GetRealIp();
            question.ShareRange = (byte)ShareRange.Self;
            question.Status = (byte)NormalStatus.Normal;
            question.UsedCount = 0;
            question.AnswerCount = 0;
            question.ErrorCount = 0;
        }

        /// <summary> 小问转换 </summary>
        /// <param name="details"></param>
        /// <param name="questionId"></param>
        /// <param name="saveAs"></param>
        /// <returns></returns>
        public static List<TQ_SmallQuestion> ParseDetails(this IList<SmallQuestionDto> details, string questionId,
            bool saveAs = false)
        {
            if (details == null || !details.Any())
                return null;
            var detailList = new List<TQ_SmallQuestion>();
            foreach (var detail in details)
            {
                var index = details.IndexOf(detail);
                var tqDetail = new TQ_SmallQuestion
                {
                    QID = questionId,
                    SmallQContent = detail.Body.FormatBody().ConvertImgs().HtmlEncode(),
                    IsObjective = (detail.Answers != null && detail.Answers.Count > 1),
                    Sort = index,
                    OptionStyle = detail.OptionStyle
                };
                if (saveAs || string.IsNullOrWhiteSpace(detail.Id))
                    tqDetail.Id = IdHelper.Instance.Guid32;
                else
                    tqDetail.Id = detail.Id;

                if (detail.Images != null && detail.Images.Length > 0)
                    tqDetail.SmallQImages = detail.Images.ToJson();
                detail.Id = tqDetail.Id;
                detailList.Add(tqDetail);
            }
            return detailList;
        }

        /// <summary> 选项/答案转换 </summary>
        public static IEnumerable<TQ_Answer> ParseAnswers(this List<AnswerDto> answers, string questionId,
            bool saveAs = false)
        {
            var answerList = new List<TQ_Answer>();
            if (answers == null || !answers.Any())
                return answerList;
            foreach (var answer in answers)
            {
                var answerIndex = answers.IndexOf(answer);

                var tqAnswer = new TQ_Answer
                {
                    QuestionID = questionId,
                    QContent = answer.Body.FormatBody().ConvertImgs().HtmlEncode(),
                    IsCorrect = answer.IsCorrect,
                    Sort = answerIndex
                };
                if (saveAs || string.IsNullOrWhiteSpace(answer.Id))
                    tqAnswer.Id = IdHelper.Instance.Guid32;
                else
                    tqAnswer.Id = answer.Id;

                if (answer.Images != null && answer.Images.Any())
                    tqAnswer.QImages = answer.Images.ToJson();
                answerList.Add(tqAnswer);
            }
            return answerList;
        }

        /// <summary>
        /// 问题基础逻辑验证
        /// </summary>
        /// <param name="question">数据原型</param>
        /// <returns></returns>
        internal static DResult CheckQuestion(this QuestionDto question)
        {
            if (question == null)
                return DResult.Error("提交数据异常！");
            if (question.Stage <= 0)
                return DResult.Error("请选择学段！");
            if (question.Type <= 0)
                return DResult.Error("请选择题型！");
            if (question.UserId <= 0)
                return DResult.Error("用户信息异常！");
            if (question.Knowledges == null || question.Knowledges.Count < 1)
                return DResult.Error("知识点信息异常！");
            // 基础逻辑判断
            if (string.IsNullOrWhiteSpace(question.Body) && (question.Images == null || !question.Images.Any()))
                return DResult.Error("题干不能为空！");
            var allowEmpty = (question.OptionStyle == (byte)OptionStyle.AddFromPaper);
            //小问逻辑验证
            if (question.Details != null && question.Details.Any())
            {
                var details = question.Details.OrderBy(t => t.Sort).ToList();
                foreach (var detail in details)
                {
                    var index = details.IndexOf(detail);
                    //小问内容判断
                    if (!allowEmpty &&
                        (string.IsNullOrWhiteSpace(detail.Body) && (detail.Images == null || !detail.Images.Any())))
                        return DResult.Error(string.Format("小问{0}内容不能为空！", index + 1));
                    //小问选项判断
                    if (detail.Answers != null && detail.Answers.Any())
                    {
                        if (detail.Answers.Count < 2)
                            return DResult.Error(string.Format("小问{0}选项至少2个！", index + 1));
                        var answers = detail.Answers.OrderBy(t => t.Sort).ToList();
                        int correct = 0;
                        foreach (var answer in answers)
                        {
                            var answerIndex = answers.IndexOf(answer);
                            if (!allowEmpty &&
                                (string.IsNullOrWhiteSpace(answer.Body) &&
                                 (answer.Images == null || !answer.Images.Any())))
                                return
                                    DResult.Error(string.Format("小问{0}选项{1}不能为空！", index + 1,
                                        Consts.OptionWords[answerIndex]));
                            //words
                            if (answer.IsCorrect)
                                correct++;
                        }
                        if (correct == 0 && !allowEmpty)
                            return DResult.Error(string.Format("小问{0}没有设置正确答案！", index + 1));
                    }
                }
            }
            //选择题判断
            var ischoice = question.Type.In(1, 2, 3);
            if (ischoice && (question.Answers == null || question.Answers.Count < 2))
                return DResult.Error("至少2个选项！");
            //大题答案或选项处理
            if (question.Answers != null && question.Answers.Count > 1)
            {
                var answers = question.Answers.OrderBy(t => t.Sort).ToList();
                int correct = 0;
                foreach (var answer in answers)
                {
                    var answerIndex = answers.IndexOf(answer);
                    if (!allowEmpty &&
                        (string.IsNullOrWhiteSpace(answer.Body) && (answer.Images == null || !answer.Images.Any())))
                        return DResult.Error(string.Format("选项{0}不能为空！",
                            Consts.OptionWords[answerIndex]));
                    if (answer.IsCorrect)
                        correct++;
                }
                if (question.Type.In(1, 2, 3) && correct == 0 && !allowEmpty)
                    return DResult.Error("没有设置正确答案！");
                if (question.Type == 1 && correct > 1)
                    return DResult.Error("单项选择题只能设置1个正确答案！");
            }
            //格式化题干
            return DResult.Success;
        }
    }
}
