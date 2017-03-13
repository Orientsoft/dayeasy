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
    public class TS_SchoolBookChapter : DEntity<string>
    {
        [Key]
        [Column("Id")]
        [StringLength(32)]
        public override string Id { get; set; }
        
        [Required]
        [StringLength(32)]
        public string Code { get; set; }

        [Required]
        [StringLength(64)]
        public string Title { get; set; }

        public string Knowledge { get; set; }

        public byte Status { get; set; }

        public int Sort { get; set; }

        public bool HasChild { get; set; }

        public bool IsLast { get; set; }

    }
}
