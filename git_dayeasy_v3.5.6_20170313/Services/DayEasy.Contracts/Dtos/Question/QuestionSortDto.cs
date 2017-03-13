using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Question
{
    /// <summary> 问题序号基类 </summary>
    public class DQuestionSortDto : DDto
    {
        /// <summary> 问题ID </summary>
        public string Id { get; set; }
        /// <summary> 序号（包含AB卷) </summary>
        public string Sort { get; set; }
    }

    /// <summary> 题目序号</summary>
    public class QuestionSortDto : DDto
    {
        /// <summary> 题目ID </summary>
        public string Id { get; set; }

        /// <summary> 序号 </summary>
        public string Sort { get; set; }
        /// <summary> 分数 </summary>
        public decimal Score { get; set; }

        /// <summary> 题型 </summary>
        public int Type { get; set; }

        /// <summary> 题型描述 </summary>
        public string TypeDesc { get; set; }

        /// <summary> 是否客观题 </summary>
        public bool Objective { get; set; }
    }
}
