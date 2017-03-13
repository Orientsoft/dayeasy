using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 获取消息dto </summary>
    public class DynamicSearchDto : DPage
    {
        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 家长Id </summary>
        public long ParentId { get; set; }

        /// <summary> 用户角色 </summary>
        public UserRole Role { get; set; }

        /// <summary> 消息类型 </summary>
        public int Type { get; set; }

        /// <summary> 圈子ID </summary>
        public string GroupId { get; set; }

        public DynamicSearchDto()
        {
            Type = -1;
        }
    }
}
