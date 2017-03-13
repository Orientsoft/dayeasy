using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_ErrorReasonComment : DEntity<string>
    {

        [ForeignKey("TP_ErrorReason")]
        public string ReasonId { get; set; }
        public string Content { get; set; }
        public long AddedBy { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }

        public virtual TP_ErrorReason TP_ErrorReason { get; set; }
    }
}
