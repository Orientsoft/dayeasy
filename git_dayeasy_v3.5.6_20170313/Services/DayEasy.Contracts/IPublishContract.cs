using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Publish;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts
{
    /// <summary> 发布类契约 </summary>
    public partial interface IPublishContract : IDependency
    {
        /// <summary>
        /// 推送试卷
        /// </summary>
        /// <returns></returns>
        DResult PulishPaper(string pId, string groupIds, string sourceGId, long userId, string sendMsg);

        /// <summary>
        /// 获取发布详细信息
        /// </summary>
        /// <returns></returns>
        DResult<PublishModelDto> GetUsageDetail(string batch);

        /// <summary>
        /// 获取发布详细信息
        /// </summary>
        /// <returns></returns>
        DResults<PublishModelDto> GetUsageDetailByJointBatch(string jointBatch);

        /// <summary>
        /// 获取学生的作业
        /// </summary>
        /// <returns></returns>
        DResults<StudentWorksDto> GetStudentHomeworks(SearchStuWorkDto searchDto);

        /// <summary>
        /// 获取教师布置的作业
        /// </summary>
        /// <returns></returns>
        DResults<PublishPaperDto> GetTeacherPubWorks(long teacherId, string key, DPage page);

        /// <summary> 根据批次号查询圈子 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult<GroupDto> LoadByBatch(string batch);

        /// <summary>
        /// 查询试卷中题目的错误人数
        /// </summary>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        DResult<Dictionary<string, List<long>>> GetPaperErrorQuCount(string batchNo);

        /// <summary>
        /// 学生成绩统计
        /// </summary>
        /// <returns></returns>
        DResult<StudentScoreStatisticDto> StudentScoreStatistic(string batchNo, long userId);
        
        /// <summary>
        /// 获取相关辅导
        /// </summary>
        /// <returns></returns>
        DResult<RecommendTutorDto> GetRecommendTutors(string batchNo, long userId);

        /// <summary>
        /// 获取未提交信息（A卷或B卷）
        /// </summary>
        /// <returns></returns>
        DResult<string> GetNotSubmitted(string batchNo, long userId);

        /// <summary>
        /// 撤回试卷
        /// </summary>
        /// <returns></returns>
        DResult DeletePubPaper(string batch, long userId);
    }
}
