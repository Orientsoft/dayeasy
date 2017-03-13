using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> ѡ�޿γ� </summary>
    public class TS_ElectiveCourse : DEntity
    {
        [Key]
        [Column("CourseId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        /// <summary> �γ����� </summary>
        [Required]
        [StringLength(64)]
        public string CourseName { get; set; }

        /// <summary> �Ͽε�ַ </summary>
        [Required]
        [StringLength(128)]
        public string Address { get; set; }

        /// <summary> �Ͽν�ʦ </summary>
        [Required]
        [StringLength(64)]
        public string TeacherName { get; set; }

        /// <summary> �༶���� </summary>
        public int ClassCapacity { get; set; }

        /// <summary> ������ </summary>
        public int TotalCapacity { get; set; }

        /// <summary> ״̬ </summary>
        public byte Status { get; set; }

        /// <summary> ����ID </summary>
        [Required]
        [StringLength(32)]
        public string AgencyId { get; set; }
        /// <summary> ѧ�� </summary>
        public byte? Stage { get; set; }

        /// <summary> �꼶 </summary>
        public int? Grade { get; set; }

        /// <summary> ���ʱ�� </summary>
        public System.DateTime AddedAt { get; set; }

        /// <summary> ����� </summary>
        public long AddedBy { get; set; }
        /// <summary> ѡ������ </summary>
        [Required]
        [StringLength(32)]
        public string Batch { get; set; }
        /// <summary> �༶�б� </summary>
        [StringLength(1024)]
        public string ClassList { get; set; }
        /// <summary> ��ѡ���� </summary>
        public int SelectedCount { get; set; }
    }
}
