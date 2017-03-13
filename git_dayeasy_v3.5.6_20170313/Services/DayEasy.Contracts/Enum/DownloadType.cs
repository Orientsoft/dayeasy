using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 下载类型 </summary>
    public enum DownloadType : byte
    {
        /// <summary> 试卷 </summary>
        [Description("试卷")]
        Paper = 0,

        /// <summary> 错题 </summary>
        [Description("错题")]
        ErrorQuestion = 1,

        /// <summary> 变式 </summary>
        [Description("变式")]
        Variant = 2,

        /// <summary> 考试统计 </summary>
        [Description("考试统计")]
        ClassStatistic = 3,

        /// <summary> 分数段排名 </summary>
        [Description("分数段排名")]
        ClassSegment = 4,

        /// <summary> 年级排名 </summary>
        [Description("年级排名")]
        GradeRank = 5,

        /// <summary> 班级分析 </summary>
        [Description("班级分析")]
        ClassAnalysis = 6,

        /// <summary> 学科分析 </summary>
        [Description("学科分析")]
        SubjectAnalysis = 7,

        /// <summary> 圈子成员 </summary>
        [Description("圈子成员")]
        GroupMember = 8,

        /// <summary> 协同统计 </summary>
        [Description("协同统计")]
        JointStatistic = 9,

        /// <summary> 关联报表 </summary>
        [Description("关联报表")]
        UnionReport = 10
    }
}
