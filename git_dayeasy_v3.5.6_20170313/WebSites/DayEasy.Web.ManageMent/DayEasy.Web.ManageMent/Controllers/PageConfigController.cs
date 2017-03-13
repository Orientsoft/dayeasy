using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Core.Config;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Config;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.SiteManager)]
    [RoutePrefix("operate")]
    public class PageConfigController : AdminController
    {
        private readonly IAdvertContract _contract;
        private readonly IGroupContract _groupContract;
        public PageConfigController(
            IUserContract userContract,
            IManagementContract managementContract,
            IAdvertContract adverContract,
            IGroupContract groupContract)
            : base(userContract, managementContract)
        {
            _contract = adverContract;
            _groupContract = groupContract;
        }

        #region Views
        [Route("index-config")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("home-config")]
        public ActionResult HomeConfig()
        {
            return View();
        }

        [Route("personal-config")]
        public ActionResult PersonalConfig()
        {
            return View();
        }

        [Route("index-config/generate")]
        [StaticFile("[saveHomePath]", true)]
        public ActionResult Generate()
        {
            var config = ConfigUtils<IndexConfig>.Config;
            if (config == null) return View();
            if (config.Carousels == null || !config.Carousels.Any() || config.Fixeds == null || !config.Fixeds.Any())
            {
                ViewData["errorMsg"] = "缺少轮播广告或固定广告";
                return View();
            }
            if (config.Sections == null || !config.Sections.Any())
            {
                ViewData["errorMsg"] = "没有任何版块配置";
                return View();
            }

            #region 广告文件
            var advertIds = new List<string>();
            advertIds.AddRange(config.Carousels);
            advertIds.AddRange(config.Fixeds);
            foreach (var section in config.Sections)
            {
                if (section.Tabs == null || !section.Tabs.Any())
                    continue;
                if (section.Sources != null && section.Sources.Any())
                    advertIds.AddRange(section.Sources);
                foreach (var tab in section.Tabs)
                {
                    if (tab.Sources != null && tab.Sources.Any())
                        advertIds.AddRange(tab.Sources);
                    if (tab.Adverts != null && tab.Adverts.Any())
                        advertIds.AddRange(tab.Adverts);
                    if (tab.Groups != null && tab.Groups.Any())
                        advertIds.AddRange(from g in tab.Groups where g.Type == 1 select g.Id);
                }
            }
            var adverts = _contract.Adverts(advertIds);
            if (!adverts.Any())
            {
                ViewData["errorMsg"] = "没有查询图文广告数据";
                return View();
            }
            #endregion

            #region 图文数据
            var indexAdvert = new IndexAdvertDto
            {
                Carousels = adverts.Where(a => config.Carousels.Contains(a.Id)).OrderBy(a => a.Index).ToList(),
                Fixeds = adverts.Where(a => config.Fixeds.Contains(a.Id)).OrderBy(a => a.Index).ToList(),
                Sections = new List<SectionAdvertDto>()
            };
            foreach (var section in config.Sections)
            {
                if (section.Sources == null) section.Sources = new string[] { };
                var sectionAdvert = new SectionAdvertDto
                {
                    Sources = adverts.Where(a => section.Sources.Contains(a.Id)).OrderBy(a => a.Index).ToList(),
                    Tabs = new List<TabAdvertDto>()
                };
                if (section.Tabs == null) section.Tabs = new Tab[] { };
                foreach (var tab in section.Tabs)
                {
                    if (tab.Sources == null) tab.Sources = new string[] { };
                    if (tab.Adverts == null) tab.Adverts = new string[] { };
                    if (tab.Groups == null) tab.Groups = new Config.Group[] { };
                    var tabAdvert = new TabAdvertDto
                    {
                        Sources = adverts.Where(a => tab.Sources.Contains(a.Id)).OrderBy(a => a.Index).ToList(),
                        Adverts = adverts.Where(a => tab.Adverts.Contains(a.Id)).OrderBy(a => a.Index).ToList(),
                        Groups = new List<GroupDto>(),
                        GroupCodes = tab.Groups.Where(g => g.Type == 0).Select(g => g.Id).ToList()
                    };
                    var groupIds = tab.Groups.Where(g => g.Type == 1).Select(g => g.Id).ToList();
                    if (groupIds.Any())
                    {
                        tabAdvert.GroupCodes.AddRange(
                            adverts.Where(a => groupIds.Contains(a.Id))
                                .OrderBy(a => a.Index).Select(a => a.ForeignKey));
                    }
                    sectionAdvert.Tabs.Add(tabAdvert);
                }
                indexAdvert.Sections.Add(sectionAdvert);
            }
            #endregion

            #region 圈子数据

            var codes = new List<string>();
            indexAdvert.Sections.ForEach(s => s.Tabs.ForEach(t => codes.AddRange(t.GroupCodes)));
            if (codes.Any())
            {
                var groupResult = _groupContract.SearchGroupsByCode(codes);
                if (groupResult.Status && groupResult.Data != null && groupResult.Data.Any())
                {
                    indexAdvert.Sections.ForEach(s => s.Tabs.ForEach(t =>
                    {
                        if (!t.GroupCodes.Any()) return;
                        t.GroupCodes.ForEach(code => t.Groups.AddRange(groupResult.Data.Where(g => g.Code == code)));
                    }));
                }
            }

            #endregion

            return View(indexAdvert);
        }

        [Route("home-config/generate")]
        [StaticFile("[saveHomePath]", true)]
        public ActionResult HomeGenerate()
        {
            var config = ConfigUtils<HomeConfig>.Config;
            if (config == null)
                return View(new HomePageDataDto());
            var ids = new List<string>();
            ids.AddRange(config.Coverages);
            ids.AddRange(config.Resources);
            ids.AddRange(config.Examples.Left);
            ids.AddRange(config.Examples.Right);
            var list = _contract.Adverts(ids).ToDictionary(k => k.Id, v => v);
            Func<string[], List<AdvertDto>> getList = idList =>
            {
                return list.Where(t => idList.Contains(t.Key)).Select(t => t.Value).OrderBy(t => t.Index).ToList();
            };
            var dto = new HomePageDataDto
            {
                Coverages = getList(config.Coverages),
                Resources = getList(config.Resources),
                ExamplesLeft = getList(config.Examples.Left),
                ExamplesRight = getList(config.Examples.Right)
            };
            return View(dto);
        }

        #endregion

        #region Ajax

        [HttpPost]
        [Route("index-config/save")]
        public ActionResult ConfigSave(string data)
        {
            var config = data.JsonToObject<IndexConfig>();
            if (config == null) return DeyiJson(DResult.Error("没有获取到数据"));
            ConfigUtils<IndexConfig>.Instance.Set(config);
            return DeyiJson(DResult.Success);
        }

        [Route("index-config/data")]
        public ActionResult ConfigData()
        {
            var config = ConfigUtils<IndexConfig>.Config ?? new IndexConfig();
            return DJson.Json(DResult.Succ(config), namingType: NamingType.CamelCase);
        }

        [HttpPost]
        [Route("home-config/save")]
        public ActionResult HomeConfigSave(string data)
        {
            var config = data.JsonToObject<HomeConfig>();
            if (config == null) return DeyiJson(DResult.Error("没有获取到数据"));
            ConfigUtils<HomeConfig>.Instance.Set(config);
            return DeyiJson(DResult.Success);
        }

        [Route("home-config/data")]
        public ActionResult HomeConfigData()
        {
            var config = ConfigUtils<HomeConfig>.Config ?? new HomeConfig();
            return DJson.Json(DResult.Succ(config), namingType: NamingType.CamelCase);
        }

        [HttpPost]
        [Route("personal-config/save")]
        public ActionResult PersonalConfigSave(string data)
        {
            var config = data.JsonToObject<PersonalConfig>();
            if (config == null) return DeyiJson(DResult.Error("没有获取到数据"));
            ConfigUtils<PersonalConfig>.Instance.Set(config);
            return DeyiJson(DResult.Success);
        }

        [Route("personal-config/data")]
        public ActionResult PersonalConfigData()
        {
            var config = ConfigUtils<PersonalConfig>.Config ?? new PersonalConfig();
            return DJson.Json(DResult.Succ(config), namingType: NamingType.CamelCase);
        }
        #endregion
    }
}