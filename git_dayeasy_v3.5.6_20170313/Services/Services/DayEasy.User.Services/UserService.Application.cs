using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Services;
using DayEasy.User.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.User.Services
{
    public partial class UserService
    {
        public IDayEasyRepository<TS_Application, int> ApplicationRepository { private get; set; }
        public IDayEasyRepository<TU_UserApplication, string> UserApplicationRepository { private get; set; }

        public List<ApplicationDto> UserApplications(long userId, bool fromCache = true)
        {
            if (!fromCache)
                return UserApplicationsFromDb(userId) ?? new List<ApplicationDto>();
            var cache = UserCache.Instance;
            var apps = cache.GetApps(userId);
            if (apps != null) return apps;
            apps = UserApplicationsFromDb(userId);
            cache.SetApps(apps, userId);
            return apps;
        }

        public DResult AddApplication(long userId, int applicationId, string agencyId = null)
        {
            var item = ApplicationRepository.Load(applicationId);
            if (item == null)
                return DResult.Error("应用不存在！");
            if (item.Status != (byte)NormalStatus.Normal)
                return DResult.Error("应用已被删除！");
            var user = UserRepository.Load(userId);
            if (user == null)
                return DResult.Error("用户数据异常！");
            if ((item.AppRoles & user.Role) == 0)
                return DResult.Error("用户角色与应用角色不匹配！");

            var app =
                UserApplicationRepository.FirstOrDefault(a => a.UserID == userId && a.ApplicationID == applicationId);
            if (app != null)
            {
                if (app.Status == (byte)NormalStatus.Normal)
                    return DResult.Error("用户已有该应用！");
                app.Status = (byte)NormalStatus.Normal;
                app.AgencyId = agencyId;
                UserApplicationRepository.Update(a => new { a.Status, a.AgencyId }, app);
            }
            else
            {
                UserApplicationRepository.Insert(new TU_UserApplication
                {
                    Id = IdHelper.Instance.Guid32,
                    UserID = userId,
                    ApplicationID = applicationId,
                    AgencyId = agencyId,
                    AddedAt = Clock.Now,
                    AddedIP = Utils.GetRealIp(),
                    Status = (byte)NormalStatus.Normal
                });
            }
            return DResult.Success;
        }

        public DResult<AgencyDto> ApplicationAgency(long userId, int appId)
        {
            var agencyId = UserApplicationRepository.Where(a =>
                a.UserID == userId
                && a.ApplicationID == appId
                && a.Status == (byte)NormalStatus.Normal)
                .Select(a => a.AgencyId).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(agencyId))
                return DResult.Error<AgencyDto>("没有该应用的权限！");
            var agency =
                AgencyRepository.Where(t => t.Id == agencyId)
                    .Select(a => new { a.Id, a.AgencyName, a.Stage })
                    .FirstOrDefault();
            if (agency == null)
                return DResult.Error<AgencyDto>("没有该应用的权限！");
            return DResult.Succ(new AgencyDto
            {
                Id = agency.Id,
                Name = agency.AgencyName,
                Stage = agency.Stage
            });
        }
    }
}
