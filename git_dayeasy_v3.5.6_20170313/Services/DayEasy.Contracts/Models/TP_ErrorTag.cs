using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_ErrorTag : DEntity<string>
    {
        public TP_ErrorTag()
        {
            TP_ErrorTagStatistic = new HashSet<TP_ErrorTagStatistic>();
        }

        [Key]
        [StringLength(32)]
        public override string Id { get; set; }
        public int SubjectId { get; set; }
        public string TagName { get; set; }
        public long AddedBy { get; set; }
        public System.DateTime AddedAt { get; set; }
        public int UseCount { get; set; }

        public virtual ICollection<TP_ErrorTagStatistic> TP_ErrorTagStatistic { get; set; }
    }
}
