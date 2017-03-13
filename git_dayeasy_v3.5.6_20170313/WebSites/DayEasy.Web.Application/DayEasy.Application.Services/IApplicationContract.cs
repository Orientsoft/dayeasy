using DayEasy.Application.Services.Dto;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Application.Services
{
    /// <summary> 应用中心相关 </summary>
    public interface IApplicationContract : IDependency
    {
        /// <summary> 新批阅任务 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<int> NewMarking(long userId);

        /// <summary> 批阅任务 </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<VMarkingDto> MarkingList(long userId, DPage page);

        /// <summary> 报表中心 - 班级统计 </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<VReportDto> ClassReports(long userId, DPage page);

        /// <summary> 年级报表 </summary>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<VGradeReportDto> GradeReports(long userId, int role, DPage page);

        /// <summary> 关联报表 - 基础信息 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DResult<VUnionChartsDto> UnionCharts(string id);
    }
}
