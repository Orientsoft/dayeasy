using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contract.Open.Dtos
{
    /// <summary> 名师大神 </summary>
    public class VTeacherGodInputDto : DDto
    {
        /// <summary> 区域编码 </summary>
        public int AreaCode { get; set; }
        /// <summary> 学校名称 </summary>
        public string School { get; set; }
        /// <summary> 名字 </summary>
        public string Name { get; set; }
        /// <summary> 创建者 </summary>
        public string Creator { get; set; }
        /// <summary> 选择的模板 </summary>
        public byte Type { get; set; }
        /// <summary> 自定义文字 </summary>
        public string Word { get; set; }
        /// <summary> 自定义图片：Base64 </summary>
        public string ImageData { get; set; }
    }
}
