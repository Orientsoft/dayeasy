using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Extend;
using Newtonsoft.Json;

namespace DayEasy.Contracts.Dtos.Question
{
    /// <summary> 答案传输对象 </summary>
    [AutoMap(typeof(TQ_Answer))]
    public class AnswerDto : DDto
    {
        public string Id { get; set; }

        [MapFrom("QContent")]
        public string Body { get; set; }

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
            set { QImages = (value == null ? null : value.ToJson()); }
        }

        public int Sort { get; set; }

        public string Tag
        {
            get
            {
                if (Sort < 0 || Sort > 25) return string.Empty;
                return Consts.OptionWords[Sort];
            }
        }

        public bool IsCorrect { get; set; }
    }
}
