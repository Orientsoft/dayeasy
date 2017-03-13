using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 协同阅卷 - 题目分配 </summary>
    public class TP_JointDistribution : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string JointBatch { get; set; }

        public long TeacherId { get; set; }

        [Required]
        [StringLength(32)]
        public string QuestionGroupId { get; set; }
        /// <summary> 批阅数量 </summary>
        public int MarkingCount { get; set; }
        /// <summary> 批阅模式 </summary>
        public byte? MarkingType { get; set; }
    }
}
