using System.Collections.Generic;
using System.Linq;
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.User.Services
{
    public partial class UserService
    {
        public IDayEasyRepository<TU_ThirdPlatform, string> PlatformRepository { private get; set; }

        /// <summary> 第三方登录 </summary>
        /// <param name="platformDto"></param>
        /// <param name="comefrom"></param>
        /// <param name="partner"></param>
        /// <returns></returns>
        public DResult<PlatformLoginResultDto> Login(PlatformDto platformDto, Comefrom comefrom, string partner="")
        {
            var plat =
                PlatformRepository.FirstOrDefault(
                    t => t.PlatformId == platformDto.PlatformId
                         && t.PlatformType == platformDto.PlatformType);
            if (plat == null)
            {
                plat = platformDto.MapTo<TU_ThirdPlatform>();
                plat.Id = IdHelper.Instance.Guid32;
                plat.AddedAt = Clock.Now;
                PlatformRepository.Insert(plat);
                return DResult.Succ(new PlatformLoginResultDto { PlatId = plat.Id });
            }
            if (plat.UserID <= 0 || (platformDto.UserId > 0 && plat.UserID != platformDto.UserId))
            {
                //已绑定帐号和当前登录帐号不一致时，重新绑定 --add by shay 2016-02-25
                return DResult.Succ(new PlatformLoginResultDto { PlatId = plat.Id });
            }
            var user = UserRepository.Load(plat.UserID);
            if (user == null)
            {
                return DResult.Error<PlatformLoginResultDto>("已绑定的用户未找到！");
            }
            if (user.Status == (byte)UserStatus.Delete)
            {
                return DResult.Error<PlatformLoginResultDto>("用户已被禁用！");
            }
            var updateList = new List<string>();
            var userUpdates = new List<string>();
            //更新AccessToken
            if (plat.AccessToken != platformDto.AccessToken)
            {
                plat.AccessToken = platformDto.AccessToken;
                updateList.Add("AccessToken");
            }
            if (plat.Profile != platformDto.Profile)
            {
                if (user.HeadPhoto == plat.Profile)
                {
                    user.HeadPhoto = platformDto.Profile;
                    userUpdates.Add("HeadPhoto");
                }
                plat.Profile = platformDto.Profile;
                updateList.Add("AccessToken");
            }
            var result = 1;
            if (!updateList.IsNullOrEmpty() || !userUpdates.IsNullOrEmpty())
            {
                result = UnitOfWork.Transaction(() =>
                {
                    if (!updateList.IsNullOrEmpty())
                        PlatformRepository.Update(plat, updateList.ToArray());
                    if (!userUpdates.IsNullOrEmpty())
                        UserRepository.Update(user, userUpdates.ToArray());
                });
            }
            if (result <= 0)
                return DResult.Error<PlatformLoginResultDto>("更新用户信息失败~！");
            var token = SetLoginStatus(user, comefrom, partner);
            return DResult.Succ(new PlatformLoginResultDto { Token = token });
        }

        public DResult<string> CreatePlatAccount(string platId)
        {
            if (string.IsNullOrWhiteSpace(platId))
                return DResult.Error<string>("创建新用户失败，请稍后重试！");
            var plat = PlatformRepository.Load(platId);
            if (plat == null)
                return DResult.Error<string>("创建新用户失败，请稍后重试！");
            if (plat.UserID <= 0)
            {
                //创建新用户
                var userResult = CreateTuUser(new RegistUserDto
                {
                    Nick = plat.Nick,
                    Avatar = plat.Profile,
                    ValidationType = ValidationType.Third
                });
                if (!userResult.Status)
                    return DResult.Error<string>(userResult.Message);
                plat.UserID = userResult.Data.Id;

                //生成新用户，并修改第三方登录UserId
                var result = UnitOfWork.Transaction(() =>
                {
                    UserRepository.Insert(userResult.Data);
                    PlatformRepository.Update(t => new { t.UserID }, plat);
                });
                if (result <= 0)
                    return DResult.Error<string>("创建新用户失败，请稍后重试！");
            }

            //自动登录
            var user = UserRepository.Load(plat.UserID);
            var token = SetLoginStatus(user, Comefrom.Web);
            return DResult.Succ(token);
        }

        public DResult<string> AccountBind(string platId, string account, string pwd)
        {
            var plat = PlatformRepository.Load(platId);
            if (plat == null || plat.UserID > 0)
                return DResult.Error<string>("第三方帐号已经被绑定,如需重置,请联系客服！");
            var userResult = AccountLogin(account, pwd);
            if (!userResult.Status)
                return DResult.Error<string>(userResult.Message);
            UnitOfWork.Transaction(() =>
            {
                var user = userResult.Data;
                //更新ThirdPlatform
                plat.UserID = user.Id;
                PlatformRepository.Update(t => new
                {
                    t.UserID
                }, plat);
                //更新user
                if (string.IsNullOrWhiteSpace(user.NickName))
                    user.NickName = plat.Nick;
                if (string.IsNullOrWhiteSpace(user.HeadPhoto))
                    user.HeadPhoto = plat.Profile;
                user.ValidationType |= (byte)ValidationType.Third;
                UserRepository.Update(u => new
                {
                    u.ValidationType,
                    u.NickName,
                    u.HeadPhoto
                }, user);
            });
            var token = SetLoginStatus(userResult.Data, Comefrom.Web);
            return DResult.Succ(token);
        }

        public DResult AccountBind(string platId, long userId)
        {
            var plat = PlatformRepository.Load(platId);
            if (plat == null || plat.UserID > 0)
                return DResult.Error("第三方帐号已经被绑定,如需重置,请联系客服！");

            if (PlatformRepository.Exists(t => t.UserID == userId && t.PlatformType == plat.PlatformType))
                return DResult.Error("登录帐号已经有第三方绑定,不能重复绑定,如需重置,请联系客服！");

            var user = UserRepository.Load(userId);

            UnitOfWork.Transaction(() =>
            {
                //更新ThirdPlatform
                plat.UserID = user.Id;
                PlatformRepository.Update(t => new
                {
                    t.UserID
                }, plat);
                //更新user
                user.ValidationType |= (byte)ValidationType.Third;
                UserRepository.Update(u => new
                {
                    u.ValidationType
                }, user);
            });
            return DResult.Success;
        }

        private DResult CancelAccountBind(TU_User user, TU_ThirdPlatform plat)
        {
            if (user == null || plat == null)
                return DResult.Error("用户或第三方登录信息为空！");
            if ((user.ValidationType & (byte)ValidationType.Mobile) == 0
                && user.Email.IsNullOrEmpty() || user.Password.IsNullOrEmpty())
                return DResult.Error("请先设置登录邮箱、密码，再解除第三方登录绑定!");
            var result = UnitOfWork.Transaction(() =>
            {
                //更新ThirdPlatform
                plat.UserID = 0;
                PlatformRepository.Update(t => new
                {
                    t.UserID
                }, plat);
                //是否还存在其他第三方登录
                if (PlatformRepository.Exists(p => p.UserID == user.Id && p.Id != plat.Id))
                    return;
                //更新user
                user.ValidationType ^= (byte)ValidationType.Third;
                UserRepository.Update(u => new
                {
                    u.ValidationType
                }, user);
            });
            return DResult.FromResult(result);
        }

        public DResult CancelAccountBind(long userId, byte platType)
        {
            var user = UserRepository.Load(userId);
            var plat = PlatformRepository.FirstOrDefault(t => t.UserID == userId && t.PlatformType == platType);
            return CancelAccountBind(user, plat);
        }

        public IEnumerable<PlatformDto> Platforms(long userId, int platformType = -1)
        {
            var plats = PlatformRepository.Where(t => t.UserID == userId);
            if (plats == null || !plats.Any())
                return new List<PlatformDto>();
            if (platformType >= 0)
                plats = plats.Where(p => p.PlatformType == platformType);
            return plats.MapTo<List<PlatformDto>>();
        }
    }
}
