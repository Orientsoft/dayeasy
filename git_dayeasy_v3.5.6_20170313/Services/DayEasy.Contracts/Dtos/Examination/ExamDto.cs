using AutoMapper;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Contracts.Dtos.Examination
{
    public class DExamDto : DDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AgencyId { get; set; }
        public string AgencyName { get; set; }
    }

    /// <summary> 大型考试实体 </summary>
    [AutoMap(typeof(TE_Examination))]
    public class ExamDto : DDto
    {
        public string Id { get; set; }

        /// <summary> 标题 </summary>
        public string Name { get; set; }

        /// <summary> 机构ID </summary>
        public string AgencyId { get; set; }

        /// <summary> 机构名称 </summary>
        public string AgencyName { get; set; }

        /// <summary> 学段 </summary>
        public byte Stage { get; set; }

        /// <summary> 涉及科目 </summary>
        public string Subjects { get; set; }

        /// <summary> 大型考试类型 </summary>
        [MapFrom("ExamType")]
        public byte Type { get; set; }

        /// <summary> 包含的系统批次号 </summary>
        [JsonIgnore]
        public string JointBatches { get; set; }

        /// <summary> 批次号列表 </summary>
        [IgnoreMap]
        public string[] JointList
        {
            get { return JsonHelper.JsonList<string>(JointBatches).ToArray(); }
        }

        /// <summary> 状态 </summary>
        public byte Status { get; set; }

        /// <summary> 考试时间 </summary>
        public DateTime ExamTime { get; set; }

        /// <summary> 创建人ID </summary>
        public long CreatorId { get; set; }

        /// <summary> 创建时间 </summary>
        public DateTime? CreationTime { get; set; }

        /// <summary> 发布人ID </summary>
        public long PublisherId { get; set; }

        /// <summary> 发布时间 </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary> 学生人数 </summary>
        public int? TotalCount { get; set; }

        /// <summary> 年级平均分 </summary>
        public decimal? AverageScore { get; set; }
        /// <summary> 班级列表 </summary>
        public List<string> ClassList { get; set; }
        /// <summary>
        /// 是否有关联联考
        /// </summary>
        [IgnoreMap]
        public bool IsUnion { get { return !string.IsNullOrWhiteSpace(UnionBatch); } }
        /// <summary> 联考批次 </summary>
        public string UnionBatch { get; set; }
    }
}
