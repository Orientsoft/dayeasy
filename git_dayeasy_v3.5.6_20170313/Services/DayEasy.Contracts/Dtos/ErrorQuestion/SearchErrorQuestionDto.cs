using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.ErrorQuestion
{
    public class SearchErrorQuestionDto : DDto
    {
        /// <summary>
        /// 圈子ID
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 时间范围
        /// </summary>
        public int DateRange { get; set; }
        /// <summary>
        /// 问题类型
        /// </summary>
        public int QuestionType { get; set; }
        /// <summary>
        /// 排列顺序
        /// </summary>
        public byte OrderOfArr { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }
        public int SubjectId { get; set; }
        /// <summary>
        /// 知识点Code
        /// </summary>
        public string KnowledgeCode { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }
}
