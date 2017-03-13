using DayEasy.Contracts.Dtos.Statistic.Agency;
using DayEasy.Contracts.Enum.Statistic;

namespace DayEasy.Contracts
{
    /// <summary> 教务管理相关统计 </summary>
    public partial interface IStatisticContract
    {
        /// <summary> 学校概况 </summary>
        /// <param name="agencyId"></param>
        /// <param name="refresh">强制刷新</param>
        AgencySurveyDto AgencySurvey(string agencyId, bool refresh = false);

        /// <summary> 学校师生画像 </summary>
        /// <param name="agencyId"></param>
        /// <param name="timeArea">一周，一个月，三个月，6个月</param>
        /// <param name="refresh">强制刷新</param>
        AgencyPortraitDto AgencyPortrait(string agencyId, TimeArea timeArea = TimeArea.Week, bool refresh = false);

        /// <summary> 学校考试地图 </summary>
        /// <param name="agencyId"></param>
        /// <param name="timeArea"></param>
        /// <param name="refresh">强制刷新</param>
        AgencyExaminationMapDto AgencyExaminationMap(string agencyId, TimeArea timeArea = TimeArea.Week, bool refresh = false);

        /// <summary> 学校学情补救 </summary>
        /// <param name="agencyId"></param>
        /// <param name="timeArea"></param>
        /// <param name="refresh">强制刷新</param>
        AgencyRemedyDto AgencyRemedy(string agencyId, TimeArea timeArea = TimeArea.Week, bool refresh = false);
    }
}
