using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 年级 </summary>
    public enum Grade : byte
    {
        [Description("1年级")]
        Grade1 = 1,
        [Description("2年级")]
        Grade2 = 2,
        [Description("3年级")]
        Grade3 = 3,
        [Description("4年级")]
        Grade4 = 4,
        [Description("5年级")]
        Grade5 = 5,
        [Description("6年级")]
        Grade6 = 6,
        [Description("7年级")]
        Grade7 = 7,
        [Description("8年级")]
        Grade8 = 8,
        [Description("9年级")]
        Grade9 = 9,
        [Description("高中1年级")]
        Grade10 = 10,
        [Description("高中2年级")]
        Grade11 = 11,
        [Description("高中3年级")]
        Grade12 = 12,
    }
}
