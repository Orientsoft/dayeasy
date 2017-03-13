using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 协同阅卷 - 题目组合 </summary>
    public class TP_JointQuestionGroup : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string JointBatch { get; set; }

        [Required]
        [StringLength(32)]
        public string PaperId { get; set; }

        public byte SectionType { get; set; }

        [Required]
        [StringLength(1024)]
        public string QuestionIds { get; set; }
    }
}
