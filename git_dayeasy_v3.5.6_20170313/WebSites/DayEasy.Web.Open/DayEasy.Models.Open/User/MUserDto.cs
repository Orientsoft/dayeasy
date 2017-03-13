
namespace DayEasy.Models.Open.User
{
    public class MUserDto : MUserBaseDto
    {
        public string Nick { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public string Mobile { get; set; }
        public int Role { get; set; }
        public string StudentNum { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public byte ValidationType { get; set; }
    }
}
