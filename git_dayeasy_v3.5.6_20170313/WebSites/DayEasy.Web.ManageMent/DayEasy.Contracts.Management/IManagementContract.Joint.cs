
using DayEasy.Contracts.Dtos.Marking.Joint;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts.Management
{
    public partial interface IManagementContract
    {
        DResults<JointMarkingDto> JointList(JointSearchDto searchDto);

        DResult JointRecall(string jointBatch);

        DResult<JointUnSubmitDto> JointUnsubmits(string jointBatch);

        List<DKeyValue> JointPictures(string jointBatch, byte type);
        DKeyValue Picture(string id);

        /// <summary> 异常数 </summary>
        /// <param name="jointBatchs"></param>
        /// <returns></returns>
        Dictionary<string, int> JointExceptionCount(ICollection<string> jointBatchs);

        DResults<JointExceptionDto> JointExceptions(string jointBatch, int status = -1, DPage page = null, byte? type = null);

        /// <summary> 解决异常 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DResult SolveJointException(string id);
        /// <summary> 重置协同 </summary>
        /// <param name="jointBatch"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult ResetJoint(string jointBatch, long userId);
        
        /// <summary> 导出协同数据 </summary>
        /// <param name="jointBatch"></param>
        void ExportJoint(string jointBatch);

        #region 普通阅卷 & 推送
        DResults<VMarkingDto> MarkingList(VMarkingInputDto inputDto);

        List<DKeyValue> MarkingPictures(string batch, byte type);
        /// <summary> 撤回 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult Recall(string batch);

        void ExportMarking(string batch);

        #endregion
    }
}
