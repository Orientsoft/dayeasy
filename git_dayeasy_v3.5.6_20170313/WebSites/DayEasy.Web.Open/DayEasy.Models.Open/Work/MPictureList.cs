using System.Collections.Generic;

namespace DayEasy.Models.Open.Work
{
    public class MPictureList : DDto
    {
        /// <summary> 上传用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }

        /// <summary> 试卷编号 </summary>
        public string PaperNum { get; set; }

        /// <summary> 协同批次号 </summary>
        public string JointBatch { get; set; }

        /// <summary> 同事圈ID </summary>
        public string GroupId { get; set; }

        /// <summary> 分割线坐标 每个题目记录4个点，起始x坐标，起始y坐标，终止y坐标，宽度。如[0,250,320,780]表示x坐标为0开始从y坐标250到320宽度为780的区域。</summary>
        public IList<int[]> Lines { get; set; }

        /// <summary> 试卷图片列表 </summary>
        public IList<MPictureInfo> Pictures { get; set; }

        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }

        public MPictureList()
        {
            Pictures = new List<MPictureInfo>();
        }
    }
}
