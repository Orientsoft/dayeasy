using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Portal.Services.Contracts;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.Portal.Controllers
{
    /// <summary> 首页 </summary>
    public class HomeController : DController
    {
        private readonly IHomeContract _homeContract;

        public HomeController(IUserContract userContract, IHomeContract homeContract)
            : base(userContract)
        {
            _homeContract = homeContract;
        }

        #region Ajax

        [Route("check-login")]
        public ActionResult CheckLogin()
        {
            return new JsonpResult(new
            {
                user = CurrentUser,
                apps = UserApps
            }, "callback".Query(string.Empty));
        }

        [Route("search-groups")]
        public ActionResult SearchGroupsByCode(string codes)
        {
            return DeyiJson(_homeContract.GroupSearch(codes));
        }

        [Route("hot")]
        public ActionResult Hot()
        {

            return new JsonpResult(_homeContract.HotTopicAndGroup(), "callback".Query(string.Empty));
        }

        [Route("home-data")]
        public ActionResult HomeData()
        {
            return new JsonpResult(_homeContract.HomeData(CurrentUser), "callback".Query(string.Empty));
        }

        [Route("agency-search")]
        public ActionResult AgencySearch(string keyword, int stage = -1, int count = 5)
        {
            return new JsonpResult(_homeContract.AgencySearch(keyword, stage, count), "callback".Query(string.Empty));
        }
        #endregion
    }
}