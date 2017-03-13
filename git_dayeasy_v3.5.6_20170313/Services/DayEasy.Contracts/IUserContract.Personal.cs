

using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IUserContract
    {
        /// <summary> 访问用户 </summary>
        int Visit(long userId, long visitorId = 0);

        /// <summary> 编辑个性签名 </summary>
        DResult EditSignature(long userId, string signature);
        /// <summary> 添加贴纸 </summary>
        DResult AddImpression(ImpressionInputDto dto);

        /// <summary> 支持贴纸 </summary>
        DResult SupportImpression(string id, long userId);
        /// <summary> 取消支持贴纸 </summary>
        DResult CancelSupportImpression(string id, long userId);

        /// <summary> 删除贴纸 </summary>
        DResult DeleteImpression(string id, long userId);

        /// <summary> 贴纸列表 </summary>
        DResults<ImpressionDto> ImpressionList(long userId, DPage page);

        /// <summary> 新收到的贴纸 </summary>
        /// <param name="userId"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        List<string> LastImpressions(long userId, long visitorId);

        /// <summary> 热门贴纸 </summary>
        /// <param name="role"></param>
        /// <param name="count"></param>
        List<string> HotImpressions(UserRole role, int count);

        /// <summary> 添加语录 </summary>
        DResult AddQuotations(QuotationsInputDto dto);

        /// <summary> 支持语录 </summary>
        DResult SupportQuotations(string id, long userId);
        /// <summary> 取消支持语录 </summary>
        DResult CancelSupportQuotations(string id, long userId);

        /// <summary> 删除语录 </summary>
        DResult DeleteQuotations(string id, long userId);

        /// <summary> 语录列表 </summary>
        DResults<QuotationsDto> QuotationsList(long userId, DPage page);
    }
}