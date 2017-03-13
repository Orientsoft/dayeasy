using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    [AutoMap(typeof(TS_StudentStatistic))]
    public class StudentStatisticDto : DDto
    {
        [MapFrom("Id")]
        public long UserID { get; set; }
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
    }
}
