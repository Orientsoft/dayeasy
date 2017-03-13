using DayEasy.Core.Domain.Entities;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

namespace DayEasy.Contracts.Models
{
    /// <summary> Í¬ÊÂÈ¦ </summary>
    public partial class TG_Colleague : DEntity<string>
    {
        [Key]
        [Column("GroupId")]
        [StringLength(32)]
        public override string Id { get; set; }
        
        [Required]
        [StringLength(32)]
        public string AgencyId { get; set; }

        public byte Stage { get; set; }

        public int SubjectId { get; set; }
    }
}
