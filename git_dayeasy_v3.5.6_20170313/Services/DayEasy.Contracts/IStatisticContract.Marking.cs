using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IStatisticContract : IDependency
    {

        /// <summary>
        /// 客观题选项统计
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="smallQuestionId"></param>
        /// <returns></returns>
        DResult<StatisticsQuestionDto> StatisticsQuestionDetail(string batch, string paperId,
            string questionId, string smallQuestionId = "");

        /// <summary>
        /// 排名统计
        /// </summary>
        /// <returns></returns>
        DResults<StudentRankInfoDto> GetStatisticsRank(string batch, string paperId, string jointBatch = "", string groupId = "");

        /// <summary>
        /// 考试统计概况 - 排名分析
        /// </summary>
        DResult<SurveyAnalysis> GetSurveyAnalysis(string batch, string paperId);

        /// <summary>
        /// 发送成绩通知短信
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        DResult SendScoreSms(string batch, string paperId, List<long> studentIds);
        /// <summary>
        /// 分数段统计
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="keepSixty">保留60%以下</param>
        /// <param name="single">不查询年级统计</param>
        /// <returns></returns>
        DResults<StatisticsScoreDto> GetStatisticsAvges(string batch, string paperId, bool keepSixty = false, bool single = false);

        /// <summary>
        /// 分数段统计 - 协同
        /// </summary>
        /// <param name="jointBatch"></param>
        /// <param name="paperId"></param>
        /// <param name="keepSixty"></param>
        /// <returns></returns>
        DResults<StatisticsScoreDto> GetJointStatisticsAvges(string jointBatch, string paperId, bool keepSixty = false);

        /// <summary>
        /// 共享、取消共享试卷统计到同事圈
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="teacherId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        DResult StatisticsShare(string batch, long teacherId, string groupId = "");

        /// <summary> 每题得分统计 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult<QuestionScoresDto> QuestionScores(string batch);

        /// <summary> 协同批次每题得分 </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        DResult<QuestionScoresDto> JointQuestionScores(string joint);
    }
}
