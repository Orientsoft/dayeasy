using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_AgencyVisit : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }
        [StringLength(32)]
        public string AgencyId { get; set; }
        public long VisitUserId { get; set; }
        public DateTime VisitTime { get; set; }
    }
}
