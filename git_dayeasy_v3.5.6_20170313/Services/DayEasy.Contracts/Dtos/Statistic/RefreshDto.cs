using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary> 缓存基类 </summary>
    public abstract class RefreshDto : DDto
    {
        /// <summary> 最后刷新时间 </summary>
        public DateTime LastRefresh { get; set; }
    }
}
