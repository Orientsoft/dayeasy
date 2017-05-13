using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Group
{
    public class JGroupInfoDto : DDto
    {
        /// <summary> 班级圈ID </summary>
        public string Id { get; set; }

        /// <summary> 班级圈名称 </summary>
        public string Name { get; set; }

        /// <summary> 班级圈学生列表 </summary>
        public List<DUserDto> Students { get; set; }
    }
}
