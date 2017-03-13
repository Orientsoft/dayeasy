using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TC_ClassContentRecord : DEntity<string>
    {
        [Key]
        [Column("RecordId")]
        public override string Id { get; set; }
        public string ClassContentId { get; set; }
        public long UserId { get; set; }
        public string Batch { get; set; }
        public DateTime AddedAt { get; set; }
        public string AddedIp { get; set; }

        public virtual TU_User TU_User { get; set; }
    }
}
