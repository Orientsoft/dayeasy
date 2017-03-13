using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 贴纸印象 </summary>
    public class ImpressionDto : DDto
    {
        /// <summary> 贴纸ID </summary>
        public string Id { get; set; }

        /// <summary> 创建者ID </summary>
        public long CreatorId { get; set; }

        /// <summary> 贴纸内容 </summary>
        public string Content { get; set; }

        /// <summary> 支持者 </summary>
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
