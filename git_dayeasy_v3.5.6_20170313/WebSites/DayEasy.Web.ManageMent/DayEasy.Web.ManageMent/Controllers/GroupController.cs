using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.MemberManager)]
    [RoutePrefix("user/group")]
    public class GroupController : AdminController
    {
        private IGroupContract _groupContract;
        public GroupController(IUserContract userContract, IManagementContract managementContract, IGroupContract groupContract)
            : base(userContract, managementContract)
        {
            _groupContract = groupContract;
        }

        #region Views
        [Route("")]
        public ActionResult Index(GroupSearchDto searchDto)
        {
            searchDto.Page = "pageindex".Query(1) - 1;
            searchDto.Size = "pagesize".Query(15);
            searchDto.Level = "level".Query(-1);
            searchDto.ShowDelete = "showDelete".Query(string.Empty) == "on";
            var ct = "ct".Query(-1);
            searchDto.ClassType = ct;

            var result = ManagementContract.GroupSearch(searchDto);
            if (!result.Status)
                return Content(result.Message);
            var levels = MvcHelper.EnumToDropDownList<GroupCertificationLevel>(searchDto.Level, true, "所有认证");
            levels.Add(new SelectListItem { Value = "-2", Text = "未认证", Selected = (searchDto.Level == -2) });
            ViewData["levels"] = levels;
            ViewData["types"] = MvcHelper.EnumToDropDownList<GroupType>(searchDto.Type, true, "所有类型");
            ViewBag.TotalCount = result.TotalCount;
            var uids = result.Data.Where(g => g.ManagerId > 0).Select(g => g.ManagerId).Distinct();
            var users = UserContract.UserNames(uids);
            ViewData["managers"] = users;

            var groups = result.Data == null ? new List<TG_Group>() : result.Data.ToList();
            if (groups.Any())
                ViewData["groupTags"] = ManagementContract.GroupTags(groups.Select(g => g.Id).ToList());

            return View(groups);
        }

        [HttpGet]
        [Route("export")]
        public ActionResult Export(string groupId)
        {
            var result = ManagementContract.GroupStudentExcel(groupId);
            return Content(result.Status ? "导出成功！" : result.Message);
        }

        #endregion

        #region Ajax

        /// <summary> 删除圈子 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("delete")]
        public ActionResult Delete(string groupId)
        {
            return DeyiJson(ManagementContract.GroupDelete(groupId));
        }

        [AjaxOnly]
        [HttpGet]
        [Route("load")]
        public ActionResult Load(string groupId)
        {
            var groupContract = CurrentIocManager.Resolve<IGroupContract>();
            return DeyiJson(groupContract.LoadById(groupId));
        }

        [AjaxOnly]
        [HttpPost]
        [Route("update")]
        public ActionResult Update(UpdateGroupInputDto inputDto)
        {
            return DeyiJson(ManagementContract.UpdateGroup(inputDto), true);
        }

        /// <summary> 认证圈子 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("certificate")]
        public ActionResult Certificate(string groupId)
        {
            return DeyiJson(ManagementContract.GroupCertificate(groupId));
        }

        /// <summary> 圈子成员列表 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("members")]
        public ActionResult Members(string groupId)
        {
            var members = _groupContract.GroupMembers(groupId);
            if (members.Status && members.Data != null)
            {
                members.Data = members.Data.OrderBy(m => (m.SubjectId == 0 ? 99 : m.SubjectId))
                    .ThenBy(m => m.Name);
            }
            return DeyiJson(members);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("delete-member")]
        public ActionResult DeleteMember(string groupId, long userId)
        {
            var result = _groupContract.DeleteMember(groupId, userId, UserId);
            return DeyiJson(result);
        }

        /// <summary> 搜索用户 </summary>
        /// <param name="groupId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [AjaxOnly]
        [HttpGet]
        [Route("search-user")]
        public ActionResult SearchUsers(string groupId, string keyword)
        {
            var result = ManagementContract.SearchUsers(groupId, keyword);
            return DeyiJson(result);
        }

        /// <summary> 添加成员 </summary>
        /// <param name="users"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [AjaxOnly]
        [HttpPost]
        [Route("add-members")]
        public ActionResult AddMembers(string groupId, long[] users)
        {
            //return DeyiJson(DResult.Succ(new { groupId, users }), true);
            var result = _groupContract.AddMembers(groupId, users, UserId);
            return DeyiJson(result, true);
        }

        /// <summary> 发布协同 </summary>
        /// <param name="paperCode"></param>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [AjaxOnly]
        [HttpPost]
        [Route("publish-joint")]
        public ActionResult PublishJoint(string paperCode, string groupId, long userId)
        {
            var result = ManagementContract.PublishJoint(paperCode, groupId, userId, UserId);
            return DeyiJson(result, true);
        }
        #endregion
    }
}