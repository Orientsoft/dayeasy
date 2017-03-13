using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_Chapter : DEntity<string>
    {
        public TE_Chapter()
        {
            TE_TextBookContent = new HashSet<TE_TextBookContent>();
        }

        [Key]
        [Column("ChapterId")]
        public override string Id { get; set; }
        [NotMapped]
        public string ChapterId { get { return Id; } }
        public string TextBookId { get; set; }
        public string ChapterName { get; set; }
        public string ParentId { get; set; }
        public string ChapterCode { get; set; }
        public int Sort { get; set; }
        public byte Status { get; set; }
        public int ChildrenCount { get; set; }
        public bool HasChild { get; set; }

        public virtual TE_TextBook TE_TextBook { get; set; }
        public virtual ICollection<TE_TextBookContent> TE_TextBookContent { get; set; }
    }
}
