using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Topic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Cache;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Portal.Services.Contracts;
using DayEasy.Portal.Services.Dto;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Portal.Services.Services
{
    public class HomeService : DayEasyService, IHomeContract
    {
        //        public IDayEasyRepository<TP_Paper> PaperRepository { private get; set; }
        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }
        public IDayEasyRepository<TU_UserAgency> UserAgencyRepository { private get; set; }
        public IVersion3Repository<TS_Agency> AgencyRepository { private get; set; }
        public IGroupContract GroupContract { private get; set; }
        public ITopicContract TopicContract { private get; set; }
        public IMessageContract MessageContract { private get; set; }
        public HomeService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        {
        }

        private VHomeDto HomeDataCache()
        {
            const string region = "system";
            const string cacheKey = "dayeasy_home_data";
            var cache = new RuntimeMemoryCache(region);
            var dto = cache.Get<VHomeDto>(cacheKey);
            if (dto != null)
                return dto;
            var models = AgencyRepository.Where(t => t.CertificationLevel == (byte)CertificationLevel.Official)
                .Select(a => new { a.Id, a.AgencyName })
                .RandomSort()
                .Take(3)
                .ToList()
                .ToDictionary(k => k.Id, v => v.AgencyName);
            dto = new VHomeDto
            {
                UserCount = UserRepository.Count(u => u.Status != (byte)UserStatus.Delete),
                AgencyCount = AgencyRepository.Count(a => a.CertificationLevel == (byte)CertificationLevel.Official),
                HotAgencies = models
            };
            dto.TargetCount = dto.UserCount;
            cache.Set(cacheKey, dto, DateTime.Now.AddMinutes(5));
            return dto;
        }

        public DResult<VHomeDto> HomeData(UserDto user)
        {
            var dto = HomeDataCache();
            if (user == null || user.Id <= 0)
                return DResult.Succ(dto);
            return DResult.Succ(new VHomeDto
            {
                UserCount = dto.UserCount,
                AgencyCount = dto.AgencyCount,
                HotAgencies = dto.HotAgencies,
                TargetCount = dto.TargetCount,
                UserId = user.Id,
                IsTeacher = user.IsTeacher(),
                Avatar = user.Avatar,
                MessageCount = MessageContract.UserMessageCount(user.Id)
            });
        }

        public object GroupSearch(string codes)
        {
            var codeList = codes.JsonToObject<List<string>>();
            var result = GroupContract.SearchGroupsByCode(codeList);
            return new
            {
                groups = result.Status && result.Data != null && result.Data.Any()
                    ? result.Data.Select(g => new
                    {
                        g.Id,
                        g.Logo,
                        g.Code,
                        g.Name,
                        g.Owner,
                        g.Count
                    })
                    : null
            };
        }

        public object HotTopicAndGroup()
        {
            var topicResult = TopicContract.GetTopics(new SearchTopicDto
            {
                ClassType = -1,
                Page = 0,
                Size = 6,
                Order = TopicOrder.ReadNum
            });
            var groupResult = GroupContract.SearchShareGroups(0, ShareGroupOrder.TopicNumDesc, DPage.NewPage(0, 6));

            return new
            {
                topics = topicResult.Status && topicResult.Data != null && topicResult.Data.Any()
                    ? topicResult.Data.Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.ReadNum,
                        t.ReplyNum
                    })
                    : null,
                groups = groupResult.Status && groupResult.Data != null && groupResult.Data.Any()
                    ? groupResult.Data.Select(g => new
                    {
                        g.Id,
                        g.Logo,
                        g.Code,
                        g.Name,
                        g.Owner,
                        g.Count
                    })
                    : null
            };
        }

        public object AgencySearch(string keyword, int stage = -1, int count = 6)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return new object[] { };
            }
            if (count > 20) count = 20;
            Expression<Func<TS_Agency, bool>> condition = t =>
                t.Status == (byte)NormalStatus.Normal
                //                && !t.AgencyName.Contains("得一")
                && t.AgencyName.Contains(keyword);
            if (stage > 0)
                condition = condition.And(t => t.Stage == stage);
            var models =
                AgencyRepository.Where(condition)
                    .OrderByDescending(a => a.CertificationLevel)
                    .ThenByDescending(a => a.Sort)
                    .Select(a => new
                    {
                        id = a.Id,
                        name = a.AgencyName,
                        stage = a.Stage,
                        logo = a.AgencyLogo,
                        level = a.CertificationLevel
                    })
                    .Take(count)
                    .ToList();
            return models;
        }

    }
}
