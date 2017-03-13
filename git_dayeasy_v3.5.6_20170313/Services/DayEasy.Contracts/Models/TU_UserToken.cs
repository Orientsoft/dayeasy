using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_UserToken : DEntity<string>
    {
        [Key]
        [Column("Token")]
        public override string Id { get; set; }

        [ForeignKey("TU_User")]
        public long UserID { get; set; }
        public System.DateTime AddedAt { get; set; }
        public Nullable<DateTime> ExpireTime { get; set; }
        public string SystemEnvironment { get; set; }
        public string AddedIp { get; set; }
        public byte Comefrom { get; set; }

        public virtual TU_User TU_User { get; set; }
    }
}
