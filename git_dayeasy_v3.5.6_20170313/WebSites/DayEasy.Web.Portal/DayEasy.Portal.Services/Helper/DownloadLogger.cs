using System.Threading.Tasks;
using System.Web;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.MongoDb;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.Portal.Services.Helper
{
    /// <summary> 下载日志 </summary>
    public class DownloadLogger
    {
        /// <summary> 记录日志 </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Task LogAsync(long userId, string type)
        {
            string ip = Utils.GetRealIp(),
                referer = string.Empty,
                agent = string.Empty;
            var context = HttpContext.Current;
            if (context != null)
            {
                if (context.Request.UrlReferrer != null)
                    referer = context.Request.UrlReferrer.AbsoluteUri;
                agent = context.Request.UserAgent;
            }
            return Task.Factory.StartNew(() => MongoHelper.Insert(new MongoDownloadLog
            {
                Id = IdHelper.Instance.Guid32,
                UserId = userId,
                Type = type,
                AddedAt = Clock.Normalize(Clock.Now),
                Ip = ip,
                Referer = referer,
                UserAgent = agent
            }));
        }
    }
}