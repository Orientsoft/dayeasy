
using DayEasy.Contracts.Enum;
using System;

namespace DayEasy.Contracts.Dtos.User
{
    [Serializable]
    public class RegistUserDto : DUserDto
    {
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
        public int Subject { get; set; }
        public UserRole Role { get; set; }
        public ValidationType ValidationType { get; set; }
    }
}
