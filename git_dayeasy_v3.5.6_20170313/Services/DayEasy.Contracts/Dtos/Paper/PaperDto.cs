
using System;
using System.Collections.Generic;
using AutoMapper;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary> 试卷传输对象 </summary>
    [AutoMap(typeof(TP_Paper))]
    public class PaperDto : DDto
    {
        public string Id { get; set; }
        /// <summary> 试卷编号 </summary>
        public string PaperNo { get; set; }
        /// <summary> 出卷时间 </summary>
        public DateTime AddedAt { get; set; }

        /// <summary> 分享范围 </summary>
        [MapFrom("ShareRange")]
        public byte Share { get; set; }
        /// <summary> 出卷人 </summary>
        public string UserName { get; set; }
        /// <summary> 试卷标题 </summary>
        public string PaperTitle { get; set; }
        /// <summary> 试卷类型 </summary>
        public byte PaperType { get; set; }
        /// <summary> 学段 </summary>
        public byte Stage { get; set; }
        /// <summary> 年级 </summary>
        public byte Grade { get; set; }
        /// <summary> 年级 </summary>
        public string GradeName { get; set; }
        /// <summary> 状态 </summary>
        public byte Status { get; set; }
        /// <summary> 出卷人ID </summary>
        public long AddedBy { get; set; }
        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }
        /// <summary> 标签列表 </summary>
        public List<string> Tags { get; set; }

        /// <summary> 分数 </summary>
        [IgnoreMap]
        public PaperScoresDto PaperScores { get; set; }

        /// <summary> 知识点列表 </summary>
        public Dictionary<string, string> Kps { get; set; }

        /// <summary> 是否AB卷 </summary>
        public bool IsAb
        {
            get { return PaperType == (byte)Enum.PaperType.AB; }
        }
    }
}
