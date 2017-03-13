using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TG_Class : DEntity<string>
    {
        [Key]
        [Column("GroupId")]
        [StringLength(32)]
        public override string Id { get; set; }
        
        [Required]
        [StringLength(32)]
        public string AgencyId { get; set; }

        public byte Stage { get; set; }

        public int GradeYear { get; set; }
    }
}
