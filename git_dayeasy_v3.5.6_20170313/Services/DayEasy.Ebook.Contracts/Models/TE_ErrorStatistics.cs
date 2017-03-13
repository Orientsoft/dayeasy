using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_ErrorStatistics : DEntity<string>
    {
        [Key]
        [Column("StatisticsId")]
        public override string Id { get; set; }
        public string StatisticsId { get; set; }
        public System.DateTime StatisticsDate { get; set; }
        public int SubjectId { get; set; }
        public long StudentId { get; set; }
        public int ErrorCount { get; set; }
        public string ClassId { get; set; }
    }
}
