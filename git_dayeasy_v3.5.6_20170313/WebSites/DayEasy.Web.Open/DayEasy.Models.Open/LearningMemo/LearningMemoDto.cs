using System;

namespace DayEasy.Models.Open.LearningMemo
{
    public class LearningMemoDto : DDto
    {
        /// <summary> 标题 </summary>
        public string Title { get; set; }

        /// <summary> 笺类型 </summary>
        public byte MemoType { get; set; }

        /// <summary> 标签 </summary>
        public string[] Tags { get; set; }

        /// <summary> 文字内容 </summary>
        public string Words { get; set; }

        /// <summary> 图片内容 </summary>
        public string[] Pictures { get; set; }

        /// <summary> 语音内容 </summary>
        public SpeechDto Speeches { get; set; }

        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }

        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 创建时间 </summary>
        public DateTime AddedAt { get; set; }

        /// <summary> 创建时间 </summary>
        public long AddedAtLong
        {
            get { return ToLong(AddedAt); }
            set { AddedAt = ToDateTime(value); }
        }
    }
}
