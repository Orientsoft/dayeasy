using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 协同图片信息 </summary>
    public class JPictureDto : DDto
    {
        /// <summary> 图片ID </summary>
        public string Id { get; set; }
        /// <summary> 图片地址 </summary>
        public string Picture { get; set; }
        /// <summary> 标记列表 </summary>
        public List<MkSymbol> Marks { get; set; }
        /// <summary> 阅卷详情 </summary>
        public Dictionary<string, JSubmitDetailDto> Details { get; set; }
        /// <summary> 已阅数 </summary>
        public int Marked { get; set; }
        /// <summary> 未阅数 </summary>
        public int Left { get; set; }
        /// <summary> 异常状态：0,未报异常;1,未解决;2,已解决 </summary>
        public byte Status { get; set; }

        /// <summary> 构造函数 </summary>
        public JPictureDto()
        {
            Marks = new List<MkSymbol>();
            Details = new Dictionary<string, JSubmitDetailDto>();
        }
    }

    /// <summary> 批阅组 </summary>
    public class JGroupStepDto : DDto
    {
        public string[] Ids { get; set; }
        public int Step { get; set; }
    }
}
