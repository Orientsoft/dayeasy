using System;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Repositories;
using DayEasy.EntityFramework;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Services.Services
{
    public class LogService : DayEasyService, ILogContract
    {
        public LogService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        public IDayEasyRepository<TL_UserLog, long> UserLogs { private get; set; }

        public IRepository<TL_UserLog, long> UserLogRepository
        {
            get { return UserLogs; }
        }

        public void GenerateUserLog(long userId, LogLevel level, string title, string detail)
        {
            var log = new TL_UserLog
            {
                Id = IdHelper.Instance.LongId,
                AddedAt = DateTime.Now,
                AddedIp = Utils.GetRealIp(),
                UserId = userId,
                LogTitle = title,
                LogDetail = detail,
                LogLevel = (byte)level
            };
            UserLogs.Insert(log);
        }

        public void GenerateLoginLog(long userId, LogLevel level, string account, string resean, string partner = "")
        {
            var title = string.IsNullOrWhiteSpace(partner)
                ? "用户登录"
                : string.Format("用户登录[{0}]", partner);

            const string detail = "用户[{0}]，于{1}登录，状态为{2}{3}";

            if (!string.IsNullOrWhiteSpace(resean))
                resean = "，原因：" + resean;

            GenerateUserLog(userId, level, title,
                string.Format(detail, account, Utils.GetTimeNow(),
                    (level == LogLevel.Error ? "登录失败" : "登录成功"), resean));
        }
    }
}
