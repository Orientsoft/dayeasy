using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using System.Threading.Tasks;
using System.Web;

namespace DayEasy.Application.Services.Helper
{
    /// <summary> 下载日志 </summary>
    public static class DownloadLogger
    {
        /// <summary> 记录日志 </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TS_DownloadLog CreateLog(this long userId, DownloadType type, UserAgencyDto agency = null, int count = 1)
        {
            string referer = string.Empty,
                agent = string.Empty;
            var context = HttpContext.Current;
            if (context != null)
            {
                if (context.Request.UrlReferrer != null)
                    referer = context.Request.UrlReferrer.AbsoluteUri;
                agent = context.Request.UserAgent;
            }
            return CurrentIocManager.Resolve<ISystemContract>().CreateDownload(new Contracts.Dtos.Download.DownloadLogInputDto
            {
                Type = type,
                UserId = userId,
                AgencyId = agency == null ? null : agency.AgencyId,
                Referer = referer,
                Agent = agent,
                Count = count
            });
        }

        public static void Complete(this TS_DownloadLog log)
        {
            Task.Factory.StartNew(() =>
            {
                CurrentIocManager.Resolve<ISystemContract>().CompleteDownload(log);
            });
        }
    }
}