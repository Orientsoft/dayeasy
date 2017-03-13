using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> Ìû×ÓÍ¶Æ± </summary>
    public partial class TB_Vote : DEntity<string>
    {
        [Key]
        [Column("ID")]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string TopicId { get; set; }

        [StringLength(256)]
        public string Title { get; set; }

        [StringLength(512)]
        public string ImgUrl { get; set; }

        public bool IsSingleSelection { get; set; }

        public bool IsPublic { get; set; }

        public DateTime? FinishedAt { get; set; }

        public long AddedBy { get; set; }

        public DateTime AddedAt { get; set; }

        public byte Status { get; set; }
    }
}
