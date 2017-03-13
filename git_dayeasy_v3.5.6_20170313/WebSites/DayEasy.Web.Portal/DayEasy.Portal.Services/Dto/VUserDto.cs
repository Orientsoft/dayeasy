using DayEasy.Contracts.Dtos.User;

namespace DayEasy.Portal.Services.Dto
{
    public class VUserDto : DUserDto
    {
        public string Code { get; set; }
        public int Role { get; set; }
        public int SubjectId { get; set; }
        public string Subject { get; set; }
        public byte Level { get; set; }
    }
}
