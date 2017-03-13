using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Portal.Services.Contracts;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.Portal.Controllers
{
    [RoutePrefix("user")]
    public class UserController : DController
    {
        private readonly IHomePageContract _pageContract;
        private readonly IGroupContract _groupContract;
        public UserController(IUserContract userContract, IGroupContract groupContract, IHomePageContract pageContract)
            : base(userContract)
        {
            _groupContract = groupContract;
            _pageContract = pageContract;
        }

        [DAuthorize]
        public ActionResult Index()
        {
            ViewBag.PageNav = 1;
            ViewBag.IsOwner = true;
            var groupResult = _groupContract.Groups(ChildOrUserId, loadMessageCount: true);
            if (groupResult.Status && groupResult.TotalCount > 0)
                ViewData["groups"] = groupResult.Data.ToList();
            ViewBag.VisitCount = UserContract.Visit(ChildOrUserId, ChildOrUserId);
            var config = ConfigUtils<PersonalConfig>.Config;
            if (config != null)
            {
                var list = CurrentUser.IsTeacher() ? config.Teachers : config.Students;
                if (!list.IsNullOrEmpty())
                    ViewData["topics"] = CurrentIocManager.Resolve<IAdvertContract>().Adverts(list.ToList());
            }
            var user = CurrentUser.IsParents() ? UserContract.Load(ChildOrUserId) : CurrentUser;
            ViewBag.SetTarget = (user.IsStudent() && CurrentAgency.Stage < (byte)StageEnum.HighSchool);
            return View(user);
        }

        /// <summary> 用户主页 </summary>
        [Route("{code:regex(\\d{5})}")]
        public ActionResult Visit(string code)
        {
            if (CurrentUser != null && CurrentUser.Code == code)
            {
                return RedirectToAction("Index");
            }
            var user = UserContract.LoadByCode(code);
            if (!user.Status)
                return MessageView(user.Message);
            var visitor = ChildOrUserId;
            if (CurrentUser != null && CurrentUser.IsParents() && Children.IsNullOrEmpty())
                visitor = 0;
            if (visitor > 0)
            {
                ViewData["newImpressions"] = UserContract.LastImpressions(user.Data.Id, visitor);
            }

            ViewBag.VisitCount = UserContract.Visit(user.Data.Id, visitor);
            return View("~/Views/User/Index.cshtml", user.Data);
        }

        /// <summary> 用户主页 </summary>
        [Route("visit/{id}")]
        public ActionResult Visit(long id)
        {
            if (CurrentUser != null && ChildOrUserId == id)
            {
                return RedirectToAction("Index");
            }
            var user = UserContract.Load(id);
            var visitor = ChildOrUserId;
            if (CurrentUser != null && CurrentUser.IsParents() && Children.IsNullOrEmpty())
                visitor = 0;
            if (visitor > 0)
            {
                ViewData["newImpressions"] = UserContract.LastImpressions(user.Id, visitor);
            }
            ViewBag.VisitCount = UserContract.Visit(user.Id, visitor);
            return View("~/Views/User/Index.cshtml", user);
        }


        [DAuthorize]
        [Route("history")]
        public ActionResult History()
        {
            var agencyList = new List<UserAgencyDto>();
            var agencyResult = UserContract.AgencyList(ChildOrUserId, DPage.NewPage(0, 30));
            if (agencyResult.Status)
            {
                agencyList = agencyResult.Data.OrderBy(t => t.Status)
                    .ThenByDescending(t => t.Start).ToList();
            }
            return PartialView("~/Views/User/_HistoryAgency.cshtml", agencyList);
        }

        [DAuthorize(true, false)]
        [Route("role")]
        public ActionResult Role()
        {
            //仅游客身份可进入
            if (CurrentUser.Role != (byte)UserRole.Caird)
                return Redirect(Consts.Config.MainSite);
            ViewData["subjects"] = SystemCache.Instance.Subjects();
            return View();
        }

        [HttpGet]
        [Route("complete")]
        [DAuthorize(needAgency: false)]
        public ActionResult Complete()
        {
            if (CurrentAgency != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        [DAuthorize]
        [Route("introduce")]
        public ActionResult Introduce()
        {
            var introduce = _pageContract.RecommendImpression(CurrentUser.Role);
            if (introduce == null)
                return RedirectToAction("RecommendGroups");
            ViewBag.IsTeacher = CurrentUser.IsTeacher();
            return View(introduce);
        }

        [HttpGet]
        [DAuthorize]
        [Route("rec-groups")]
        public ActionResult RecommendGroups()
        {
            var agency = CurrentAgency;
            if (CurrentUser.IsTeacher())
            {
                var classList = _groupContract.SearchGroups(new SearchGroupDto
                {
                    AgencyId = agency.AgencyId,
                    Types = new List<int> { (byte)GroupType.Class },
                    Size = 12
                });
                var colleagueList = _groupContract.SearchGroups(new SearchGroupDto
                {
                    AgencyId = agency.AgencyId,
                    Types = new List<int> { (byte)GroupType.Colleague },
                    SubjectId = CurrentUser.SubjectId,
                    Size = 6
                });
                if ((!classList.Status || classList.TotalCount == 0) &&
                    (!colleagueList.Status || colleagueList.TotalCount == 0))
                {
                    return Redirect(Consts.Config.MainSite + "/user");
                }
                ViewData["classList"] = classList.Data.ToList();
                ViewData["colleagueList"] = colleagueList.Data.ToList();
            }
            else
            {
                var classList = _groupContract.SearchGroups(new SearchGroupDto
                {
                    AgencyId = agency.AgencyId,
                    Types = new List<int> { 0 },
                    GradeYear = agency.Start.Year,
                    Size = 18
                });
                if (!classList.Status || classList.TotalCount == 0)
                {
                    return Redirect(Consts.Config.MainSite + "/user/rec-targets");
                }
                ViewData["classList"] = classList.Data.ToList();
            }
            return View();
        }

        [HttpGet]
        [DAuthorize]
        [Route("rec-targets")]
        public ActionResult RecommendTargets()
        {
            var agency = CurrentAgency;
            if (CurrentUser.IsTeacher() || agency.Stage == (byte)StageEnum.HighSchool)
            {
                return Redirect(Consts.Config.MainSite + "/user");
            }
            var targets = _pageContract.TargetAgencies((byte)(agency.Stage + 1));
            if (targets == null || !targets.Any())
            {
                return Redirect(Consts.Config.MainSite + "/user");
            }
            return View(targets);
        }

        #region Ajax

        [Route("init-data")]
        public ActionResult InitData(long userId)
        {
            var agencyList = new List<UserAgencyDto>();
            var agencyResult = UserContract.AgencyList(userId, DPage.NewPage(0, 3));
            if (agencyResult.Status)
            {
                agencyList = agencyResult.Data
                    .OrderByDescending(t => (t.Status == 2 ? -1 : t.Status))
                    .ThenBy(t => t.Start)
                    .ToList();
            }
            int targetCount = 0;
            var target = agencyList.FirstOrDefault(t => t.Status == (byte)UserAgencyStatus.Target);
            if (target != null)
            {
                targetCount = _pageContract.TargetCount(target.AgencyId);
            }
            return DeyiJson(new
            {
                agencies = agencyList,
                targetCount,
                visitors = _pageContract.UserLastVisitor(userId, 10)
            });
        }

        [HttpPost]
        [DAuthorize]
        [Route("edit-signature")]
        public ActionResult EditSignature(string content)
        {
            return DeyiJson(UserContract.EditSignature(ChildOrUserId, content));
        }

        /// <summary> 印象贴纸 </summary>
        [AjaxOnly]
        [Route("impressions")]
        public ActionResult Impressions(long userId, int page, int size)
        {
            return DeyiJson(_pageContract.ImpressionList(userId, DPage.NewPage(page, size), ChildOrUserId));
        }

        /// <summary> 经典语录 </summary>
        [AjaxOnly]
        [Route("quotations")]
        public ActionResult Quotations(long userId, int page, int size)
        {
            var list = UserContract.QuotationsList(userId, DPage.NewPage(page, size));
            if (list.Status && list.Data.Any())
            {
                list.Data.Each(t =>
                {
                    t.IsOwner = (t.UserId == ChildOrUserId || t.CreatorId == ChildOrUserId);
                    t.Supported = t.SupportCount > 0 && t.SupportList.Contains(ChildOrUserId);
                });
            }
            return DeyiJson(list);
        }

        /// <summary> 最近访问 </summary>
        [AjaxOnly]
        [Route("last-visited")]
        public ActionResult LastVisit(long userId)
        {
            return DeyiJson(_pageContract.UserLastVisitor(userId, 10));
        }

        [AjaxOnly]
        [Route("related-teachers")]
        public ActionResult RelatedTeachers(long userId)
        {
            return DeyiJson(_pageContract.RelatedTeachers(userId, 6));
        }

        [AjaxOnly]
        [Route("related-students")]
        public ActionResult RelatedStudents(long userId)
        {
            return DeyiJson(_pageContract.RelatedStudents(userId, 10));
        }

        [AjaxOnly]
        [Route("hot-agencies")]
        public ActionResult HotAgencies(long userId)
        {
            return DeyiJson(_pageContract.HotAgencies(userId, 6));
        }

        /// <summary> 添加关系 </summary>
        [HttpPost]
        [DAuthorize]
        [Route("add-relation")]
        public ActionResult AddRelation(UserAgencyInputDto dto)
        {
            dto.UserId = ChildOrUserId;
            return DeyiJson(UserContract.AddAgency(dto));
        }

        /// <summary> 删除关系 </summary>
        [HttpPost]
        [DAuthorize]
        [Route("remove-relation")]
        public ActionResult RemoveRelation(string id)
        {
            return DeyiJson(UserContract.RemoveAgency(id, ChildOrUserId));
        }

        [HttpPost]
        [DAuthorize]
        [Route("update-relation")]
        public ActionResult UpdateRelation(UpdateRelationInputDto dto)
        {
            return DeyiJson(UserContract.UpdateRelation(dto));
        }

        #region 贴纸 & 语录

        /// <summary> 热门贴纸 </summary>
        [AjaxOnly]
        [Route("hot-impressions")]
        public ActionResult HotImpression(long userId)
        {
            var user = UserContract.Load(userId);
            if (user == null)
                return DeyiJson(DResult.Error("用户不存在！"));
            var role = (user.IsTeacher() ? UserRole.Teacher : UserRole.Student);
            var hotList = UserContract.HotImpressions(role, 8) ?? new List<string>();
            if (hotList.Count == 8)
                return DeyiJson(hotList);
            var recommend = _pageContract.RecommendImpression((byte)role);
            if (recommend == null)
                return DeyiJson(hotList);
            var len = 8 - hotList.Count;
            hotList.AddRange(recommend.FeatureList.Take(len));
            return DeyiJson(hotList);
        }

        /// <summary> 添加贴纸 </summary>
        [AjaxOnly]
        [HttpPost]
        [DAuthorize]
        [Route("add-impression")]
        public ActionResult AddImpression(ImpressionInputDto dto)
        {
            if (dto.UserId == 0)
            {
                dto.UserId = ChildOrUserId;
            }
            dto.CreatorId = ChildOrUserId;
            return DeyiJson(UserContract.AddImpression(dto), true);
        }

        [AjaxOnly]
        [HttpPost]
        [DAuthorize]
        [Route("add-quotations")]
        public ActionResult AddQuotations(QuotationsInputDto dto)
        {
            dto.CreatorId = UserId;
            return DeyiJson(UserContract.AddQuotations(dto), true);
        }

        [AjaxOnly]
        [HttpPost]
        [DAuthorize]
        [Route("support-impression")]
        public ActionResult SupportImpression(string id)
        {
            return DeyiJson(UserContract.SupportImpression(id, ChildOrUserId), true);
        }

        [AjaxOnly]
        [HttpPost]
        [DAuthorize]
        [Route("cancel-support-impression")]
        public ActionResult CancelSupportImpression(string id)
        {
            return DeyiJson(UserContract.CancelSupportImpression(id, ChildOrUserId), true);
        }

        [AjaxOnly]
        [HttpPost]
        [DAuthorize]
        [Route("support-quotations")]
        public ActionResult SupportQuotations(string id)
        {
            return DeyiJson(UserContract.SupportQuotations(id, ChildOrUserId), true);
        }

        [AjaxOnly]
        [HttpPost]
        [DAuthorize]
        [Route("cancel-support-quotations")]
        public ActionResult CancelSupportQuotations(string id)
        {
            return DeyiJson(UserContract.CancelSupportQuotations(id, ChildOrUserId), true);
        }

        [AjaxOnly]
        [DAuthorize]
        [Route("delete-impression")]
        public ActionResult DeleteImpression(string id)
        {
            return DeyiJson(UserContract.DeleteImpression(id, ChildOrUserId));
        }

        [AjaxOnly]
        [DAuthorize]
        [Route("delete-quotations")]
        public ActionResult DeleteQuotations(string id)
        {
            return DeyiJson(UserContract.DeleteQuotations(id, ChildOrUserId));
        }
        #endregion

        [AjaxOnly]
        [HttpPost]
        [Route("bind-role")]
        public ActionResult BindRole(byte role, int subject_id = 0)
        {
            if (role == (byte)UserRole.Teacher && subject_id < 1)
                return DJson.Json("请选择任教学科");
            var result = UserContract.Update(new UserDto
            {
                Id = UserId,
                Role = role,
                SubjectId = role == (byte)UserRole.Teacher ? subject_id : 0
            });
            return DJson.Json(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("complete")]
        public ActionResult Complete(UserCompleteInputDto inputDto)
        {
            inputDto.UserId = ChildOrUserId;
            var result = UserContract.CompleteInfo(inputDto);
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [Route("group-list")]
        public ActionResult GroupList(int type, int gradeYear = 0, int count = 12)
        {
            var agency = CurrentAgency;
            var list = _groupContract.SearchGroups(new SearchGroupDto
            {
                AgencyId = agency.AgencyId,
                Types = new List<int> { type },
                GradeYear = gradeYear,
                Size = count
            });
            return DeyiJson(list);
        }

        #endregion

    }
}