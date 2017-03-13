using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.User.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.User.Services
{
    public class TempOldService : DayEasyService, ITempOldContract
    {
        public TempOldService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }

        public IDayEasyRepository<TU_Agency, string> Agencies { private get; set; }
        public IDayEasyRepository<TU_UserAgencyRelation, string> AgencyRelations { private get; set; }

        public IDayEasyRepository<TU_Class, string> Classes { get; set; }

        public IDayEasyRepository<TU_UserApplication, string> UserApplicationRepository { private get; set; }

        public IDayEasyRepository<TS_DynamicNews, string> DynamicRepository { private get; set; }

        public DResults<TU_Agency> AgencyList(DPage page)
        {
            var ordered = Agencies.Where(a => a.Status == (byte)CheckStatus.Normal).OrderBy(a => a.CheckPassTime);
            return Agencies.PageList(ordered, page);
        }

        public bool InAgencies(long userId, IEnumerable<string> agencyIds)
        {
            if (userId <= 0 || agencyIds == null)
                return false;
            return
                AgencyRelations.Exists(
                    a => agencyIds.Contains(a.AgencyID) && a.UserID == userId && a.Status == (byte)CheckStatus.Normal);
        }

        public int ImportUsers(IEnumerable<TU_User> users, IEnumerable<TU_UserAgencyRelation> relations)
        {
            try
            {
                return UnitOfWork.Transaction(() =>
                {
                    UserRepository.Insert(users);
                    AgencyRelations.Insert(relations);
                });
            }
            catch
            {
                return 0;
            }
        }

        public DResults<TU_Class> ClassList(DPage page)
        {
            var relations = AgencyRelations.Where(a =>
                a.Status == (byte)CheckStatus.Normal
                && a.ClassID != null);
            var ordered =
                Classes.Where(c => c.Status == 0)
                    .GroupJoin(relations, d => d.Id, s => s.ClassID, (c, a) => new { c, a })
                    .Where(t => t.a.Any())
                    .Select(t => t.c)
                    .OrderBy(c => c.CreateTime)
;
            return Classes.PageList(ordered, page);
        }

        public DResults<TU_UserAgencyRelation> RelationList(string groupId)
        {
            var relations =
                AgencyRelations.Where(a => a.ClassID == groupId && a.UserRole != 16).OrderBy(a => a.ApplyTime).ToList();
            return DResult.Succ(relations, relations.Count());
        }

        public DResult<int> UpdateDCode()
        {
            var users = UserRepository.Where(u =>
                (u.Status == (byte)UserStatus.Normal || u.Status == (byte)UserStatus.UnBind)
                && (u.UserCode == null || u.UserCode == string.Empty));
            var count = users.Count();
            Console.WriteLine("共找到{0}条用户数据", count);
            const int size = 50;
            var pages = (int)Math.Ceiling(count / (double)size);
            int updateCount = 0;
            var codeManager = UserCodeManager.Instance;

            for (int i = 0; i < pages; i++)
            {
                updateCount += UnitOfWork.Transaction(() =>
                {
                    var list = users.OrderBy(u => u.Id).Skip(i * size).Take(size).ToList();
                    foreach (var user in list)
                    {
                        user.UserCode = codeManager.Code();
                    }
                    UserRepository.Update(u => new { u.UserCode }, list.ToArray());
                    Console.WriteLine("更新{0}条用户数据", i * size);
                });
            }
            return DResult.Succ(updateCount);
        }

        public DResult AddApplication(long userId, int applicationId)
        {
            var app =
                UserApplicationRepository.FirstOrDefault(a => a.UserID == userId && a.ApplicationID == applicationId);
            if (app != null)
            {
                if (app.Status == (byte)NormalStatus.Normal)
                    return DResult.Success;
                app.Status = (byte)NormalStatus.Normal;
                UserApplicationRepository.Update(a => new { a.Status }, app);
            }
            else
            {
                UserApplicationRepository.Insert(new TU_UserApplication
                {
                    Id = IdHelper.Instance.Guid32,
                    UserID = userId,
                    ApplicationID = applicationId,
                    AddedAt = Clock.Now,
                    AddedIP = Utils.GetRealIp(),
                    Status = (byte)NormalStatus.Normal
                });
            }
            return DResult.Success;
        }

        public List<TS_DynamicNews> LoadDynamics(int page, int size)
        {
            var list = DynamicRepository.Where(
                d => (d.NewsType == 0 || d.NewsType == 2)
                     && d.Status == 0
                     && d.SourceType == 0
                     && d.RecieveID.Length == 32)
                .OrderBy(d => d.SendTime)
                .Skip(page * size)
                .Take(size);
            return list.ToList();
        }
    }
}
