using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Portal.Services.Config;
using DayEasy.Portal.Services.Contracts;
using DayEasy.Portal.Services.Dto;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Timing;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Portal.Services.Services
{
    public class HomePageService : DayEasyService, IHomePageContract
    {
        #region 注入 & 私有方法
        public IDayEasyRepository<TU_UserAgency> UserAgencyRepository { private get; set; }
        public IDayEasyRepository<TU_Visit> VisitRepository { private get; set; }
        public IDayEasyRepository<TU_AgencyVisit> AgencyVisitRepository { private get; set; }
        public IDayEasyRepository<TU_Impression> ImpressionRepository { private get; set; }
        public IDayEasyRepository<TU_Quotations> QuotationRepository { private get; set; }

        public IDayEasyRepository<TU_ImpressionLike> ImpressionLikeRepository { private get; set; }
        public IVersion3Repository<TS_Agency> AgencyRepository { private get; set; }
        public IUserContract UserContract { private get; set; }
        public ISystemContract SystemContract { private get; set; }
        public HomePageService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        {
        }

        private List<VUserDto> ParseUserDto(IList<long> userIds)
        {
            var users = UserContract.LoadList(userIds);
            return users.Select(u => new VUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Avatar = u.Avatar,
                Code = u.Code,
                Role = u.Role,
                Level = u.Level,
                Nick = u.Nick,
                SubjectId = u.SubjectId,
                Subject = u.SubjectName
            }).OrderBy(t => userIds.IndexOf(t.Id)).ToList();
        }
        #endregion

        public List<VHotTeacherDto> HotTeachers(string agencyId, int count)
        {
            var list = new List<VHotTeacherDto>();
            var userIds = UserAgencyRepository.Where(t => t.AgencyId == agencyId && t.Role == (byte)UserRole.Teacher)
                .Select(t => t.UserId);
            if (!userIds.Any())
                return list;
            var models = ImpressionRepository.Table
                .Join(userIds, t => t.UserId, u => u, (t, u) => new
                {
                    t.UserId,
                    t.GoodsCount,
                    t.Impression
                })
                .GroupBy(t => t.UserId)
                .OrderByDescending(t => t.Sum(g => g.GoodsCount + 1))
                .Take(count)
                .ToList();
            if (!models.Any())
                return list;
            var ids = models.Select(t => t.Key).ToList();
            var users = UserContract.LoadList(ids);
            list.AddRange(from model in models
                          let user = users.FirstOrDefault(u => u.Id == model.Key)
                          where user != null
                          select new VHotTeacherDto
                          {
                              Id = model.Key,
                              Name = user.Name,
                              Nick = user.Nick,
                              Avatar = user.Avatar,
                              Code = user.Code,
                              Level = user.Level,
                              SubjectId = user.SubjectId,
                              Subject = user.SubjectName,
                              Impressions =
                                  model.OrderByDescending(t => t.GoodsCount)
                                      .Take(3)
                                      .Select(t => new DKeyValue<string, int>(t.Impression, t.GoodsCount))
                                      .ToList()
                          });
            return list;
        }

        public DResult<QuotationResultDto> HotQuotations(string agencyId, DPage page, long userId = 0)
        {
            var dto = new QuotationResultDto();
            var userIds = UserAgencyRepository.Where(t => t.AgencyId == agencyId && t.Role == (byte)UserRole.Teacher)
                .Select(t => t.UserId);
            if (!userIds.Any())
                return DResult.Succ(dto);
            var models = QuotationRepository.Table
                .Join(userIds, t => t.UserId, u => u, (t, u) => new
                {
                    t.Id,
                    t.UserId,
                    t.GoodsCount,
                    t.CreationTime,
                    t.Content
                });
            var quotations = models
                .OrderByDescending(t => t.GoodsCount)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            if (!quotations.Any())
                return DResult.Succ(dto);
            var ids = quotations.Select(t => t.Id).ToList();
            //支持者
            var supports = ImpressionLikeRepository.Where(t => ids.Contains(t.ContentId))
                .Select(t => new { t.ContentId, t.UserId })
                .ToList()
                .GroupBy(t => t.ContentId)
                .ToDictionary(k => k.Key, v => v.Select(t => t.UserId).ToList());
            foreach (var quotation in quotations)
            {
                var item = new QuotationsDto
                {
                    Id = quotation.Id,
                    Content = quotation.Content.FormatMessage().HtmlDecode(),
                    UserId = quotation.UserId,
                    CreationTime = quotation.CreationTime
                };
                if (supports.ContainsKey(item.Id))
                {
                    item.SupportList = supports[item.Id];
                    item.Supported = (!item.SupportList.IsNullOrEmpty() && item.SupportList.Contains(userId));
                }
                dto.Quotations.Add(item);
            }
            dto.Count = models.Count();
            var uids = quotations.Select(q => q.UserId).Distinct().ToList();
            uids.AddRange(supports.SelectMany(t => t.Value));
            uids = uids.Distinct().ToList();
            var users = UserContract.LoadList(uids);
            dto.Users = users.ToDictionary(k => k.Id, v => new VUserDto
            {
                Id = v.Id,
                Name = v.Name,
                Avatar = v.Avatar,
                Code = v.Code,
                Level = v.Level,
                Nick = v.Nick,
                SubjectId = v.SubjectId,
                Subject = v.SubjectName,
                Role = v.Role
            });
            return DResult.Succ(dto);
        }

        public List<VHotTeacherDto> AgencyTeachers(string agencyId)
        {
            var list =
                UserAgencyRepository.Where(
                    u =>
                        u.AgencyId == agencyId &&
                        u.Role == (byte)UserRole.Teacher)
                    .Select(t => t.UserId)
                    .Distinct()
                    .ToList();
            var users = UserContract.LoadList(list).OrderByDescending(u => u.AddedAt);
            return users.Select(u => new VHotTeacherDto
            {
                Id = u.Id,
                Name = u.Name,
                Avatar = u.Avatar,
                Code = u.Code,
                Level = u.Level,
                Nick = u.Nick,
                SubjectId = u.SubjectId,
                Subject = u.SubjectName
            }).ToList();
        }

        public List<VUserDto> AgencyLastVisitor(string agencyId, int count)
        {
            var list = AgencyVisitRepository.Where(a => a.AgencyId == agencyId)
                .Select(a => new { id = a.VisitUserId, time = a.VisitTime })
                .GroupBy(a => a.id)
                .OrderByDescending(a => a.Max(t => t.time))
                .Select(a => a.Key)
                .Take(count)
                .ToList();
            return ParseUserDto(list);
        }

        public List<VUserDto> UserLastVisitor(long userId, int count)
        {
            var list = VisitRepository.Where(v => v.UserId == userId)
                .Select(a => new { id = a.VisitUserId, time = a.VisitTime })
                .GroupBy(a => a.id)
                .OrderByDescending(a => a.Max(t => t.time))
                .Select(a => a.Key)
                .Take(count)
                .ToList();
            return ParseUserDto(list);
        }

        public IDictionary<string, string> OftenAgenies(string agencyId, int count)
        {
            var list = AgencyRepository.Where(t => t.Id != agencyId)
                .OrderByDescending(t => t.VisitCount)
                .Select(t => new { t.Id, t.AgencyName })
                .Take(count)
                .ToList();
            return list.ToDictionary(k => k.Id, v => v.AgencyName);
        }

        public DKeyValue<byte, int> UserAgencyRelation(string agencyId, long userId)
        {
            var model = UserAgencyRepository.FirstOrDefault(t => t.AgencyId == agencyId && t.UserId == userId);
            if (model == null)
                return null;
            var count = 0;
            if (model.Role == (byte)UserRole.Teacher)
                return new DKeyValue<byte, int>(model.Status, count);
            switch (model.Status)
            {
                case (byte)UserAgencyStatus.History:
                case (byte)UserAgencyStatus.Current:
                    //同届校友
                    count = UserAgencyRepository.Count(t => t.AgencyId == agencyId
                                                            && t.Status == model.Status
                                                            && t.StartTime.Year == model.StartTime.Year) - 1;
                    break;
                case (byte)UserAgencyStatus.Target:
                    //相同目标
                    count =
                        UserAgencyRepository.Count(
                            t =>
                                t.AgencyId == agencyId && t.Status == (byte)UserAgencyStatus.Target) - 1;
                    break;
            }
            return new DKeyValue<byte, int>(model.Status, count);
        }

        public object ImpressionList(long userId, DPage page, long visitorId)
        {
            var result = UserContract.ImpressionList(userId, page);
            if (!result.Status || result.TotalCount == 0)
                return new { count = 0 };
            var impressions = result.Data.ToList();
            impressions.ForEach(t =>
            {
                t.IsOwner = (t.CreatorId == visitorId || userId == visitorId);
                t.Supported = t.SupportCount > 0 && t.SupportList.Contains(visitorId);
            });
            var uids = result.Data.Select(t => t.CreatorId).ToList();
            uids.AddRange(result.Data.Where(t => t.SupportList != null).SelectMany(t => t.SupportList));
            uids = uids.Distinct().ToList();
            var agencyDict = UserAgencyRepository.Where(
                t => t.Status == (byte)UserAgencyStatus.Current && uids.Contains(t.UserId))
                .Select(t => new { t.UserId, t.AgencyId })
                .ToList()
                .GroupBy(t => t.UserId)
                .ToDictionary(k => k.Key, v => v.First().AgencyId);
            var dict = SystemContract.AgencyList(agencyDict.Values);

            var users = UserContract.LoadList(uids).ToDictionary(k => k.Id, v =>
            {
                var agency = string.Empty;
                if (agencyDict.ContainsKey(v.Id) && dict.ContainsKey(agencyDict[v.Id]))
                    agency = dict[agencyDict[v.Id]];
                return new
                {
                    avatar = v.Avatar,
                    name = v.Name,
                    level = v.Level,
                    agency,
                    code = v.Code
                };
            });
            return new
            {
                count = result.TotalCount,
                users,
                impressions = result.Data.ToList()
            };
        }

        public object RelatedTeachers(long userId, int count)
        {
            var agencyList = UserAgencyRepository.Where(t => t.UserId == userId)
                .Select(t => t.AgencyId)
                .ToList();
            var userList =
                UserAgencyRepository.Where(
                    t => t.UserId != userId && agencyList.Contains(t.AgencyId) && t.Role == (byte)UserRole.Teacher)
                    .Select(t => t.UserId)
                    .Distinct()
                    .RandomSort()
                    .Take(count)
                    .ToList();
            var agencyDict = UserAgencyRepository.Where(
                t => t.Status == (byte)UserAgencyStatus.Current && userList.Contains(t.UserId))
                .Select(t => new { t.UserId, t.AgencyId })
                .ToList()
                .GroupBy(t => t.UserId)
                .ToDictionary(k => k.Key, v => v.First().AgencyId);
            var dict = SystemContract.AgencyList(agencyDict.Values);

            var users = UserContract.LoadList(userList).Select(v =>
            {
                var agency = string.Empty;
                if (agencyDict.ContainsKey(v.Id) && dict.ContainsKey(agencyDict[v.Id]))
                    agency = dict[agencyDict[v.Id]];
                return new
                {
                    avatar = v.Avatar,
                    subject = v.SubjectName,
                    name = v.Name,
                    level = v.Level,
                    agency,
                    code = v.Code
                };
            });
            return users;
        }

        public object RelatedStudents(long userId, int count)
        {
            var agencyList = UserAgencyRepository.Where(t => t.UserId == userId)
                .Select(t => t.AgencyId)
                .ToList();
            var userList =
                UserAgencyRepository.Where(
                    t => t.UserId != userId
                        && agencyList.Contains(t.AgencyId)
                        && t.Role == (byte)UserRole.Student
                        && t.Status != (byte)UserAgencyStatus.Target)
                    .Select(t => t.UserId)
                    .Distinct()
                    .RandomSort()
                    .Take(count)
                    .ToList();

            var users = UserContract.LoadList(userList).Select(v => new
            {
                avatar = v.Avatar,
                name = v.Name,
                level = v.Level,
                code = v.Code
            });
            return users;
        }

        public object HotAgencies(long userId, int count)
        {
            var start = Clock.Now.AddMonths(-3);
            var agencyList = AgencyVisitRepository.Where(t => t.VisitTime > start)
                .Select(t => t.AgencyId)
                .GroupBy(t => t)
                .OrderByDescending(t => t.Count())
                .Select(t => t.Key)
                .Take(count)
                .ToList();
            return SystemContract.AgencyList(agencyList)
                .OrderBy(t => agencyList.IndexOf(t.Key))
                .Select(t => new
                {
                    id = t.Key,
                    name = t.Value
                });
        }

        public int TargetCount(string agencyId)
        {
            return UserAgencyRepository.Count(t => t.AgencyId == agencyId && t.Status == (byte)UserAgencyStatus.Target);
        }

        public List<VTargetAgencyDto> TargetAgencies(byte stage)
        {
            var list =
                AgencyRepository.Where(
                    t => t.Stage == stage && t.CertificationLevel == (byte)CertificationLevel.Official)
                    .Select(t => new VTargetAgencyDto
                    {
                        Id = t.Id,
                        Name = t.AgencyName,
                        Logo = t.AgencyLogo,
                        Stage = t.Stage
                    }).ToList();
            var ids = list.Select(t => t.Id).ToList();
            var countDict = UserAgencyRepository.Where(
                t => t.Status == (byte)UserAgencyStatus.Target && ids.Contains(t.AgencyId))
                .GroupBy(t => t.AgencyId)
                .Select(t => new
                {
                    id = t.Key,
                    count = t.Count()
                })
                .ToList()
                .ToDictionary(k => k.id, v => v.count);
            foreach (var dto in list)
            {
                if (countDict.ContainsKey(dto.Id))
                    dto.Count = countDict[dto.Id];
                if (string.IsNullOrWhiteSpace(dto.Logo))
                    dto.Logo = Consts.DefaultAvatar(RecommendImageType.AgencyLogo);
            }
            return list.OrderByDescending(t => t.Count).Take(18).ToList();
        }

        public ImpressionCategory RecommendImpression(byte role)
        {
            var config = ConfigUtils<ImpressionConfig>.Config;
            return config == null ? null : config.Categories.FirstOrDefault(t => (t.Role & role) > 0);
        }
    }
}
