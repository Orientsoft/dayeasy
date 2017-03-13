using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts
{
    public partial interface IExaminationContract
    {
        /// <summary> 关联报表 </summary>
        /// <param name="ids"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult UnionReport(List<string> ids, long userId);

        /// <summary> 取消关联 </summary>
        /// <param name="unionBatch"></param>
        /// <returns></returns>
        DResult CancelUnion(string unionBatch);
        /// <summary> 关联列表 </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<UnionExamDto> UnionList(DPage page = null);

        /// <summary> 关联报表 - 综合统计 </summary>
        /// <param name="unionBatch"></param>
        DResult<UnionSourceDto> UnionSource(string unionBatch);
    }
}
