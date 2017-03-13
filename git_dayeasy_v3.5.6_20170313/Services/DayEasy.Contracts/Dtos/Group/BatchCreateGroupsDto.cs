using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary>
    /// 批量创建圈子
    /// </summary>
 public   class BatchCreateGroupsDto
    {
        public BatchCreateGroupsDto()
        {
            ClassGroups = new List<ClassGroupDto>();
            ColleagueGroups = new List<ColleagueGroupDto>();
        }
        public List<ClassGroupDto> ClassGroups { get; set; }
        public List<ColleagueGroupDto> ColleagueGroups { get; set; }
    }
}
