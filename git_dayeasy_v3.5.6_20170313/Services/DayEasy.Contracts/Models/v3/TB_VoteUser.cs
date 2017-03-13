using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TB_VoteUser : DEntity<string>
    {
        [Key]
        [Column("ID")]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string VoteId { get; set; }

        [Required]
        [StringLength(32)]
        public string VoteOptionId { get; set; }

        public long UserId { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
