using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary>
    /// 检查成员名称
    /// </summary>
   public class CheckMemberDto:MemberDto
    {
        /// <summary>
        /// 检查类型
        /// </summary>
        public int State { get; set; }
    }
}
