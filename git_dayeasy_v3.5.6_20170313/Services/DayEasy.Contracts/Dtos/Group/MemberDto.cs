using System;
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;

namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary> 圈成员 </summary>
    public class MemberDto : UserDto
    {
        /// <summary> 群名片 </summary>
        public string BusinessCard { get; set; }

        /// <summary> 入圈时间 </summary>
        public DateTime AddedTime { get; set; }
        /// <summary> 最近活跃时间 </summary>
        public DateTime LastActive { get; set; }
        /// <summary> 发帖数 </summary>
        public int TopicCount { get; set; }

        /// <summary> 已关联的父母 </summary>
        public IEnumerable<DUserDto> Parents { get; set; }
    }
}
