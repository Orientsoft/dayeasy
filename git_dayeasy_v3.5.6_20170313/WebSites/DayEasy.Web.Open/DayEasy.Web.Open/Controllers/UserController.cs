using DayEasy.AutoMapper;
using DayEasy.Contract.Open.Contracts;
using DayEasy.Contract.Open.Helper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Models.Open.User;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 用户相关接口 </summary>
    [DApi]
    public class UserController : DApiController
    {
        private readonly IOpenContract _openContract;
        public UserController(IUserContract userContract, IOpenContract openContract)
            : base(userContract)
        {
            _openContract = openContract;
        }

        /// <summary> 用户登录 </summary>
        [HttpPost]
        public DResult<MLoginDto> Login(LoginDto dto)
        {
            dto.Comefrom = (Comefrom)PartnerBusi.Instance.Comefrom;
            dto.Partner = PartnerBusi.Instance.Partner;
            if (dto.IsEncrypt)
                dto.Password = dto.Password.DecodePwd();
            var result = UserContract.Login(dto);
            if (!result.Status) return DResult.Error<MLoginDto>(result.Message);
            return DResult.Succ(new MLoginDto
            {
                Token = result.Data,
                User = UserContract.Load(result.Data, dto.Comefrom).MapTo<MUserDto>()
            });
        }

        /// <summary> 第三方登录 </summary>
        [HttpPost]
        public DResult<PlatformLoginResultDto> PlatformLogin(PlatformDto dto)
        {
            return UserContract.Login(dto, (Comefrom)PartnerBusi.Instance.Comefrom, PartnerBusi.Instance.Partner);
        }

        /// <summary> 绑定老帐号 </summary>
        [HttpPost]
        public DResult PlatformBind(PlatformBindDto dto)
        {
            var pwd = dto.Password.DecodePwd();
            return UserContract.AccountBind(dto.PlatId, dto.Account, pwd);
        }

        /// <summary> 创建新帐号 </summary>
        [HttpPost]
        public DResult<string> PlatformCreate(string platId)
        {
            return UserContract.CreatePlatAccount(platId);
        }

        /// <summary> 用户信息 </summary>
        [HttpGet]
        [DApiAuthorize]
        public DResult<MUserDto> Load(int comefrom = -1)
        {
            var user = CurrentUser.MapTo<MUserDto>();
            return DResult.Succ(user);
        }

        /// <summary> 修改用户资料 </summary>
        [HttpPost]
        [DApiAuthorize]
        public DResult Update([FromBody] UserUpdateDto dto)
        {
            var result = UserContract.Update(new UserDto
            {
                Id = UserId,
                Avatar = dto.Avatar,
                Nick = dto.Nick
            });
            return result;
        }

        /// <summary> 修改登录密码 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [DApiAuthorize]
        public DResult ChangePwd(UserChangePwdDto dto)
        {
            if (dto.IsEncrypt)
            {
                dto.OldPwd = dto.OldPwd.DecodePwd();
                dto.Password = dto.Password.DecodePwd();
            }
            return UserContract.ChangPwd(UserId, dto.OldPwd, dto.Password, dto.Password);
        }

        /// <summary> 重置密码 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public DResult ResetPwd(UserResetPwdDto dto)
        {
            var key = MessageController.VcodeCacheKey.FormatWith(dto.Mobile);
            if (dto.Vcode.IsNullOrEmpty() || CacheHelper.Get<string>(key) != dto.Vcode)
                return DResult.Error<long>("短信验证码验证失败！");
            if (dto.IsEncrypt)
            {
                dto.Password = dto.Password.DecodePwd();
            }
            return UserContract.ChangPwd(dto.Mobile, dto.Password, dto.Password);
        }

        /// <summary> 手机号码注册 </summary>
        /// <param name="dto"></param>
        [HttpPost]
        public DResult<long> Regist(UserRegistDto dto)
        {
            if (dto == null || dto.Mobile.IsNullOrEmpty() || dto.Password.IsNullOrEmpty())
                return DResult.Error<long>("短信验证码验证失败！");
            if (dto.Role == (int)UserRole.Teacher && dto.SubjectId == 0)
                return DResult.Error<long>("教师用户请先选择科目！");
            if (!Enum.IsDefined(typeof(UserRole), (byte)dto.Role))
                return DResult.Error<long>("用户角色异常！");
            var key = MessageController.VcodeCacheKey.FormatWith(dto.Mobile);
            if (dto.Vcode.IsNullOrEmpty() || CacheHelper.Get<string>(key) != dto.Vcode)
                return DResult.Error<long>("短信验证码验证失败！");
            if (dto.IsEncrypt)
            {
                dto.Password = dto.Password.DecodePwd();
            }
            return UserContract.Regist(new RegistUserDto
            {
                Mobile = dto.Mobile,
                Password = dto.Password,
                Role = (UserRole)dto.Role,
                Subject = dto.SubjectId,
                ValidationType = ValidationType.Mobile
            });
        }

        /// <summary> 用户选择身份 </summary>
        [HttpPost]
        [DApiAuthorize]
        public DResult BindRole(UserChooseRoleDto dto)
        {
            if (dto.Role == (int)UserRole.Teacher && dto.SubjectId == 0)
                return DResult.Error<long>("教师用户请先选择科目！");
            if (!new[] { (int)UserRole.Student, (int)UserRole.Parents, (int)UserRole.Teacher }.Contains(dto.Role))
                return DResult.Error<long>("所选用户角色异常！");
            var user = new UserDto
            {
                Id = UserId,
                Role = (byte)dto.Role
            };
            if (dto.Role == (int)UserRole.Teacher)
                user.SubjectId = dto.SubjectId;
            return UserContract.Update(user);
        }

        /// <summary> 绑定学生 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Parents)]
        public DResult BindStudent(MUserBindChildInputDto dto)
        {
            return _openContract.BindStudent(UserId, dto);
        }

        /// <summary> 绑定学生 - 第三方登录 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Parents)]
        public DResult BindStudentByPlatfrom(MUserBindChildPlatformInputDto dto)
        {
            return _openContract.BindStudentByPlatfrom(UserId, dto);
        }

        /// <summary> 解除绑定 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Parents)]
        public DResult UnBindStudent(long studentId)
        {
            return UserContract.CancelBindRelation(UserId, studentId);
        }

        /// <summary> 家长绑定的学生列表 </summary>
        [DApiAuthorize(UserRole.Parents)]
        public DResults<MRelationUserDto> Children()
        {
            var list = new List<MRelationUserDto>();
            var children = UserContract.Children(UserId);
            if (!children.IsNullOrEmpty())
            {
                list = children.Select(t => new MRelationUserDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Avatar = t.Avatar,
                    Account = t.Account,
                    RelationType = t.RelationType
                }).ToList();
            }
            return DResult.Succ<MRelationUserDto>(list);
        }

        /// <summary> 学生绑定的家长 </summary>
        [HttpPost]
        [DApiAuthorize(UserRole.Student)]
        public DResults<MRelationUserDto> Parents()
        {
            var list = new List<MRelationUserDto>();
            var parents = UserContract.Parents(UserId);
            if (!parents.IsNullOrEmpty())
            {
                list = parents.Select(t => new MRelationUserDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Avatar = t.Avatar,
                    Account = t.Account,
                    RelationType = t.RelationType
                }).ToList();
            }
            return DResult.Succ<MRelationUserDto>(list);
        }

        /// <summary> 学生信息 </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult<StudentDto> Student(string code)
        {
            return _openContract.Student(code);
        }

        /// <summary> 学生信息 </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [DApiAuthorize(UserRole.Teacher)]
        public DResults<StudentClassDto> SearchStudent(string keyword)
        {
            return _openContract.SearchStudent(keyword);
        }
    }
}
