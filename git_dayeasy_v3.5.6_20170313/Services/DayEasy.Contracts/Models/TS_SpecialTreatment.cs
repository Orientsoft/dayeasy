using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_SpecialTreatment : DEntity<string>
    {
        public string Batch { get; set; }
        public byte SourceType { get; set; }
        public string SourceIDs { get; set; }
        public bool IsShowAnswer { get; set; }
        public byte StageID { get; set; }
        public int SubjectID { get; set; }
        public long SendUserID { get; set; }
        public long RecieveUserID { get; set; }
        public System.DateTime SendTime { get; set; }
        public byte Status { get; set; }
        public string Remarks { get; set; }
    }
}
