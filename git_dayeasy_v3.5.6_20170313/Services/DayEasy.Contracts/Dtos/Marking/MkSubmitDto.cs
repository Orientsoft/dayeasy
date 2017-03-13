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
    /// 阅卷提交
    /// </summary>
    public class MkSubmitDto : DDto
    {
        /// <summary>
        /// 当前登录用户ID
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 阅卷图片ID
        /// </summary>
        public string PictureId { get; set; }
        /// <summary>
        /// 操作行为：2更改阅卷图标、4更改批注图标、8更改分数
        /// </summary>
        public int Operation { get; set; }
        /// <summary>
        /// 试卷类型：普通、A卷、B卷
        /// </summary>
        public int PaperType { get; set; }
        /// <summary>
        /// 批阅图标
        /// </summary>
        public string Icons { get; set; }
        /// <summary>
        /// 批注图标
        /// </summary>
        public string Marks { get; set; }
        /// <summary>
        /// 待移除的图标
        /// </summary>
        public string RemoveIcons { get; set; }
        /// <summary>
        /// 阅卷分数字符串
        /// </summary>
        public string DetailData { get; set; }
        /// <summary>
        /// 阅卷分数
        /// </summary>
        public List<MkDetailDto> Details { get; set; }
    }
}
