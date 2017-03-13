using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_Question : DEntity<string>
    {
        public TE_Question()
        {
            TE_SmallQuestion = new HashSet<TE_SmallQuestion>();
        }

        [Key]
        [Column("QID")]
        public override string Id { get; set; }
        [NotMapped]
        public string QID { get { return Id; } }
        public string SortNo { get; set; }
        public int OptionCount { get; set; }
        public string Answers { get; set; }
        public bool IsMultipleChoice { get; set; }
        public int SmallQuNum { get; set; }

        public virtual ICollection<TE_SmallQuestion> TE_SmallQuestion { get; set; }
    }
}
