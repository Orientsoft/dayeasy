using System.ComponentModel;

namespace DayEasy.AsyncMission.Models
{
    public enum MissionType
    {
        /// <summary> 完成阅卷 </summary>
        [Description("完成阅卷")]
        FinishMarking = 0,
        /// <summary> 图片提交 </summary>
        [Description("图片提交")]
        CommitPicture = 1,
        /// <summary> 修改答案 </summary>
        [Description("修改答案")]
        ChangeAnswer = 2,
        /// <summary> 重置协同 </summary>
        [Description("重置协同")]
        ResetJoint = 3,
        /// <summary> 知识点移动 </summary>
        [Description("知识点移动")]
        MoveKnowledges = 4
    }
}
