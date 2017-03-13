using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class SubjectDto : DDto
    {
        public int Id { get; set; }
        public string SubName { get; set; }
        public int[] QType { get; set; }
        public bool LoadFormula { get; set; }
        public byte Status { get;set; }
    }
}
