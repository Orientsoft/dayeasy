using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    [AutoMap(typeof(TU_User))]
    public class DUserDto : DDto
    {
        public long Id { get; set; }

        [MapFrom("TrueName")]
        public string Name { get; set; }

        [MapFrom("HeadPhoto")]
        public string Avatar { get; set; }

        [MapFrom("NickName")]
        public string Nick { get; set; }

    }
}
