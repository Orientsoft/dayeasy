using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 印象贴纸 </summary>
    public class ImpressionInputDto : DDto
    {
        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 创建人ID </summary>
        public long CreatorId { get; set; }

        /// <summary> 贴纸内容 </summary>
        public List<string> Content { get; set; }
    }
}
