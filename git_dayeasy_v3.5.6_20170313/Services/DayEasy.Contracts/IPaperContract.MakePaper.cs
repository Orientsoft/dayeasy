using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IPaperContract
    {
        /// <summary>
        /// 出卷基础信息处理
        /// </summary>
        /// <param name="paperBaseStr"></param>
        /// <param name="paperType"></param>
        /// <param name="subjectId"></param>
        /// <param name="stage"></param>
        DResult<ChooseQuestionDataDto> PaperBaseAction(string paperBaseStr, string paperType, int subjectId, int stage);

        /// <summary>
        /// 自动出卷基础信息处理
        /// </summary>
        DResult<ChooseQuestionDataDto> AutoPaperBaseAction(string autoData, string paperType, int subjectId, int stage);

        /// <summary>
        /// 试卷添加问题--保存
        /// </summary>
        /// <returns></returns>
        DResult<string> AddPaperQuestion(AddQuestionDto addQuestion);

        /// <summary>
        /// 构造预览数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>[NonAction]
        PaperDetailDto MakePreviewData(MakePaperDto data);
    }
}
