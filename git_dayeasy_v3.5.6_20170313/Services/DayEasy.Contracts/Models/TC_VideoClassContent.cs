using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{

    public class TC_VideoClassContent : DEntity<string>
    {
        [Key]
        [Column("ContentId")]
        public override string Id { get; set; }
        [ForeignKey("TC_VideoClass")]
        public string ClassRID { get; set; }
        public byte ContentType { get; set; }
        public string SourceId { get; set; }
        public byte ContentSort { get; set; }

        public virtual TC_VideoClass TC_VideoClass { get; set; }
    }
}
