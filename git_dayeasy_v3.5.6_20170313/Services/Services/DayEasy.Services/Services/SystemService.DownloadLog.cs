using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.Download;
using DayEasy.Contracts.Models;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Services.Services
{
    public partial class SystemService
    {
        public IDayEasyRepository<TS_DownloadLog> DownloadLogRepository { private get; set; }

        /// <summary> 创建下载任务 </summary>
        public TS_DownloadLog CreateDownload(DownloadLogInputDto dto)
        {
            dto.CreateTime = dto.CreateTime ?? Clock.Now;
            var model = new TS_DownloadLog
            {
                Id = IdHelper.Instance.Guid32,
                Type = (byte)dto.Type,
                UserId = dto.UserId,
                Count = dto.Count,
                AgencyId = dto.AgencyId,
                AddedAt = dto.CreateTime.Value,
                AddedIp = Utils.GetRealIp(),
                RefererUrl = dto.Referer,
                UserAgent = dto.Agent
            };
            return model;
        }

        /// <summary> 完成下载 </summary>
        public DResult CompleteDownload(TS_DownloadLog log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.Id))
                return DResult.Error("参数错误");
            if (log.CompleteTime == DateTime.MinValue)
                log.CompleteTime = Clock.Now;
            var id = DownloadLogRepository.Insert(log);
            return string.IsNullOrWhiteSpace(id) ? DResult.Error("提交失败") : DResult.Success;
        }

        /// <summary> 查询下载日志 </summary>
        public DResults<DownloadLogDto> DownloadLogs(DownloadLogSearchDto searchDto)
        {
            Expression<Func<TS_DownloadLog, bool>> condition = d => true;
            if (searchDto.Type >= 0)
                condition = condition.And(d => d.Type == searchDto.Type);
            if (!string.IsNullOrWhiteSpace(searchDto.AgencyId))
            {
                condition = condition.And(d => d.AgencyId == searchDto.AgencyId);
            }
            var count = DownloadLogRepository.Count(condition);
            var dtos = DownloadLogRepository.Where(condition)
                .Skip(searchDto.Page * searchDto.Size)
                .Take(searchDto.Size)
                .ToList()
                .MapTo<List<DownloadLogDto>>();
            return DResult.Succ(dtos, count);
        }
    }
}
