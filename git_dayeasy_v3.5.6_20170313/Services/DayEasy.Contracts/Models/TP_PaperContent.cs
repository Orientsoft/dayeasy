using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_PaperContent : DEntity<string>
    {
        [Key]
        [Column("ContentID")]
        public override string Id { get; set; }
        [ForeignKey("TP_Paper")]
        public string PaperID { get; set; }
        public byte PaperSectionType { get; set; }
        [ForeignKey("TP_PaperSection")]
        public string SectionID { get; set; }
        [ForeignKey("TQ_Question")]
        public string QuestionID { get; set; }
        public int Sort { get; set; }
        public decimal Score { get; set; }

        public virtual TP_Paper TP_Paper { get; set; }
        public virtual TP_PaperSection TP_PaperSection { get; set; }
        public virtual TQ_Question TQ_Question { get; set; }
    }
}
