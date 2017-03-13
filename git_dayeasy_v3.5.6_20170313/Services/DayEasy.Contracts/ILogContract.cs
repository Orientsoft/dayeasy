using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Domain.Repositories;

namespace DayEasy.Contracts
{
    /// <summary> 日志业务模块 </summary>
    public interface ILogContract : IDependency
    {
        IRepository<TL_UserLog, long> UserLogRepository { get; }  
        /// <summary> 生成用户日志 </summary>
        /// <param name="userId"></param>
        /// <param name="level"></param>
        /// <param name="title"></param>
        /// <param name="detail"></param>
        void GenerateUserLog(long userId, LogLevel level, string title, string detail);

        /// <summary> 生成用户登录日志 </summary>
        /// <param name="userId"></param>
        /// <param name="level"></param>
        /// <param name="account"></param>
        /// <param name="resean"></param>
        /// <param name="partner"></param>
        void GenerateLoginLog(long userId, LogLevel level, string account, string resean, string partner="");
    }
}
