using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_StudentKpStatistic : DEntity<string>
    {
        public long StudentID { get; set; }
        public int KpID { get; set; }
        public string KpLayerCode { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public int AnswerCount { get; set; }
        public int ErrorCount { get; set; }
        public int SubjectID { get; set; }
    }
}
