using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary> 掌握情况 </summary>
    public class GraspingDto : DDto
    {
        /// <summary> 题目掌握情况 </summary>
        public List<QuestionGraspingDto> Questions { get; set; }

        /// <summary> 知识点掌握情况 </summary>
        public List<KnowledgeGraspingDto> Knowledges { get; set; }

        /// <summary> 构造函数 </summary>
        public GraspingDto()
        {
            Questions = new List<QuestionGraspingDto>();
            Knowledges = new List<KnowledgeGraspingDto>();
        }
    }
    /// <summary>客观题得分率</summary>
    public class ObjectQuestionScoreRate
    {
        public ObjectQuestionScoreRate()
        {
            PaperAQuestions = new List<QuestionGraspingDto>();
            PaperBQuestions = new List<QuestionGraspingDto>();
        }
        public int PaperACount { get; set; }
        public int PaperBCount { get; set; }
        public bool IsAb { get; set; }
        public List<QuestionGraspingDto> PaperAQuestions { get; set; }
        public List<QuestionGraspingDto> PaperBQuestions { get; set; }
    }

    /// <summary> 问题得分 </summary>
    public class QuestionScoreDto : DDto
    {
        /// <summary> 问题ID </summary>
        public string Id { get; set; }

        /// <summary> 小问ID </summary>
        public string SmallId { get; set; }

        /// <summary> 总分 </summary>
        public decimal Total { get; set; }

        /// <summary> 得分 </summary>
        public decimal Score { get; set; }
    }

    /// <summary> 题目掌握情况 </summary>
    public class QuestionGraspingDto : DQuestionSortDto
    {
        /// <summary> 得分率 </summary>
        public int ScoreRate { get; set; }
        public string RightKey { get; set; }
    }

    /// <summary> 知识点掌握情况 </summary>
    public class KnowledgeGraspingDto : DDto
    {
        /// <summary> 知识点编码 </summary>
        public string Code { get; set; }

        /// <summary> 知识点 </summary>
        public string Name { get; set; }

        /// <summary> 得分率 </summary>
        public int ScoreRate { get; set; }

        /// <summary> 包含的题目 </summary>
        public List<DQuestionSortDto> Questions { get; set; }
    }
}
