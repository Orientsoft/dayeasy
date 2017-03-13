using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TQ_AgencyQuestion : DEntity<string>
    {
        [Key]
        [Column("RelationID")]
        public override string Id { get; set; }
        [ForeignKey("TQ_Question")]
        public string QID { get; set; }
        public string AgencyID { get; set; }
        public byte Status { get; set; }
        public System.DateTime ShareTime { get; set; }
        public long UserID { get; set; }

        public virtual TQ_Question TQ_Question { get; set; }
        public virtual TU_Agency TU_Agency { get; set; }
    }
}
