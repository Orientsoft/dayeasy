using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_ImpressionLike : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }
        [StringLength(32)]
        public string ContentId { get; set; }

        public long UserId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
