using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class KnowledgeSearchDto : DDto
    {
        public string Keyword { get; set; }
        public int SubjectId { get; set; }
        public int Stage { get; set; }
        public int ParentId { get; set; }

        public KnowledgeSearchDto()
        {
            SubjectId = -1;
            Stage = -1;
        }
    }
}
