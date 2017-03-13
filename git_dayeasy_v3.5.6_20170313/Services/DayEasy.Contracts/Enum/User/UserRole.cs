using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 用户角色 </summary>
    [Flags]
    public enum UserRole : byte
    {
        [Description("游民")]
        Caird = 0,
        [Description("家长")]
        Parents = 1,
        [Description("学生")]
        Student = 2,
        [Description("教师")]
        Teacher = 4,
        [Description("系统管理员")]
        SystemManager = 128
    }
}
