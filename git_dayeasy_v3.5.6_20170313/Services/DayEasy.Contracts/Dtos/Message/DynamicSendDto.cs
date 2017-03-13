using System.Collections.Generic;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 发送消息dto </summary>
    public class DynamicSendDto : DDto
    {
        public long UserId { get; set; }

        public GroupDynamicType DynamicType { get; set; }
        public byte? ContentType { get; set; }
        public string ContentId { get; set; }

        public string GroupId { get; set; }

        public UserRole ReceivRole { get; set; }
        public IEnumerable<long> RecieveIds { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
    }
}
