using DayEasy.Core.Domain;
using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Dtos.ErrorQuestion
{
    /// <summary>
    /// 错题列表查询条件
    /// </summary>
    public class ErrorQuestionSearchDto : DDto
    {
        public DPage Page { get; set; }
        public long StudentId { get; set; }
        //public int SourceType { get; set; }
        public int SubjectId { get; set; }
        public int QType { get; set; }
        public string Key { get; set; }
        /// <summary> 开始时间 </summary>
        public DateTime? StartTime { get; set; }
        /// <summary> 截至时间 </summary>
        public DateTime? EndTime { get; set; }
        /// <summary> 是否已分析 </summary>
        public bool? HasReason { get; set; }
        /// <summary> 是否已过关 </summary>
        public bool? IsPass { get; set; }
    }
}
