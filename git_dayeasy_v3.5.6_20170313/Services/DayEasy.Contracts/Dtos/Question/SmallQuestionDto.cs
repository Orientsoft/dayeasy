using System.Collections.Generic;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Extend;
using Newtonsoft.Json;

namespace DayEasy.Contracts.Dtos.Question
{
    /// <summary> 小问传输对象 </summary>
    [AutoMap(typeof(TQ_SmallQuestion))]
    public class SmallQuestionDto : DDto
    {
        public string Id { get; set; }

        [MapFrom("SmallQContent")]
        public string Body { get; set; }

        public bool IsObjective { get; set; }

        public int Sort { get; set; }

        [JsonIgnore]
        public string SmallQImages { get; set; }

        public string[] Images
        {
            get
            {
                return string.IsNullOrWhiteSpace(SmallQImages)
                ? new string[] { }
                : SmallQImages.JsonToObject<string[]>();
            }
            set
            {
                SmallQImages = (value == null ? null : value.ToJson());
            }
        }

        public byte OptionStyle { get; set; }

        public List<AnswerDto> Answers { get; set; }

        public SmallQuestionDto()
        {
            Answers = new List<AnswerDto>();
        }
    }
}
