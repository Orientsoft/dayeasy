using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_TeacherStatistic : DEntity<long>
    {
        [Key]
        [ForeignKey("User")]
        [Column("UserID")]
        public override long Id { get; set; }
        public int AddQuestionCount { get; set; }
        public int AddClassCount { get; set; }
        public int AddPaperCount { get; set; }
        public int PublishPaperCount { get; set; }
        public int PublishClassCount { get; set; }
        public int PushQuestionCount { get; set; }
        public int MarkingHomeworkCount { get; set; }
        public int HandleClassCount { get; set; }

        public virtual TU_User User { get; set; }
    }
}
