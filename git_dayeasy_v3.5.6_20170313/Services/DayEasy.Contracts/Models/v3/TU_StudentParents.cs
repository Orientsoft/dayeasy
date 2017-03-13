using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 学生家长关系表 </summary>
    public class TU_StudentParents : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        public long ParentId { get; set; }

        [Required]
        public long StudentId { get; set; }

        [Required]
        public byte RelationType { get; set; }

        [Required]
        public byte Status { get; set; }

        [Required]
        public DateTime AddedAt { get; set; }

        [Required]
        public long AddedBy { get; set; }
    }
}
