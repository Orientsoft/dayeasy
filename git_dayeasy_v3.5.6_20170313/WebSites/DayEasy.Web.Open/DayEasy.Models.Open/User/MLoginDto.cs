
namespace DayEasy.Models.Open.User
{
    public class MLoginDto : DDto
    {
        public string Token { get; set; }
        public MUserDto User { get; set; }
    }
}
