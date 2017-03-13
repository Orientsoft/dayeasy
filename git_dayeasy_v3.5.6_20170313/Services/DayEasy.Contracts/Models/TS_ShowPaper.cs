using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_ShowPaper : DEntity<string>
    {
        public long SendUserID { get; set; }
        public byte Type { get; set; }
        public string SourceID { get; set; }
        public string Batch { get; set; }
        public string QID { get; set; }
        public long SourceUserID { get; set; }
        public int SubjectID { get; set; }
        public byte StageID { get; set; }
        public System.DateTime SendTime { get; set; }
        public string RecieveID { get; set; }
        public byte Status { get; set; }
        public string Remarks { get; set; }
    }
}
