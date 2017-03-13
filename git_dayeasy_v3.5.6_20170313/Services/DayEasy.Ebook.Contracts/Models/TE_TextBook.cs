using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{

    public class TE_TextBook : DEntity<string>
    {
        public TE_TextBook()
        {
            TE_BookResult = new HashSet<TE_BookResult>();
            TE_Chapter = new HashSet<TE_Chapter>();
            TE_TextBookUsage = new HashSet<TE_TextBookUsage>();
        }
        [Key]
        [Column("TextBookId")]
        public override string Id { get; set; }
        [NotMapped]
        public string TextBookId { get { return Id; } }
        public string TextBookName { get; set; }
        public string FrontCover { get; set; }
        public string Author { get; set; }
        public string TeachingMaterial { get; set; }
        public string PublishingCompany { get; set; }
        public int SubjectId { get; set; }
        public byte Stage { get; set; }
        public byte Term { get; set; }
        public int UseCount { get; set; }
        public byte Status { get; set; }
        public long UserId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIP { get; set; }

        public virtual ICollection<TE_BookResult> TE_BookResult { get; set; }
        public virtual ICollection<TE_Chapter> TE_Chapter { get; set; }
        public virtual ICollection<TE_TextBookUsage> TE_TextBookUsage { get; set; }
    }
}
