using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 学生考试成绩 </summary>
    public class TE_StudentScore : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        /// <summary> 考试Id </summary>
        [Required]
        [StringLength(32)]
        public string ExamId { get; set; }

        /// <summary> 学生ID </summary>
        [Required]
        public long StudentId { get; set; }

        /// <summary> 班级ID </summary>
        [Required]
        [StringLength(32)]
        public string ClassId { get; set; }

        /// <summary> 总分 </summary>
        public decimal TotalScore { get; set; }

        /// <summary> 班级排名 </summary>
        public int ClassRank { get; set; }

        /// <summary> 年级排名 </summary>
        public int Rank { get; set; }
    }
}
