using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts
{
    /// <summary> 答案分享业务模块 </summary>
    public interface IAnswerShareContract : IDependency
    {
        /// <summary>
        /// 同学分享的答案
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="classId"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        DResults<AnswerShareDto> Shares(string questionId, string classId, bool all = false);

        /// <summary>
        /// 添加分享
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        DResult Add(AnswerShareAddModelDto item);

        /// <summary>
        /// 膜拜同学分享的答案
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <param name="studentName"></param>
        /// <returns></returns>
        DResult Worship(string id, long studentId, string studentName);

        /// <summary>
        /// 分享的答案详细
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<AnswerShareDetailDto> Detail(string id, long userId);

        /// <summary>
        /// 试卷中已经分享答案的题目
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResults<string> SharedQuestions(string batch, string paperId, long studentId);

        /// <summary>
        /// 已分享的答案 - 在线阅卷使用
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        List<TP_AnswerShare> BatchShares(string batch, AnswerShareStatus status = AnswerShareStatus.PreShare);
        
    }
}
