using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TG_Resources : DEntity<string>
    {
        [Key]
        [Column("ResourcesId")]
        [StringLength(32)]
        public override string Id { get; set; }
        
        [Required]
        [StringLength(32)]
        public string GroupId { get; set; }

        public byte ContentType { get; set; }

        [Required]
        [StringLength(32)]
        public string ContentId { get; set; }

        public long AddedBy { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
