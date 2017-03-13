
using System.Collections.Generic;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 试卷业务模块 - 题库相关业务 </summary>
    public partial interface IPaperContract
    {
        /// <summary> 加载问题 </summary>
        /// <param name="questionId"></param>
        /// <param name="fromCache">是否从缓存中读取</param>
        /// <returns></returns>
        QuestionDto LoadQuestion(string questionId, bool fromCache = true);

        /// <summary> 批量加载问题 </summary>
        /// <param name="questionIds"></param>
        /// <returns></returns>
        List<QuestionDto> LoadQuestions(ICollection<string> questionIds);

        /// <summary> 搜索问题 </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        DResults<QuestionDto> SearchQuestion(SearchQuestionDto search);

        /// <summary> 删除问题 </summary>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult DeleteQuestion(string questionId, long userId);

        /// <summary> 客观题答案 </summary>
        /// <param name="questionId"></param>
        /// <param name="smallId"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        Dictionary<string, string[]> QuestionAnswer(string questionId, string smallId = null, string paperId = null);

        /// <summary> 问题分享 </summary>
        /// <param name="questionId"></param>
        /// <param name="share"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult QuestionShare(string questionId, ShareRange share, long userId);

        /// <summary> 保存问题 </summary>
        /// <param name="questionDto">题目信息</param>
        /// <param name="saveAs">是否另存为</param>
        /// <param name="isDraft">是否草稿</param>
        /// <returns>问题ID</returns>
        DResult<string> SaveQuestion(QuestionDto questionDto, bool saveAs = false, bool isDraft = false);

        /// <summary> 保存题目知识点 </summary>
        /// <param name="questionId"></param>
        /// <param name="knowledges"></param>
        /// <returns></returns>
        DResult SaveQuestionKnowledge(string questionId, List<NameDto> knowledges);

        /// <summary> 保存题干 </summary>
        /// <param name="questionId"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        DResult SaveQuestionBody(string questionId, string body);

        /// <summary> 题目变式(Solr直接搜索) </summary>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <param name="excepArray"></param>
        /// <returns></returns>
        List<QuestionDto> Variant(string questionId, int count = 1, List<string> excepArray = null,bool isNullData = false);

        /// <summary> 题目变式(Solr直接搜索) </summary>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <param name="excepArray"></param>
        /// <returns></returns>
        List<string> VariantForIds(string questionId, int count = 1, List<string> excepArray = null);

        /// <summary> 重置缓存及全文检索 </summary>
        /// <param name="qids"></param>
        /// <returns></returns>
        Task ClearQuestionCacheAsync(params string[] qids);
       
    }
}
