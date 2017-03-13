using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_Visit : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }
        public long UserId { get; set; }

        public long VisitUserId { get; set; }
        public DateTime VisitTime { get; set; }
    }
}
