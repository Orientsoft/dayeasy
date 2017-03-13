using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.SiteManager)]
    [RoutePrefix("operate/advert")]
    public class AdvertController : AdminController
    {
        #region 注入

        private readonly IAdvertContract _contract;
        public AdvertController(
            IUserContract userContract,
            IManagementContract managementContract,
            IAdvertContract adverContract)
            : base(userContract, managementContract)
        {
            _contract = adverContract;
        }

        #endregion

        #region View

        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            var categoryResult = _contract.Categorys();
            if (categoryResult.Status)
                ViewData["categorys"] = categoryResult.Data;

            if ("partial".Query("").IsNotNullOrEmpty())
                return PartialView("PartialList");

            return View();
        }

        [Route("detail/{id?}")]
        public ActionResult Detail(string id)
        {
            var categoryResult = _contract.Categorys();
            if (categoryResult.Status) ViewData["categorys"] = categoryResult.Data;
            if (!id.IsNotNullOrEmpty()) return View();
            AdvertDto advert = null;
            var advertResult = _contract.Advert(id);
            if (advertResult.Status) advert = advertResult.Data;
            return View(advert);
        }

        [Route("categorys")]
        public ActionResult Categorys()
        {
            var categoryResult = _contract.Categorys();
            var categorys = new List<AdvertCategoryDto>();
            if (categoryResult.Status && categoryResult.Data.Any())
                categorys = categoryResult.Data.ToList();
            return View(categorys);
        }

        #endregion

        #region Ajax

        #region Advert

        [HttpPost]
        [AjaxOnly]
        [Route("adverts")]
        public ActionResult Adverts(int index, int size, string category, string key)
        {
            if (--index < 0) index = 0;
            return DJson.Json(_contract.Adverts(index, size, category, key),
                namingType: NamingType.CamelCase);
        }

        [Route("edit")]
        public ActionResult AdvertEdit(AdvertDto dto)
        {
            return DJson.Json(_contract.Edit(dto), namingType: NamingType.CamelCase);
        }

        [Route("delete")]
        public ActionResult AdvertDelete(string id)
        {
            return DJson.Json(_contract.Delete(id));
        }

        #endregion

        #region Category

        [Route("category-edit")]
        public ActionResult CategoryEdit(string id, string name)
        {
            var json = _contract.CategoryEdit(new AdvertCategoryDto { Id = id, CategoryName = name });
            return DJson.Json(json, namingType: NamingType.CamelCase);
        }

        [Route("category-delete")]
        public ActionResult CategoryDelete(string id)
        {
            return DJson.Json(_contract.CategoryDelete(id));
        }

        #endregion

        #endregion
    }
}