using System;

namespace DayEasy.AsyncMission.Models
{
    public class StudentScore
    {
        public string StudentNo { get; set; }
        public string PaperTitle { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public DateTime ExamTime { get; set; }
        public decimal Score { get; set; }
    }
}
