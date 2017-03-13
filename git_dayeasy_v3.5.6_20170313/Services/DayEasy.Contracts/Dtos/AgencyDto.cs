using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    [AutoMapFrom(typeof(TS_Agency))]
    public class AgencyDto : DDto
    {
        public string Id { get; set; }

        [MapFrom("AgencyName")]
        public string Name { get; set; }
        public byte Stage { get; set; }
        public string Initials { get; set; }
    }

    [AutoMapFrom(typeof(TS_Agency))]
    public class AgencySearchDto : DDto
    {
        public string Id { get; set; }
        [MapFrom("AgencyName")]
        public string Name { get; set; }
        [MapFrom("AgencyLogo")]
        public string Logo { get; set; }
        public byte Stage { get; set; }
    }

    [AutoMapFrom(typeof(TS_Agency))]
    public class AgencyItemDto : AgencySearchDto
    {
        public string Banner { get; set; }
        public string Summary { get; set; }
        public int VisitCount { get; set; }
        public int TargetCount { get; set; }
        [MapFrom("CertificationLevel")]
        public byte Level { get; set; }

        public int UserCount { get; set; }
    }
}
