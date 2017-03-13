using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_ClassScoreStatistics : DEntity<string>
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string ClassId { get; set; }
        public int SubjectId { get; set; }
        public decimal TheHighestScore { get; set; }
        public decimal TheLowestScore { get; set; }
        public decimal AverageScore { get; set; }
        public string SectionScores { get; set; }
        public string ScoreGroups { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
        public byte Status { get; set; }
        public string SectionScoreGroups { get; set; }
    }
}
