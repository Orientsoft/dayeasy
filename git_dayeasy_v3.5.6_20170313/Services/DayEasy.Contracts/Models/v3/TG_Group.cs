using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TG_Group : DEntity<string>
    {
        [Key]
        [Column("GroupId")]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(16)]
        public string GroupCode { get; set; }

        [Required]
        [StringLength(64)]
        public string GroupName { get; set; }

        [StringLength(256)]
        public string GroupAvatar { get; set; }

        [StringLength(256)]
        public string GroupSummary { get; set; }

        public int GroupType { get; set; }

        public int Capacity { get; set; }

        public int MemberCount { get; set; }

        public int UnCheckedCount { get; set; }

        public byte Status { get; set; }

        public long ManagerId { get; set; }

        /// <summary> 圈子等级 </summary>
        public byte Level { get; set; }
        /// <summary> 认证等级 </summary>
        public byte? CertificationLevel { get; set; }

        public long AddedBy { get; set; }

        public DateTime AddedAt { get; set; }

        [StringLength(128)]
        public string GroupBanner { get; set; }
    }
}
