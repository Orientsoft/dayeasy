using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_StudentGroupMember : DEntity<string>
    {
        [Key]
        [Column("GroupId")]
        public override string Id { get; set; }
        [NotMapped]
        public string GroupId { get { return Id; } }
        public long StudentId { get; set; }
        public string ClassId { get; set; }
    }
}
