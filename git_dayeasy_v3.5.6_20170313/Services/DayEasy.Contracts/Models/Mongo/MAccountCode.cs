using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary>
    /// 终端接收验证码记录
    /// </summary>
    public class MongoAccountCode : DEntity<long>
    {
        /// <summary> ID </summary>
        public override long Id { get; set; }

        /// <summary> 接收帐号 </summary>
        public string Account { get; set; }

        /// <summary> 操作次数 </summary>
        public int Count { get; set; }

        /// <summary> 累计操作次数 </summary>
        public int Total { get; set; }

        /// <summary> 最后操作时间 </summary>
        public DateTime Time { get; set; }
    }
}
