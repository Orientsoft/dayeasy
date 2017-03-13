using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Core.Domain;
using DayEasy.Office;
using DayEasy.Portal.Services.Dto;
using DayEasy.Portal.Services.Helper;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Timing;
using DayEasy.Web.Filters;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DayEasy.Web.Portal.Controllers
{
    [RoutePrefix("group")]
    public class GroupController : DController
    {
        private readonly IGroupContract _groupContract;
        private readonly ISystemContract _systemContract;

        public GroupController(IUserContract userContract, IGroupContract groupContract, ISystemContract systemContract)
            : base(userContract)
        {
            _groupContract = groupContract;
            _systemContract = systemContract;
            //            ViewBag.PageNav = 1;
        }

        #region Views

        /// <summary> 我的圈子 </summary>
        [HttpGet]
        [Route("")]
        [DAuthorize]
        public ActionResult Home()
        {
            return RedirectToAction("Index", "User");
            //            //家长绑定
            //            if (CurrentUser.IsParents())
            //            {
            //                var childGroupList = new List<GroupDto>();
            //                if (!Children.IsNullOrEmpty())
            //                {
            //                    var childGroups = _groupContract.Groups(Children.First().Id, (int)GroupType.Class);
            //                    if (childGroups.Status && childGroups.Data != null)
            //                        childGroupList = childGroups.Data.ToList();
            //                }
            //                ViewData["childGroups"] = childGroupList;
            //            }
            //            var groups = _groupContract.Groups(UserId, loadMessageCount: true);
            //            if (!groups.Status || groups.Data == null || !groups.Data.Any())
            //            {
            //                return View(new List<GroupDto>());
            //            }
            //            return View(groups.Data.ToList());
        }

        /// <summary> 圈子内页 </summary>
        [HttpGet]
        [Route("{id:regex([0-9a-z]{32})}")]
        public ActionResult Item(string id)
        {
            var groupResult = _groupContract.LoadById(id);
            if (!groupResult.Status)
                return MessageView(groupResult.Message);
            var group = groupResult.Data;
            if (group.Status == (int)NormalStatus.Delete)
                return MessageView("圈子已经被解散！", group.Name);
            //私有模式
            var privateMode = group.Type != (byte)GroupType.Share ||
                              ((ShareGroupDto)group).JoinAuth == (byte)GroupJoinAuth.Private;
            var userId = group.Type == (byte)GroupType.Share ? UserId : ChildOrUserId;
            var isManager = _groupContract.IsManager(group, userId);

            var status = _groupContract.IsGroupMember(userId, id);
            if (privateMode && !isManager)
            {
                switch (status)
                {
                    case CheckStatus.Invalid:
                        return MessageView("你还不是该圈子的成员！");
                    case CheckStatus.Pending:
                        return Redirect("/group/apply/" + id);
                }
            }
            ViewBag.IsMember = isManager || (status == CheckStatus.Normal);
            ViewBag.IsManager = isManager;
            if (group.Type != (byte)GroupType.Share)
            {
                //圈成员
                var membersResult = _groupContract.GroupMembers(id, includeParents: true);
                if (membersResult.Status)
                {
                    var members =
                        membersResult.Data
                            .OrderBy(u => u.Id == group.ManagerId
                                ? 0
                                : (u.SubjectId > 0 ? u.SubjectId : (u.Id == userId ? 998 : 999)))
                            .ThenBy(u => u.AddedTime)
                            .ToList();
                    ViewData["Members"] = members;
                }
            }
            //申请列表
            if (group.PendingCount > 0 && isManager)
            {
                var pendings = _groupContract.PendingList(id, UserId);
                if (pendings.Status)
                    ViewData["PendingList"] = pendings.Data;
            }
            //其他圈子
            if (CurrentUser != null && CurrentUser.IsTeacher() && group.Type == (byte)GroupType.Class)
            {
                var classGroups = _groupContract.Groups(UserId, (int)GroupType.Class);
                if (classGroups.Status)
                    ViewData["classes"] = classGroups.Data.Where(g => g.Id != id).ToDictionary(k => k.Id, v => v.Name);
            }
            _groupContract.UpdateLastTime(id, UserId);
            return @group.Type == (byte)GroupType.Share
                ? View("~/Views/Group/ShareIndex.cshtml", @group)
                : View(@group);
        }

        [HttpGet]
        [DAuthorize]
        [Route("setting/{id:regex([0-9a-z]{32})}")]
        public ActionResult Setting(string id)
        {
            var groupResult = _groupContract.LoadById(id);
            if (!groupResult.Status)
                return MessageView("圈子未被找到！");
            var group = groupResult.Data;
            var userId = group.Type == (byte)GroupType.Share ? UserId : ChildOrUserId;
            var status = _groupContract.IsGroupMember(userId, id);
            if (status != CheckStatus.Normal)
            {
                var isManager = _groupContract.IsManager(group, UserId);
                if (!isManager)
                    return MessageView("您还不是该圈子的成员！");
                ViewBag.isManager = true;
            }

            return View(group);
        }

        [HttpGet]
        [Route("members/{id:regex([0-9a-z]{32})}")]
        public ActionResult Members(string id, int pageIndex = 1, int pageSize = 15)
        {
            var groupResult = _groupContract.LoadById(id);
            if (!groupResult.Status)
                return MessageView("圈子未被找到！");
            var group = groupResult.Data;
            var userId = group.Type == (byte)GroupType.Share ? UserId : ChildOrUserId;
            var status = _groupContract.IsGroupMember(userId, id);
            var isManager = _groupContract.IsManager(group, UserId);
            if (!isManager && status != CheckStatus.Normal)
            {
                return MessageView("您还不是该圈子的成员！");
            }
            ViewBag.IsManager = isManager;

            ViewData["group"] = group;
            ViewBag.Page = pageIndex;
            //圈成员
            var membersResult = _groupContract.GroupMembers(id, includeParents: true,
                page: DPage.NewPage(pageIndex - 1, pageSize));
            var members = new List<MemberDto>();
            if (membersResult.Status)
            {
                members =
                    membersResult.Data
                        .OrderBy(u => u.Id == group.ManagerId
                            ? 0
                            : (u.SubjectId > 0 ? u.SubjectId : (u.Id == userId ? 998 : 999)))
                        .ThenBy(u => u.AddedTime)
                        .ToList();
                ViewBag.Total = membersResult.TotalCount;
            }
            return View(members);
        }

        /// <summary> 查找圈子 </summary>
        [HttpGet]
        [Route("find")]
        [RoleAuthorize(UserRole.Teacher | UserRole.Student, "/")]
        public ActionResult Find(string keyword, int groupType = -1, int pageindex = 1, int pagesize = 15)
        {
            List<int> types;
            if (groupType < 0)
            {
                types = new List<int>
                {
                    (int) GroupType.Class,
                    (int) GroupType.Share
                };
                if (CurrentUser.IsTeacher())
                {
                    types.Add((int)GroupType.Colleague);
                }
            }
            else
            {
                types = new List<int> { groupType };
            }
            if (string.IsNullOrWhiteSpace(keyword))
            {
                types.Remove((int)GroupType.Class);
                types.Remove((int)GroupType.Colleague);
            }
            if (types.IsNullOrEmpty())
                return View(new List<GroupDto>());

            var page = DPage.NewPage(pageindex - 1, pagesize);
            var search = new SearchGroupDto
            {
                Keyword = keyword,
                Page = page.Page,
                Size = page.Size,
                Types = types
            };

            var result = _groupContract.SearchGroups(search);
            ViewBag.Keyword = keyword;
            ViewBag.Total = result.TotalCount;
            ViewBag.Page = search.Page + 1;
            ViewBag.Size = search.Size;
            return View((List<GroupDto>)(result.Data ?? new List<GroupDto>()));
        }

        /// <summary> 创建圈子 </summary>
        [HttpGet]
        [DAuthorize()]
        [Route("create")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [DAuthorize]
        [Route("success/{id:regex([0-9a-z]{32})}")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult CreateSuccess(string id)
        {
            var group = _groupContract.LoadById(id);
            return View(group.Data);
        }

        /// <summary> 申请加入 </summary>
        [HttpGet]
        [DAuthorize]
        [Route("apply/{id:regex([0-9a-z]{32})}")]
        public ActionResult Apply(string id)
        {
            //验证逻辑
            var groupResult = _groupContract.LoadById(id);
            if (!groupResult.Status)
                return MessageView("圈子未找到！");
            var group = groupResult.Data;
            var user = CurrentUser;
            switch (group.Type)
            {
                case (byte)GroupType.Class:
                    if (CurrentUser.IsParents())
                    {
                        if (Children.IsNullOrEmpty())
                        {
                            //家长需绑定
                            return MessageView("您需要先绑定学生帐号才能加入班级圈", returnText: "绑定学生",
                                returnUrl: Consts.Config.AccountSite + "/bind/child?return_url=" + RawUrl.UrlEncode());
                        }
                        var child = Children[0];
                        user = new UserDto
                        {
                            Id = child.Id,
                            Name = child.Name
                        };
                    }
                    if (CurrentUser.Role == (byte)UserRole.Caird)
                        return MessageView("游客身份不能申请加入班级圈！");
                    break;
                case (byte)GroupType.Colleague:
                    if (!CurrentUser.IsTeacher())
                        return MessageView("同事圈只有教师才能加入！");
                    break;
                case (byte)GroupType.Share:
                    break;
            }
            ViewData["UserOrChild"] = user;

            var status = _groupContract.IsGroupMember(user.Id, id);
            if (status == CheckStatus.Normal)
            {
                //已经是圈内成员则直接进入
                return RedirectToAction("Item", "Group", new RouteValueDictionary
                {
                    {"id", id}
                });
            }
            if (status == CheckStatus.Pending)
            {
                //等待审核
                ViewBag.Pending = true;
            }
            if (group.Type == (byte)GroupType.Share)
            {
                var share = (ShareGroupDto)group;
                if (share.JoinAuth == (byte)GroupJoinAuth.Public)
                {
                    //公开的，直接加入
                    var result = _groupContract.JoinGroup(id, UserId);
                    if (result.Status)
                    {
                        return MessageView("加入圈子成功！", returnUrl: "/group/" + id, returnText: "进入圈子");
                    }
                    return MessageView(result.Message);
                }
            }
            return View(group);
        }

        [AjaxOnly]
        [Route("agency-selector")]
        public ActionResult AgencySelector(int stage = 1, int code = 5101, string keyword = null, string agencyId = null)
        {
            var areas = new Dictionary<int, string>
            {
                {5101, "全市"}
            };
            foreach (var area in _systemContract.Areas(5101))
            {
                areas.Add(area.Id, area.Name);
            }
            ViewData["Areas"] = areas;
            ViewBag.Stage = stage;
            ViewBag.Code = code;
            ViewBag.Keyword = keyword;
            ViewBag.AgencyId = agencyId;
            ViewBag.AgencyId = agencyId;
            var agencies = _systemContract.AgencyList((StageEnum)stage, code, AgencyType.K12, keyword);
            return PartialView(agencies.ToList());
        }

        /// <summary> 图片选择 </summary>
        /// <param name="image"></param>
        /// <param name="type"></param>
        [HttpGet]
        [Route("choose-image")]
        public ActionResult ChooseImage(string image, int type = 0)
        {
            ViewBag.Image = image.UrlDecode();
            var list = new List<RecommendImageItem>();
            var config = ConfigUtils<RecommendImageConfig>.Config;
            if (config != null)
            {
                var recommend = config.Recommends.FirstOrDefault(t => (int)t.Type == type);
                if (recommend != null)
                {
                    list = recommend.Images.OrderBy(t => t.Sort).ToList();
                }
            }
            return PartialView("~/Views/Shared/Helper/ChooseImage.cshtml", list);
        }

        #endregion

        #region Ajax

        [HttpGet]
        [Route("current-agency")]
        public ActionResult Agency()
        {
            var agency = CurrentAgency;
            if (agency == null)
                return DeyiJson(DResult.Error("无当前机构"));
            return DeyiJson(DResult.Succ(new
            {
                id = agency.AgencyId,
                stage = agency.Stage,
                name = agency.AgencyName
            }));
        }

        [HttpGet]
        [Route("subjects")]
        public ActionResult Subjects()
        {
            var subjects = SystemCache.Instance.Subjects();
            return DeyiJson(DResult.Succ(subjects.Select(s => new { id = s.Key, name = s.Value })));
        }
        /// <summary> 提交入圈申请 </summary>
        /// <param name="groupId"></param>
        /// <param name="message"></param>
        /// <param name="trueName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [AjaxOnly]
        [DAuthorize]
        public ActionResult Apply(string groupId, string message, string trueName = null, long userId = 0)
        {
            var result = _groupContract.ApplyGroup(groupId, userId <= 0 ? ChildOrUserId : userId, message, trueName);
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [Route("create")]
        [HttpPost]
        [DAuthorize]
        public ActionResult Create(CreateGroupDto createDto)
        {
            var group = new GroupDto
            {
                Type = createDto.Type,
                Name = createDto.Name,
                GroupSummary = createDto.Summary,
                Capacity = 200,
                ManagerId = UserId,
                CreationTime = Clock.Now
            };
            DResult<GroupDto> result;
            switch (createDto.Type)
            {
                case (int)GroupType.Class:
                    var classGroup = @group.MapTo<ClassGroupDto>();
                    classGroup.Capacity = 200;
                    classGroup.AgencyId = createDto.AgencyId;
                    classGroup.GradeYear = createDto.Grade;
                    classGroup.Stage = createDto.Stage;
                    result = _groupContract.CreateGroup(classGroup, (int)UserRole.Teacher, createDto.UserName, createDto.IsManager);
                    break;
                case (int)GroupType.Colleague:
                    var colleagueGroup = @group.MapTo<ColleagueGroupDto>();
                    colleagueGroup.Capacity = 100;
                    colleagueGroup.AgencyId = createDto.AgencyId;
                    colleagueGroup.SubjectId = (createDto.IsManager ? createDto.SubjectId : CurrentUser.SubjectId);
                    colleagueGroup.Stage = createDto.Stage;
                    result = _groupContract.CreateGroup(colleagueGroup, (int)UserRole.Teacher, createDto.UserName, createDto.IsManager);
                    break;
                case (int)GroupType.Share:
                    var shareGroup = @group.MapTo<ShareGroupDto>();
                    shareGroup.Capacity = 5000;
                    shareGroup.ChannelId = createDto.ChannelId;
                    shareGroup.PostAuth = createDto.PostAuth;
                    shareGroup.JoinAuth = createDto.JoinAuth;
                    shareGroup.Tags = createDto.Tags;
                    result = _groupContract.CreateGroup(shareGroup, (int)UserRole.Teacher, createDto.UserName,
                        createDto.IsManager);
                    break;
                default:
                    result = DResult.Error<GroupDto>("圈子类型异常！");
                    break;
            }
            return DeyiJson(result, true);
        }

        [HttpPost]
        [AjaxOnly]
        [Route("update")]
        [DAuthorize]
        public ActionResult Update(UpdateGroupDto updateDto)
        {
            var result = _groupContract.Update(updateDto);
            return DeyiJson(result, true);
        }

        [HttpPost]
        [AjaxOnly]
        [Route("teachers")]
        [DAuthorize]
        public ActionResult Teachers(string id)
        {
            var groupResult = _groupContract.LoadById(id);
            if (!groupResult.Status)
                return DeyiJson(groupResult);
            //            var role = UserRole.Teacher;
            //            if (groupResult.Data.Type == (byte)GroupType.Share)
            //                role = UserRole.Caird;
            var result = _groupContract.GroupMembers(id, UserRole.Teacher);
            if (result.Status)
            {
                result.Data = result.Data.Where(t => t.Id != UserId);
            }
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("transfer")]
        [DAuthorize]
        public ActionResult Transfer(string groupId, long userId)
        {
            var result = _groupContract.TransferOwner(groupId, userId, UserId);
            return DeyiJson(result, true);
        }

        [HttpPost]
        [AjaxOnly]
        [Route("delete")]
        [DAuthorize]
        public ActionResult DeleteMember(string groupId, long userId)
        {
            var result = _groupContract.DeleteMember(groupId, userId, UserId);
            return DeyiJson(result, true);
        }

        [HttpPost]
        [AjaxOnly]
        [Route("quit")]
        [DAuthorize]
        public ActionResult Quit(string id)
        {
            var result = _groupContract.QuitGroup(UserId, id);
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("dissolution")]
        [DAuthorize]
        public ActionResult Dissolution(string id)
        {
            var result = _groupContract.DissolutionGroup(id, UserId);
            return DeyiJson(result, true);
        }

        [HttpPost]
        [AjaxOnly]
        [Route("verify")]
        [DAuthorize]
        public ActionResult Verify(string id, bool pass, string message = null)
        {
            var result = _groupContract.Verify(id, pass ? CheckStatus.Normal : CheckStatus.Refuse, message);
            return DeyiJson(result);
        }

        #endregion

        [Route("export-members")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public void ExportMembers(string groupId)
        {
            var groupResult = _groupContract.LoadById(groupId);
            if (!groupResult.Status || groupResult.Data == null)
                return;
            var groupName = groupResult.Data.Name;
            var result = _groupContract.GroupMembers(groupId);
            if (!result.Status || result.Data == null)
                return;
            const int roles = ((byte)UserRole.Student | (byte)UserRole.Teacher);
            var dict = result.Data.GroupBy(t => (t.Role & roles))
                .OrderBy(t => t.Key)
                .ToDictionary(k => (byte)k.Key, v => v.Select(t => new
                {
                    t.Name,
                    t.Code,
                    t.SubjectName
                }).ToList());
            var ds = new DataSet();
            foreach (var item in dict)
            {
                var title = item.Key.GetEnumText<UserRole, byte>();
                var dt = new DataTable(title + "列表");
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("GroupName", typeof(string));
                if (item.Key == (byte)UserRole.Teacher)
                {
                    dt.Columns.Add("Subject", typeof(string));
                    dt.Rows.Add("姓名", "得一号", "班级圈名", "科目");
                }
                else
                {
                    dt.Rows.Add("姓名", "得一号", "班级圈名");
                }

                foreach (var user in item.Value)
                {
                    if (item.Key == (byte)UserRole.Teacher)
                        dt.Rows.Add(user.Name, user.Code, groupName, user.SubjectName ?? string.Empty);
                    else
                        dt.Rows.Add(user.Name, user.Code, groupName);
                }
                ds.Tables.Add(dt);
            }
            DownloadLogger.LogAsync(UserId, "圈子成员");
            ExcelHelper.Export(ds, groupName + "-成员列表.xls");
        }
    }
}