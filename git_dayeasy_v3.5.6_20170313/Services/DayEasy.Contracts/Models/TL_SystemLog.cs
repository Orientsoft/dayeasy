using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TL_SystemLog : DEntity<long>
    {
        [Key]
        [Column("LogId")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        public byte LogType { get; set; }
        public byte LogLevel { get; set; }
        public string LogTitle { get; set; }
        public string LogDetail { get; set; }
        public long CreatorId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIp { get; set; }
    }
}
