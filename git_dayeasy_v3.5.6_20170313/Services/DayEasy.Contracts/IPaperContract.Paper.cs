
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DayEasy.Contracts
{
    /// <summary> 试卷业务模块 - 试卷相关业务 </summary>
    public partial interface IPaperContract
    {
        /// <summary> 保存试卷 </summary>
        /// <param name="paper"></param>
        /// <param name="answerList"></param>
        /// <returns></returns>
        DResult SavePaper(MakePaperDto paper, List<MakePaperAnswerDto> answerList);

        /// <summary> 试卷列表 </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        DResults<PaperDto> PaperList(SearchPaperDto search);

        /// <summary> 试卷列表 </summary>
        /// <param name="paperIds"></param>
        /// <returns></returns>
        DResults<PaperDto> PaperList(List<string> paperIds);

        /// <summary> 试卷发布列表 </summary>
        /// <param name="paperId"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        DResults<PaperPublishDto> PaperPublishList(string paperId, SearchPaperDto search);

        ///// <summary> 加载试卷信息 </summary>
        ///// <param name="paperId"></param>
        ///// <returns></returns>
        //NPaperDto LoadPaperById(string paperId);

        /// <summary> 试卷详情 </summary>
        /// <param name="paperId"></param>
        /// <param name="loadQuestion"></param>
        /// <returns></returns>
        DResult<PaperDetailDto> PaperDetailById(string paperId, bool loadQuestion = true);

        /// <summary> 试卷详情(试卷编号) </summary>
        /// <param name="paperNo"></param>
        /// <param name="loadQuestion"></param>
        /// <returns></returns>
        DResult<PaperDetailDto> PaperDetailByPaperNo(string paperNo, bool loadQuestion = true);

        /// <summary>
        /// 编辑试卷
        /// </summary>
        /// <param name="paperId"></param>
        /// <param name="paper"></param>
        /// <param name="answerList"></param>
        /// <returns></returns>
        DResult EditPaper(string paperId, MakePaperDto paper, List<MakePaperAnswerDto> answerList);

        /// <summary> 删除试卷 </summary>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult DeletePaper(string paperId, long userId);

        /// <summary> 试卷列表 - 选题 </summary>
        /// <param name="topicDto"></param>
        /// <returns></returns>
        DResults<TopicPaperDto> TopicsPaper(SearchTopicDto topicDto);

        /// <summary>
        /// 获取试卷问题包含知识点的数量
        /// </summary>
        /// <returns></returns>
        dynamic GetPaperKpCount(List<string> paperIds, string kp);

        /// <summary> 修改试卷答案 </summary>
        DResult EditPaperAnswer(string paperId, IEnumerable<MakePaperAnswerDto> paperAnswerDto, long userId, bool containsFinished = false);

        /// <summary>
        /// 获取试卷所有的小问分数
        /// </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        DResults<SmallQuScoreDto> GetSmallQuScore(string paperId);

        /// <summary>
        /// 获取草稿的数量
        /// </summary>
        /// <returns></returns>
        DResult<int> GetCount(long userId, int subjectId, PaperStatus status);

        /// <summary>
        /// 查询试卷的答案
        /// </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        DResults<PaperAnswerDto> GetPaperAnswers(string paperId);

        /// <summary>
        /// 是否全是客观题
        /// </summary>
        /// <returns></returns>
        bool AllObjectiveQuestion(string paperId, PaperSectionType paperSectionType);

        /// <summary> 试卷题目编号(通排) </summary>
        /// <param name="paperId"></param>
        /// <param name="smallSort">小问参与编号，返回的字段Key将包含小问ID</param>
        /// <returns></returns>
        Dictionary<string, DKeyValue<byte, int>> QuestionSorts(string paperId, bool smallSort = true);

        /// <summary> 试卷知识点对应的问题 </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        DResults<KnowledgeQuestionsDto> KnowledgeQuestions(string paperId);

        /// <summary> 试卷知识点对应的问题 </summary>
        /// <param name="paper"></param>
        /// <returns></returns>
        DResults<KnowledgeQuestionsDto> KnowledgeQuestions(PaperDetailDto paper);

        /// <summary> 试卷题号 </summary>
        /// <param name="paperId"></param>
        /// <param name="isObjective">是否客观题</param>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        Dictionary<string, string> PaperSorts(string paperId, bool? isObjective = false, int sectionType = -1);

        /// <summary> 试卷题号 </summary>
        /// <param name="paper"></param>
        /// <param name="isObjective">是否客观题</param>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        Dictionary<string, string> PaperSorts(PaperDetailDto paper, bool? isObjective = false, int sectionType = -1,bool includeQid = false);

        /// <summary> 重置缓存 </summary>
        /// <param name="paperIds"></param>
        /// <returns></returns>
        Task ClearPaperCacheAsync(params string[] paperIds);
    }
}
