using System;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.SystemManager)]
    [RoutePrefix("sys/apps")]
    public class AppController : AdminController
    {
        public AppController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        #region Views
        /// <summary> 列表 </summary>
        [Route("")]
        public ActionResult Index(bool hasDelete = false)
        {
            var appList = ManagementContract.Applications(hasDelete);
            ViewBag.HasDelete = hasDelete;

            ViewData["appType"] = MvcHelper.EnumToDropDownList<ApplicationType>(true);
            ViewData["roles"] = MvcHelper.EnumToDropDownList<UserRole>();

            return View(appList);
        }

        /// <summary> 修改应用 </summary>
        [HttpPost]
        [Route("edit")]
        public ActionResult Edit()
        {
            int id = "id".Form(-1);
            if (id > 0)
            {
                var app = ManagementContract.Application(id);

                ViewData["appTypes"] = MvcHelper.EnumToDropDownList<ApplicationType>(app.AppType, true);

                var roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Where(u => ((byte)u & app.AppRoles) > 0).ToList();

                var roleInt = roles.Select(u => (int)u).ToList();

                ViewData["roles"] = MvcHelper.EnumToDropDownList<UserRole>(roleInt);
                ViewData["appStatus"] = MvcHelper.EnumToDropDownList<NormalStatus>(app.Status);
                return PartialView(app);
            }
            return Content("参数错误，请稍后重试！");
        }

        /// <summary> 应用配置 </summary>
        /// <param name="id"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [Route("configure")]
        public ActionResult Configure(int id,int pageIndex=0)
        {
            var app = ManagementContract.Application(id);
            if (app == null)
                return RedirectToAction("Index");
            ViewData["App"] = app;
            var users = ManagementContract.ApplicationUsers(id, DPage.NewPage(pageIndex - 1));
            if (!users.Status)
                return RedirectToAction("Index");
            ViewData["Index"] = pageIndex;
            ViewData["Total"] = users.TotalCount;
            return View(users.Data.ToList());
        }

        #endregion

        #region Ajax
        /// <summary> 添加应用 </summary>
        [HttpPost]
        [AjaxOnly]
        [Route("add")]
        public ActionResult Add(AppDto dto)
        {
            var result = ManagementContract.InsertOrUpdateApp(dto);
            if (result.Status)
            {
                UserContract.ResetAppCache();
            }
            return DeyiJson(result);
        }

        /// <summary> 删除应用 </summary>
        [HttpPost]
        [AjaxOnly]
        [Route("delete")]
        public ActionResult Delete(int id = -1)
        {
            var result = ManagementContract.DeleteApplication(id);
            if (result.Status)
            {
                UserContract.ResetAppCache();
            }
            return DeyiJson(result);
        }

        /// <summary> 删除用户应用 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("configure-remove")]
        public ActionResult ConfigureRemove(int appId, long userId)
        {
            var result = ManagementContract.RemoveUserApp(appId, userId);
            if (result.Status)
            {
                UserContract.ResetAppCache(userId);
            }
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("user")]
        public ActionResult User(string code)
        {
            var userResult = UserContract.LoadByCode(code);
            return DeyiJson(userResult, true);
        }

        /// <summary> 为用户分配应用 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("configure-submit")]
        public ActionResult ConfigureSubmit(int appId, long userId, string agencyId = null)
        {
            var result = UserContract.AddApplication(userId, appId, agencyId);
            if (result.Status)
            {
                UserContract.ResetAppCache(userId);
            }
            return DeyiJson(result, true);
        }

        #endregion
    }
}