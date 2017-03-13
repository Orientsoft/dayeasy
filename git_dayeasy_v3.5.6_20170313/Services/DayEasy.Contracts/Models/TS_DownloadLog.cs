using DayEasy.Core.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace DayEasy.Contracts.Models
{
    /// <summary>
    /// 下载日志
    /// </summary>
    public class TS_DownloadLog : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 机构ID </summary>
        [StringLength(32)]
        public string AgencyId { get; set; }
        /// <summary>  下载类型  </summary>
        public byte Type { get; set; }
        /// <summary> 下载数量 </summary>
        public int Count { get; set; }
        /// <summary> 下载时间 </summary>
        public DateTime AddedAt { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string AddedIp { get; set; }
        /// <summary>
        /// 下载环境
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// 来源URL
        /// </summary>
        public string RefererUrl { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime CompleteTime { get; set; }
    }
}
