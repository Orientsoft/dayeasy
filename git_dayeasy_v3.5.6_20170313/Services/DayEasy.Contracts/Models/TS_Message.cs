using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_Message : DEntity<string>
    {
        [Key]
        [Column("MessageId")]
        [StringLength(32)]
        public override string Id { get; set; }
        public long UserId { get; set; }
        public byte MessageType { get; set; }
        public byte MessageStatus { get; set; }
        [Required]
        [StringLength(300)]
        public string MessageTitle { get; set; }
        public string MessageContent { get; set; }
        public DateTime? ReadTime { get; set; }
        public DateTime CreateOn { get; set; }
        public long CreatorId { get; set; }
        [Required]
        [StringLength(50)]
        public string CreateIp { get; set; }
    }
}
