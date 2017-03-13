using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 考试排名信息 </summary>
    public class ExamRanksDto : DDto
    {
        /// <summary> 考试名称 </summary>
        public string Name { get; set; }
        /// <summary> 总人数 </summary>
        public int StudentCount { get; set; }
        /// <summary> 平均分 </summary>
        public decimal AverageScore { get; set; }
        /// <summary> 科目信息 </summary>
        public Dictionary<string, List<string>> Subjects { get; set; }
        /// <summary> 学生列表 </summary>
        public List<ExamStudentDto> Students { get; set; }
        /// <summary> 构造函数 </summary>
        public ExamRanksDto()
        {
            Subjects = new Dictionary<string, List<string>>();
            Students = new List<ExamStudentDto>();
        }
    }
}
