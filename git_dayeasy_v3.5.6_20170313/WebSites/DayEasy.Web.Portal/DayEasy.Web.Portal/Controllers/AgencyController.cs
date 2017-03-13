using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Portal.Services.Contracts;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Portal.Controllers
{
    [RoutePrefix("agency")]
    public class AgencyController : DController
    {
        private readonly ISystemContract _systemContract;
        private readonly IHomePageContract _pageContract;

        public AgencyController(IUserContract userContract, ISystemContract systemContract,
            IHomePageContract pageContract)
            : base(userContract)
        {
            _systemContract = systemContract;
            _pageContract = pageContract;
        }

        /// <summary> 机构主页 </summary>
        [HttpGet]
        [Route("{id:regex([0-9a-z]{32}|[0-9a-z]{11})}")]
        public ActionResult Index(string id)
        {
            var visitor = ChildOrUserId;
            if (CurrentUser != null && CurrentUser.IsParents() && Children.IsNullOrEmpty())
                visitor = 0;
            var item = _systemContract.VisitAgency(id, visitor);
            if (item == null)
                return MessageView("机构不存在！");
            if (CurrentUser != null)
            {
                ViewData["relation"] = _pageContract.UserAgencyRelation(id, ChildOrUserId);
            }
            ViewBag.UserStage = (CurrentAgency == null ? -1 : CurrentAgency.Stage);
            return View(item);
        }

        [AjaxOnly]
        [Route("init-data")]
        public ActionResult AgencyHome(string agencyId)
        {
            return DeyiJson(new
            {
                visitors = _pageContract.AgencyLastVisitor(agencyId, 10),
                otherTeachers = _pageContract.AgencyTeachers(agencyId),
                hotAgencies = _pageContract.OftenAgenies(agencyId, 5),
                impressions = _pageContract.HotTeachers(agencyId, 4)
            });
        }

        [Route("hot-quotations")]
        public ActionResult HotQuotations(string agencyId, int page, int size)
        {
            return DeyiJson(_pageContract.HotQuotations(agencyId, DPage.NewPage(page, size), ChildOrUserId));
        }
    }
}