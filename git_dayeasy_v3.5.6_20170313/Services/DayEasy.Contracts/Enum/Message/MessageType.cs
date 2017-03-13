using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 消息类型 </summary>
    public enum MessageType : byte
    {
        /// <summary> 系统消息 </summary>
        [Description("系统消息")]
        System = 0,

        /// <summary> 评论/回复 </summary>
        [Description("评论回复")]
        CommentReply = 1,

        /// <summary> 晒答案 </summary>
        [Description("晒答案")]
        DryingAnswer = 2,

        /// <summary> 帐号关联 </summary>
        [Description("帐号关联")]
        AssociateAccount = 3,

        /// <summary> 点赞 </summary>
        [Description("点赞")]
        Like = 4
    }
}
