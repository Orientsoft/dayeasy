
using DayEasy.Contract.Open.Helper;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Models.Open.User;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Contract.Open.Services
{
    public partial class OpenService
    {
        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }
        public IDayEasyRepository<TU_ThirdPlatform, string> ThirdPlatfomRepository { private get; set; }

        private DResult<long> AccountLogin(string account, string pwd)
        {
            if (string.IsNullOrWhiteSpace(account) ||
                (!account.As<IRegex>().IsEmail() && !account.As<IRegex>().IsMobile()))
                return DResult.Error<long>("登录帐号格式不正确！");
            if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 6)
                return DResult.Error<long>("登录密码长度不少于6位！");

            var user = UserRepository.SingleOrDefault(u =>
                u.Email == account ||
                (u.Mobile == account && (u.ValidationType & (byte)ValidationType.Mobile) > 0));
            if (user == null)
                return DResult.Error<long>("登录帐号未注册！");
            if (user.Status == (byte)UserStatus.Delete)
                return DResult.Error<long>("用户已被禁用！");

            var passwordMd5 = string.Concat(pwd, user.PasswordSalt).Md5();
            if (!string.Equals(passwordMd5, user.Password, StringComparison.InvariantCultureIgnoreCase))
            {
                return DResult.Error<long>("登录密码不正确！");
            }
            return DResult.Succ(user.Id);
        }

        private DResult<long> PlatformLogin(int type, string platformId)
        {
            var item = ThirdPlatfomRepository.SingleOrDefault(t => t.PlatformType == type && t.PlatformId == platformId);
            if (item == null)
                return DResult.Error<long>("该帐号尚未注册！");
            if (item.UserID <= 0)
                return DResult.Error<long>("该帐号尚未生成得一帐号！");
            return DResult.Succ(item.UserID);
        }

        public DResult BindStudent(long parentId, MUserBindChildInputDto dto)
        {
            var result = AccountLogin(dto.Account, dto.Password.DecodePwd());
            return !result.Status ? result : UserContract.BindChild(parentId, result.Data, (FamilyRelationType)dto.RelationType);
        }

        public DResult BindStudentByPlatfrom(long parentId, MUserBindChildPlatformInputDto dto)
        {
            var result = PlatformLogin(dto.PlatformType, dto.PlatformId);
            return !result.Status ? result : UserContract.BindChild(parentId, result.Data, (FamilyRelationType)dto.RelationType);
        }

        public DResult<StudentDto> Student(string code)
        {
            var userResult = UserContract.LoadByCode(code);
            if (!userResult.Status)
                return DResult.Error<StudentDto>(userResult.Message);
            if (userResult.Data == null || !userResult.Data.IsStudent())
            {
                return DResult.Error<StudentDto>("[{0}]不是学生身份".FormatWith(code));
            }
            var dto = new StudentDto
            {
                Id = userResult.Data.Id,
                Name = userResult.Data.Name
            };
            var groupResult = GroupContract.Groups(dto.Id, (int)GroupType.Class);
            if (!groupResult.Status || groupResult.Data == null || !groupResult.Data.Any())
            {
                return DResult.Error<StudentDto>("[{0}]没有班级".FormatWith(code));
            }
            dto.ClassList = groupResult.Data.ToDictionary(k => k.Id, v => v.Name);
            return DResult.Succ(dto);
        }

        public DResults<StudentClassDto> SearchStudent(string keyword)
        {
            var models =
                UserRepository.Where(
                    u =>
                        u.Status == (byte)UserStatus.Normal && (u.Role & (byte)UserRole.Student) > 0 &&
                        (u.UserCode == keyword || u.TrueName == keyword));
            var users = models.Select(u => new { u.Id, u.UserCode, u.TrueName }).ToList();
            var dtos = new List<StudentClassDto>();
            foreach (var user in users)
            {
                var results = GroupContract.Groups(user.Id, (int)GroupType.Class);
                if (!results.Status || results.Data == null || !results.Data.Any())
                    continue;
                foreach (var groupDto in results.Data)
                {
                    var classDto = groupDto as ClassGroupDto;
                    if (classDto == null)
                        continue;
                    var item = new StudentClassDto
                    {
                        Id = user.Id,
                        Code = user.UserCode,
                        Name = user.TrueName,
                        ClassId = classDto.Id,
                        ClassName = classDto.Name,
                        Agency = classDto.AgencyName
                    };
                    dtos.Add(item);
                }
            }
            return DResult.Succ(dtos, -1);
        }
    }
}
