using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace DayEasy.Contracts.Management.Dto
{
    /// <summary> 用户活跃信息业务类 </summary>
    public class UserActiveDto : UserDto
    {
        public string RegistIp { get; set; }
        public DateTime RegistTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public int ValidationType { get; set; }
        public int LoginCount { get; set; }
        public int LoginErrorCount { get; set; }
        public int LoginCountInMonth { get; set; }
        public int LoginErrorCountInMonth { get; set; }

        public List<TokenInfo> Tokens { get; set; }
        public List<LoginInfo> LoginErrors { get; set; }
        public List<GroupDto> Groups { get; set; }
        public int WorkCount { get; set; }

        public UserActiveDto()
        {
            Tokens = new List<TokenInfo>();
            LoginErrors = new List<LoginInfo>();
            Groups = new List<GroupDto>();
        }
    }

    public class TokenInfo : DDto
    {
        public Comefrom Comefrom { get; set; }
        public string Ip { get; set; }
        public DateTime Time { get; set; }
    }

    public class LoginInfo : DDto
    {
        public bool Status { get; set; }
        public string Ip { get; set; }
        public DateTime Time { get; set; }
        public string ErrorMsg { get; set; }
    }
}
