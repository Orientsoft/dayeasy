using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_TempGroup : DEntity<string>
    {
        public long UserID { get; set; }
        public string MemberIDs { get; set; }
        public byte Status { get; set; }
        public System.DateTime AddedAt { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
    }
}
