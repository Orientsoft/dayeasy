
using System.Collections.Generic;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Question
{
    /// <summary> 搜索问题对象 </summary>
    public class SearchQuestionDto : DPage
    {
        public long UserId { get; set; }
        public string Keyword { get; set; }
        public int ShareRange { get; set; }

        public int QuestionType { get; set; }

        public int SubjectId { get; set; }

        public bool IsHighLight { get; set; }

        /// <summary> 加载出题人 </summary>
        public bool LoadCreator { get; set; }
        public IEnumerable<int> Stages { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<string> Points { get; set; }

        public IEnumerable<double> Difficulties { get; set; }

        public IEnumerable<string> NotInIds { get; set; }

        public QuestionOrder Order { get; set; }
    }
}
