using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Publish
{
    /// <summary>
    /// 学生作业中心列表
    /// </summary>
    public class StudentWorksDto : DDto
    {
        public int SubjectId { get; set; }
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public bool IsFinished { get; set; }
        public string GroupName { get; set; }
        public string GroupId { get; set; }
        public string PaperName { get; set; }
        public DateTime StartTime { get; set; }
        public byte SourceType { get; set; }
    }

    /// <summary>
    /// 学生作业中心搜索条件
    /// </summary>
    public class SearchStuWorkDto : DPage
    {
        public SearchStuWorkDto()
        {
            SubjectId = -1;
        }

        public long UserId { get; set; }
        public string Key { get; set; }
        public int SubjectId { get; set; }
    }

}
