using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Services.Helper;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.SystemManager)]
    [RoutePrefix("sys/qtypes")]
    public class QTypeController : AdminController
    {
        public QTypeController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        #region 列表
        /// <summary>
        /// 列表
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public ActionResult Index()
        {
            var qTypes = ManagementContract.QuestionTypes();

            return View(qTypes);
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("delete")]
        public ActionResult Delete(int id = -1)
        {
            var result = ManagementContract.DeleteQuestionType(id);
            if (result.Status)
            {
                SystemCache.Instance.Clear();
            }
            return DeyiJson(result, true);
        }
        #endregion

        #region 添加
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("add")]
        public ActionResult Add(QuestionTypeDto dto)
        {
            var result = ManagementContract.InsertOrUpdateQuestionType(dto);
            if (result.Status)
            {
                SystemCache.Instance.Clear();
            }
            return DeyiJson(result, true);
        }
        #endregion

        #region 编辑--显示
        /// <summary>
        /// 编辑--显示
        /// </summary>
        /// <returns></returns>
        [Route("edit")]
        public ActionResult Edit(int id = -1)
        {
            if (id > 0)
            {
                var editModel = ManagementContract.QuestionType(id);
                if (editModel != null)
                {
                    ViewData["qTypeStatus"] = MvcHelper.EnumToDropDownList<TempStatus>(editModel.Status);

                    return PartialView(editModel);
                }

            }
            return Content("参数错误");
        }
        #endregion
    }
}