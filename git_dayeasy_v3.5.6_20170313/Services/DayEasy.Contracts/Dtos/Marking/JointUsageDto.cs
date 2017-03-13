using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    public class JointUsageDto : DDto
    {
        public string PaperId { get; set; }
        public string GroupId { get; set; }
        public long UserId { get; set; }
        public int SubjectId { get; set; }

        /// <summary> 分配详情 </summary>
        public List<JointDistDto> Dists { get; set; }
    }

    /// <summary> 题目组 </summary>
    public class JointDistDto : DDto
    {
        /// <summary> 试卷类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 题目列表 </summary>
        public List<string> QuestionIds { get; set; } 
        //public List<JointQuestionDto> QuestionIds { get; set; }

        /// <summary> 可批阅此分组的教师列表 </summary>
        public List<long> TeacherIds { get; set; }
    }

    public class JointQuestionDto : DDto
    {
        public int Sort { get; set; }
        public string Id { get; set; }
    }
}
