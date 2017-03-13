using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 语录 </summary>
    public class QuotationsDto : DDto
    {
        /// <summary> 语录ID </summary>
        public string Id { get; set; }

        /// <summary> 语录内容 </summary>
        public string Content { get; set; }

        /// <summary> 目标用户 </summary>
        public long UserId { get; set; }

        /// <summary> 得一号 </summary>
        public string UserCode { get; set; }

        /// <summary> 创建者 </summary>
        public string UserName { get; set; }
        /// <summary> 创建者 </summary>
        public long CreatorId { get; set; }

        /// <summary> 创建时间 </summary>
        public DateTime CreationTime { get; set; }

        /// <summary> 支持者列表 </summary>
        public List<long> SupportList { get; set; }

        /// <summary> 支持者数 </summary>
        public int SupportCount
        {
            get { return SupportList == null ? 0 : SupportList.Count; }
        }
        /// <summary> 是否已支持 </summary>
        public bool Supported { get; set; }

        /// <summary> 创建人/贴纸对象 </summary>
        public bool IsOwner { get; set; }
    }
}
