
using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using Newtonsoft.Json;

namespace DayEasy.Contracts.Dtos.Question
{
    /// <summary> 问题传输对象 </summary>
    [AutoMap(typeof(TQ_Question))]
    public class QuestionDto : DDto
    {
        public string Id { get; set; }

        [MapFrom("QContent")]
        public string Body { get; set; }

        [MapFrom("QType")]
        public int Type { get; set; }

        public int SubjectId { get; set; }

        public byte Stage { get; set; }

        public byte? OptionStyle { get; set; }

        [MapFrom("HasSmallQuestion")]
        public bool HasSmall { get; set; }

        public bool IsUsed { get; set; }

        [JsonIgnore]
        public string KnowledgeIDs { get; set; }

        public List<NameDto> Knowledges { get; set; }

        [JsonIgnore]
        public string TagIDs { get; set; }

        public string[] Tags
        {
            get
            {
                return string.IsNullOrWhiteSpace(TagIDs)
                    ? new string[] { }
                    : TagIDs.JsonToObject<string[]>();
            }
            set { TagIDs = (value == null ? null : JsonHelper.ToJson(value, NamingType.CamelCase)); }
        }

        [JsonIgnore]
        public string QImages { get; set; }

        public string[] Images
        {
            get
            {
                return string.IsNullOrWhiteSpace(QImages)
                  ? new string[] { }
                  : QImages.JsonToObject<string[]>();
            }
            set { QImages = (value == null ? null : JsonHelper.ToJson(value, NamingType.CamelCase)); }
        }

        [MapFrom("DifficultyStar")]
        public decimal Difficulty { get; set; }

        [MapFrom("AddedAt")]
        [JsonIgnore]
        public DateTime CreateTime { get; set; }

        public string Time
        {
            get { return CreateTime.ShowTime(); }
        }

        public bool ShowOption
        {
            get
            {
                if (HasSmall)
                {
                    if (Details == null || !Details.Any())
                        return false;
                    return
                        Details.Any(
                            d =>
                                d.Answers != null &&
                                d.Answers.Any(
                                    a => !string.IsNullOrWhiteSpace(a.Body) || (a.Images != null && a.Images.Any())));
                }
                return Answers != null &&
                       Answers.Any(a => !string.IsNullOrWhiteSpace(a.Body) || (a.Images != null && a.Images.Any()));
            }
        }

        public bool IsObjective { get; set; }

        [MapFrom("AddedBy")]
        public long UserId { get; set; }

        public string UserName { get; set; }

        [MapFrom("ShareRange")]
        public byte Range { get; set; }

        /// <summary> 使用次数 </summary>
        [MapFrom("UsedCount")]
        public int UseCount { get; set; }

        /// <summary> 答题次数 </summary>
        public long AnswerCount { get; set; }

        /// <summary> 错误次数 </summary>
        public long ErrorCount { get; set; }

        public List<SmallQuestionDto> Details { get; set; }

        public List<AnswerDto> Answers { get; set; }

        public AnalysisDto Analysis { get; set; }

        public QuestionDto()
        {
            Details = new List<SmallQuestionDto>();
            Answers = new List<AnswerDto>();
        }
    }
}
