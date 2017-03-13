
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 学科分析 </summary>
    public class SubjectAnalysisDto : ClassAnalysisKeyDto
    {
        /// <summary> A卷平均分 </summary>
        public decimal AverageScoreA { get; set; }

        /// <summary> B卷平均分 </summary>
        public decimal AverageScoreB { get; set; }

        public Dictionary<string, int> SegmentA { get; set; }
        public Dictionary<string, int> SegmentB { get; set; }
        public Dictionary<string, int> Segment { get; set; }
    }
}
