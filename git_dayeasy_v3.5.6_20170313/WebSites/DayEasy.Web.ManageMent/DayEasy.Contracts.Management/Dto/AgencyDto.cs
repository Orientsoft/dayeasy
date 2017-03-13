using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class AgencyDto : DDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int StageCode { get; set; }
        public string Stage { get; set; }
        public string Area { get; set; }
    }
}
