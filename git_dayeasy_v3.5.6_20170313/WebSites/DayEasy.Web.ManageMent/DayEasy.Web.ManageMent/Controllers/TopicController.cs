using System;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.OperationManager)]
    [RoutePrefix("operate/topic")]
    public class TopicController : AdminController
    {
        public TopicController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        [Route("")]
        public ActionResult Index()
        {
            var pageIndex = "pageindex".Query(1);
            var auth = "auth".Query(-1);
            var ts = "ts".Query(-1);
            var ct = "ct".Query(-1);
            var sort = "sort".Query("");
            var keyWord = "keyword".Query("");

            var result = ManagementContract.GetTopics(new TopicSearchDto()
            {
                Auth = auth,
                ClassType = ct,
                KeyWord = keyWord,
                Sort = sort,
                TopicStatus = ts,
                Page = pageIndex - 1,
                Size = 15
            });

            ViewData["joinAuths"] = MvcHelper.EnumToDropDownList<GroupJoinAuth>(auth, true, "圈子权限");
            ViewData["topicStas"] = MvcHelper.EnumToDropDownList<TopicStatus>(ts, true, "帖子状态");

            if (!result.Status || !result.Data.Any()) return View();

            var addByUserIds = result.Data.Select(u => u.AddedBy).ToList();

            //查找圈子
            var groupIds = result.Data.Select(u => u.GroupId).ToList();
            var groups = ManagementContract.GroupSearch(groupIds);
            if (groups.Status && groups.Data.Any())
            {
                ViewData["groups"] = groups.Data.ToList();
                ViewData["shares"] = ManagementContract.ShareGroups(groups.Data.Select(u => u.Id).ToList());

                addByUserIds.AddRange(groups.Data.Select(u => u.ManagerId));
            }

            //查找用户
            var userList = ManagementContract.UserSearch(addByUserIds.Distinct().ToList());
            if (userList.Status && userList.Data.Any())
                ViewData["users"] = userList.Data.ToList();

            ViewData["pageIndex"] = pageIndex;
            ViewData["TotalCount"] = result.TotalCount;

            return View(result.Data.ToList());
        }

        [HttpPost]
        public ActionResult UpdateTopicStatus(string id)
        {
            var status = "status".Form(-1);

            if (!Enum.IsDefined(typeof(TopicStatus), (byte)status))
                return Json(DResult.Error("参数错误！"), JsonRequestBehavior.AllowGet);

            var result = ManagementContract.UpdateTopicStatus(id, (TopicStatus)status);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}