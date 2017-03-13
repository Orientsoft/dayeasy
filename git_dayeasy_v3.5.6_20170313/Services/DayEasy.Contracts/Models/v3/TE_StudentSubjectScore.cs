using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TE_StudentSubjectScore : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        /// <summary> 考试Id </summary>
        [Required]
        [StringLength(32)]
        public string ExamId { get; set; }

        /// <summary> 考试科目汇总ID </summary>
        [Required]
        [StringLength(32)]
        public string ExamSubjectId { get; set; }

        /// <summary> 学生ID </summary>
        public long StudentId { get; set; }

        /// <summary> 班级ID </summary>
        [Required]
        [StringLength(32)]
        public string ClassId { get; set; }

        /// <summary> 考试批次 </summary>
        [StringLength(32)]
        public string Batch { get; set; }

        /// <summary> A卷分数 </summary>
        public decimal ScoreA { get; set; }

        /// <summary> B卷分数 </summary>
        public decimal ScoreB { get; set; }

        /// <summary> 总分 </summary>
        public decimal Score { get; set; }

        /// <summary> 单科班级排名 </summary>
        public int ClassRank { get; set; }

        /// <summary> 单科年级排名 </summary>
        public int Rank { get; set; }
    }
}
