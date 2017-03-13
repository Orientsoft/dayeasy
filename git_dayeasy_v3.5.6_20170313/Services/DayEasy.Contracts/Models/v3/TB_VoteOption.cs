using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TB_VoteOption : DEntity<string>
    {
        [Key]
        [Column("ID")]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string VoteId { get; set; }

        [Required]
        [StringLength(1024)]
        public string OptionContent { get; set; }

        public int Count { get; set; }

        public int Sort { get; set; }
    }
}
