using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 协同阅卷提交数据 </summary>
    public class JSubmitDto : DDto
    {
        /// <summary> 教师ID </summary>
        public long TeacherId { get; set; }

        /// <summary> 图片标记更新 </summary>
        public List<JSubmitPictureDto> Pictures { get; set; }

        /// <summary> 阅卷详情更新 </summary>
        public List<JSubmitDetailDto> Details { get; set; }

        /// <summary> 构造函数 </summary>
        public JSubmitDto()
        {
            Pictures = new List<JSubmitPictureDto>();
            Details = new List<JSubmitDetailDto>();
        }
    }

    /// <summary> 图片信息 </summary>
    public class JSubmitPictureDto : DDto
    {
        /// <summary> 图片ID </summary>
        public string Id { get; set; }

        /// <summary> 标记列表 </summary>
        public List<MkSymbol> Marks { get; set; }
    }

    /// <summary> 阅卷信息 </summary>
    public class JSubmitDetailDto : DDto
    {
        /// <summary> DetailId </summary>
        public string Id { get; set; }

        /// <summary> 分数 </summary>
        public decimal Score { get; set; }
    }
}
