
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.User.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

namespace DayEasy.User.Services
{
    public partial class UserService
    {
        /// <summary> 从数据库加载用户信息 </summary>
        /// <param name="token"></param>
        /// <param name="comefrom"></param>
        /// <returns></returns>
        private UserDto LoadFromDb(string token, Comefrom comefrom)
        {
            var dt = DateTime.Now;
            var userToken =
                TokenRepository.SingleOrDefault(
                    t =>
                        t.Id == token && t.Comefrom == (byte)comefrom &&
                        (!t.ExpireTime.HasValue || t.ExpireTime.Value > dt));
            if (userToken == null)
                return null;
            var user = UserRepository.SingleOrDefault(u => u.Id == userToken.UserID).ToDto();
            user.ExpireTime = userToken.ExpireTime;
            return user;
        }

        /// <summary> 设置登录状态 </summary>
        /// <param name="user"></param>
        /// <param name="comefrom"></param>
        /// <param name="partner"></param>
        /// <returns></returns>
        private string SetLoginStatus(TU_User user, Comefrom comefrom, string partner = "")
        {
            if (user == null)
                return string.Empty;
            var oldToken = TokenRepository.Where(t =>
                t.UserID == user.Id && t.Comefrom == (byte)comefrom)
                .ToList();
            if (oldToken.Any())
            {
                oldToken.ForEach(t =>
                {
                    //移除之前的token
                    UserCache.Instance.Remove(t.Id, comefrom);
                    TokenRepository.Delete(t);
                });
            }

            var token = new TU_UserToken
            {
                Id = IdHelper.Instance.Guid32,
                UserID = user.Id,
                Comefrom = (byte)comefrom,
                AddedAt = Clock.Now,
                AddedIp = Utils.GetRealIp(),
                SystemEnvironment = Utils.SystemEnvironment()
            };
            if (comefrom == Comefrom.Web)
            {
                CookieHelper.Set(Consts.UserCookieName, token.Id, 0, Consts.Config.CookieDomain);
            }
            else
            {
                token.ExpireTime = Clock.Now.AddMonths(1);
            }
            UnitOfWork.Transaction(() =>
            {
                TokenRepository.Insert(token);
                LogContract.GenerateLoginLog(user.Id, LogLevel.Info, user.Email, string.Empty, partner);
                //更新登录时间
                user.LastLoginAt = DateTime.Now;
                user.LastLoginIP = Utils.GetRealIp();
                UserRepository.Update(u => new { u.LastLoginAt, u.LastLoginIP }, user);
            });

            //登录成功，清理用户相关缓存
            UserCache.Instance.RemoveApps(user.Id);
            return token.Id;
        }

        private DResult<TU_User> AccountLogin(string account, string pwd, string partner = "")
        {
            if (string.IsNullOrWhiteSpace(account))
                return DResult.Error<TU_User>("请输入登录帐号！");
            var reg = account.As<IRegex>();
            if (!reg.IsEmail() && !reg.IsMobile() && !reg.IsMatch("^[\\d]{5,}$"))
                return DResult.Error<TU_User>("登录帐号格式不正确！");

            if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 6)
                return DResult.Error<TU_User>("登录密码长度不少于6位！");

            var user = UserRepository.SingleOrDefault(u =>
                u.Email == account ||
                (u.Mobile == account && (u.ValidationType & (byte)ValidationType.Mobile) > 0)
                || u.UserCode == account);
            if (user == null)
                return DResult.Error<TU_User>((reg.IsEmail() || reg.IsMobile()) ? "登录帐号未注册！" : "得一号不存在！");
            if (user.Status == (byte)UserStatus.Delete)
                return DResult.Error<TU_User>("用户已被禁用！");
            if (string.IsNullOrWhiteSpace(user.Password))
                return DResult.Error<TU_User>("帐号还未设置密码！");
            var passwordMd5 = string.Concat(pwd, user.PasswordSalt).Md5();
            if (!string.Equals(passwordMd5, user.Password, StringComparison.InvariantCultureIgnoreCase))
            {
                const string reason = "登录密码不正确！";
                LogContract.GenerateLoginLog(user.Id, LogLevel.Error, account, reason, partner);
                return DResult.Error<TU_User>(reason);
            }
            return DResult.Succ(user);
        }

        /// <summary> 密码格式检测 </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private DResult CheckPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return DResult.Error("密码不能为空！");
            if (!password.As<IRegex>().IsMatch("[0-9a-z_\\.\\@]{6,20}", RegexOptions.IgnoreCase))
                return DResult.Error("密码长度为6-20位数字、字母或[_.@]！");
            return DResult.Success;
        }

        private DResult<long> UpdatePwd(TU_User model, string password)
        {
            var salt = IdHelper.Instance.Guid32.Substring(5, 16);
            model.Password = (password + salt).Md5().ToUpper();
            model.PasswordSalt = salt;

            var result = UserRepository.Update(u => new
            {
                u.Password,
                u.PasswordSalt
            }, model);

            if (result > 0)
            {
                UserCache.Instance.Remove(model.Id);
            }

            return result > 0
                ? DResult.Succ(model.Id)
                : DResult.Error<long>("修改失败！");
        }


        /// <summary> 从数据库读取用户应用 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private List<ApplicationDto> UserApplicationsFromDb(long userId)
        {
            var user = Load(userId);
            if (user == null)
                return new List<ApplicationDto>();

            var apps = new List<TS_Application>();
            //角色App
            var roleApps = ApplicationRepository.Where(
                t =>
                    (t.AppRoles & user.Role) > 0 && t.AppType == (byte)ApplicationType.Normal &&
                    t.Status == (byte)NormalStatus.Normal)
                .OrderBy(a => a.Sort);
            if (roleApps.Any())
                apps.AddRange(roleApps);
            //用户App
            if (UserApplicationRepository.Exists(a => a.UserID == userId && a.Status == (byte)NormalStatus.Normal))
            {
                var userApps =
                    UserApplicationRepository.Where(a =>
                        a.UserID == userId && a.Status == (byte)NormalStatus.Normal)
                        .Include(a => a.TS_Application)
                        .Where(a => a.TS_Application.Status == (byte)NormalStatus.Normal)
                        .Select(a => a.TS_Application)
                        .OrderBy(a => a.Sort);
                if (userApps.Any())
                    apps.AddRange(userApps);
            }
            var dtos = apps.Distinct(t => t.Id).OrderBy(a => a.Sort).MapTo<List<ApplicationDto>>();
            dtos.ForEach(d =>
            {
                d.Url = d.IsSLD
                    ? string.Concat("http://", d.Url, Consts.Config.CookieDomain)
                    : string.Concat(Consts.Config.AppSite, "/", d.Url);
            });
            return dtos;
        }

        private string UserAccount(UserDto user)
        {
            if (user.Email.IsNotNullOrEmpty())
                return user.Email;
            if (user.Nick.IsNotNullOrEmpty())
                return user.Nick;
            if (user.Mobile.IsNotNullOrEmpty() && (user.ValidationType & (byte)ValidationType.Mobile) > 0)
                return user.Mobile;
            return user.Name ?? "游客";
        }
    }
}
