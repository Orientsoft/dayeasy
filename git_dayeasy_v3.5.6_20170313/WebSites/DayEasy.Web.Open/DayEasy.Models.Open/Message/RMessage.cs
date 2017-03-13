
namespace DayEasy.Models.Open.Message
{
    /// <summary> 消息业务实体 </summary>
    public class RMessage : DDto
    {
        /// <summary> 发送人Id </summary>
        public long SenderId { get; set; }

        /// <summary> 发送人 </summary>
        public string Sender { get; set; }

        /// <summary> 科目ID（针对学生端的改错和全班通知） </summary>
        public int SubjectId { get; set; }

        /// <summary> 科目（针对学生端的改错和全班通知） </summary>
        public string SubjectName { get; set; }

        /// <summary> 消息类型：见RMessageType </summary>
        public byte Type { get; set; }

        /// <summary> 发送时间 </summary>
        public long SendTime { get; set; }

        /// <summary> 消息内容 </summary>
        public string Content { get; set; }
    }
}
