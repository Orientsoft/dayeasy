using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 选择评论的枚举 </summary>
    public enum ChooseComment
    {
        [Description("很有帮助，针对性很强")]
        One = 2,
        [Description("需要更多配套练习")]
        Two = 4,
        [Description("需要更多例题讲解")]
        Three = 8,
        [Description("需要名校老师的针对性点拨")]
        Four = 16,
        [Description("帮助不大，形式不喜欢")]
        Five = 32,
    }
}
