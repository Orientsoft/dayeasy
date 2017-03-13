using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{

    public class TP_AnswerShare : DEntity<string>
    {
        public TP_AnswerShare()
        {
            TP_WorshipDetail = new HashSet<TP_WorshipDetail>();
        }

        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string QuestionId { get; set; }
        public string ClassId { get; set; }
        public string AddedName { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedAt { get; set; }
        public int WorshipCount { get; set; }
        public byte Status { get; set; }

        public virtual ICollection<TP_WorshipDetail> TP_WorshipDetail { get; set; }
    }
}
