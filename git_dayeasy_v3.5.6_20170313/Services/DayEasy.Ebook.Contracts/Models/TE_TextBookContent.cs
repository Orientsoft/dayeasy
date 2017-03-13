using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{

    public class TE_TextBookContent : DEntity<string>
    {
        [Key]
        [Column("ContentId")]
        public override string Id { get; set; }
        [NotMapped]
        public string ContentId { get { return Id; } }
        public string ChapterId { get; set; }
        public string TextBookId { get; set; }
        public string SectionName { get; set; }
        public string QuestionIDs { get; set; }
        public Nullable<int> Sort { get; set; }
        public System.DateTime AddedAt { get; set; }

        public virtual TE_Chapter TE_Chapter { get; set; }
    }
}
