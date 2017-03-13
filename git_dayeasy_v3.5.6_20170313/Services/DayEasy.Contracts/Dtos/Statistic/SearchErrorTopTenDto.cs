using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary>
    /// 查询错题 Top 10
    /// </summary>
    public class SearchErrorTopTenDto : DDto
    {
        public SearchErrorTopTenDto()
        {
            WeekNum = 1;
            RegisTime = DateTime.Now;
        }

        public long UserId { get; set; }
        public DateTime RegisTime { get; set; }
        public int WeekNum { get; set; }
        public int SubjectId { get; set; }
        public string GroupId { get; set; }
    }

    /// <summary>
    /// 错题 Top 10 数据
    /// </summary>
    public class ErrorTopTenDataDto : DDto
    {
        public List<QuestionDto> Questions { get; set; }
        public Dictionary<string, int> ErrorAndCount { get; set; }
        public bool IsClickPrevWeek { get; set; }
        public string StartTimeStr { get; set; }
        public string EndTimeStr { get; set; }
    }
}
