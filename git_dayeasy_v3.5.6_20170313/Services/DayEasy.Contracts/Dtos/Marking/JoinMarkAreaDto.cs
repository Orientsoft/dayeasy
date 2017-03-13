using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary>
    /// 标记协同阅卷数据
    /// </summary>
    public class JoinMarkAreaDto : DDto
    {
        public string JoinBatch { get; set; }
        public string PaperId { get; set; }
        public string PaperName { get; set; }
        public string GroupId { get; set; }
        public List<AreaDataDto> AreaData { get; set; }
    }

    public class AreaDataDto : DDto
    {
        public int Sort { get; set; }
        public string QId { get; set; }
        public bool IsTianKong { get; set; }
    }
}
