using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_StudentStatistic : DEntity<long>
    {
        [Key]
        [Column("UserID")]
        [ForeignKey("User")]
        public override long Id { get; set; }
        public int FinishPaperCount { get; set; }
        public int FinishClassCount { get; set; }
        public int FinishQuestionCount { get; set; }
        public int ErrorQuestionCount { get; set; }
        public int TopTenFinishPaperCount { get; set; }
        public int TopTenFinishClassCount { get; set; }
        public int TheFirstCount { get; set; }
        public int NoHandHomeworkCount { get; set; }
        public int HomeworkCount { get; set; }
        public int ClassCount { get; set; }

        public virtual TU_User User { get; set; }
    }
}
