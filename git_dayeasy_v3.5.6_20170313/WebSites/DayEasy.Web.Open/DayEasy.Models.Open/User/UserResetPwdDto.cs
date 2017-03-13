
namespace DayEasy.Models.Open.User
{
    public class UserResetPwdDto : DDto
    {
        public string Mobile { get; set; }
        public string Vcode { get; set; }
        public string Password { get; set; }
        public bool IsEncrypt { get; set; }

        public UserResetPwdDto()
        {
            IsEncrypt = true;
        }
    }
}
