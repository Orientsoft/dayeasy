using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 协同阅卷 </summary>
    public class TP_JointMarking : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        [Column("JointBatch")]
        public override string Id { get; set; }

        /// <summary> 同事圈ID </summary>
        [Required]
        [StringLength(32)]
        public string GroupId { get; set; }

        [Required]
        [StringLength(32)]
        public string PaperId { get; set; }

        public byte Status { get; set; }

        public int PaperACount { get; set; }
        public int PaperBCount { get; set; }

        public long AddedBy { get; set; }
        public DateTime AddedAt { get; set; }

        public DateTime? FinishedTime { get; set; }
    }
}
