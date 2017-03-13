
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IStatisticContract
    {
        /// <summary> 试卷掌握情况 </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        DResult<GraspingDto> Graspings(string batch, string paperId);
    }
}
