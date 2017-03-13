using System;
using System.Collections.Generic;

namespace DayEasy.Models.Open.Work
{
    /// <summary> 单张扫描图片 </summary>
    public class MPictureInfo : DDto
    {
        public string Id { get; set; }
        public int Index { get; set; }

        /// <summary> 扫描类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 图片链接 </summary>
        public string ImagePath { get; set; }

        /// <summary> 学生ID </summary>
        public long StudentId { get; set; }

        /// <summary> 学生姓名 </summary>
        public string StudentName { get; set; }

        /// <summary> 圈子ID </summary>
        public string GroupId { get; set; }

        /// <summary> 图片页数 </summary>
        public int PageCount { get; set; }

        /// <summary> 是否单面 </summary>
        public bool IsSingle { get; set; }

        /// <summary> 是否识别成功 </summary>
        public bool IsSuccess
        {
            get { return StudentId > 0; }
        }

        /// <summary> 答题卡识别答案  [0]:A;[-1]:未选;[124]:BCE. </summary>
        public IList<int[]> SheetAnwers { get; set; }

        public MPictureInfo()
        {
            Id = Guid.NewGuid().ToString("N");
            SheetAnwers = new List<int[]>();
        }
    }
}
