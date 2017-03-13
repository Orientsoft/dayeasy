using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 协同阅卷-按题批阅-题目分包 </summary>
    public class TP_JointBag : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string JointBatch { get; set; }

        public byte SectionType { get; set; }

        [Required]
        [StringLength(32)]
        public string DistributionId { get; set; }

        [Required]
        [StringLength(1024)]
        public string QuestionIds { get; set; }

        public int MarkingCount { get; set; }

        [StringLength(128)]
        public string Region { get; set; }
    }
}
