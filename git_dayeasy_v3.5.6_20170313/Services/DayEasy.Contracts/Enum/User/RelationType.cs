
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 成员关系 </summary>
    public enum FamilyRelationType : byte
    {
        [Description("爸爸")] Father = 1,
        [Description("妈妈")] Mother = 2,
        [Description("爷爷")] GrandFather = 3,
        [Description("奶奶")] GrandMother = 4,
        [Description("其他家属")] Other = 10
    }
}
