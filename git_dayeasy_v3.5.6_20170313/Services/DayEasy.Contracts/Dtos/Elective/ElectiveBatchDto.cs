using System;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Elective
{
    /// <summary> 选修课批次 </summary>
    [AutoMapFrom(typeof(TS_ElectiveBatch))]
    public class ElectiveBatchDto : DDto
    {
        /// <summary> 选课批次 </summary>
        public string Id { get; set; }
        /// <summary> 选课标题 </summary>
        public string Title { get; set; }
        /// <summary> 创建时间 </summary>
        public DateTime CreationTime { get; set; }
        /// <summary> 状态 </summary>
        public byte Status { get; set; }
        /// <summary> 创建人 </summary>
        public long CreatorId { get; set; }
    }
}
