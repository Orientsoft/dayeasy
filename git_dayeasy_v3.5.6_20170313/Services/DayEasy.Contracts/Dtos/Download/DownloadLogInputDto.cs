using System;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Download
{
    /// <summary> 创建下载日志对象 </summary>
    public class DownloadLogInputDto : DDto
    {
        public DownloadType Type { get; set; }
        public long UserId { get; set; }
        public string AgencyId { get; set; }
        public int Count { get; set; }
        public string Referer { get; set; }
        public string Agent { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
