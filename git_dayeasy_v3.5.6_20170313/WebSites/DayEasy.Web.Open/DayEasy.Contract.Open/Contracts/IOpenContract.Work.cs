
using System.Collections.Generic;
using DayEasy.Contracts.Dtos;
using DayEasy.Models.Open.Group;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.Work;
using DayEasy.Utility;

namespace DayEasy.Contract.Open.Contracts
{
    public partial interface IOpenContract
    {
        /// <summary> 学生答案 </summary>
        DResult<MStudentAnswerDto> StudentAnswer(string batch, string paperId, string questionId, long userId);

        /// <summary> 同学分享的答案 </summary>
        DResults<MShareAnswerDto> ShareAnswers(string questionId, string classId, int size = 10);

        /// <summary> 分享答案详情 </summary>
        /// <param name="shareId"></param>
        /// <returns></returns>
        DResult<string> ShareAnswer(string shareId);

        /// <summary> 错误人数 </summary>
        int ErrorCount(string batch, string paperId, string questionId);

        /// <summary> 标记错题 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult<string> SetError(string batch, string paperId, string questionId, long studentId);

        /// <summary> 标记错题列表 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        IEnumerable<MPaperErrorDto> PaperErrors(string batch, string paperId, long studentId);

        /// <summary> 推荐的错因分析标签 </summary>
        /// <param name="errorId"></param>
        /// <returns></returns>
        DResults<NameDto> RecommendErrorTags(string errorId);

        DResult SetAnalysis(MErrorAnalysisInputDto dto);

        DResult<MErrorAnalysisDto> ErrorAnalysis(string errorId);

        string GetErrorId(string batch, string questionId, long studentId);

        DResults<MAnswerSheetDto> AnswerSheet(string batch, string paperId, long studentId);

        DResult<MScoreStatisticsDto> ScoreStatistics(string batch, string paperId, long studentId);

        DResults<string> SendedSmsScore(string batch, string paperId);

        DResult SendSmsScore(MSendSmsScoreDto dto);

        DResult<MAnswerPaperDto> ErrorStatistics(string batch);

        DResults<MMarkingDetailDto> StudentDetails(string batch, string paperId, long studentId);

        DResult VariantPass(string batch, long studentId);

        DResults<MMemberDto> UnSubmits(string batch, string paperId);

        DResult<MPaperVariantDto> PaperVariant(string batch, string paperId);

        DResults<MQuestionDto> NewVariant(string questionId, int count = 1, string excepts = null);

        DResult SendVariant(MSendVariantDto dto);
    }
}
