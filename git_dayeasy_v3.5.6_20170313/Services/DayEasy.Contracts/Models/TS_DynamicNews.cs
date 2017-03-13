using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_DynamicNews : DEntity<string>
    {
        [Key]
        [Column("NewsID")]
        public override string Id { get; set; }
        public long SendUserID { get; set; }
        public byte NewsType { get; set; }
        public byte SourceType { get; set; }
        public string SourceID { get; set; }
        public string Batch { get; set; }
        public string RecieveID { get; set; }
        public System.DateTime SendTime { get; set; }
        public byte Status { get; set; }
        public int SeeCount { get; set; }
        public int GoodCount { get; set; }
        public string Remarks { get; set; }
    }
}
