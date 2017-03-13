using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TB_Praise : DEntity<string>
    {
        [Key]
        [Column("ID")]
        [StringLength(32)]
        public override string Id { get; set; }
        public string TopicId { get; set; }
        public long UserId { get; set; }
    }
}
