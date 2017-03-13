using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary> 试卷知识点包含的问题 </summary>
    public class KnowledgeQuestionsDto : DDto
    {
        /// <summary> 知识点编号 </summary>
        public string Code { get; set; }

        /// <summary> 知识点 </summary>
        public string Name { get; set; }

        /// <summary> 题目ID列表 </summary>
        public List<DQuestionSortDto> Questions { get; set; }

        /// <summary> 构造函数 </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        public KnowledgeQuestionsDto(string code, string name)
        {
            Code = code;
            Name = name;
            Questions = new List<DQuestionSortDto>();
        }
    }
}
