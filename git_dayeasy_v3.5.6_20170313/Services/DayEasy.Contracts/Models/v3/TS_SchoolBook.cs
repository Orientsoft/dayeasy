using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_SchoolBook : DEntity<string>
    {
        [Key]
        [Column("Id")]
        [StringLength(32)]
        public override string Id { get; set; }
        
        [Required]
        [StringLength(8)]
        public string Code { get; set; }

        [Required]
        [StringLength(64)]
        public string Title { get; set; }
        
        public int SubjectId { get; set; }

        public byte Stage { get; set; }

        public byte Status { get; set; }
        
        public DateTime AddedAt { get; set; }

    }
}
