using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 错题库来源类型 </summary>
    public enum ErrorQuestionSourceType : byte
    {
        [Description("试卷")]
        Paper = 0,
        [Description("电子课本")]
        EBook = 1
    }
}
