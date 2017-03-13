using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class QuestionTypeDto : DDto
    {
        public int Id { get; set; }

        public string TypeName { get; set; }
        public int MultiAnswer { get; set; }
        public int[] QStyle { get; set; }
        public byte Status { get; set; }
    }
}
