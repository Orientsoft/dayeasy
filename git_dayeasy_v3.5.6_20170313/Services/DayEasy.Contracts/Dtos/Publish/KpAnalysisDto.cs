using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Publish
{
    public class KpAnalysisDto : DDto
    {
        /// <summary>
        /// 知识点名称
        /// </summary>
        public string KpName { get; set; }
        /// <summary>
        /// 知识点编码
        /// </summary>
        public string KpCode { get; set; }
        /// <summary>
        /// 知识点错误的数量
        /// </summary>
        public int ErrorCount { get; set; }
    }

}
