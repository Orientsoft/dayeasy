using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 协同阅卷 - 试卷分配 </summary>
    public class TP_JointPictureDistribution : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string PictureId { get; set; }

        [Required]
        [StringLength(32)]
        public string QuestionGroupId { get; set; }

        public long TeacherId { get; set; }

        public DateTime? DistributionTime { get; set; }

        public DateTime? LastMarkingTime { get; set; }
    }
}
