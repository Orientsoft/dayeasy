using DayEasy.AsyncMission.Models;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Marking.Services.Helper
{
    public static class OtherPlatformHelper
    {
        private const string ScoreHost = "http://www.bsgxkj.com/score_add";
        private static string Sha1(this string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);
            return BitConverter.ToString(data).Replace("-", string.Empty);
            //var sb = new StringBuilder();
            //foreach (var t in data)
            //{
            //    sb.Append(t.ToString("X2"));
            //}
            //return sb.ToString();
        }

        /// <summary> 发送成绩到第三方 </summary>
        /// <param name="code">学籍号</param>
        /// <param name="name">考试名称</param>
        /// <param name="subject">科目</param>
        /// <param name="score">分数</param>
        /// <param name="examTime">考试时间</param>
        /// <param name="type">考试类型</param>
        public static void SendScore(string code, string name, string subject, decimal score, DateTime examTime, string type)
        {
            var ps = new Dictionary<string, string>
            {
                {"stu_id", code },
                {"exam_name", name },
                {"exam_type", type},
                {"exam_subject", subject },
                {"score", score.ToString(CultureInfo.InvariantCulture) },
                {"exam_time", examTime.ToString("yyyy-MM-dd HH:mm:ss") },//"2017-06-29 20:56:36"},//
                {"post_time", Clock.Now.ToString("yyyy-MM-dd HH:mm:ss") }//"2017-07-04 20:56:38"}//
            };
            var unsign = string.Join("&", ps.Select(t => $"{t.Key}={t.Value}"));
            ps.Add("sig", string.Concat(unsign, "bsgx").Sha1());

            var logger = LogManager.Logger("SendScore");

            //var parameters = string.Join("&", ps.Select(t => $"{t.Key}={t.Value}"));
            //using (var http = new HttpHelper(ScoreHost, "POST", Encoding.UTF8, parameters))
            //{
            //    var html = http.GetHtml();
            //    var m = new { rc = 0, msg = string.Empty };
            //    m = JsonHelper.Json(html, m);
            //    logger.Info(JsonHelper.ToJson(m));
            //}
            var client = new HttpClient();
            var result = client.PostAsync(ScoreHost, new FormUrlEncodedContent(ps)).Result;
            var html = result.Content.ReadAsStringAsync().Result;
            if (result.StatusCode != HttpStatusCode.OK)
                logger.Warn($"参数:{JsonHelper.ToJson(ps)},result:{html}");
            var m = new { rc = 0, msg = string.Empty };
            m = JsonHelper.Json(html, m);
            if (m.rc != 0)
            {
                logger.Info($"参数:{JsonHelper.ToJson(ps)},result:{JsonHelper.ToJson(m)}");
            }

        }

        public static void BatchSendScores(List<StudentScore> scores)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var item in scores)
                {
                    try
                    {
                        SendScore(item.StudentNo, item.PaperTitle, item.Subject, item.Score, item.ExamTime,
                            string.IsNullOrWhiteSpace(item.Type) ? "期中考试" : item.Type);
                    }
                    catch (Exception ex)
                    {
                        var logger = LogManager.Logger("SendScore");
                        logger.Error(ex.Message, ex);
                    }
                }
            });
        }
    }
}
