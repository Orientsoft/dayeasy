using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TC_StudentQuestion : DEntity
    {
        [Key]
        [Column("RecordId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public string StudentQuestionId { get; set; }
        public long StudentId { get; set; }
        public string Batch { get; set; }
        public byte SourceType { get; set; }
        public string SourceId { get; set; }
        public string Content { get; set; }
        public Nullable<int> Sort { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIp { get; set; }
    }
}
