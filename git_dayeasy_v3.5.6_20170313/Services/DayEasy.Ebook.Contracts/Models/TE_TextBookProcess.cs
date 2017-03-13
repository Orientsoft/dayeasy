using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_TextBookProcess : DEntity<string>
    {
        [Key]
        [Column("TextBookId")]
        public override string Id { get; set; }
        [NotMapped]
        public string TextBookId { get { return Id; } }
        public long StudentId { get; set; }
        public string ChapterId { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime LastTime { get; set; }
    }
}
