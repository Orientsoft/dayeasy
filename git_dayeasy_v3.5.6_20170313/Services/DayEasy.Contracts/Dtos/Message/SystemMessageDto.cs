using System;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Message
{
    [AutoMapFrom(typeof (TS_Message))]
    public class SystemMessageDto : DDto
    {
        public string Id { get; set; }
        public byte MessageType { get; set; }
        public string MessageTitle { get; set; }
        public string MessageContent { get; set; }
        public string Link { get; set; }
        public DateTime CreateOn { get; set; }

        [MapFrom("CreatorId")]
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
    }
}
