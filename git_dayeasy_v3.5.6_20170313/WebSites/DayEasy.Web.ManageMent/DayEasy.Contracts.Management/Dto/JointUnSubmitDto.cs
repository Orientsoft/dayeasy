using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    /// <summary>
    /// 协同考试 - 未交卷的学生名单
    /// </summary>
    public class JointUnSubmitDto : DDto
    {
        public string PaperId { get; set; }
        public string PaperTitle { get; set; }
        public bool IsAb { get; set; }
        public List<JointUnsGroup> UnSubmits { get; set; }
    }

    public class JointUnsGroup : DDto
    {
        public string GroupId { get; set; }

        public string GroupName { get; set; }

        public List<NameDto<long, string>> UnsA { get; set; }
        public List<NameDto<long, string>> UnsB { get; set; }
    }
}
