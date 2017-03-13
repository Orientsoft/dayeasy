using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_TextBookUsage : DEntity<string>
    {
        [Key]
        [Column("Batch")]
        public override string Id { get; set; }
        [NotMapped]
        public string Batch { get { return Id; } }
        public string TextBookId { get; set; }
        public string ClassId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
        public byte Status { get; set; }

        public virtual TE_TextBook TE_TextBook { get; set; }
    }
}
