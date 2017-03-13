using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.LearningMemo
{
    /// <summary> 学习笺分组 </summary>
    public class MemoGroupDto : DDto
    {
        /// <summary> 组名 </summary>
        public string GroupName { get; set; }

        /// <summary> 学生组头像 </summary>
        public string Profile { get; set; }

        /// <summary> 推送学习笺数量 </summary>
        public int MemoCount { get; set; }

        /// <summary> 学生人数 </summary>
        public int StudentCount { get; set; }

        /// <summary> 分组状态 </summary>
        public byte Status { get; set; }

        /// <summary> 学生ID列表 </summary>
        public List<long> StudentIds { get; set; }

        /// <summary> 未读评论数 </summary>
        public int UnViewCount { get; set; }

        /// <summary> 最后发布时间 </summary>
        public long LastRecommendTime { get; set; }
    }
}
