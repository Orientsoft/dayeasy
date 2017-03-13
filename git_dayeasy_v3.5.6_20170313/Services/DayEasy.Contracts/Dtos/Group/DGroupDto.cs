using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Group
{
    [AutoMap(typeof(TG_Group))]
    public class DGroupDto : DDto
    {
        public string Id { get; set; }

        [MapFrom("GroupName")]
        public string Name { get; set; }

        [MapFrom("GroupCode")]
        public string Code { get; set; }

        [MapFrom("GroupAvatar")]
        public string Logo { get; set; }

        [MapFrom("GroupType")]
        public byte Type { get; set; }
    }
}
