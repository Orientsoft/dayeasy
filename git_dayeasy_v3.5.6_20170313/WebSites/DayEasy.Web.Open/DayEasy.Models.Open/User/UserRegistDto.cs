
namespace DayEasy.Models.Open.User
{
    public class UserRegistDto : DDto
    {
        public string Mobile { get; set; }
        public string Password { get; set; }
        public string Vcode { get; set; }
        public int Role { get; set; }
        public int SubjectId { get; set; }
        public bool IsEncrypt { get; set; }

        public UserRegistDto()
        {
            IsEncrypt = true;
        }
    }
}
