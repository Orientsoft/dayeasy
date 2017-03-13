using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 协同异常 </summary>
    public class JExceptionDto : DDto
    {
        /// <summary> 协同批次 </summary>
        public string JointBatch { get; set; }
        /// <summary> 图片ID </summary>
        public string PictureId { get; set; }
        /// <summary> 教师ID </summary>
        public long TeacherId { get; set; }
        /// <summary> 异常类型 </summary>
        public byte Type { get; set; }
        /// <summary> 异常消息 </summary>
        public string Message { get; set; }
        /// <summary> 问题IDs </summary>
        public List<string> QuestionIds { get; set; }
    }
}
