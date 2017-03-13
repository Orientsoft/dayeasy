using System;
using DayEasy.Contracts.Dtos.User;

namespace DayEasy.Contracts.Dtos.Group
{
    public class PendingUserDto : UserDto
    {
        public string RecordId { get; set; }
        public DateTime CreationTime { get; set; }
        public string Message { get; set; }
    }
}
