using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Portal.Services.Dto
{
    public class QuotationResultDto : DDto
    {
        public int Count { get; set; }
        public Dictionary<long, VUserDto> Users { get; set; }
        public List<QuotationsDto> Quotations { get; set; }

        public QuotationResultDto()
        {
            Users = new Dictionary<long, VUserDto>();
            Quotations = new List<QuotationsDto>();
        }
    }
}
