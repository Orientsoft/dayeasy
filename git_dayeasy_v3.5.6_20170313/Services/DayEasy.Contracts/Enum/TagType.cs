using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 标签类型 </summary>
    public enum TagType : byte
    {
        [Description("系统标签")]
        System = 0,
        [Description("题库标签")]
        Question = 1,
        [Description("试卷标签")]
        Paper = 2,
        [Description("课堂标签")]
        ClassRoom = 4,
    }
}
