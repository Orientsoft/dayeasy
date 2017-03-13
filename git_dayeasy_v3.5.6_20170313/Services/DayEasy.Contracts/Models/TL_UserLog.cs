using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TL_UserLog : DEntity<long>
    {
        [Key]
        [Column("UserLogId")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        public byte LogLevel { get; set; }

        [Required]
        [StringLength(300)]
        public string LogTitle { get; set; }

        [StringLength(2000)]
        public string LogDetail { get; set; }
        public long UserId { get; set; }
        public System.DateTime AddedAt { get; set; }

        [Required]
        [StringLength(30)]
        public string AddedIp { get; set; }
    }
}
