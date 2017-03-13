using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary>
    /// 批量生产圈子返回信息
    /// </summary>
   public class OutGroupMessage
    {
        public OutGroupMessage()
        {
            GroupsSuccess = new List<string>();
        }
        /// <summary>
        /// 添加成功的圈子数量
        /// </summary>
        public int GroupCount { get; set;}
        /// <summary>
        /// 添加成功的班级圈数量
        /// </summary>
        public int ClassCount { get; set; }
        /// <summary>
        /// 添加成功的同事圈数量
        /// </summary>
        public int ColleagueCount { get; set; }
        /// <summary>
        /// 添加成功的
        /// </summary>
        public List<string> GroupsSuccess { get; set; }

    }
}
