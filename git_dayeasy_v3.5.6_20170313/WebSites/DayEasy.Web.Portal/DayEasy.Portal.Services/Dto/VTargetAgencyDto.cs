using DayEasy.Core.Domain.Entities;

namespace DayEasy.Portal.Services.Dto
{
    public class VTargetAgencyDto : DDto
    {
        public string Id { get; set; }
        public string Logo { get; set; }
        public byte Stage { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
