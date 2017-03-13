using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
   public class AddQuestionDto:DDto
   {
       public int QType { get; set; }
       public string Kps { get; set; }
       public string QContent { get; set; }
       public int OptionNum { get; set; }
       public int SmallQuNum { get; set; }
       public long UserId { get; set; }
       public string RealName { get; set; }
       public byte Stage { get; set; }
       public int SubjectId { get; set; }
    }
}
