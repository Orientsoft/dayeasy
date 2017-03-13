using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_TeacherKpStatistic : DEntity<string>
    {
        public int KpID { get; set; }
        public string KpLayerCode { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public int AnswerCount { get; set; }
        public int ErrorCount { get; set; }
        public int SubjectID { get; set; }
        public string ClassID { get; set; }
    }
}
