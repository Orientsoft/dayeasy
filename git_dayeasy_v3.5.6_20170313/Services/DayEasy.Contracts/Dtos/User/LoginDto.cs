using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    public class LoginDto : DDto
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public string Vcode { get; set; }
        public bool IsEncrypt { get; set; }

        public Comefrom Comefrom { get; set; }
        public string Partner { get; set; }

        public LoginDto()
        {
            IsEncrypt = true;
        }
    }
}
