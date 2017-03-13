
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 发送系统消息Dto </summary>
    public class SystemMessageSendDto : DDto
    {
        public long SenderId { get; set; }
        public List<long> Receivers { get; set; }
        public string Title { get; set; }
        /// <summary> 消息内容(格式：key,链接;value,文字内容) </summary>
        public DKeyValue Content { get; set; }
        public MessageType MessageType { get; set; }

        public SystemMessageSendDto()
        {
            Receivers = new List<long>();
            MessageType = MessageType.System;
        }

        public SystemMessageSendDto(long receiverId)
            : this()
        {
            Receivers.Add(receiverId);
        }
    }
}
