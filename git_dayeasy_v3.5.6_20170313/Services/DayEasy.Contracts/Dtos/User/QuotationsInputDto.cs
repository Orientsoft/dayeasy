using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 语录 </summary>
    public class QuotationsInputDto : DDto
    {
        /// <summary> 用户ID </summary>
        public long UserId { get; set; }
        /// <summary> 创建人ID </summary>
        public long CreatorId { get; set; }
        /// <summary> 语录内容 </summary>
        public string Content { get; set; }
    }
}
