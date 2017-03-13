using DayEasy.Contracts.Management.Dto;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts.Management
{
    public partial interface IManagementContract
    {
        DResults<AsyncMissionDto> AsyncMissions(int type, int status = -1, string keyword = null, DPage page = null);
        DResult ResetMission(string id);
        DResult UpdateMissionPriority(string id, int priority);
    }
}
