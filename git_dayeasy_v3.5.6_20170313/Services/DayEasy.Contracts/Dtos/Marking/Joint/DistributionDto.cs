
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 协同分配 </summary>
    public class DistributionDto : DDto
    {
        /// <summary> 用户ID </summary>
        public long UserId { get; set; }
        /// <summary> 协同批次 </summary>
        public string JointBatch { get; set; }

        /// <summary> 分配详情 </summary>
        public List<DistributionDetailDto> Details { get; set; }
    }

    /// <summary> 分配详情 </summary>
    public class DistributionDetailDto : DDto
    {
        /// <summary> 试卷类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 题目ID </summary>
        public List<string> Questions { get; set; }

        /// <summary> 教师ID </summary>
        public List<long> TeacherIds { get; set; }
    }
}
