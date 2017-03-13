using System.Collections.Generic;
using DayEasy.Contracts.Dtos;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class HomePageDataDto : DDto
    {
        public List<AdvertDto> Coverages { get; set; }
        public List<AdvertDto> Resources { get; set; }
        public List<AdvertDto> ExamplesLeft { get; set; }
        public List<AdvertDto> ExamplesRight { get; set; }

        public HomePageDataDto()
        {
            Coverages = new List<AdvertDto>();
            Resources = new List<AdvertDto>();
            ExamplesLeft = new List<AdvertDto>();
            ExamplesRight = new List<AdvertDto>();
        }
    }
}
