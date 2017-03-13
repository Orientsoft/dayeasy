using System;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 试卷类消息详情 </summary>
    public class PaperDynamicMessageDto : DDynamicMessageDto
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public DateTime? ExpireTime { get; set; }
    }
}
