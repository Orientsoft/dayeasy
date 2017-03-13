using DayEasy.AsyncMission;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public DResults<AsyncMissionDto> AsyncMissions(int type, int status = -1, string keyword = null,
            DPage page = null)
        {
            var results = MissionHelper.Missions(type, status, keyword, page);
            if (!results.Status)
                return DResult.Errors<AsyncMissionDto>(results.Message);
            var userIds =
                results.Data.Where(t => t.CreatorId.HasValue).Select(t => t.CreatorId.Value).Distinct().ToList();
            var users = UserContract.LoadList(userIds).ToDictionary(k => k.Id, v => v);
            var dtos = new List<AsyncMissionDto>();
            foreach (var mission in results.Data)
            {
                var dto = new AsyncMissionDto
                {
                    Id = mission.Id,
                    Type = (int)mission.Type,
                    Name = mission.Name,
                    CreationTime = mission.CreationTime,
                    Priority = mission.Priority,
                    Param = mission.Params,
                    FailCount = mission.FailCount ?? 0,
                    Message = mission.Message,
                    StartTime = mission.StartTime,
                    Status = (byte)mission.Status,
                    Logs = mission.Logs,
                    FinishedTime = mission.FinishedTime
                };
                if (mission.CreatorId.HasValue && users.ContainsKey(mission.CreatorId.Value))
                    dto.Creator = users[mission.CreatorId.Value];

                dtos.Add(dto);
            }
            return DResult.Succ(dtos, results.TotalCount);
        }

        public DResult ResetMission(string id)
        {
            return MissionHelper.ResetMission(id);

        }

        public DResult UpdateMissionPriority(string id, int priority)
        {
            return MissionHelper.UpdatePriority(id, priority);
        }
    }
}
