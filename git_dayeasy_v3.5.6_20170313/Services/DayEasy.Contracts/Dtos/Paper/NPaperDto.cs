
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary> 新版试卷详情 </summary>
    public class NPaperDto : PaperDto
    {
        /// <summary> 是否按小问通排 </summary>
        public bool IsDetailSort { get { return SubjectId == 3; } }
        /// <summary> 题型列表 </summary>
        public List<NPaperSectionDto> Sections { get; set; }

        /// <summary> 问题详情 </summary>
        public Dictionary<string, QuestionDto> Questions { get; set; }

        /// <summary> 构造函数 </summary>
        public NPaperDto()
        {
            Sections = new List<NPaperSectionDto>();
            Questions = new Dictionary<string, QuestionDto>();
        }
    }

    /// <summary> 试卷Section </summary>
    public class NPaperSectionDto : DDto
    {
        /// <summary> ID </summary>
        public string Id { get; set; }

        /// <summary> 标题 </summary>
        public string Title { get; set; }

        /// <summary> 排序 </summary>
        public int Sort { get; set; }

        /// <summary> 题型 </summary>
        public int QuestionType { get; set; }

        /// <summary> Section类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 分数 </summary>
        public decimal SectionScore { get; set; }

        /// <summary> 问题列表 </summary>
        public Dictionary<string, NPaperQuestionDto> Questions { get; set; }

        /// <summary> 构造函数 </summary>
        public NPaperSectionDto()
        {
            Questions = new Dictionary<string, NPaperQuestionDto>();
        }
    }

    /// <summary> 试卷题目 </summary>
    public class NPaperQuestionDto : DDto
    {
        /// <summary> 排序 </summary>
        public int Sort { get; set; }

        /// <summary> 分数 </summary>
        public decimal Score { get; set; }
    }
}
