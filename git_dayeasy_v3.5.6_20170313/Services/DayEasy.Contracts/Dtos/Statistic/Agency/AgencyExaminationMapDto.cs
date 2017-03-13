using System;
using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Statistic.Agency
{
    /// <summary> 考试地图 </summary>
    public class AgencyExaminationMapDto : RefreshDto
    {
        /// <summary> 考试人次 </summary>
        public int UserCount { get; set; }

        /// <summary> 考试班次 </summary>
        public int ClassCount { get; set; }

        /// <summary> 原创试卷 </summary>
        public int PaperCount { get; set; }

        /// <summary> 考察知识点 </summary>
        public int KnowledgeCount { get; set; }

        /// <summary> 生成报告数 </summary>
        public int ReportCount { get; set; }

        /// <summary> 考试学科分布 </summary>
        public Dictionary<string, int> Subjects { get; set; }

        /// <summary> 考试班次分布 </summary>
        public List<AgencyGroupServeyDto> ClassList { get; set; }
    }
}
