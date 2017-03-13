
using DayEasy.Models.Open.Work;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contract.Open.Contracts
{
    public partial interface IOpenContract
    {
        /// <summary> 获取协同批次 </summary>
        DResults<MJointUsageDto> JointUsages(string paperId, long userId = -1);

        /// <summary> 扫描工具提交图片 </summary>
        DResults<MHandinResult> HandinPictures(long creator, MPictureList result);

        /// <summary> 套打列表 </summary>
        /// <param name="paperId"></param>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        DResults<MPrintInfo> PrintList(string paperId, long teacherId = -1);

        /// <summary> 套打详情 </summary>
        /// <param name="batch"></param>
        /// <param name="sectionType"></param>
        /// <param name="skip"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        DResults<MPrintDetail> PrintDetails(string batch, byte sectionType, int skip = 0, int size = 100);

        /// <summary> 协同套打列表 </summary>
        /// <param name="paperId"></param>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        DResults<MJointPrintInfo> JointPrintList(string paperId, long teacherId = -1);

        /// <summary> 协同学校 </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        DResult<Dictionary<string, string>> JointAgencies(string joint);

        /// <summary> 协同套打详情 </summary>
        /// <param name="joint"></param>
        /// <param name="sectionType"></param>
        /// <param name="skip"></param>
        /// <param name="size"></param>
        /// <param name="agencyId">机构ID</param>
        /// <returns></returns>
        DResults<MPrintDetail> JointPrintDetails(string joint, byte sectionType, int skip = 0, int size = 50,
            string agencyId = null);
    }
}
