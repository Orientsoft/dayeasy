using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_ErrorReason : DEntity<string>
    {
        public TP_ErrorReason()
        {
            TP_ErrorReasonComment = new HashSet<TP_ErrorReasonComment>();
        }

        public string ErrorId { get; set; }
        public string QuestionId { get; set; }
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string ClassId { get; set; }
        public long StudentId { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public System.DateTime AddedAt { get; set; }
        public int CommentCount { get; set; }
        public int ShareType { get; set; }

        public virtual ICollection<TP_ErrorReasonComment> TP_ErrorReasonComment { get; set; }
    }
}
