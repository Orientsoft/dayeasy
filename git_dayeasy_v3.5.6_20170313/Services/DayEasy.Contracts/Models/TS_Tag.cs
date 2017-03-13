using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_Tag : DEntity
    {
        [Key]
        [Column("TagID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public string TagName { get; set; }
        public string FullPinYin { get; set; }
        public string SimplePinYin { get; set; }
        public int UsedCount { get; set; }
        public byte Status { get; set; }
        public byte TagType { get; set; }
    }
}
