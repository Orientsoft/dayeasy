using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TG_Share : DEntity<string>
    {
        [Key]
        [Column("GroupId")]
        [StringLength(32)]
        public override string Id { get; set; }
        /// <summary> 频道 </summary>
        public int ClassType { get; set; }

        /// <summary> 标签 </summary>
        [StringLength(512)]
        public string Tags { get; set; }

        /// <summary> 发帖权限 </summary>
        public byte PostAuth { get; set; }
        /// <summary> 加圈权限 </summary>
        public byte JoinAuth { get; set; }

        /// <summary> 帖子数 </summary>
        public int TopicNum { get; set; }

        /// <summary> 公告 </summary>
        public string Notice { get; set; }
    }
}
