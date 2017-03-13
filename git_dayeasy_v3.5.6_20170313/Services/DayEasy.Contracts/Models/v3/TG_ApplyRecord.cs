using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TG_ApplyRecord : DEntity<string>
    {
        [Key]
        [Column("RecordId")]
        [StringLength(32)]
        public override string Id { get; set; }
        
        [Required]
        [StringLength(32)]
        public string GroupId { get; set; }

        public long MemberId { get; set; }

        public byte Status { get; set; }

        [StringLength(256)]
        public string Message { get; set; }

        [StringLength(128)]
        public string CheckedMessage { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime? CheckedAt { get; set; }

        public long? AddedBy { get; set; }
    }
}
