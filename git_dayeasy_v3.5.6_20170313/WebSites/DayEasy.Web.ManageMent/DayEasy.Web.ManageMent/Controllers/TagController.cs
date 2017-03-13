using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Models;
using DayEasy.Utility.Extend;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [AdminRoles(AdminRole.SystemManager)]
    [RoutePrefix("sys/tags")]
    public class TagController : AdminController
    {
        private readonly TS_TagFacade _tagFacade = new TS_TagFacade();

        public TagController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }
        #region 首页--列表
        /// <summary>
        /// 首页--列表
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public ActionResult Index()
        {
            int pageIndex = RequestHelper.GetQueryInt32("pageIndex", 1);
            int pageSize = RequestHelper.GetQueryInt32("pageSize", 10);
            int t = RequestHelper.GetQueryInt32("t", -1);
            int s = RequestHelper.GetQueryInt32("s", -1);
            string k = RequestHelper.GetQueryString("k");

            var condition = ExpressionExtension.True<TS_Tag>();
            if (t > -1)
            {
                condition = condition.And(u => u.TagType == t);
            }

            if (s > -1)
            {
                condition = condition.And(u => u.Status == s);
            }

            if (!string.IsNullOrEmpty(k))
            {
                condition = condition.And(u => u.TagName.Contains(k) || u.FullPinYin.Contains(k) || u.SimplePinYin.Contains(k));
            }

            int totalCount = 0;
            var tagList = _tagFacade.GetEntitiesByPage(condition, pageIndex, pageSize, u => u.UsedCount, false, out totalCount).ToList();

            ViewData["totalCount"] = totalCount;
            ViewData["tagType"] = MVCHelper.EnumToDropDownList(typeof(TagType), t, true);
            ViewData["tagStatus"] = MVCHelper.EnumToDropDownList(typeof(TagStatus), s, true);

            return System.Web.UI.WebControls.View(tagList);
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete()
        {
            int id = RequestHelper.GetFormInt32("id", -1);

            if (id > -1)
            {
                var delModel = _tagFacade.GetEntityByWhereLambda(u => u.TagID == id);
                if (delModel != null)
                {
                    if (delModel.Status == (byte)TagStatus.Delete)
                    {
                        var result = _tagFacade.DeleteEntity(delModel);
                        if (result > 0)
                        {
                            return System.Web.Helpers.Json(new JsonResultBase(true, "操作成功！"), JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        delModel.Status = (byte)TagStatus.Delete;

                        var result = _tagFacade.UpdateEntity(delModel);
                        if (result != null)
                        {
                            return System.Web.Helpers.Json(new JsonResultBase(true, "操作成功！"), JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            return System.Web.Helpers.Json(new JsonResultBase(false, "操作失败！"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add()
        {
            string tagName = RequestHelper.GetFormString("tagName");
            int type = RequestHelper.GetFormInt32("type", -1);

            if (string.IsNullOrEmpty(tagName))
            {
                return System.Web.Helpers.Json(new JsonResultBase(false, "请填写名称！"), JsonRequestBehavior.AllowGet);
            }
            if (type < 0)
            {
                return System.Web.Helpers.Json(new JsonResultBase(false, "请先选择类型！"), JsonRequestBehavior.AllowGet);
            }

            var newTag = new TS_Tag();

            newTag.TagName = tagName;
            newTag.FullPinYin = Chs2Spell.ChsString2Spell(tagName);
            newTag.SimplePinYin = Chs2Spell.GetHeadOfChs(tagName);
            newTag.UsedCount = 0;
            newTag.Status = (byte)TagStatus.Normal;
            newTag.TagType = (byte)type;

            int id = RequestHelper.GetFormInt32("id", -1);

            if (id > 0)//修改
            {
                int status = RequestHelper.GetFormInt32("status", (byte)TagStatus.Normal);
                string fullPy = RequestHelper.GetFormString("fullPy");
                string simplyPy = RequestHelper.GetFormString("simplyPy");
                var editModel = _tagFacade.GetEntityByWhereLambda(u => u.TagID == id);
                if (editModel != null)
                {
                    editModel.TagName = newTag.TagName;
                    editModel.FullPinYin = string.IsNullOrEmpty(fullPy) ? Chs2Spell.ChsString2Spell(newTag.TagName) : fullPy;
                    editModel.SimplePinYin = string.IsNullOrEmpty(simplyPy) ? Chs2Spell.GetHeadOfChs(newTag.TagName) : simplyPy;
                    editModel.UsedCount = RequestHelper.GetFormInt32("userCount", 0);
                    editModel.Status = (byte)status;
                    editModel.TagType = newTag.TagType;

                    var result = _tagFacade.UpdateEntity(editModel);
                    if (result != null)
                    {
                        return System.Web.Helpers.Json(new JsonResultBase(true, "操作成功！"), JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else//添加
            {
                var result = _tagFacade.AddEntity(newTag);
                if (result != null)
                {
                    return System.Web.Helpers.Json(new JsonResultBase(true, "操作成功！"), JsonRequestBehavior.AllowGet);
                }
            }

            return System.Web.Helpers.Json(new JsonResultBase(false, "操作失败！"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 编辑--显示
        /// <summary>
        /// 编辑--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            int id = RequestHelper.GetFormInt32("id", -1);
            if (id > 0)
            {
                var editModel = _tagFacade.GetEntityByWhereLambda(u => u.TagID == id);
                if (editModel != null)
                {
                    ViewData["tagType"] = MVCHelper.EnumToDropDownList(typeof(TagType), editModel.TagType);
                    ViewData["tagStatus"] = MVCHelper.EnumToDropDownList(typeof(TagStatus), editModel.Status);

                    return PartialView(editModel);
                }

            }
            return Content("参数错误");
        }
        #endregion

        
    }
}