using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    public class ExamUpdateDto : DDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
