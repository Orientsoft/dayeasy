using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_UserAgencyRelation : DEntity<string>
    {
        [Key]
        [Column("RelationID")]
        public override string Id { get; set; }

        [ForeignKey("TU_User")]
        public long UserID { get; set; }
        public string ClassID { get; set; }
        public string AgencyID { get; set; }
        public byte Status { get; set; }
        public System.DateTime ApplyTime { get; set; }
        public Nullable<DateTime> CheckPassTime { get; set; }
        public Nullable<long> CheckedUserID { get; set; }
        public string ApplyMsg { get; set; }
        public byte UserRole { get; set; }
        public byte RelationType { get; set; }
        public string RefuseMsg { get; set; }

        public virtual TU_User TU_User { get; set; }
    }
}
