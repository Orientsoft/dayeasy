
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.User.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public IDayEasyRepository<TU_AdminUserRole, int> AdminUserRoleRepository { private get; set; }

        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }

        public IDayEasyRepository<TL_UserLog, long> UserLogRepository { private get; set; }

        public IDayEasyRepository<TU_UserToken, string> UserTokenRepository { private get; set; }


        public long AdminRole(long userId)
        {
            var role = AdminUserRoleRepository.FirstOrDefault(t => t.UserId == userId);
            if (role == null)
                return -1L;
            return role.UserRole;
        }

        public DResult SetAdminRole(long userId, long adminRole)
        {
            var result = AdminUserRoleRepository.Update(new TU_AdminUserRole
            {
                UserRole = adminRole
            }, r => r.UserId == userId, "UserRole");
            return DResult.FromResult(result);
        }

        public DResults<TU_User> UserSearch(UserSearchDto searchDto)
        {
            Expression<Func<TU_User, bool>> condition = u => true;
            if (searchDto.Role == 0)
                condition = condition.And(u => u.Role == 0);
            else if (searchDto.Role > 0)
                condition = condition.And(u => (u.Role & searchDto.Role) > 0);
            if (searchDto.ValidationType >= 0)
                condition = condition.And(u => (u.ValidationType & searchDto.ValidationType) > 0);
            if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                var key = searchDto.Keyword.Trim();
                condition = condition.And(u => u.UserCode == key
                                               || (u.Email != null && u.Email.Contains(key))
                                               || (u.Mobile != null && u.Mobile.Contains(key))
                                               || (u.TrueName != null && u.TrueName.Contains(key))
                                               || (u.NickName != null && u.NickName.Contains(key)));
            }
            var count = UserRepository.Count(condition);
            var list = UserRepository.Where(condition)
                .OrderByDescending(u => u.AddedAt)
                .Skip(searchDto.Page * searchDto.Size)
                .Take(searchDto.Size)
                .ToList();
            return DResult.Succ(list, count);
        }

        public DResults<TU_User> UserSearch(List<long> userIds)
        {
            if (userIds == null || userIds.Count < 1)
                return DResult.Errors<TU_User>("参数错误！");
            var result = UserRepository.Where(u => userIds.Contains(u.Id)).ToList();

            return DResult.Succ(result, result.Count);
        }

        public DResult UserDelete(long userId)
        {
            if (userId <= 0)
                return DResult.Error("用户ID无效！");
            var user = UserRepository.Load(userId);
            if (user == null)
                return DResult.Error("用户ID不存在！");
            if (user.Status == (byte)UserStatus.Delete)
            {
                if (string.IsNullOrWhiteSpace(user.Email) && string.IsNullOrWhiteSpace(user.NickName))
                    user.Status = (byte)UserStatus.UnBind;
                else
                    user.Status = (byte)UserStatus.Normal;
            }
            else
            {
                user.Status = (byte)NormalStatus.Delete;
            }
            var result = UserRepository.Update(u => new { u.Status }, user);
            return DResult.FromResult(result);
        }

        public DResult ResetPassword(long userId)
        {
            if (userId <= 0)
                return DResult.Error("用户ID无效！");
            var user = UserRepository.Load(userId);
            if (user == null)
                return DResult.Error("用户ID不存在！");
            //if (string.IsNullOrWhiteSpace(user.Email) && string.IsNullOrWhiteSpace(user.Mobile))
            //    return DResult.Error("用户帐号未绑定！");
            const string initPwd = "123456";
            var salt = IdHelper.Instance.Guid32.Substring(5, 16);
            user.Password = (initPwd + salt).Md5().ToUpper();
            user.PasswordSalt = salt;
            var result = UserRepository.Update(u => new
            {
                u.PasswordSalt,
                u.Password
            }, user);
            return DResult.FromResult(result);
        }

        public DResult UserEdit(long userId, string realName)
        {
            if (userId < 1) return DResult.Error("用户ID无效");
            if (realName.IsNullOrEmpty()) return DResult.Error("真实姓名不能为空");
            var user = UserRepository.Load(userId);
            if (user == null) return DResult.Error("用户不存在");
            user.TrueName = realName;
            var result = UserRepository.Update(u => new { u.TrueName }, user);
            if (result > 0)
            {
                UserContract.ResetCache(userId);
            }
            return DResult.FromResult(result);
        }

        public DResult<UserActiveDto> UserActiveInfo(long userId)
        {
            var user = UserRepository.Load(userId);
            if (user == null)
                return DResult.Error<UserActiveDto>("用户不存在！");
            var dto = user.MapTo<UserDto>().MapTo<UserActiveDto>();
            dto.RegistIp = user.AddedIp;
            dto.RegistTime = user.AddedAt;
            dto.LastLoginTime = user.LastLoginAt;
            dto.ValidationType = user.ValidationType;
            Expression<Func<TL_UserLog, bool>> condition = l => l.UserId == userId;
            dto.LoginCount = UserLogRepository.Count(condition.And(l => l.LogLevel == (byte)LogLevel.Info));
            dto.LoginErrorCount = UserLogRepository.Count(condition.And(l => l.LogLevel == (byte)LogLevel.Error));
            var now = Clock.Now;
            var month = new DateTime(now.Year, now.Month, 1);
            var lastMonth = month.AddMonths(1);
            dto.LoginCountInMonth =
                UserLogRepository.Count(
                    condition.And(
                        l => l.LogLevel == (byte)LogLevel.Info && l.AddedAt >= month && l.AddedAt < lastMonth));
            dto.LoginErrorCountInMonth =
                UserLogRepository.Count(
                    condition.And(
                        l => l.LogLevel == (byte)LogLevel.Error && l.AddedAt >= month && l.AddedAt < lastMonth));
            dto.Tokens = UserTokenRepository.Where(t => t.UserID == userId)
                .Select(t => new TokenInfo
                {
                    Comefrom = (Comefrom)t.Comefrom,
                    Ip = t.AddedIp,
                    Time = t.AddedAt
                }).ToList();
            dto.LoginErrors =
                UserLogRepository.Where(l => l.UserId == userId && l.LogLevel == (byte)LogLevel.Error)
                    .Select(l => new LoginInfo
                    {
                        Status = (l.LogLevel == (byte)LogLevel.Info),
                        ErrorMsg = l.LogDetail,
                        Ip = l.AddedIp,
                        Time = l.AddedAt
                    })
                    .OrderByDescending(t => t.Time)
                    .Take(3).ToList();
            var groupResult = GroupContract.Groups(userId);
            if (groupResult.Status && groupResult.Data != null)
                dto.Groups = groupResult.Data.ToList();
            if (dto.IsTeacher())
            {
                dto.WorkCount = UsageRepository.Count(t => t.UserId == dto.Id);
            }
            else if (dto.IsStudent())
            {
                dto.WorkCount =
                    CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>().Count(t => t.StudentID == dto.Id);
            }
            return DResult.Succ(dto);
        }

        public DResults<UserManagerDto> ManagerSearch(ManagerSearchDto searchDto)
        {
            if (searchDto == null)
                return DResult.Errors<UserManagerDto>("参数错误！");
            Expression<Func<TU_User, bool>> condition = u =>
                u.Status == (byte)UserStatus.Normal && (u.Role & (byte)UserRole.SystemManager) > 0;
            if (searchDto.Keyword.IsNotNullOrEmpty())
            {
                var key = searchDto.Keyword.Trim();
                condition = condition.And(u =>
                    u.Email.Contains(key)
                    || u.TrueName.Contains(key)
                    || u.NickName.Contains(key)
                    || u.UserCode == key);
            }

            int totalCount = UserRepository.Count(condition);
            var users = UserRepository.Where(condition)
                .Join(AdminUserRoleRepository.Table, u => u.Id, m => m.UserId, (u, m) => new { u, role = m.UserRole })
                .OrderByDescending(t => t.u.AddedAt)
                .Skip(searchDto.Page * searchDto.Size)
                .Take(searchDto.Size)
                .Select(t => new UserManagerDto
                {
                    UserId = t.u.Id,
                    Email = t.u.Email,
                    NickName = t.u.NickName,
                    TrueName = t.u.TrueName,
                    Role = t.role
                }).ToList();
            return DResult.Succ(users, totalCount);
        }

        public DResult SetManager(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return DResult.Error("邮箱或得一号为空！");
            TU_User user;
            if (keyword.Contains("@"))
                user = UserRepository.FirstOrDefault(t => t.Email == keyword && t.Status == (byte)UserStatus.Normal);
            else
                user = UserRepository.SingleOrDefault(t => t.UserCode == keyword && t.Status == (byte)UserStatus.Normal);
            if (user == null)
                return DResult.Error("用户未找到！");
            if ((user.Role & (byte)UserRole.SystemManager) > 0)
                return DResult.Error("用户已经是管理员！");
            user.Role |= (byte)UserRole.SystemManager;
            var result = UnitOfWork.Transaction(() =>
            {
                UserRepository.Update(u => new { u.Role }, user);
                AdminUserRoleRepository.Insert(new TU_AdminUserRole
                {
                    UserId = user.Id,
                    UserRole = 0
                });
            });
            return DResult.FromResult(result);
        }

        public DResult RemoveManager(long userId)
        {
            if (userId <= 0)
                return DResult.Error("用户ID不存在！");
            var user =
                UserRepository.FirstOrDefault(
                    u =>
                        u.Id == userId
                        && (u.Role & (byte)UserRole.SystemManager) > 0
                        && u.Status == (byte)UserStatus.Normal);
            if (user == null)
                return DResult.Error("用户不存在！");
            var result = UnitOfWork.Transaction(() =>
            {
                user.Role ^= (byte)UserRole.SystemManager;
                UserRepository.Update(u => new { u.Role }, user);
                AdminUserRoleRepository.Delete(a => a.UserId == userId);
            });
            return DResult.FromResult(result);
        }

        public DResult UpdateManager(long userId, long role)
        {
            if (userId <= 0)
                return DResult.Error("用户ID不存在！");
            if (role < 0)
                return DResult.Error("权限值不正确！");
            var user =
                UserRepository.FirstOrDefault(
                    u =>
                        u.Id == userId
                        && (u.Role & (byte)UserRole.SystemManager) > 0
                        && u.Status == (byte)UserStatus.Normal);
            if (user == null)
                return DResult.Error("用户不存在！");
            var result = AdminUserRoleRepository.Update(new TU_AdminUserRole
            {
                UserRole = role
            }, a => a.UserId == userId, "UserRole");
            return DResult.FromResult(result);
        }

        public DResult ImportStudentNums(Dictionary<string, string> studentDict)
        {
            if (studentDict == null || !studentDict.Any())
                return DResult.Error("没有任何学生数据！");
            var result = UserRepository.UnitOfWork.Transaction(() =>
            {
                foreach (var student in studentDict)
                {
                    string code = student.Key,
                        num = student.Value;

                    UserRepository.Update(new TU_User
                    {
                        StudentNum = num
                    }, u => u.UserCode == code && (u.Role & (byte)UserRole.Student) > 0, "StudentNum");
                }
            });
            if (result > 0)
            {
                var userIds =
                    UserRepository.Where(u => studentDict.Keys.Contains(u.UserCode)).Select(u => u.Id).ToList();
                foreach (var userId in userIds)
                {
                    UserCache.Instance.Remove(userId);
                }
            }
            return DResult.FromResult(result);
        }

        public DResult CertificateUser(long userId)
        {
            var result = UserRepository.Update(new TU_User
            {
                CertificationLevel = (byte)CertificationLevel.Official
            }, a => a.Id == userId, "CertificationLevel");
            return DResult.FromResult(result);
        }
    }
}
