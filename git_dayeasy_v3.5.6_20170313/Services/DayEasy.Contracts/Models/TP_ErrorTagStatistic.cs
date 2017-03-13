using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_ErrorTagStatistic : DEntity<string>
    {
        [ForeignKey("TP_ErrorTag")]
        public string TagId { get; set; }
        public string ClassId { get; set; }
        public string QuestionId { get; set; }
        public int UsgCount { get; set; }
    
        public virtual TP_ErrorTag TP_ErrorTag { get; set; }
    }
}
