using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    [AutoMap(typeof(TS_TeacherStatistic))]
    public class TeacherStatisticDto : DDto
    {
        [MapFrom("Id")]
        public long UserID { get; set; }
        public int AddQuestionCount { get; set; }
        public int AddClassCount { get; set; }
        public int AddPaperCount { get; set; }
        public int PublishPaperCount { get; set; }
        public int PublishClassCount { get; set; }
        public int PushQuestionCount { get; set; }
        public int MarkingHomeworkCount { get; set; }
        public int HandleClassCount { get; set; }
    }
}
