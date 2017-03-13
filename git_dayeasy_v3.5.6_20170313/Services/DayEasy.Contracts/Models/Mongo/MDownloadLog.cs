using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary> 用户下载日志 </summary>
    public class MongoDownloadLog : DEntity<string>
    {
        /// <summary> ID </summary>
        public override string Id { get; set; }

        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 下载时间 </summary>
        public DateTime AddedAt { get; set; }

        /// <summary> 类型 </summary>
        public string Type { get; set; }
        /// <summary> 来源网址 </summary>
        public string Referer { get; set; }
        /// <summary> 用户环境 </summary>
        public string UserAgent { get; set; }

        /// <summary> 下载IP </summary>
        public string Ip { get; set; }
    }
}
