using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    public enum SchoolBookStatus
    {
        [Description("正常")]
        Normal = 0,

        [Description("待发布")]
        Edit = 1,

        [Description("删除")]
        Delete = 4
    }
}
