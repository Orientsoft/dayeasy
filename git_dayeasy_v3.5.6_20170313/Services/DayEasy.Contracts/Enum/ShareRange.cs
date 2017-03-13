
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    public enum ShareRange : byte
    {
        [Description("个人")]
        Self = 0,
        [Description("全网")]
        Public = 2
    }

    public enum AnswerShareStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("预分享")]
        PreShare = 2,
        [Description("删除")]
        Delete = 4
    }
}
