using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Extend;
using Newtonsoft.Json;

namespace DayEasy.Contracts.Dtos.Question
{
    /// <summary> 解析传输对象 </summary>
    [AutoMap(typeof(TQ_Analysis))]
    public class AnalysisDto : DDto
    {
        public string Id { get; set; }

        [MapFrom("AnalysisContent")]
        public string Body { get; set; }

        [JsonIgnore]
        public string AnalysisImage { get; set; }

        public string[] Images
        {
            get
            {
                return string.IsNullOrWhiteSpace(AnalysisImage)
                  ? new string[] { }
                  : AnalysisImage.JsonToObject<string[]>();
            }
            set { AnalysisImage = (value == null ? null : value.ToJson()); }
        }
    }
}
