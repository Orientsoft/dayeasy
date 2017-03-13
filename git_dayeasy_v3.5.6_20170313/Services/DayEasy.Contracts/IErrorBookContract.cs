
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts
{
    /// <summary> 错题本业务模块 </summary>
    public interface IErrorBookContract : IDependency
    {
        /// <summary> 更改错题状态 </summary>
        DResult UpdateErrorQuestionStatus(string errorId, ErrorQuestionStatus status,long studentId);

        /// <summary> 设置过关、取消过关 </summary>
        DResult SetPass(string errorId, long studentId, bool pass);

        /// <summary>
        /// 错题列表
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        DResults<ErrorQuestionDto> ErrorQuestions(ErrorQuestionSearchDto search);
        /// <summary>
        /// 错题医院错题列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        DResults<DErrorQuestionDto> ErrorQuestions(SearchErrorQuestionDto dto);

        /// <summary>
        /// 错题详细 - 学生
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult<ErrorQuestionDto> ErrorQuestion(string errorId, long studentId);

        /// <summary>
        /// 错题详细 - 教师
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        DResult<ErrorQuestionDto> ErrorQuestion(string batch, string paperId, string questionId);

        /// <summary>
        /// 标记推送试卷中的错题
        /// </summary>
        /// <returns></returns>
        DResult<string> MarkErrorQuestion(string batch, string paperId, string qId, long studentId);

        /// <summary>
        /// 学生错题答案
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult<dynamic> ErrorAnswer(string batch, string paperId, string questionId, long studentId);

        /// <summary>
        /// 试卷错题对应答错的学生
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="loadUserInfo"></param>
        /// <returns></returns>
        DResults<ErrorQuestionStudentDto> ErrorStudents(string batch, string paperId, string questionId = "",
            bool loadUserInfo = false);

        /// <summary>
        /// 模拟智能匹配搜索关键字
        /// </summary>
        /// <param name="key"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        DResults<string> SearchKeys(string key, int subjectId);

        /// <summary>
        /// 有错题的科目
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        IDictionary<int, string> ErrorQuestionSubjects(long studentId);

        /// <summary>
        /// 有错题的题型
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        List<QuestionTypeDto> ErrorQuestionTypes(long studentId, int subjectId);

        /// <summary>
        /// 错题错误率
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        DResult<dynamic> ErrorQuestionRate(string batch, string paperId, string questionId);


        /// <summary>
        /// 查询试卷中的题目错因分析数量
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        IDictionary<string, int> ReasonCountDict(string batch);

        /// <summary> 错因标签列表 </summary>
        /// <param name="errorId"></param>
        /// <param name="batch"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        DResults<ReasonExtDto> Reasons(string errorId, string batch, string questionId);

        /// <summary> 添加错因标签 </summary>
        /// <param name="errorId"></param>
        /// <param name="content"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        DResult AddReason(string errorId, string content, List<string> tags);

        /// <summary> 删除错因及评论 </summary>
        /// <param name="reasonId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult DeleteComment(string reasonId, long studentId);

        /// <summary> 错因分析评论列表 </summary>
        /// <param name="page"></param>
        /// <param name="reasonId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        DResults<ReasonCommentDto> Comments(DPage page, string reasonId, string parentId);

        /// <summary> 添加错因分析评论 </summary>
        /// <param name="reasonId"></param>
        /// <param name="parentId"></param>
        /// <param name="content"></param>
        /// <param name="userId"></param>
        /// <param name="receiveId"></param>
        /// <param name="receiveName"></param>
        /// <returns></returns>
        DResult AddComment(string reasonId, string parentId, string content, long userId, long receiveId,
            string receiveName);

        /// <summary> 推荐标签 </summary>
        /// <param name="errorId"></param>
        /// <returns></returns>
        DResults<NameDto> RecomTags(string errorId);

        /// <summary> 班级内错因标签统计 </summary>
        /// <param name="batch"></param>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        DResults<DKeyValue<string, int>> TagStatistics(string batch, string questionId, int count = 6);

        /// <summary>
        /// 指定错题的错因分析 - 编辑错因分析使用
        /// </summary>
        /// <param name="errorId"></param>
        /// <returns></returns>
        DResult<dynamic> Load(string errorId);
        
        /// <summary>
        /// 获取错题ID
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="questionId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        string GetErrorId(string batch, string questionId, long studentId);

        /// <summary>
        /// 获取试卷里面的错题Ids
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResults<string> GetPaperErrorQIds(string batch, string paperId, long userId);
        
        /// <summary>
        /// 根据错题ID集查询问题ID集
        /// </summary>
        /// <param name="eids"></param>
        /// <returns></returns>
        DResults<string> GetErrorQIdsByEIds(List<string> eids);

        /// <summary>
        /// 指定试卷问题有答错的学生
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        List<long> HasErrorStudentIds(string batch, string paperId, string questionId);

        /// <summary>
        /// 已提交的学生ID
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        List<long> IsSubmitStudentIds(string batch, string paperId);

    }
}
