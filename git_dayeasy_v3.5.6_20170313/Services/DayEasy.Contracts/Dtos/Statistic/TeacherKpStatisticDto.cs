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
    [AutoMap(typeof(TS_TeacherKpStatistic))]
    public class TeacherKpStatisticDto : DDto
    {
        [MapFrom("Id")]
        public string ID { get; set; }
        public int KpID { get; set; }
        public string KpLayerCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AnswerCount { get; set; }
        public int ErrorCount { get; set; }
        public int SubjectID { get; set; }

        [MapFrom("ClassID")]
        public string GroupId { get; set; }
    }
}
