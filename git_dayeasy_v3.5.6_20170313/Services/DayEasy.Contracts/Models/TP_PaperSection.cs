using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_PaperSection : DEntity<string>
    {
        public TP_PaperSection()
        {
            TP_PaperContent = new HashSet<TP_PaperContent>();
        }

        [Key]
        [Column("SectionID")]
        public override string Id { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        [ForeignKey("TP_Paper")]
        public string PaperID { get; set; }
        public long AddedBy { get; set; }
        public int SectionQuType { get; set; }
        public byte PaperSectionType { get; set; }
        public decimal SectionScore { get; set; }

        public virtual TP_Paper TP_Paper { get; set; }
        public virtual ICollection<TP_PaperContent> TP_PaperContent { get; set; }
    }
}
