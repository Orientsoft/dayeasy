using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_Quotations : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }
        public long UserId { get; set; }
        [StringLength(512)]
        public string Content { get; set; }
        public int GoodsCount { get; set; }

        public DateTime CreationTime { get; set; }
        public long CreatorId { get; set; }
    }
}
