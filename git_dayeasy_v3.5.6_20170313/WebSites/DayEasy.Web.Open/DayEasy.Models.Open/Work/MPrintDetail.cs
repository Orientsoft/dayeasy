using Newtonsoft.Json;
using System.Collections.Generic;

namespace DayEasy.Models.Open.Work
{
    /// <summary> 套打详情信息 </summary>
    public class MPrintDetail : DDto
    {
        /// <summary> ID </summary>
        public string Id { get; set; }

        /// <summary> 学生ID </summary>
        public long StudentId { get; set; }

        /// <summary> 学生姓名 </summary>
        public string StudentName { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        /// <summary> 学校ID </summary>
        public string AgencyId { get; set; }
        /// <summary> 学校名称 </summary>
        public string Agency { get; set; }

        /// <summary> 图片路径 </summary>
        public string ImagePath { get; set; }

        /// <summary> 试卷类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 序号 </summary>
        public int Index { get; set; }

        /// <summary> 得分 </summary>
        public decimal Score { get; set; }

        /// <summary> 客观题错误信息 </summary>
        public string ObjectiveErrorInfo { get; set; }
        /// <summary> 客观题得分 </summary>
        public decimal? ObjectiveScore { get; set; }

        /// <summary> 是否是单面 </summary>
        public bool IsSingle { get; set; }

        /// <summary> 试卷总页数 </summary>
        public int PageCount { get; set; }

        [JsonIgnore]
        public string Markings { get; set; }

        [JsonIgnore]
        public string Marks { get; set; }

        /// <summary> 阅卷标记 </summary>
        public List<SymbolInfo> SymbolInfos { get; set; }

        /// <summary> 注释标记 </summary>
        public List<CommentInfo> CommentInfos { get; set; }

        public MPrintDetail()
        {
            SymbolInfos = new List<SymbolInfo>();
            CommentInfos = new List<CommentInfo>();
        }
    }
}
