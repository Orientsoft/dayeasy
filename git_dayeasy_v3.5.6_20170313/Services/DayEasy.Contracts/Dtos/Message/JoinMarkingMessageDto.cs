using DayEasy.Contracts.Enum;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 协同阅卷动态模型 </summary>

    public class JointMarkingMessageDto : DDynamicMessageDto
    {
        /// <summary> 协同批次 </summary>
        public string JointBatch { get; set; }

        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }

        /// <summary> 同事圈ID </summary>
        public string GroupId { get; set; }

        /// <summary> 发布人 </summary>
        public long AddedBy { get; set; }

        /// <summary> 试卷类型 </summary>
        public PaperType PaperType { get; set; }

        /// <summary> 协同状态 </summary>
        public JointStatus JointStatus { get; set; }

        /// <summary> A卷份数 </summary>
        public int PaperACount { get; set; }

        /// <summary> B卷份数 </summary>
        public int PaperBCount { get; set; }

        /// <summary> 是否已分配任务 </summary>
        public bool Distributed { get; set; }

        /// <summary> 是否有批阅任务 </summary>
        public bool HasMission { get; set; }

        //public DateTime AddedAt { get; set; }
        //public DateTime FinishedAt { get; set; }
    }
}
