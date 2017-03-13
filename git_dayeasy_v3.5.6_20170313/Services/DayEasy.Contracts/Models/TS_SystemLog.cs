using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_SystemLog : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }
        public DateTime Time { get; set; }

        [Required]
        [StringLength(50)]
        public string RunTime { get; set; }
        public string Depth { get; set; }
        public string WebSite { get; set; }
        public string Method { get; set; }
        public string File { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
        public byte Status { get; set; }
        public DateTime? ResolutionTime { get; set; }
    }
}
