using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Message
{
    public class CommentResultDto : DDto
    {
        /// <summary> 总评论数 </summary>
        public int Count { get; set; }

        /// <summary> 一级评论数，用于分页 </summary>
        public int CommentCount { get; set; }

        /// <summary> 用户列表 </summary>
        public IDictionary<long, DUserDto> Users { get; set; }

        /// <summary> 评论列表 </summary>
        public IEnumerable<CommentDto> Comments { get; set; }
    }
}
