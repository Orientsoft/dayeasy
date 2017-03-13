using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Dtos.Group
{
    [AutoMap(typeof(TG_Group))]
    public class GroupDto : DDto
    {
        public string Id { get; set; }

        [MapFrom("GroupName")]
        public string Name { get; set; }

        [MapFrom("GroupCode")]
        public string Code { get; set; }

        [MapFrom("GroupAvatar")]
        public string Logo { get; set; }

        [MapFrom("GroupType")]
        public byte Type { get; set; }

        [MapFrom("MemberCount")]
        public int Count { get; set; }

        public int TeacherCount { get; set; }

        [MapFrom("UnCheckedCount")]
        public int PendingCount { get; set; }

        public int Capacity { get; set; }
        public string GroupSummary { get; set; }

        public long ManagerId { get; set; }
        public int Status { get; set; }

        [MapFrom("AddedAt")]
        public DateTime CreationTime { get; set; }

        public string AgencyId { get; set; }

        public string AgencyName { get; set; }

        public string Owner { get; set; }

        /// <summary> 消息数 </summary>
        public int MessageCount { get; set; }

        /// <summary> 认证等级 </summary>
        public byte? CertificationLevel { get; set; }
        /// <summary> 圈子banner图 </summary>
        public string GroupBanner { get; set; }
    }
}
