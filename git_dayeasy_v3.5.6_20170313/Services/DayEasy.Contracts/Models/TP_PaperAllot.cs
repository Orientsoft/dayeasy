using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_PaperAllot : DEntity<string>
    {
        public string PaperId { get; set; }
        public long SenderId { get; set; }
        public long ReceiveId { get; set; }
        public byte Status { get; set; }
        public System.DateTime Time { get; set; }
    }
}
