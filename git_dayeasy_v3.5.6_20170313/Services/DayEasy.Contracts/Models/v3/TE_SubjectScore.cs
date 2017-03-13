using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 考试科目汇总 </summary>
    public class TE_SubjectScore : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        /// <summary> 考试Id </summary>
        [Required]
        [StringLength(32)]
        public string ExamId { get; set; }

        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }

        /// <summary> 科目 </summary>
        [Required]
        [StringLength(8)]
        public string Subject { get; set; }

        /// <summary> 协同批次 </summary>
        [Required]
        [StringLength(32)]
        public string JointBatch { get; set; }

        /// <summary> 同事圈ID </summary>
        [Required]
        [StringLength(32)]
        public string GroupId { get; set; }

        /// <summary> 试卷Id </summary>
        [Required]
        [StringLength(32)]
        public string PaperId { get; set; }

        /// <summary> 试卷类型 </summary>
        public byte PaperType { get; set; }

        /// <summary> 试卷标题/// </summary>
        [Required]
        [StringLength(128)]
        public string PaperTitle { get; set; }

        /// <summary> A卷平均分 </summary>
        public decimal AverageScoreA { get; set; }

        /// <summary> B卷平均分 </summary>
        public decimal? AverageScoreB { get; set; }

        /// <summary> 平均分 </summary>
        public decimal AverageScore { get; set; }
    }
}
