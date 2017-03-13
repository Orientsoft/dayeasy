using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class UpdateGroupInputDto : DDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AgencyId { get; set; }
        public int GradeYear { get; set; }
        public string Summary { get; set; }
    }
}
