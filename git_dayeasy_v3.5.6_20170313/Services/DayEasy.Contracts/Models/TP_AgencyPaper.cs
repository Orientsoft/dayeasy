using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_AgencyPaper : DEntity<string>
    {
        [Key]
        [Column("RelationID")]
        public override string Id { get; set; }

        [ForeignKey("TP_Paper")]
        public string PaperID { get; set; }
        public string AgencyID { get; set; }
        public byte Status { get; set; }
        public System.DateTime ShareTime { get; set; }
        public long UserID { get; set; }

        public virtual TP_Paper TP_Paper { get; set; }
        public virtual TU_Agency TU_Agency { get; set; }
    }
}
