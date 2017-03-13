using System.Collections.Generic;
using DayEasy.Contracts.Models;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.Marking.Services.Helper
{
    /// <summary> 阅卷基类 </summary>
    internal class MarkingConsts
    {
        /// <summary> 提交信息为空 </summary>
        public const string MsgSubmitNull = "提交信息不能为空！";
        /// <summary> 试卷未找到 </summary>
        public const string MsgPaperNotFind = "试卷未找到！";
        /// <summary> 批次号未找到 </summary>
        public const string MsgBatchNotFind = "发布批次号未找到！";
        /// <summary> 测试未开始 </summary>
        public const string MsgNotStart = "测试尚未开始！";
        /// <summary> 测试已结束 </summary>
        public const string MsgEnded = "测试已经结束，不能再提交答卷！";
        /// <summary> 试卷ID错误 </summary>
        public const string MsgPaperIdError = "发布号对应的试卷ID不正确！";
        /// <summary> 重复的提交 </summary>
        public const string MsgSubmitAgain = "亲，不要重复提交哦！";

        public const string MsgMarkingNull = "批阅结果不能为空！";
        public const string MsgTeacherIdError = "教师ID不正确！";
        public const string MsgCommitError = "提交数据异常！";
        public const string MsgMarkingError = "批阅失败，数据异常！";
        public const string MsgMarkingNotFinished = "试卷还未完成批阅！";
        private const string MsgSuccess = "操作成功！";

        public static readonly DResult Success = DResult.Succ(MsgSuccess);

        /// <summary> 判断选项 </summary>
        /// <param name="source">学生答案</param>
        /// <param name="target">正确答案</param>
        /// <returns></returns>
        public static bool ArrayEquals<T>(IEnumerable<T> source, IEnumerable<T> target)
        {
            return source.ArrayEquals(target, true);
        }
    }

    /// <summary>
    /// 分数统计
    /// </summary>
    public class ScoreStatistics
    {
        public ScoreStatistics()
        {
            StuScoreStatisticses = new List<TS_StuScoreStatistics>();
        }
        public List<TS_StuScoreStatistics> StuScoreStatisticses { get; set; }
        public TS_ClassScoreStatistics ClassScoreStatisticses { get; set; } 
    }
}
