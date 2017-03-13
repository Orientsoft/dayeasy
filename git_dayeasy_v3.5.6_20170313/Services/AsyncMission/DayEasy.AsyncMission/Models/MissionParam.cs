using System.Collections.Generic;

namespace DayEasy.AsyncMission.Models
{
    public interface IMissionParam { }
    public class MissionParam : IMissionParam { }

    /// <summary> 完成阅卷参数 </summary>
    public class FinishMarkingParam : MissionParam
    {
        /// <summary> 批次号 </summary>
        public string Batch { get; set; }

        /// <summary> 是否协同 </summary>
        public bool IsJoint { get; set; }
        /// <summary> 默认打"勾" </summary>
        public bool SetIcon { get; set; }
        /// <summary> 主观题得分标记 </summary>
        public bool SetMarks { get; set; }

        public FinishMarkingParam()
        {
            SetIcon = true;
            SetMarks = true;
        }
    }

    /// <summary> 提交图片参数 </summary>
    public class CommitPictureParam : MissionParam
    {
        /// <summary> 试卷Id </summary>
        public string PaperId { get; set; }
        /// <summary> 图片列表 </summary>
        public List<string> PictureIds { get; set; }
        /// <summary> 协同批次号 </summary>
        public string JointBatch { get; set; }
    }

    /// <summary> 问题答案 </summary>
    public class QuestionAnswer
    {
        /// <summary> 问题Id </summary>
        public string QuestionId { get; set; }
        /// <summary> 小问Id </summary>
        public string SmallId { get; set; }
        /// <summary> 正确答案 </summary>
        public string Answer { get; set; }
    }

    /// <summary> 修改答案参数 </summary>
    public class ChangeAnswerParam : MissionParam
    {
        public string PaperId { get; set; }
        /// <summary> 答案列表 </summary>
        public List<QuestionAnswer> Answers { get; set; }
        /// <summary> 是否包含已结束 </summary>
        public bool ContainsFinished { get; set; }
    }

    /// <summary> 重置协同参数 </summary>
    public class ResetJointParam : MissionParam
    {
        /// <summary> 协同批次号 </summary>
        public string JointBatch { get; set; }
    }

    /// <summary> 知识点转移参数 </summary>
    public class MoveKnowledgesParam : MissionParam
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }
        public string TargetName { get; set; }
    }
}
