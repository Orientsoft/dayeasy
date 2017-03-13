using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_ElectiveDetail : DEntity<string>
    {
        public long StudentId { get; set; }
        public int CourseId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public byte Status { get; set; }
        public string ClassId { get; set; }
        public string Batch { get; set; }
    }
}
