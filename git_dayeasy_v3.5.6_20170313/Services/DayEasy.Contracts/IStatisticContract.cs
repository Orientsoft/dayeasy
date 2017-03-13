using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary>
    /// 统计类契约
    /// </summary>
    public partial interface IStatisticContract : IDependency
    {
        /// <summary>
        /// 更新学生统计
        /// </summary>
        /// <param name="statistic"></param>
        /// <returns></returns>
        DResult UpdateStatistic(StudentStatisticDto statistic);

        /// <summary>
        /// 更新老师统计
        /// </summary>
        /// <param name="statistic"></param>
        /// <returns></returns>
        DResult UpdateStatistic(TeacherStatisticDto statistic);

        /// <summary>
        /// 更新老师知识点统计
        /// </summary>
        /// <param name="kpStatistics"></param>
        /// <returns></returns>
        void UpdateKpStatistic(IEnumerable<TeacherKpStatisticDto> kpStatistics);

        /// <summary>
        /// 更新学生知识点统计
        /// </summary>
        /// <param name="kpStatistics"></param>
        /// <returns></returns>
        void UpdateKpStatistic(IEnumerable<StudentKpStatisticDto> kpStatistics);

        /// <summary>
        /// 获取知识点统计数据
        /// </summary>
        /// <returns></returns>
        DResult<KpStatisticDataDto> GetKpStatistic(SearchKpStatisticDataDto searchDto);

        /// <summary>
        /// 错题top 10
        /// </summary>
        /// <returns></returns>
        DResult<ErrorTopTenDataDto> ErrorTopTen(SearchErrorTopTenDto searchDto);

        /// <summary>
        /// 获取班级综合成绩统计数据
        /// </summary>
        /// <returns></returns>
        DResults<SeriesDto> GetClassScores(SearchClassScoresDto searchDto);

        /// <summary>
        /// 学生成绩排名升降图
        /// </summary>
        /// <returns></returns>
        DResult<StudentRankDataDto> StudentRank(string groupId, int subjectId);

        /// <summary>
        /// 获取教师端学生排名折线图数据
        /// </summary>
        /// <returns></returns>
        DResults<StudentSeriesDto> GetStuRankDetail(SearchStudentRankDto searchDto);

        /// <summary>
        /// 获取学生时间段内考过的科目
        /// </summary>
        /// <returns></returns>
        DResult<IDictionary<int, string>> GetStudentSubject(SearchStudentRankDto searchDto);

        /// <summary>
        /// 获取学生成绩统计
        /// </summary>
        /// <returns></returns>
        DResults<StudentSeriesDto> GetStudentScores(SearchStudentRankDto searchDto);

        /// <summary>
        /// 获取学生成绩
        /// </summary>
        /// <param name="batchs"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResults<StudentScoreDto> GetStudentScores(List<string> batchs, long userId);
        
        /// <summary>
        /// 获取学生成绩
        /// </summary>
        /// <param name="batchs"></param>
        /// <returns></returns>
        DResult<Dictionary<string, decimal>> GetGroupAvgScores(List<string> batchs);
    }
}
