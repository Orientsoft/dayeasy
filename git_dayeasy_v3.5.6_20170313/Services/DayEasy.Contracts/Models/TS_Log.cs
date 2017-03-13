using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_Log : DEntity<string>
    {
        public byte LogType { get; set; }
        public string LogContent { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedUserID { get; set; }
        public string AddedIP { get; set; }
    }
}
