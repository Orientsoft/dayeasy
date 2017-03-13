using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Office;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.MemberManager)]
    [RoutePrefix("user")]
    public class UserController : AdminController
    {
        public UserController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }
        #region Views

        [Route("list")]
        public ActionResult Index(UserSearchDto searchDto)
        {
            searchDto.Page = "pageIndex".Query(1) - 1;
            searchDto.Size = "pageSize".Query(15);
            var userList = ManagementContract.UserSearch(searchDto);
            if (!userList.Status)
                return View(new List<TU_User>());
            ViewData["roles"] = MvcHelper.EnumToDropDownList<UserRole>(searchDto.Role, true, "所有角色");
            ViewData["totalCount"] = userList.TotalCount;
            return View(userList.Data.ToList());
        }

        [HttpGet]
        [Route("active/{userId}")]
        public ActionResult Active(long userId)
        {
            var result = ManagementContract.UserActiveInfo(userId);
            if (!result.Status)
                return Content(result.Message);
            ViewBag.IsSupper = IsSupperAccount();
            return PartialView(result.Data);
        }

        [Route("special")]
        public ActionResult Special()
        {
            var list = Consts.SpecialAccountList;
            var codes = list.SelectMany(t => t.UserCodes).ToList();
            if (codes.Any())
            {
                var users = UserContract.LoadDListByCodes(codes);
                ViewData["users"] = users;
            }
            return View(list);
        }

        #endregion

        #region Ajax

        [AjaxOnly]
        [HttpPost]
        [Route("certificate")]
        public ActionResult Certificate(long id)
        {
            return DeyiJson(ManagementContract.CertificateUser(id), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("delete")]
        public ActionResult Delete(long userId)
        {
            return DeyiJson(ManagementContract.UserDelete(userId), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("reset")]
        public ActionResult ResetPwd(long userId)
        {
            return DeyiJson(ManagementContract.ResetPassword(userId), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("edit")]
        public ActionResult Edit(long userId, string name)
        {
            //暂时只需更改真实姓名 - 需要扩展再改为实体类接收参数
            return DeyiJson(ManagementContract.UserEdit(userId, name), true);
        }

        //        [AjaxOnly]
        [HttpPost]
        [Route("import-num")]
        public ActionResult ImportStudentNum(string numbers)
        {
            var numDict = new Dictionary<string, string>();
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    var ext = Path.GetExtension(file.FileName);
                    if (string.Equals(ext, ".xls", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var ds = ExcelHelper.Read(file.InputStream);
                        if (ds != null)
                        {
                            var dt = ds.Tables[0];
                            var code = -1;
                            foreach (DataRow row in dt.Rows)
                            {
                                if (code < 0)
                                {
                                    for (var i = 0; i < row.ItemArray.Length; i++)
                                    {
                                        if (!string.Equals(row[i], "得一号"))
                                            continue;
                                        code = i;
                                        break;
                                    }
                                    continue;
                                }
                                numDict.Add(row[code].ToString(), row[code + 1].ToString());
                            }
                        }
                    }
                }
            }
            if (!numDict.Any() && !string.IsNullOrWhiteSpace(numbers))
            {
                var list = numbers.Trim().Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                numDict =
                    list.Select(item => Regex.Split(item, "[,，]|\\s{4,}"))
                        .Where(student => student.Length == 2)
                        .ToDictionary(student => student[0], student => student[1]);
            }
            var result = ManagementContract.ImportStudentNums(numDict);
            return new ScriptResult(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("delete-special")]
        public ActionResult DeleteSpecial(int type, string code)
        {
            if (type < 0 || string.IsNullOrWhiteSpace(code))
                return DeyiJson(DResult.Error("得一号未找到！"));
            var utils = ConfigUtils<SpecialAccountConfig>.Instance;
            var config = utils.Get();
            var special = config.SpecialAccounts.FirstOrDefault(t => t.Type == (SpecialAccountType)type);
            if (special != null)
            {
                var result = special.UserCodes.Remove(code);
                if (result)
                {
                    utils.Set(config);
                    return DeyiJson(DResult.Success, true);
                }
            }
            return DeyiJson(DResult.Error("删除失败！"), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("add-special")]
        public ActionResult AddSpecial(int type, string codes)
        {
            var list = codes.Split(',');
            if (!list.Any())
                return DeyiJson(DResult.Error("得一号不能为空！"), true);
            var utils = ConfigUtils<SpecialAccountConfig>.Instance;
            var config = utils.Get();
            var special = config.SpecialAccounts.FirstOrDefault(t => t.Type == (SpecialAccountType)type);
            if (special == null)
            {
                special = new SpecialAccount
                {
                    Type = (SpecialAccountType)type,
                    UserCodes = new List<string>()
                };
                config.SpecialAccounts.Add(special);
            }
            var count = 0;
            foreach (var code in list)
            {
                var result = UserContract.LoadByCode(code);
                if (result.Status && result.Data.Role != (byte)UserRole.Caird && !special.UserCodes.Contains(code))
                {
                    count++;
                    special.UserCodes.Add(code);
                }
            }
            if (count <= 0)
                return DeyiJson(DResult.Error("添加失败！"), true);
            utils.Set(config);
            return DeyiJson(new DResult(true, "成功添加{0}个特殊帐号！".FormatWith(count)), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("release-user")]
        public ActionResult ReleaseUser(long userId)
        {
            var active = ManagementContract.UserActiveInfo(userId);
            if (!active.Status)
                return DeyiJson(active, true);
            if (!IsSupperAccount() && (active.Data.WorkCount > 0 || active.Data.Groups.Any()))
                return DeyiJson(DResult.Error("不能释放已有圈子或完成过作业的用户"), true);
            var result = UserContract.ReleaseUser(userId);
            return DeyiJson(result, true);
        }

        private bool IsSupperAccount()
        {
            var codes = "supperCode".Config(string.Empty);
            if (codes.IsNullOrEmpty())
                return false;
            return codes.Split(',').Contains(CurrentUser.Code);
        }

        #endregion
    }
}