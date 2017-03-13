using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_SmallQuestion : DEntity<string>
    {
        [Key]
        [Column("SmallQId")]
        public override string Id { get; set; }
        [NotMapped]
        public string SmallQId { get { return Id; } }
        public string QID { get; set; }
        public string SortNo { get; set; }
        public int OptionCount { get; set; }
        public string Answers { get; set; }
        public bool IsMultipleChoice { get; set; }

        public virtual TE_Question TE_Question { get; set; }
    }
}
