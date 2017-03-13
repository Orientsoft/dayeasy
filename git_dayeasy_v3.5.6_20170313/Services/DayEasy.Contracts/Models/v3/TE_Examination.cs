using DayEasy.Core.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace DayEasy.Contracts.Models
{
    /// <summary> 大型考试 </summary>
    public class TE_Examination : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        /// <summary> 考试名称 </summary>
        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        /// <summary> 机构ID </summary>
        [Required]
        [StringLength(32)]
        public string AgencyId { get; set; }

        /// <summary> 机构名称 </summary>
        [Required]
        [StringLength(64)]
        public string AgencyName { get; set; }

        /// <summary> 学段 </summary>
        public byte Stage { get; set; }

        /// <summary> 涵盖科目 </summary>
        [Required]
        [StringLength(64)]
        public string Subjects { get; set; }

        /// <summary> 协同批次列表 </summary>
        [Required]
        [StringLength(640)]
        public string JointBatches { get; set; }

        /// <summary> 状态 </summary>
        public byte Status { get; set; }
        /// <summary> 考试类型 </summary>
        public byte ExamType { get; set; }
        /// <summary> 包含班级 </summary>
        [StringLength(1024)]
        public string ClassList { get; set; }

        /// <summary> 考试时间 </summary>
        public DateTime ExamTime { get; set; }

        /// <summary> 推送时间 </summary>
        public DateTime? CreationTime { get; set; }

        /// <summary> 创建人 </summary>
        public long CreatorId { get; set; }

        /// <summary> 发布时间 </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary> 发布人 </summary>
        public long? PublisherId { get; set; }

        /// <summary> 学生总人数 </summary>
        public int? StudentCount { get; set; }

        /// <summary> 年级平均分 </summary>
        public decimal? AverageScore { get; set; }

        /// <summary> 联考批次 </summary>
        [StringLength(32)]
        public string UnionBatch { get; set; }
    }
}
