using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    /// <summary> 用户应用实体 </summary>
    public class UserAppDto : DDto
    {
        public long UserId { get; set; }
        public string Account { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string UserCode { get; set; }
        public int Role { get; set; }
        public string AgencyId { get; set; }
        public string AgencyName { get; set; }
        public byte Stage { get; set; }
        public string Area { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
