using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Publish;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Dtos.Variant;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IPublishContract
    {
        /// <summary> 变式过关的问题 </summary>
        DResult<VariantQuDto> VariantQuestions(string batchNo, long userId);

        /// <summary> 单题系统变式(优先推荐系统记录过的变式) </summary>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <param name="excepArray"></param>
        /// <returns></returns>
        DResults<QuestionDto> Variant(string questionId, int count = 1, List<string> excepArray = null,bool isNull=false);

        /// <summary> 多题系统变式(优先推荐系统记录过的变式) </summary>
        /// <param name="questionIds"></param>
        /// <param name="count"></param>
        /// <param name="excepArray"></param>
        /// <returns></returns>
        DResult<Dictionary<string, List<QuestionDto>>> Variants(List<string> questionIds, int count = 1,
            List<string> excepArray = null);

        /// <summary> 推荐变式题 </summary>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        DResults<QuestionDto> VariantRelationQuestion(string questionId, int count = 1);

        /// <summary> 历史推荐的变式题 </summary>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResults<QuestionDto> VariantHistory(string paperId, string questionId, long userId);

        /// <summary> 添加试卷中单题的变式推送 </summary>
        /// <param name="userId"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="vids"></param>
        /// <returns></returns>
        DResult AddVariant(long userId, string paperId, string questionId, List<string> vids);

        /// <summary> 是否已经推送变式 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        bool IsSendVariant(string batch, string paperId);

        /// <summary> 推送试卷变式 </summary>
        /// <param name="userId"></param>
        /// <param name="paperId"></param>
        /// <param name="variantDict"></param>
        /// <param name="classIds">班级Id</param>
        /// <returns></returns>
        DResult SendVariant(long userId, string paperId, Dictionary<string, List<string>> variantDict,
            List<string> classIds);

        /// <summary> 推送的变式题列表 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        DResult<VariantListDto> VariantList(string batch, string paperId);

        /// <summary> 系统推荐变式 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentId"></param>
        /// <param name="max"></param>
        /// <param name="pre"></param>
        /// <returns></returns>
        DResult<VariantListDto> VariantListFromSystem(string batch, string paperId, long studentId = 0, int max = 15,
            int pre = 1);
        /// <summary> 试卷薄弱点 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        DResult<PaperWeakDto> PaperWeak(string batch, string paperId);

        /// <summary> 教师推送过该试卷的班级列表 </summary>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Dictionary<string, string> UsageList(string paperId, long userId);
    }
}
