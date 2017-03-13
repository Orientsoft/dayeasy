using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 第三方登录结果DTO </summary>
    public class PlatformLoginResultDto : DDto
    {
        public string Token { get; set; }
        public string PlatId { get; set; }
    }
}
