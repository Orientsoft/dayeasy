using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 来源 </summary>
    public enum Comefrom : byte
    {
        [Description("Web站点")]
        Web = 0,
        [Description("手机网站")]
        Mobile = 10,
        [Description("阅卷工具")]
        MarkingTool = 20,
        [Description("Android")]
        Android = 30,
        [Description("IOS")]
        Ios = 40
    }
}
