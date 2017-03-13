
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 学生统计相关业务 </summary>
    public partial interface IStatisticContract
    {
        /// <summary> 学生考试成绩 </summary>
        /// <param name="studentId"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult<StudentScoreDto> StudentScore(long studentId, string batch);

        /// <summary> 学生成绩报表 </summary>
        /// <param name="studentId"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult<StudentReportDto> StudentReport(long studentId, string batch);

        /// <summary> 学生成绩对比 </summary>
        /// <param name="userId"></param>
        /// <param name="compareId"></param>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        DResult<StudentCompareDto> StudentCompare(long userId, long compareId, string batch, string paperId);
    }
}
