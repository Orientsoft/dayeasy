using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;
using DayEasy.Contracts.Models;

namespace DayEasy.Contracts
{
    /// <summary> 用户业务模块 </summary>
    public partial interface IUserContract : IDependency
    {
        #region 加载用户信息

        /// <summary> 加载用户 </summary>
        /// <param name="userId"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        UserDto Load(long userId, bool fromCache = true);

        /// <summary> 根据得一号加载用户信息 </summary>
        /// <param name="code">得一号</param>
        DResult<UserDto> LoadByCode(string code);

        /// <summary> 加载用户集合 </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        List<UserDto> LoadList(IEnumerable<long> userIds);

        /// <summary> 加载用户基本信息 </summary>
        /// <param name="userIds"></param>
        /// <param name="showNick"></param>
        /// <returns></returns>
        List<DUserDto> LoadDList(IEnumerable<long> userIds, bool showNick = false);

        /// <summary> 加载用户基本信息 </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        Dictionary<string, DUserDto> LoadDListByCodes(IEnumerable<string> codes);

        /// <summary> 加载用户基本信息 </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        Dictionary<long, DUserDto> LoadListDictUser(IEnumerable<long> userIds);

        /// <summary> 加载用户信息 </summary>
        /// <param name="token">登录凭证</param>
        /// <param name="comefrom">登录来源</param>
        /// <param name="fromCache">从缓存中加载</param>
        /// <returns></returns>
        UserDto Load(string token, Comefrom comefrom = Comefrom.Web, bool fromCache = true);

        /// <summary> 获取用户有效登录凭证 </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        IDictionary<string, byte> UserTokens(long userId);

        /// <summary> 获取用户姓名 </summary>
        /// <param name="userIds"></param>
        /// <param name="isNick">昵称</param>
        /// <returns></returns>
        Dictionary<long, string> UserNames(IEnumerable<long> userIds, bool isNick = false);

        /// <summary> 显示用户名字 </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string DisplayName(UserDto user);

        #endregion

        /// <summary> 帐号检测 </summary>
        /// <param name="account">帐号</param>
        /// <returns>是否可用</returns>
        DResult CheckAccount(string account);

        /// <summary> 检查帐号是否存在 </summary>
        /// <param name="account">帐号</param>
        /// <returns></returns>
        bool ExistsAccount(string account);

        /// <summary> 修改密码 </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="confirmPwd"></param>
        /// <returns></returns>
        DResult<long> ChangPwd(string account, string password, string confirmPwd);

        /// <summary> 修改密码 </summary>
        /// <param name="userId"></param>
        /// <param name="oldPwd"></param>
        /// <param name="password"></param>
        /// <param name="confirmPwd"></param>
        /// <returns></returns>
        DResult<long> ChangPwd(long userId, string oldPwd, string password, string confirmPwd);

        /// <summary> 自动登录 </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="comefrom"></param>
        /// <returns></returns>
        DResult<string> AutoLogin(long userId, Comefrom comefrom = Comefrom.Web);

        DResult<long> Regist(RegistUserDto registUserDto);

        /// <summary> 保存邮箱 </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        DResult SaveEmail(long userId, string email);

        /// <summary> 更新用户资料 </summary>
        DResult Update(UserDto user);

        /// <summary> 用户登录 </summary>
        /// <param name="loginDto"></param>
        /// <returns>token</returns>
        DResult<string> Login(LoginDto loginDto);

        /// <summary> 退出登录 </summary>
        void Logout();

        /// <summary> 获取所有得一号 </summary>
        List<string> DCodes();

        /// <summary> 用户搜索 </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page"></param>
        /// <param name="userRole">角色</param>
        /// <returns></returns>
        DResults<DUserDto> Search(string keyword, DPage page, int userRole = -1);

        /// <summary> 是否已设置密码 </summary>
        bool HasPwd(long userId);

        /// <summary> 释放用户 </summary>
        DResult ReleaseUser(long userId);

        /// <summary>
        ///获取当前机构的教师
        /// </summary>
        /// <param name="subjectId">学科</param>
        /// <param name="agencyId">机构ID</param>
        /// <returns></returns>
        DResults<BeTeacherDto> LoadTeacher(int subjectId, string agencyId, DPage page);

        /// <summary>
        /// 删除用户机构
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult UpdateTeacherAgency(long userId);

        /// <summary> 检测用户状态 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<int> AccountStatus(long userId);

        /// <summary> 完善资料 </summary>
        /// <param name="inputDto"></param>
        DResult CompleteInfo(UserCompleteInputDto inputDto);

        /// <summary> 批量导入学生 </summary>
        /// <param name="names"></param>
        /// <param name="agencyId"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        List<TU_User> BatchImportStudents(IEnumerable<string> names, string agencyId, byte stage);
    }
}
