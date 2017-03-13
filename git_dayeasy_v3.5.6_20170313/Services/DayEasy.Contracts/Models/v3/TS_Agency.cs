using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 机构 </summary>
    public class TS_Agency : DEntity<string>
    {
        [Key]
        [Column("AgencyId")]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string AgencyName { get; set; }

        [Required]
        public byte AgencyType { get; set; }

        [StringLength(256)]
        public string AgencyLogo { get; set; }

        public byte Stage { get; set; }

        public int Sort { get; set; }

        public int AreaCode { get; set; }

        public byte Status { get; set; }

        [StringLength(128)]
        public string Summary { get; set; }

        [StringLength(128)]
        public string Banner { get; set; }
        public int VisitCount { get; set; }
        public int TargetCount { get; set; }
        public byte CertificationLevel { get; set; }
    }
}
