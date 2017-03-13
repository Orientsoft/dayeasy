using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
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

namespace DayEasy.User.Services
{
    /// <summary> 用户相关基础业务 </summary>
    public partial class UserService : DayEasyService, IUserContract
    {
        public UserService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }
        public IDayEasyRepository<TU_UserToken> TokenRepository { private get; set; }
        public IDayEasyRepository<TS_Subject, int> SubjectRepository { private get; set; }

        public ILogContract LogContract { private get; set; }

        #region 加载用户信息

        public UserDto Load(long userId, bool fromCache = true)
        {
            return fromCache ? LoadFromCache(userId) : UserRepository.Load(userId).ToDto();
        }

        public DResult<UserDto> LoadByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 5)
                return DResult.Error<UserDto>("得一号格式不正确！");
            var user = UserRepository.SingleOrDefault(u => u.UserCode == code);
            if (user == null)
                return DResult.Error<UserDto>("得一号不存在！");
            var dto = user.ToDto();
            return dto == null
                ? DResult.Error<UserDto>("用户信息不正确！")
                : DResult.Succ(dto);
        }

        /// <summary> 加载用户集合 </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public List<UserDto> LoadList(IEnumerable<long> userIds)
        {
            var users = UserRepository.Where(u => userIds.Contains(u.Id))
                .ToArray().Select(u => u.ToDto()).ToList();
            return users;
        }

        public List<DUserDto> LoadDList(IEnumerable<long> userIds, bool showNick = false)
        {
            var users = UserRepository.Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    Name = u.TrueName,
                    Nick = u.NickName,
                    u.Email,
                    u.Mobile,
                    Avatar = u.HeadPhoto
                }).ToList();
            var list = new List<DUserDto>();
            users.ForEach(dto =>
            {
                var item = new DUserDto
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Avatar = dto.Avatar.IsNullOrEmpty() ? Consts.DefaultAvatar() : dto.Avatar,
                    Nick = dto.Nick
                };
                if (showNick)
                {
                    if (dto.Nick.IsNullOrEmpty())
                    {
                        item.Nick = item.Name = (dto.Email.IsNotNullOrEmpty()
                            ? RegexHelper.EmailNick(dto.Email)
                            : RegexHelper.MobileNick(dto.Mobile));
                    }
                }
                list.Add(item);
            });
            return list;
        }

        public Dictionary<string, DUserDto> LoadDListByCodes(IEnumerable<string> codes)
        {
            var users = UserRepository.Where(u => codes.Contains(u.UserCode))
                .Select(u => new { u.Id, u.TrueName, u.NickName, u.HeadPhoto, u.UserCode })
                .ToDictionary(k => k.UserCode, u => new DUserDto
                {
                    Id = u.Id,
                    Name = u.TrueName,
                    Avatar = u.HeadPhoto
                });
            users.Values.Foreach(dto =>
            {
                if (string.IsNullOrWhiteSpace(dto.Avatar))
                    dto.Avatar = Consts.DefaultAvatar();
            });
            return users;
        }

        public Dictionary<long, DUserDto> LoadListDictUser(IEnumerable<long> userIds)
        {
            return LoadDList(userIds).ToDictionary(k => k.Id, v => v);
        }

        public UserDto Load(string token, Comefrom comefrom = Comefrom.Web, bool fromCache = true)
        {
            if (!fromCache)
                return LoadFromDb(token, comefrom);
            var cache = UserCache.Instance;
            var user = cache.Get(token, (byte)comefrom);
            if (user != null) return user;
            user = LoadFromDb(token, comefrom);
            if (user != null)
                cache.Set(user, token, (byte)comefrom);
            return user;
        }

        public IDictionary<string, byte> UserTokens(long userId)
        {
            return TokenRepository.Where(t => t.UserID == userId)
                .ToDictionary(k => k.Id, v => v.Comefrom);
        }

        public Dictionary<long, string> UserNames(IEnumerable<long> userIds, bool isNick = false)
        {
            return UserRepository.List(userIds)
                .Select(u => new { u.Id, u.NickName, u.TrueName, u.Email })
                .ToList()
                .ToDictionary(k => k.Id,
                    v => (isNick ? string.IsNullOrWhiteSpace(v.NickName) ? v.Email : v.NickName : v.TrueName));
        }

        public string DisplayName(UserDto user)
        {
            if (user == null)
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(user.Nick))
                return user.Nick;
            return !string.IsNullOrWhiteSpace(user.Email) ? user.Email : user.Name;
        }

        public DResult CheckAccount(string account)
        {
            if (string.IsNullOrWhiteSpace(account) || account.Length < 6)
                return DResult.Error("帐号格式不对！");
            account = account.Trim().ToLower();
            var exists = false;
            var reg = account.As<IRegex>();
            if (reg.IsEmail())
                exists = UserRepository.Exists(u => u.Email == account);
            if (reg.IsMobile())
                exists =
                    UserRepository.Exists(
                        u => u.Mobile == account && (u.ValidationType & (byte)ValidationType.Mobile) > 0);
            return exists ? DResult.Error("帐号已注册！") : DResult.Success;
        }

        public bool ExistsAccount(string account)
        {
            if (string.IsNullOrWhiteSpace(account) || account.Length < 6)
                return false;
            var exists = false;
            var reg = account.As<IRegex>();
            if (reg.IsEmail())
                exists = UserRepository.Exists(u => u.Email == account && u.Status == (byte)NormalStatus.Normal);
            if (reg.IsMobile())
                exists = UserRepository.Exists(u => u.Mobile == account && u.Status == (byte)NormalStatus.Normal);
            return exists;
        }

        public DResult<long> ChangPwd(string account, string password, string confirmPwd)
        {
            if (string.IsNullOrEmpty(account))
            {
                return DResult.Error<long>("该账户不存在！");
            }
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPwd))
            {
                return DResult.Error<long>("请输入密码！");
            }
            password = password.Trim();
            confirmPwd = confirmPwd.Trim();
            if (password != confirmPwd)
            {
                return DResult.Error<long>("两次输入的密码不一致！");
            }
            var checkResult = CheckPassword(password);
            if (!checkResult.Status)
                return DResult.Error<long>(checkResult.Message);

            TU_User model = null;
            if (account.As<IRegex>().IsEmail())
                model = UserRepository.SingleOrDefault(u => u.Email == account && u.Status == (byte)UserStatus.Normal);
            else if (account.As<IRegex>().IsMobile())
                model = UserRepository.SingleOrDefault(u => u.Mobile == account && u.Status == (byte)UserStatus.Normal);

            return model == null
                ? DResult.Error<long>("该账户不存在！")
                : UpdatePwd(model, password);
        }

        public DResult<long> ChangPwd(long userId, string oldPwd, string password, string confirmPwd)
        {
            if (userId <= 0)
                return DResult.Error<long>("该账户不存在！");

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPwd))
                return DResult.Error<long>("请输入密码！");

            password = password.Trim();
            confirmPwd = confirmPwd.Trim();
            if (password != confirmPwd)
                return DResult.Error<long>("两次输入的密码不一致！");

            var checkResult = CheckPassword(password);
            if (!checkResult.Status)
                return DResult.Error<long>(checkResult.Message);

            var model = UserRepository.SingleOrDefault(u => u.Id == userId && u.Status == (byte)UserStatus.Normal);
            if (model == null)
                return DResult.Error<long>("该账户不存在！");

            if (model.ValidationType != (byte)ValidationType.Third && !string.IsNullOrEmpty(model.Password))
            {
                var md5 = (oldPwd + model.PasswordSalt).Md5().ToUpper();
                if (!string.Equals(model.Password, md5, StringComparison.CurrentCultureIgnoreCase))
                    return DResult.Error<long>("原密码不匹配！");
            }
            return UpdatePwd(model, password);
        }

        public DResult<string> AutoLogin(long userId, Comefrom comefrom = Comefrom.Web)
        {
            var user = UserRepository.SingleOrDefault(u => u.Id == userId && u.Status == (byte)NormalStatus.Normal);
            if (user == null)
                return DResult.Error<string>("用户不存在！");
            var token = SetLoginStatus(user, comefrom);
            return DResult.Succ(token);
        }

        //组装注册用户数据
        public DResult<TU_User> CreateTuUser(RegistUserDto dto)
        {
            if (dto == null)
                return DResult.Error<TU_User>("参错错误");
            var ckEmail = dto.Email.IsNotNullOrEmpty() && dto.Email.As<IRegex>().IsEmail();
            var ckMobile = dto.Mobile.IsNotNullOrEmpty() && dto.Mobile.As<IRegex>().IsMobile();
            if (dto.ValidationType != ValidationType.Third)
            {
                //非第三方登录验证
                if (!ckEmail && !ckMobile)
                    return DResult.Error<TU_User>("帐号格式不正确");
                if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                    return DResult.Error<TU_User>("请输入密码");
                if (ckEmail && UserRepository.Exists(u => u.Email == dto.Email))
                    return DResult.Error<TU_User>("帐号已被使用");
                if (ckMobile && UserRepository.Exists(u => u.Mobile == dto.Mobile))
                    return DResult.Error<TU_User>("帐号已被使用");
            }
            var newUser = new TU_User
            {
                Id = IdHelper.Instance.LongId,
                UserCode = UserCodeManager.Instance.Code(),
                Role = (byte)UserRole.Caird,
                AddedAt = DateTime.Now,
                AddedIp = Utils.GetRealIp(),
                Status = (byte)UserStatus.Normal,
                NickName = dto.Nick,
                HeadPhoto = dto.Avatar,
                ValidationType = (byte)dto.ValidationType
            };
            if (ckEmail || ckMobile)
            {
                newUser.PasswordSalt = IdHelper.Instance.Guid32.Substring(5, 16);
                newUser.Email = dto.Email;
                newUser.Mobile = dto.Mobile;
                newUser.Role = (byte)dto.Role;
                newUser.SubjectID = dto.Role == UserRole.Teacher ? dto.Subject : 0;
                newUser.Password = (dto.Password + newUser.PasswordSalt).Md5().ToUpper();
            }
            else
            {
                newUser.PasswordSalt = string.Empty;
                newUser.Password = string.Empty;
            }
            return DResult.Succ(newUser);
        }

        public DResult<long> Regist(RegistUserDto dto)
        {
            var userResult = CreateTuUser(dto);
            if (!userResult.Status)
                return DResult.Error<long>(userResult.Message);
            return UserRepository.Insert(userResult.Data) > 0
                ? DResult.Succ(userResult.Data.Id)
                : DResult.Error<long>("注册失败，请稍后重试！");
        }

        public DResult SaveEmail(long userId, string email)
        {
            if (userId <= 0)
                return DResult.Error("用户不存在！");
            if (string.IsNullOrWhiteSpace(email) || !email.As<IRegex>().IsEmail())
                return DResult.Error("邮箱格式无效！");
            if (UserRepository.Exists(u => u.Email == email))
                return DResult.Error("邮箱已被使用！");
            var user = UserRepository.Load(userId);
            if (user == null)
                return DResult.Error("用户不存在！");
            user.Email = email;
            user.ValidationType |= (byte)ValidationType.Email;
            var result = UserRepository.Update(u => new
            {
                u.Email,
                u.ValidationType
            }, user);
            if (result > 0)
            {
                var tokens = UserTokens(user.Id);
                UserCache.Instance.Remove(tokens);
            }
            return DResult.FromResult(result);
        }

        #endregion

        #region 更新用户数据
        public DResult Update(UserDto user)
        {
            if (user == null)
                return DResult.Error("未做任何更改");
            var item = UserRepository.Load(user.Id);
            if (item == null)
                return DResult.Error("未查询到用户资料");

            var roles = new List<byte>
            {
                (byte)UserRole.Student, (byte)UserRole.Teacher, (byte)UserRole.Parents
            };

            var param = new List<string>();
            if (!string.IsNullOrWhiteSpace(user.Mobile))
            {
                if (!user.Mobile.As<IRegex>().IsMobile())
                    return DResult.Error("手机号码格式错误");
                item.Mobile = user.Mobile;
                item.ValidationType |= (byte)ValidationType.Mobile;
                param.Add("Mobile");
                param.Add("ValidationType");
            }
            if ((item.SubjectID ?? 0) == 0 && user.SubjectId > 0)
            {
                var subject = SubjectRepository.FirstOrDefault(s => s.Id == user.SubjectId);
                if (subject == null)
                    return DResult.Error("没有查询到此学科");
                item.SubjectID = user.SubjectId;
                param.Add("SubjectID");
            }
            if (!string.IsNullOrWhiteSpace(user.Nick))
            {
                item.NickName = user.Nick;
                param.Add("NickName");
            }
            if (!string.IsNullOrWhiteSpace(user.StudentNum))
            {
                item.StudentNum = user.StudentNum;
                param.Add("StudentNum");
            }
            if (user.Name.IsNotNullOrEmpty())
            {
                item.TrueName = user.Name;
                param.Add("TrueName");
            }
            if (item.Role == (byte)UserRole.Caird && roles.Contains(user.Role))
            {
                item.Role = user.Role;
                param.Add("Role");
            }
            if (!string.IsNullOrWhiteSpace(user.Avatar) && item.HeadPhoto != user.Avatar)
            {
                item.HeadPhoto = user.Avatar;
                param.Add("HeadPhoto");
            }
            if (user.Gender.HasValue)
            {
                item.Gender = user.Gender;
                param.Add("Gender");
            }
            if (!param.Any())
                return DResult.Error("未做任何更改");
            var result = UserRepository.Update(item, param.ToArray());
            if (result > 0)
            {
                UserCache.Instance.Remove(item.Id);
                UserCache.Instance.RemoveApps(item.Id);
            }
            return DResult.FromResult(result);
        }
        #endregion

        #region 用户登录
        public DResult<string> Login(LoginDto loginDto)
        {
            if (loginDto == null)
                return DResult.Error<string>("数据异常，请稍后重试！");
            if (loginDto.Comefrom == Comefrom.Web)
            {
                var errorCount = CookieHelper.GetValue(Consts.LoginCountCookieName).As<IConvert>().ToInt(0);
                if (errorCount > 2 && (string.IsNullOrWhiteSpace(loginDto.Vcode) || !VCodeHelper.Verify(loginDto.Vcode)))
                    return DResult.Error<string>("验证码不正确！");
            }
            var userResult = AccountLogin(loginDto.Account, loginDto.Password, loginDto.Partner);
            if (!userResult.Status)
                return DResult.Error<string>(userResult.Message);
            var token = SetLoginStatus(userResult.Data, loginDto.Comefrom, loginDto.Partner);
            return DResult.Succ(token);
        }

        public void Logout()
        {
            var token = CookieHelper.GetValue(Consts.UserCookieName);
            if (string.IsNullOrWhiteSpace(token) || token.Length != 32)
                return;
            TokenRepository.Delete(token);
            UserCache.Instance.Remove(token);
            CookieHelper.Delete(Consts.UserCookieName, Consts.Config.CookieDomain);
        }

        #endregion

        public List<string> DCodes()
        {
            return UserRepository.Where(u => u.UserCode != null && u.UserCode.Length > 4).Select(u => u.UserCode).ToList();
        }

        public DResults<DUserDto> Search(string keyword, DPage page, int userRole = -1)
        {
            Expression<Func<TU_User, bool>> condition = u => u.Status == (byte)NormalStatus.Normal;
            if (userRole >= 0)
                condition = condition.And(u => (u.Role & userRole) > 0);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                if (keyword.As<IRegex>().IsMatch("[0-9]{5}"))
                    condition = condition.And(u => u.UserCode == keyword);
                else
                {
                    condition =
                        condition.And(
                            u =>
                                (u.TrueName != null && u.TrueName.Contains(keyword)) ||
                                (u.NickName != null && u.NickName.Contains(keyword)));
                }
            }
            var users = UserRepository.Where(condition)
                .OrderByDescending(u => u.AddedAt)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .Select(u => new DUserDto
                {
                    Id = u.Id,
                    Name = u.TrueName,
                    Avatar = u.HeadPhoto
                }).ToList();
            users.ForEach(dto =>
            {
                if (string.IsNullOrWhiteSpace(dto.Avatar))
                    dto.Avatar = Consts.DefaultAvatar();
            });
            var count = UserRepository.Count(condition);
            return DResult.Succ(users, count);
        }

        public bool HasPwd(long userId)
        {
            var model = UserRepository.Where(t => t.Id == userId).Select(u => u.Password).FirstOrDefault();
            return !string.IsNullOrWhiteSpace(model);
        }

        public DResult ReleaseUser(long userId)
        {
            var user = UserRepository.Load(userId);
            if (user == null || user.UserCode == user.Id.ToString())
            {
                return DResult.Error("用户已经被释放！");
            }
            int result;
            if (user.ValidationType == (byte)ValidationType.None)
            {
                user.Status = (byte)UserStatus.Delete;
                user.UserCode = user.Id.ToString();
                result = UserRepository.Update(u => new
                {
                    u.Status,
                    u.UserCode
                }, user);
            }
            else
            {
                result = UnitOfWork.Transaction(() =>
                {
                    var attrs = new List<string>
                    {
                        "ValidationType",
                        "UserCode"
                    };
                    if ((user.ValidationType & (byte)ValidationType.Email) > 0)
                    {
                        attrs.Add(nameof(TU_User.Email));
                        user.Email = user.Id + "@dy.com";
                    }
                    if ((user.ValidationType & (byte)ValidationType.Mobile) > 0)
                    {
                        attrs.Add(nameof(TU_User.Mobile));
                        user.Mobile = null;
                    }
                    if ((user.ValidationType & (byte)ValidationType.Third) > 0)
                    {
                        PlatformRepository.Update(new TU_ThirdPlatform
                        {
                            UserID = 0
                        }, u => u.UserID == userId, "UserID");
                    }
                    user.ValidationType = (byte)ValidationType.None;
                    user.UserCode = user.Id.ToString();
                    user.Status = (byte)UserStatus.Delete;
                    UserRepository.Update(user, attrs.ToArray());
                });
            }
            if (result > 0)
            {
                UserCache.Instance.Remove(userId);
                UserCache.Instance.RemoveAgency(userId);
                UserCache.Instance.RemoveApps(userId);
            }
            return DResult.FromResult(result);
        }

        /// <summary>
        /// 获取当前机构教师
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="agencyId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public DResults<BeTeacherDto> LoadTeacher(int subjectId, string agencyId, DPage page)
        {
            if (string.IsNullOrEmpty(agencyId))
            {
                return DResult.Errors<BeTeacherDto>("不存在当前机构");
            }
            var users = UserRepository.Where(
                u => u.Status != (byte)UserStatus.Delete && (u.Role & (byte)UserRole.Teacher) > 0);
            var models =
                UserAgencyRepository.Where(a => a.AgencyId == agencyId && a.Status == (byte)UserAgencyStatus.Current)
                    .Join(users, ua => ua.UserId, u => u.Id,
                        (ua, u) => new BeTeacherDto
                        {
                            AddedAt = u.AddedAt,
                            Name = u.TrueName,
                            SubjectId = u.SubjectID ?? 0,
                            UserCode = u.UserCode,
                            ID = u.Id,
                            Mobile = u.Mobile,
                            Email = u.Email,
                            ValidationType = u.ValidationType
                        });
            if (subjectId > 0)
            {
                models = models.Where(w => w.SubjectId == subjectId);
            }
            var count = models.Count();
            page = page ?? DPage.NewPage(0, 10);
            var dtos = models.OrderByDescending(w => w.AddedAt)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            dtos.ForEach(dto =>
            {
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    dto.Account = dto.Email;
                    return;
                }
                if ((dto.ValidationType & (byte)ValidationType.Mobile) > 0)
                    dto.Account = dto.Mobile;
                else if ((dto.ValidationType & (byte)ValidationType.Third) > 0)
                    dto.Account = "第三方登录";
                else
                    dto.Account = string.Empty;
            });
            return DResult.Succ(dtos, count);
        }
        /// <summary>
        /// 删除用户机构
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult UpdateTeacherAgency(long userId)
        {
            if (userId <= 0)
            {
                return DResult.Error("当前用户不存在");
            }
            var user = UserRepository.SingleOrDefault(w => w.Id == userId);
            if (user == null)
            {
                return DResult.Error("当前用户不存在");
            }
            var userAgency =
                UserAgencyRepository.FirstOrDefault(
                    w =>
                        w.UserId == userId && w.Status == (byte)UserAgencyStatus.Current && w.AgencyId == user.AgencyId);
            if (userAgency == null)
                return DResult.Error("用户已不在该机构");
            var resultId = UnitOfWork.Transaction(() =>
            {
                user.AgencyId = null;
                UserRepository.Update(u => new { u.AgencyId }, user);
                UserAgencyRepository.Delete(userAgency);
            });
            return resultId < 0 ? DResult.Error("删除失败") : DResult.Succ(user);
        }

        public DResult<int> AccountStatus(long userId)
        {
            if (userId <= 0)
                return DResult.Error<int>("用户未登录");
            var user = UserRepository.Where(u => u.Id == userId && u.Status != (byte)UserStatus.Delete)
                .Select(u => new { u.ValidationType, u.Email, u.Mobile, u.Password, u.PasswordSalt })
                .FirstOrDefault();
            if (user == null)
                return DResult.Error<int>("用户不存在");
            var code = 0;
            //需要绑定手机号码
            if (user.ValidationType == 0)
                code |= 1;
            //需要修改密码
            if (string.Equals(user.Password, ("123456" + user.PasswordSalt).Md5(),
                StringComparison.CurrentCultureIgnoreCase))
                code |= 2;
            return DResult.Succ(code);
        }

        public DResult CompleteInfo(UserCompleteInputDto inputDto)
        {
            var result = AddAgency(new UserAgencyInputDto
            {
                AgencyId = inputDto.AgencyId,
                Start = inputDto.Start,
                UserId = inputDto.UserId,
                Status = (byte)UserAgencyStatus.Current
            });
            if (!result.Status)
                return result;
            result = Update(new UserDto
            {
                Id = inputDto.UserId,
                Gender = (byte)inputDto.Gender,
                Name = inputDto.Name
            });
            return result;
        }

        public List<TU_User> BatchImportStudents(IEnumerable<string> names, string agencyId, byte stage)
        {
            if (names == null) return null;
            names = names.Distinct();
            var users = new List<TU_User>();
            var userAgencies = new List<TU_UserAgency>();
            foreach (var name in names)
            {
                var id = IdHelper.Instance.LongId;
                var newUser = new TU_User
                {
                    Id = id,
                    UserCode = UserCodeManager.Instance.Code(),
                    Role = (byte)UserRole.Student,
                    TrueName = name,
                    AddedAt = Clock.Now,
                    AddedIp = Utils.GetRealIp(),
                    Status = (byte)UserStatus.Normal,
                    ValidationType = (byte)ValidationType.None,
                    PasswordSalt = IdHelper.Instance.Guid32.Substring(5, 16),
                    SubjectID = 0,
                    AgencyId = agencyId
                };
                newUser.Password = ("123456" + newUser.PasswordSalt).Md5().ToUpper(); //设置导入成员初始化密码
                users.Add(newUser);
                if (string.IsNullOrWhiteSpace(agencyId))
                    continue;
                var userAgency = new TU_UserAgency
                {
                    AgencyId = agencyId,
                    Status = (byte)NormalStatus.Normal,
                    UserId = id,
                    StartTime = Clock.Now,
                    Id = IdHelper.Instance.Guid32,
                    Role = (byte)UserRole.Student,
                    Stage = stage
                };
                userAgencies.Add(userAgency);
            }
            var result = UnitOfWork.Transaction(() =>
            {
                UserRepository.Insert(users.ToArray());
                UserAgencyRepository.Insert(userAgencies.ToArray());
            });
            return result > 0 ? users : null;
        }
    }
}
