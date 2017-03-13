using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class AgencyEditDto : DDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public int Sort { get; set; }
        public string Summary { get; set; }
    }
}
