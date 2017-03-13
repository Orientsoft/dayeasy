
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IExaminationContract
    {
        /// <summary> 协同统计 </summary>
        /// <param name="examId"></param>
        DResult<ExamRanksDto> Rankings(string examId);

        /// <summary> 班级分析 - 重点率 </summary>
        /// <param name="inputDto"></param>
        List<ClassAnalysisKeyDto> ClassAnalysisKey(AnalysisInputDto inputDto);

        /// <summary> 班级分析 - 分层 </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        List<ClassAnalysisLayerDto> ClassAnalysisLayer(ClassAnalysisLayerInputDto inputDto);

        /// <summary> 学科分析 </summary>
        /// <param name="inputDto"></param>
        List<SubjectAnalysisDto> SubjectAnalysis(SubjectAnalysisInputDto inputDto);

        /// <summary> 题目得分率统计 </summary>
        /// <param name="id">考试科目Id</param>
        /// <returns></returns>
        DResult<ScoreRateDto> SubjectScoreRates(string id);
    }
}
