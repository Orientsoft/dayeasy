using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TT_TutorContent : DEntity<string>
    {
        [Key]
        [Column("ContentId")]
        public override string Id { get; set; }
        public string TutorId { get; set; }
        public string Remarks { get; set; }
        public string Content { get; set; }
        public byte SourceType { get; set; }
        public int Sort { get; set; }
    }
}
