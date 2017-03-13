using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class KnowledgeDto : DDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int Stage { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public int ParentId { get; set; }
    }
}
