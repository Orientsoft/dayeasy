
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.User.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.User.Services
{
    public partial class UserService
    {
        public IDayEasyRepository<TU_UserAgency> UserAgencyRepository { private get; set; }
        public IVersion3Repository<TS_Agency> AgencyRepository { private get; set; }
        public IVersion3Repository<TG_Group> GroupRepository { private get; set; }

        public DResult AddAgency(UserAgencyInputDto dto)
        {
            if (dto == null)
                return DResult.Error("参数错误！");
            if (string.IsNullOrWhiteSpace(dto.AgencyId))
                return DResult.Error("请选择机构！");
            var agency = AgencyRepository.Load(dto.AgencyId);
            if (agency == null)
                return DResult.Error("机构异常！");
            var user = Load(dto.UserId);
            if (user == null)
                return DResult.Error("用户不存在！");
            if (dto.Status != (byte)UserAgencyStatus.Target && !dto.Start.HasValue)
                return DResult.Error("请选择开始时间！");
            TU_UserAgency target = null;
            TU_UserAgency current = null;
            string currentId = null;
            var item = new TU_UserAgency
            {
                Id = IdHelper.Instance.Guid32,
                AgencyId = dto.AgencyId,
                StartTime = dto.Start ?? Clock.Now,
                EndTime = dto.End,
                UserId = dto.UserId,
                Stage = agency.Stage,
                Status = dto.Status,
                Role = user.IsTeacher() ? (byte)UserRole.Teacher : (byte)UserRole.Student
            };
            switch (dto.Status)
            {
                case (byte)UserAgencyStatus.History:
                    break;
                case (byte)UserAgencyStatus.Current:
                    currentId = dto.AgencyId;
                    //当前 -> 历史
                    current =
                        UserAgencyRepository.FirstOrDefault(
                            t => t.UserId == dto.UserId && t.Status == (byte)UserAgencyStatus.Current);
                    break;
                case (byte)UserAgencyStatus.Target:
                    if (!user.IsStudent())
                        return DResult.Error("只有学生才能设置目标学校！");
                    //移除之前的目标
                    target =
                        UserAgencyRepository.FirstOrDefault(
                            t => t.UserId == dto.UserId && t.Status == (byte)UserAgencyStatus.Target);
                    break;
            }
            var result = UserAgencyRepository.UnitOfWork.Transaction(() =>
            {
                if (target != null)
                    UserAgencyRepository.Delete(target);
                if (!string.IsNullOrWhiteSpace(currentId))
                {
                    UserRepository.Update(new TU_User
                    {
                        AgencyId = currentId
                    }, u => u.Id == dto.UserId, "AgencyId");
                }
                if (current != null)
                {
                    current.Status = (byte)UserAgencyStatus.History;
                    current.EndTime = Clock.Now;
                    UserAgencyRepository.Update(current, "Status", "EndTime");
                }
                UserAgencyRepository.Insert(item);
            });
            if (result > 0 && dto.Status == (byte)UserAgencyStatus.Current)
            {
                UserCache.Instance.RemoveAgency(dto.UserId);
            }
            return DResult.FromResult(result);
        }

        public DResult RemoveAgency(string id, long userId)
        {
            var agency = UserAgencyRepository.Load(id);
            if (agency == null || agency.UserId != userId)
                return DResult.Error("不存在的机构关系！");
            if (agency.Status == (byte)UserAgencyStatus.Current)
                return DResult.Error("不能退出当前学校！");
            var result = UserAgencyRepository.Delete(agency);
            return DResult.FromResult(result);
        }

        public DResult UpdateRelation(UpdateRelationInputDto dto)
        {
            if (dto == null || dto.Id.IsNullOrEmpty())
                return DResult.Error("与学校关系数据异常！");
            var result = UserAgencyRepository.Update(new TU_UserAgency
            {
                StartTime = dto.Start,
                EndTime = dto.End
            }, t => t.Id == dto.Id, "StartTime", "EndTime");
            return DResult.FromResult(result);
        }

        public DResults<UserAgencyDto> AgencyList(long userId, DPage page)
        {
            if (userId <= 0)
                return DResult.Errors<UserAgencyDto>("用户不存在！");
            var count = UserAgencyRepository.Count(t => t.UserId == userId);
            var list = UserAgencyRepository.Where(t => t.UserId == userId)
                .OrderBy(t => (t.Status == 2 ? -1 : t.Status))
                .ThenByDescending(t => t.StartTime)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .Select(t => new
                {
                    t.Id,
                    t.AgencyId,
                    t.Status,
                    t.Stage,
                    t.StartTime,
                    t.EndTime
                }).ToList();
            var agencyIds = list.Select(t => t.AgencyId).Distinct().ToList();
            var agencyList = AgencyRepository.Where(t => agencyIds.Contains(t.Id))
                .Select(t => new { t.Id, t.AgencyName, t.AgencyLogo, t.CertificationLevel })
                .ToList()
                .ToDictionary(k => k.Id, v => v);
            var dtos = new List<UserAgencyDto>();
            foreach (var item in list)
            {
                var dto = new UserAgencyDto
                {
                    Id = item.Id,
                    AgencyId = item.AgencyId,
                    Start = item.StartTime,
                    End = item.EndTime,
                    Status = item.Status,
                    Stage = item.Stage
                };
                if (agencyList.ContainsKey(item.AgencyId))
                {
                    var agency = agencyList[item.AgencyId];
                    dto.AgencyName = agency.AgencyName;
                    dto.Level = agency.CertificationLevel;
                    dto.Logo = agency.AgencyLogo;
                }
                dtos.Add(dto);
            }
            return DResult.Succ(dtos, count);
        }

        public DResult<UserAgencyDto> CurrentAgency(long userId)
        {
            var cache = UserCache.Instance;
            var dto = cache.CurrentAgency(userId);
            if (dto != null)
                return DResult.Succ(dto);
            var agency =
                UserAgencyRepository.Where(t => t.UserId == userId && t.Status == (byte)UserAgencyStatus.Current)
                    .Select(t => new
                    {
                        t.Id,
                        t.AgencyId,
                        t.StartTime,
                        t.Stage
                    }).FirstOrDefault();
            if (agency == null)
                return DResult.Error<UserAgencyDto>("还没有当前机构");
            var dict = CurrentIocManager.Resolve<ISystemContract>().AgencyList(new[] { agency.AgencyId });
            dto = new UserAgencyDto
            {
                Id = agency.Id,
                AgencyId = agency.AgencyId,
                Start = agency.StartTime,
                Stage = agency.Stage
            };
            if (dict.ContainsKey(dto.AgencyId))
                dto.AgencyName = dict[dto.AgencyId];
            cache.SetAgency(userId, dto);
            return DResult.Succ(dto);
        }
        /// <summary>
        /// 根据机构ID获取用户信息
        /// </summary>
        /// <param name="agencyId">机构ID</param>
        /// <param name="userRole">角色</param>
        /// <param name="subjectId">科目</param>
        /// <returns></returns>
        public DResults<UserDto> LoadUsersByAgencyId(string agencyId, int userRole = -1, int subjectId = -1)
        {
            Expression<Func<TU_User, bool>> condition = u => u.Status == (byte)NormalStatus.Normal;
            if (userRole >= 0)
                condition = condition.And(u => (u.Role & userRole) > 0);
            condition = condition.And(u => u.AgencyId == agencyId);
            if (subjectId > 0)
            {
                condition = condition.And(w => w.SubjectID == subjectId);
            }
            var users = UserRepository.Where(condition)
                        .OrderByDescending(w => w.AddedAt)
                        .Select(w => new UserDto
                        {
                            Id = w.Id,
                            Avatar = w.HeadPhoto,
                            Name = w.TrueName,
                            Nick = w.NickName,
                            SubjectId = w.SubjectID ?? 0
                        }).ToList();
            users.Foreach(dto =>
            {
                if (string.IsNullOrEmpty(dto.Avatar))
                {
                    dto.Avatar = Consts.DefaultAvatar();
                }
                dto.SubjectName = SystemCache.Instance.SubjectName(dto.SubjectId);
            });
            var count = UserRepository.Count(condition);
            return DResult.Succ(users, count);
        }
    }
}
