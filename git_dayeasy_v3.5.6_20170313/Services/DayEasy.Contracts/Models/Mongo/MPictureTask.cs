using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary> 协同批阅任务 </summary>
    public class MPictureTask : DEntity<string>
    {
        /// <summary> Id </summary>
        public override string Id { get; set; }

        /// <summary> 协同批次 </summary>
        public string JointBatch { get; set; }

        /// <summary> 图片ID </summary>
        public string PictureId { get; set; }

        /// <summary> 题目任务 </summary>
        public List<QuestionTask> QuestionTask { get; set; }
    }

    /// <summary> 题目任务 </summary>
    public class QuestionTask : DEntity<string>
    {
        /// <summary> 题目ID </summary>
        public override string Id { get; set; }

        /// <summary> 教师ID </summary>
        public long TeacherId { get; set; }

        /// <summary> 批阅时间 </summary>
        public DateTime? MarkingTime { get; set; }
    }
}
