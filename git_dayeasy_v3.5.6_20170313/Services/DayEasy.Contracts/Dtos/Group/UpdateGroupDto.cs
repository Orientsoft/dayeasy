
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary> 更新圈子DTO </summary>
    public class UpdateGroupDto : DDto
    {
        public string Id { get; set; }
        /// <summary> 圈子名字 </summary>
        public string Name { get; set; }
        /// <summary> 圈子logo </summary>
        public string Avatar { get; set; }
        /// <summary> 圈子Banner图 </summary>
        public string Banner { get; set; }
        /// <summary> 圈主 </summary>
        public long Owner { get; set; }
        /// <summary> 圈子简介 </summary>
        public string Summary { get; set; }
        /// <summary> 圈子公告 </summary>
        public string Notice { get; set; }
        /// <summary> 发帖权限 </summary>
        public int PostAuth { get; set; }

        /// <summary> 圈子标签 </summary>
        public string Tags { get; set; }

        public UpdateGroupDto()
        {
            PostAuth = -1;
        }
    }
}
