using System.Collections.Generic;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Dtos.Question;

namespace DayEasy.Paper.Services.Model
{
    /// <summary>
    /// 筛选条件
    /// </summary>
    [AutoMapFrom(typeof(SearchQuestionDto))]
    public class QuestionQuery
    {
        /// <summary> 出题人 </summary>
        [MapFrom("UserId")]
        public long AddedBy { get; set; }

        /// <summary> 关键字 </summary>
        public string Keyword { get; set; }

        public IEnumerable<string> Points { get; set; }
        public IEnumerable<string> Tags { get; set; }

        /// <summary> 学段列表 </summary>
        public IEnumerable<int> Stages { get; set; }

        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }

        /// <summary> 题型ID </summary>
        public int QuestionType { get; set; }

        /// <summary> 分享范围 </summary>
        public int ShareRange { get; set; }

        /// <summary> 难度系数 </summary>
        public double[] Difficulties { get; set; }

        /// <summary> 排序类型 </summary>
        public QuestionOrderType Order { get; set; }

        /// <summary> 页码 </summary>
        public int Page { get; set; }

        /// <summary> 获取数量 </summary>
        public int Size { get; set; }

        /// <summary> 是否高亮 </summary>
        public bool IsHighLight { get; set; }

        /// <summary> 排除的问题ID </summary>
        public IEnumerable<string> NotInIds { get; set; }
        /// <summary> tag 包含xxx字段 优先排序 </summary>
        public string TagSortFirstStr { get; set; }

        /// <summary> 加载出题人 </summary>
        public bool LoadCreator { get; set; }
    }
}
