using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 选修课程 </summary>
    public class TS_ElectiveCourse : DEntity
    {
        [Key]
        [Column("CourseId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        /// <summary> 课程名称 </summary>
        [Required]
        [StringLength(64)]
        public string CourseName { get; set; }

        /// <summary> 上课地址 </summary>
        [Required]
        [StringLength(128)]
        public string Address { get; set; }

        /// <summary> 上课教师 </summary>
        [Required]
        [StringLength(64)]
        public string TeacherName { get; set; }

        /// <summary> 班级容量 </summary>
        public int ClassCapacity { get; set; }

        /// <summary> 总容量 </summary>
        public int TotalCapacity { get; set; }

        /// <summary> 状态 </summary>
        public byte Status { get; set; }

        /// <summary> 机构ID </summary>
        [Required]
        [StringLength(32)]
        public string AgencyId { get; set; }
        /// <summary> 学段 </summary>
        public byte? Stage { get; set; }

        /// <summary> 年级 </summary>
        public int? Grade { get; set; }

        /// <summary> 添加时间 </summary>
        public System.DateTime AddedAt { get; set; }

        /// <summary> 添加人 </summary>
        public long AddedBy { get; set; }
        /// <summary> 选课批次 </summary>
        [Required]
        [StringLength(32)]
        public string Batch { get; set; }
        /// <summary> 班级列表 </summary>
        [StringLength(1024)]
        public string ClassList { get; set; }
        /// <summary> 已选数量 </summary>
        public int SelectedCount { get; set; }
    }
}
