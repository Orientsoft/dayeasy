﻿using System;
using DayEasy.Core;
using DayEasy.Core.Domain.Repositories;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Wiki.Models;

namespace DayEasy.Web.Wiki.Contracts.Services
{
    public class UserService : DService, IUserContract
    {
        public UserService(WikiDbContext unitOfWork)
            : base(unitOfWork)
        {
        }

        public IRepository<WikiDbContext, User, string> UserRepository { private get; set; }

        public string CreateUser(string account, string pwd, string name, int role)
        {
            if (UserRepository.Exists(u => u.Account == account))
                return string.Empty;
            var user = new User
            {
                Id = CombHelper.Guid16,
                Account = account,
                Password = pwd.Md5().ToLower(),
                Name = name,
                Role = role
            };
            return UserRepository.Insert(user);
        }

        public DResult<User> Login(string account, string pwd)
        {
            var user = UserRepository.SingleOrDefault(t => t.Account == account);
            if (user == null)
                return DResult.Error<User>("帐号不存在！");
            if (!string.Equals(user.Password, pwd.Md5(), StringComparison.CurrentCultureIgnoreCase))
                return DResult.Error<User>("登录密码错误！");
            return DResult.Succ(user);
        }
    }
}