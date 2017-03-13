using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class AgencyInputDto : DDto
    {
        public string Name { get; set; }
        public byte Type { get; set; }
        public int Code { get; set; }
        public int[] Stages { get; set; }
    }
}
