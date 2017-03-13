using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 圈子动态消息 </summary>
    public class DynamicMessageResultDto
    {
        public int UserRole { get; set; }
        public int TotalCount { get; set; }
        public IDictionary<long, DUserDto> Users { get; set; }
        public List<DDynamicMessageDto> NewsDetails { get; set; }

        public DynamicMessageResultDto()
        {
            Users = new Dictionary<long, DUserDto>();
            NewsDetails = new List<DDynamicMessageDto>();
        }
    }
}
