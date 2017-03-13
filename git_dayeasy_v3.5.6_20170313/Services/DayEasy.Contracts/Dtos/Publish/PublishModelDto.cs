using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Dtos.Publish
{
    /// <summary> 发布表模型 </summary>
    public class PublishModelDto : DDto
    {
        public string Batch { get; set; }
        public byte? PrintType { get; set; }
        public string SourceId { get; set; }
        public byte SourceType { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public long CreatorId { get; set; }
        public byte ApplyType { get; set; }
        public byte MarkingStatus { get; set; }
        public string ColleagueGroupId { get; set; }
    }

    /// <summary> 作业中心试卷模型 </summary>
    public class PublishPaperDto
    {
        /// <summary> 批次号 </summary>
        public string Batch { get; set; }
        /// <summary> 试卷标题 </summary>
        public string PaperName { get; set; }
        /// <summary> 试卷Id </summary>
        public string PaperId { get; set; }
        /// <summary> 班级圈名 </summary>
        public string GroupName { get; set; }
        /// <summary> 班级圈ID </summary>
        public string GroupId { get; set; }
        /// <summary> 科目Id </summary>
        public int SubjectId { get; set; }
        /// <summary> 提交人数 </summary>
        //public int SubmitCount { get; set; }

        /// <summary> A卷人数 </summary>
        public int ACount { get; set; }
        /// <summary> B卷人数 </summary>
        public int BCount { get; set; }
        /// <summary> 批阅人数 </summary>
        //public int MarkingCount { get; set; }

        /// <summary> 试卷类型 </summary>
        public byte PaperType { get; set; }
        /// <summary> 批阅状态 </summary>
        public byte MarkingStatus { get; set; }
        /// <summary> 发布类型 </summary>
        public byte SourceType { get; set; }
        /// <summary> 发布表状态 </summary>
        //public int UsagePaperStatus { get; set; }
        /// <summary> 标记错题人数 </summary>
        //public int MarkCount { get; set; }
        /// <summary> 结束时间 </summary>
        public DateTime ExpireTime { get; set; }
        /// <summary> 是否协同 </summary>
        public bool IsJoint { get; set; }
    }
}
