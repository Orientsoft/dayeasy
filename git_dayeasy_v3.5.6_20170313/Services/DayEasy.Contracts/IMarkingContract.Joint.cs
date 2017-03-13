
using DayEasy.Contracts.Dtos.Marking.Joint;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Marking;

namespace DayEasy.Contracts
{
    public partial interface IMarkingContract
    {
        /// <summary> 发布协同 </summary>
        /// <param name="teacherId">教师ID</param>
        /// <param name="groupId">同事圈ID</param>
        /// <param name="paperNum">试卷编号</param>
        DResult PublishJoint(long teacherId, string groupId, string paperNum);

        /// <summary> 分配题目信息 </summary>
        /// <param name="jointBatch"></param>
        DResult<JAllotDto> JointAllot(string jointBatch);

        /// <summary> 协同任务分配 </summary>
        DResult DistributionJoint(JDistributionDto dto);

        /// <summary> 添加教师权限 </summary>
        /// <param name="id"></param>
        /// <param name="teacherIds"></param>
        /// <returns></returns>
        DResult AddDistribution(string id, List<long> teacherIds);

        /// <summary> 协同批阅任务 </summary>
        DResult<JMissionDto> JointMission(string jointBatch, long teacherId);

        /// <summary> 加载题目分组 </summary>
        /// <param name="jointBatch"></param>
        /// <param name="groups"></param>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        DResult<JCombineDto> JointCombine(string jointBatch, List<string[]> groups, long teacherId);

        /// <summary> 切换试卷 </summary>
        /// <param name="jointBatch"></param>
        /// <param name="groups"></param>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        DResults<JPictureDto> ChangePictures(string jointBatch, List<JGroupStepDto> groups, long teacherId);

        /// <summary> 提交协同批阅数据 </summary>
        DResult JointSubmit(JSubmitDto dto);

        /// <summary> 报告异常 </summary>
        DResult ReportException(JExceptionDto dto);
        /// <summary> 解决异常 </summary>
        DResult SolveException(string id);

        /// <summary> 协同列表 </summary>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResults<JointGroupDto> JointList(string paperId, long userId = -1);

        /// <summary> 按照批次号、问题、当前答卷、获取对应图片 </summary>
        DResult InterceptPicture(string batch, string paperId, long studentNo, MarkingPaperType paperType, string questionId);
        /// <summary>
        /// 客观题得分率
        /// </summary>
        /// <param name="jointBatch"></param>
        /// <returns></returns>
        DResult<ObjectQuestionScoreRate> ObjectiveQuestionScore(string jointBatch);
    }
}
