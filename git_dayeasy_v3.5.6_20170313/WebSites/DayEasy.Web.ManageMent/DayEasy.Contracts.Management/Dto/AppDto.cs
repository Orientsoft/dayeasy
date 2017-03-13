using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    [AutoMap(typeof(TS_Application))]
    public class AppDto : DDto
    {
        public int Id { get; set; }
        [MapFrom("AppName")]
        public string Name { get; set; }

        [MapFrom("AppURL")]
        public string Link { get; set; }

        [MapFrom("AppIcon")]
        public string Icon { get; set; }

        [MapFrom("AppRemark")]
        public string Remark { get; set; }

        [MapFrom("AppType")]
        public int Type { get; set; }

        [MapFrom("AppRoles")]
        public long Role { get; set; }

        public byte Status { get; set; }
        public int Sort { get; set; }
        public bool IsSLD { get; set; }
    }
}
