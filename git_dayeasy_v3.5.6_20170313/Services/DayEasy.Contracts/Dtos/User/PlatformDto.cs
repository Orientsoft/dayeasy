using System;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    [Serializable]
    [AutoMap(typeof(TU_ThirdPlatform))]
    public class PlatformDto : DDto
    {
        public string PlatformId { get; set; }
        public string AccessToken { get; set; }

        public int PlatformType { get; set; }

        public string Nick { get; set; }
        public string Profile { get; set; }
        public long UserId { get; set; }
    }
}
