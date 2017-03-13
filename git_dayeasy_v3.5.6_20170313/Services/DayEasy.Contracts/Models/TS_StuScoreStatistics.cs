using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_StuScoreStatistics : DEntity<string>
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string ClassId { get; set; }
        public long StudentId { get; set; }
        public int SubjectId { get; set; }
        public decimal CurrentScore { get; set; }
        public int CurrentSort { get; set; }
        public int ErrorQuCount { get; set; }
        public decimal SectionAScore { get; set; }
        public decimal SectionBScore { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
        public byte Status { get; set; }
    }
}
