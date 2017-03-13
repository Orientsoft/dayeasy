using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 选项样式 </summary>
    public enum OptionStyle : byte
    {
        [Description("得一")]
        Deyi = 0,
        [Description("试卷添加")]
        AddFromPaper = 2
    }
}
