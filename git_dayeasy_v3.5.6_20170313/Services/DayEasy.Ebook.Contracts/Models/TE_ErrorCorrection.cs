using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_ErrorCorrection : DEntity<string>
    {
        [Key]
        [Column("ID")]
        public override string Id { get; set; }
        [NotMapped]
        public string ID { get { return Id; } }
        public string EBookId { get; set; }
        public string ChapterId { get; set; }
        public long TeacherId { get; set; }
        public string ClassId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public byte Status { get; set; }
    }
}
