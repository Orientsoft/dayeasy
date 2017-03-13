using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_JointException : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string JointBatch { get; set; }

        public long UserId { get; set; }

        [Required]
        [StringLength(32)]
        public string PictureId { get; set; }

        [Required]
        [StringLength(8)]
        public string StudentName { get; set; }

        public byte? ExceptionType { get; set; }

        [Required]
        [StringLength(128)]
        public string Message { get; set; }

        public byte Status { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime? SolveTime { get; set; }

        public string QuestionIds { get; set; }
    }
}
