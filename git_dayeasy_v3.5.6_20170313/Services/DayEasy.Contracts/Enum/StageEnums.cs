
using DayEasy.Core.Domain;
using DayEasy.Utility.Extend;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 学段 </summary>
    public enum StageEnum : byte
    {
        /// <summary> 小学 </summary>
        [Description("小学")]
        PrimarySchool = 1,
        /// <summary> 初中 </summary>
        [Description("初中")]
        JuniorMiddleSchool = 2,
        /// <summary> 高中 </summary>
        [Description("高中")]
        HighSchool = 3
    }

    /// <summary>
    /// 小学年级
    /// </summary>
    public enum PrimarySchoolGrade : byte
    {
        /// <summary> 小学一年级 </summary>
        [Description("小学一年级")]
        FirstGrade = 1,
        /// <summary> 小学二年级 </summary>
        [Description("小学二年级")]
        SecondGrade = 2,
        /// <summary> 小学三年级 </summary>
        [Description("小学三年级")]
        ThirdGrade = 3,
        /// <summary> 小学四年级 </summary>
        [Description("小学四年级")]
        FourthGrade = 4,
        /// <summary> 小学五年级 </summary>
        [Description("小学五年级")]
        FifthGrade = 5,
        /// <summary> 小学六年级 </summary>
        [Description("小学六年级")]
        SixthGrade = 6
    }

    /// <summary>
    /// 初中年级
    /// </summary>
    public enum JuniorMiddleSchoolGrade : byte
    {
        /// <summary> 初中一年级 </summary>
        [Description("初中一年级")]
        FirstGrade = 7,
        /// <summary> 初中二年级 </summary>
        [Description("初中二年级")]
        SecondGrade = 8,
        /// <summary> 初中三年级 </summary>
        [Description("初中三年级")]
        ThirdGrade = 9
    }

    /// <summary>
    /// 高中年级
    /// </summary>
    public enum HighSchoolGrade : byte
    {
        /// <summary> 高中一年级 </summary>
        [Description("高中一年级")]
        FirstGrade = 10,
        /// <summary> 高中二年级 </summary>
        [Description("高中二年级")]
        SecondGrade = 11,
        /// <summary> 高中三年级 </summary>
        [Description("高中三年级")]
        ThirdGrade = 12
    }

    /// <summary> 学段扩展 </summary>
    public static class EnumExtensions
    {
        /// <summary> 年级对应中文名称 </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static string GradeName(this byte g)
        {
            if (g == 0)
                return string.Empty;
            if (g <= (byte)PrimarySchoolGrade.SixthGrade)
                return g.GetEnumText<PrimarySchoolGrade, byte>();
            if (g <= (byte)JuniorMiddleSchoolGrade.ThirdGrade)
                return g.GetEnumText<JuniorMiddleSchoolGrade, byte>();
            if (g <= (byte)HighSchoolGrade.ThirdGrade)
                return g.GetEnumText<HighSchoolGrade, byte>();
            return string.Empty;
        }

        /// <summary> 根据学段获取年级 </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static List<DKeyValue<int, string>> Grades(this byte stage)
        {
            switch (stage)
            {
                case (byte)StageEnum.PrimarySchool:
                    return System.Enum.GetValues(typeof(PrimarySchoolGrade))
                        .Cast<PrimarySchoolGrade>()
                        .Select(t => new DKeyValue<int, string>((int)t, t.GetText())).ToList();
                case (byte)StageEnum.JuniorMiddleSchool:
                    return System.Enum.GetValues(typeof(JuniorMiddleSchoolGrade))
                        .Cast<JuniorMiddleSchoolGrade>()
                        .Select(t => new DKeyValue<int, string>((int)t, t.GetText())).ToList();
                case (byte)StageEnum.HighSchool:
                    return System.Enum.GetValues(typeof(HighSchoolGrade))
                        .Cast<HighSchoolGrade>()
                        .Select(t => new DKeyValue<int, string>((int)t, t.GetText())).ToList();
            }
            return null;
        }
    }
}
