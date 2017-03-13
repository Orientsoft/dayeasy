using DayEasy.Contracts.Dtos.Group;
using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;
using DayEasy.Contracts.Dtos;

namespace DayEasy.Contracts.Management.Dto
{
    public class IndexAdvertDto : DDto
    {
        public List<AdvertDto> Carousels { get; set; }

        public List<AdvertDto> Fixeds { get; set; }

        public List<SectionAdvertDto> Sections { get; set; }
    }

    public class SectionAdvertDto : DDto
    {
        public List<AdvertDto> Sources { get; set; }

        public List<TabAdvertDto> Tabs { get; set; }
    }

    public class TabAdvertDto : DDto
    {
        public List<AdvertDto> Sources { get; set; }

        public List<AdvertDto> Adverts { get; set; }

        public List<GroupDto> Groups { get; set; }

        public List<string> GroupCodes { get; set; }
    }
}
