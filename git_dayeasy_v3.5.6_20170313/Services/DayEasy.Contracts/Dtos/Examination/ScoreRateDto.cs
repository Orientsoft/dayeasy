using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 得分率 </summary>
    public class ScoreRateDto : DDto
    {
        /// <summary> 题目序号 </summary>
        public Dictionary<string, string> QuestionSorts { get; set; }
        /// <summary> 班级列表 </summary>
        public Dictionary<string, string> ClassList { get; set; }
        /// <summary> 年级得分率 </summary>
        public Dictionary<string, decimal> ScoreRates { get; set; }
        /// <summary> 各题各班得分率 </summary>
        public Dictionary<string, Dictionary<string, decimal>> ClassScoreRates { get; set; }

        /// <summary> 构造函数 </summary>
        public ScoreRateDto()
        {
            QuestionSorts = new Dictionary<string, string>();
            ClassList = new Dictionary<string, string>();
            ScoreRates = new Dictionary<string, decimal>();
            ClassScoreRates = new Dictionary<string, Dictionary<string, decimal>>();
        }
    }
}
