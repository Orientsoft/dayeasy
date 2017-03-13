using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TT_TutorRecord : DEntity<string>
    {
        [Key]
        [Column("RecordId")]
        public override string Id { get; set; }
        public string TutorId { get; set; }
        public long UserId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AgencyId { get; set; }
        public string ClassId { get; set; }
        public byte Status { get; set; }
    }
}
