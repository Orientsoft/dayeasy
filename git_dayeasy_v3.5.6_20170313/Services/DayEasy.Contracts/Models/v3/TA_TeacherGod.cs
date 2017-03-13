using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 名师大神 </summary>
    public class TA_TeacherGod : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }
        /// <summary> 区域编码 </summary>
        public int AreaCode { get; set; }
        /// <summary> 学校名称 </summary>
        [Required]
        [StringLength(64)]
        public string School { get; set; }

        /// <summary> 姓名/称呼 </summary>
        [Required]
        [StringLength(16)]
        public string Name { get; set; }
        /// <summary> 创建者 </summary>
        [StringLength(16)]
        public string Creator { get; set; }
        /// <summary> 模板类型 </summary>
        public byte Type { get; set; }
        /// <summary> 文字 </summary>
        [StringLength(64)]
        public string Word { get; set; }
        /// <summary> 电话号码 </summary>
        [StringLength(16)]
        public string Mobile { get; set; }
        /// <summary> 海报链接 </summary>
        [Required]
        [StringLength(128)]
        public string PosterUrl { get; set; }

        /// <summary> 创建时间 </summary>
        public DateTime CreationTime { get; set; }
        /// <summary> 创建者IP </summary>
        [StringLength(64)]
        public string CreatorIp { get; set; }
    }
}
