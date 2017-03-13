using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 批阅模式 </summary>
    public enum JointMarkingMode : byte
    {
        /// <summary> 卷阅 </summary>
        [Description("卷阅")]
        Paper = 0,

        /// <summary> 包阅 </summary>
        [Description("包阅")]
        Bag = 1
    }
}
