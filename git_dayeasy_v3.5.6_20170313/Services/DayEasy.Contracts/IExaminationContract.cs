using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts
{
    /// <summary> 大型考试契约接口 </summary>
    public partial interface IExaminationContract : IDependency
    {
        /// <summary> 查询考试中协同详情 </summary>
        /// <param name="examId">考试ID</param>
        /// <returns></returns>
        DResults<ExamSubjectDto> ExamJointList(string examId);

        /// <summary> 协同阅卷列表 </summary>
        /// <param name="dto"></param>
        DResults<ExamSubjectDto> JointList(JointSearchDto dto);

        /// <summary> 考试列表 </summary>
        /// <param name="status"></param>
        /// <param name="agencyId"></param>
        /// <param name="page"></param>
        /// <param name="isUnion"></param>
        /// <returns></returns>
        DResults<ExamDto> Examinations(int status, string agencyId = null, DPage page = null, bool? isUnion = null);

        /// <summary> 创建大型考试 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        DResult CreateExamination(ExamDto dto);

        /// <summary> 编辑大型考试 </summary>
        /// <param name="updateDto"></param>
        /// <returns></returns>
        DResult UpdateExamination(ExamUpdateDto updateDto);

        /// <summary> 推送大型考试 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult SendExamination(string id, long userId);

        /// <summary> 删除大型考试 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult DeleteExamination(string id, long userId);

        /// <summary> 发布大型考试 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        DResult PublishExamination(string id, long userId, UserRole role);

        /// <summary> 考试科目信息 </summary>
        /// <param name="examId"></param>
        /// <returns></returns>
        List<ExamSubjectDto> ExamSubjects(string examId);

        /// <summary>
        /// 学生考试统计
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult<StudentSummaryDto> Summary(string id, long studentId);

        /// <summary>
        /// 学生考试 分数段统计
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResults<StudentSubjectSectionDto> ScoreSections(string id, long studentId);
    }
}
