using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{

    public class TE_BookResult : DEntity<string>
    {
        public TE_BookResult()
        {
            TE_BookDetail = new HashSet<TE_BookDetail>();
        }
        [Key]
        [Column("ResultId")]
        public override string Id { get; set; }
        [NotMapped]
        public string ResultId { get { return Id; } }
        public string TextBookId { get; set; }
        public string ChapterId { get; set; }
        public string ClassId { get; set; }
        public long SenderId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public bool IsMarked { get; set; }
        public int ErrorCount { get; set; }
        public Nullable<DateTime> MarkingTime { get; set; }
        public Nullable<long> MarkingBy { get; set; }
        public byte ErrorCorrectStatus { get; set; }

        public virtual ICollection<TE_BookDetail> TE_BookDetail { get; set; }
        public virtual TE_TextBook TE_TextBook { get; set; }
    }
}
