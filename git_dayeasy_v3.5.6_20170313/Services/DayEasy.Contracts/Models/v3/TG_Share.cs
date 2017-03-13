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
        /// <summary> Ƶ�� </summary>
        public int ClassType { get; set; }

        /// <summary> ��ǩ </summary>
        [StringLength(512)]
        public string Tags { get; set; }

        /// <summary> ����Ȩ�� </summary>
        public byte PostAuth { get; set; }
        /// <summary> ��ȦȨ�� </summary>
        public byte JoinAuth { get; set; }

        /// <summary> ������ </summary>
        public int TopicNum { get; set; }

        /// <summary> ���� </summary>
        public string Notice { get; set; }
    }
}
