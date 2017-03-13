using System;
using System.Collections.Generic;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    public class KpStatisticDataDto : DDto
    {
        public string OutStartTimeStr { get; set; }
        public string OutEndTimeStr { get; set; }
        public List<KpDataDto> KpData { get; set; } 
    }

    /// <summary>
    /// 知识点统计数据Dto
    /// </summary>
    public class KpDataDto : DDto
    {
        public int KpId { get; set; }
        public string KpName { get; set; }
        public int AnswerCount { get; set; }
        public int ErrorCount { get; set; }
        public List<KpDataDto> SonKps { get; set; }
    }
    
    /// <summary>
    /// 知识点统计查找参数Dto
    /// </summary>
    public class SearchKpStatisticDataDto : DDto
    {
        public long UserId { get; set; }
        public DateTime RegistTime { get; set; }
        public UserRole Role { get; set; }
        public int SubjectId { get; set; }
        public string GroupId { get; set; }
        public string StartTimeStr { get; set; }
        public string EndTimeStr { get; set; }
    }

}
