
namespace DayEasy.Models.Open.User
{
    public class UserChangePwdDto
    {
        public string OldPwd { get; set; }
        public string Password { get; set; }
        public bool IsEncrypt { get; set; }

        public UserChangePwdDto()
        {
            IsEncrypt = true;
        }
    }
}
