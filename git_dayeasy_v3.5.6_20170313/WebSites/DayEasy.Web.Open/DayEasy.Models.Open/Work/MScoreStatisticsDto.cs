
using System.Collections.Generic;

namespace DayEasy.Models.Open.Work
{
    /// <summary> 成绩统计 </summary>
    public class MScoreStatisticsDto : DDto
    {
        public decimal Score { get; set; }
        public decimal AScore { get; set; }
        public decimal BScore { get; set; }
        public int Rank { get; set; }
        public decimal AverageScore { get; set; }
        public List<MKpAnalysisDto> KpAnalysis { get; set; }
    }
}
