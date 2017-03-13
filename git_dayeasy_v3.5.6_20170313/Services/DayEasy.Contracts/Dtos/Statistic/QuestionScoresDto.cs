using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary> 每题得分 </summary>
    public class QuestionScoresDto : DDto
    {
        /// <summary> 试卷类型分布 </summary>
        public Dictionary<byte, Dictionary<string, string>> QuestionTypes { get; set; }

        /// <summary> 题目编号 </summary>
        public Dictionary<string, string> QuestionSorts { get; set; }

        /// <summary> 学生得分列表 </summary>
        public List<StudentQuestionScoresDto> Students { get; set; }

        public QuestionScoresDto()
        {
            QuestionTypes = new Dictionary<byte, Dictionary<string, string>>();
            QuestionSorts = new Dictionary<string, string>();
            Students = new List<StudentQuestionScoresDto>();
        }
    }

    /// <summary> 学生每题得分 </summary>
    public class StudentQuestionScoresDto : DDto
    {
        /// <summary> 学生ID </summary>
        public long Id { get; set; }

        /// <summary> 姓名 </summary>
        public string Name { get; set; }

        /// <summary> 得一号 </summary>
        public string Code { get; set; }

        /// <summary> 班级名称 </summary>
        public string ClassName { get; set; }

        /// <summary> 得分详情 </summary>
        public Dictionary<string, decimal> Scores { get; set; }

        public StudentQuestionScoresDto()
        {
            Scores = new Dictionary<string, decimal>();
        }
    }
}
