using System;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Download
{
    /// <summary> 下载日志 </summary>
    [AutoMap(typeof(TS_DownloadLog))]
    public class DownloadLogDto : DDto
    {
        public string Id { get; set; }
        public byte Type { get; set; }
        public long UserId { get; set; }
        public string AgencyId { get; set; }
        public int Count { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime CompleteTime { get; set; }
    }
}
