using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Publish
{
    public class StudentScoreStatisticDto : DDto
    {
        public StudentScoreStatisticDto()
        {
            CurrentScore = "—";
            AvgScore = "—";
            Rank = "—";
        }

        public string Batch { get; set; }
        public string PaperId { get; set; }
        public byte SourceType { get; set; }
        public string PaperName { get; set; }
        public decimal PaperScore { get; set; }
        public string CurrentScore { get; set; }
        public int ErrorQuCount { get; set; }
        public string Rank { get; set; }
        public string AvgScore { get; set; }
        public List<ScoreGroupsDto> ScoreGroups { get; set; }
        public List<KpAnalysisDto> KpAnalysis { get; set; }
    }
}
