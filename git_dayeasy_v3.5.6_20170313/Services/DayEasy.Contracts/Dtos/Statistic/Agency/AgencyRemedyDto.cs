using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Statistic.Agency
{
    /// <summary> 学情补救 </summary>
    public class AgencyRemedyDto : RefreshDto
    {
        /// <summary> 收集错题 </summary>
        public int Errors { get; set; }
        /// <summary> 错题科目 </summary>
        public Dictionary<string, int> ErrorDetail { get; set; }
        /// <summary> 错题知识点 </summary>
        public int ErrorKnowledge { get; set; }

        /// <summary> 错题分析 </summary>
        public AgencyRemedyItemDto ErrorAnalysis { get; set; }

        /// <summary> 错题下载 </summary>
        public AgencyRemedyItemDto ErrorDownload { get; set; }

        /// <summary> 错题过关 </summary>
        public AgencyRemedyItemDto ErrorPass { get; set; }

        /// <summary> 变式推送 </summary>
        public int Variants { get; set; }
        /// <summary> 辅导次数 </summary>
        public int Tutors { get; set; }
        /// <summary> 变式下载 </summary>
        public int VariantDownload { get; set; }
        /// <summary> 试卷下载 </summary>
        public int PaperDownload { get; set; }

        /// <summary> 科目错因分析 </summary>
        public List<AgencyGroupServeyDto> SubjectAnalysis { get; set; }
    }

    /// <summary> 学情分析 </summary>
    public class AgencyRemedyItemDto : DDto
    {
        /// <summary> 总数量 </summary>
        public int Total { get; set; }
        /// <summary> 分析/下载/过关数量 </summary>
        public int Count { get; set; }

        /// <summary> 占比 </summary>
        public float Percent
        {
            get
            {
                if (Total <= 0 || Count <= 0) return 0F;
                return (float)Math.Round((Count * 100F) / Total, 2, MidpointRounding.AwayFromZero);
            }
        }
    }
}
