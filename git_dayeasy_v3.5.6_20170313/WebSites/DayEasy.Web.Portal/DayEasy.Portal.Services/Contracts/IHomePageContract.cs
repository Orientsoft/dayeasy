using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Portal.Services.Config;
using DayEasy.Portal.Services.Dto;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Portal.Services.Contracts
{
    public interface IHomePageContract : IDependency
    {
        /// <summary> 热门教师 </summary>
        List<VHotTeacherDto> HotTeachers(string agencyId, int count);

        /// <summary> 热门语录 </summary>
        DResult<QuotationResultDto> HotQuotations(string agencyId, DPage page, long userId = 0);

        /// <summary> 随机教师 </summary>
        List<VHotTeacherDto> AgencyTeachers(string agencyId);

        /// <summary> 最近访客 </summary>
        List<VUserDto> AgencyLastVisitor(string agencyId, int count);

        /// <summary> 最近访客 </summary>
        List<VUserDto> UserLastVisitor(long userId, int count);

        /// <summary> 常逛机构 </summary>
        IDictionary<string, string> OftenAgenies(string agencyId, int count);

        /// <summary> 用户机构关系 </summary>
        DKeyValue<byte, int> UserAgencyRelation(string agencyId, long userId);

        object ImpressionList(long userId, DPage page, long visitorId);

        /// <summary> 相关人气教师 </summary>
        object RelatedTeachers(long userId, int count);

        /// <summary> 相关人气教师 </summary>
        object RelatedStudents(long userId, int count);

        /// <summary> 校友常逛学校 </summary>
        object HotAgencies(long userId, int count);
        /// <summary> 学校目标人数 </summary>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        int TargetCount(string agencyId);

        /// <summary> 目标学校 </summary>
        List<VTargetAgencyDto> TargetAgencies(byte stage);

        ImpressionCategory RecommendImpression(byte role);
    }
}
