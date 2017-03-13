using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using Newtonsoft.Json;

namespace DayEasy.Contracts.Dtos
{
    /// <summary> 应用传输对象 </summary>
    [AutoMapFrom(typeof(TS_Application))]
    public class ApplicationDto : DDto
    {
        public int Id { get; set; }
        [MapFrom("AppName")]
        public string Text { get; set; }

        [MapFrom("AppURL")]
        public string Url { get; set; }

        [MapFrom("AppIcon")]
        public string Icon { get; set; }

        /// <summary> 是否二级域名 </summary>
        [JsonIgnore]
        public bool IsSLD { get; set; }
    }
}
