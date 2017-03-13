using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TC_AgencyVideo : DEntity<string>
    {
        [Key]
        [Column("RelationID")]
        public override string Id { get; set; }
        public string VideoID { get; set; }
        public string AgencyID { get; set; }
        public byte Status { get; set; }
        public System.DateTime ShareTime { get; set; }
        public long UserID { get; set; }

        public virtual TC_Video TC_Video { get; set; }
        public virtual TU_Agency TU_Agency { get; set; }
    }
}
