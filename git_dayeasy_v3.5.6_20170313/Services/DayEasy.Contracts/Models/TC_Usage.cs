using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TC_Usage : DEntity<string>
    {
        public TC_Usage()
        {
            TP_MarkingResult = new HashSet<TP_MarkingResult>();
        }

        [Key]
        [Column("Batch")]
        public override string Id { get; set; }
        public string SourceID { get; set; }
        public byte SourceType { get; set; }

        [StringLength(32)]
        public string ClassId { get; set; }

        /// <summary> 同事圈ID </summary>
        [StringLength(32)]
        public string ColleagueGroupId { get; set; }

        /// <summary> 协同批次号 </summary>
        [StringLength(32)]
        public string JointBatch { get; set; }

        public int SubjectId { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime ExpireTime { get; set; }
        public Nullable<bool> IsControlOrder { get; set; }

        [ForeignKey("TU_User")]
        public long UserId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIP { get; set; }
        public byte ApplyType { get; set; }
        public Nullable<byte> PrintType { get; set; }
        public int Status { get; set; }
        public byte MarkingStatus { get; set; }

        public virtual TU_User TU_User { get; set; }
        public virtual ICollection<TP_MarkingResult> TP_MarkingResult { get; set; }
    }
}
