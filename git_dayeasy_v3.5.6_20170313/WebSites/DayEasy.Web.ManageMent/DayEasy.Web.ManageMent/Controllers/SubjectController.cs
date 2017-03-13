using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Services.Helper;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Web.Mvc;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.SystemManager)]
    [RoutePrefix("sys/subjects")]
    public class SubjectController : AdminController
    {
        public SubjectController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        #region Views
        /// <summary>
        /// 列表首页
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public ActionResult Index()
        {
            var subjects = ManagementContract.Subjects();

            var qTypes = ManagementContract.QuestionTypes();

            ViewData["qTypeList"] = qTypes;

            return View(subjects);
        }

        /// <summary>
        /// 编辑--显示
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("edit")]
        public ActionResult Edit(int id = -1)
        {
            if (id > 0)
            {
                var editModel = ManagementContract.Subject(id);
                if (editModel != null)
                {
                    var qTypes = ManagementContract.QuestionTypes();

                    ViewData["qTypeList"] = qTypes;

                    ViewData["qTypeStatus"] = MvcHelper.EnumToDropDownList<TempStatus>(editModel.Status);

                    return PartialView(editModel);
                }
            }
            return Content("参数错误");
        }

        #endregion

        #region Ajax
        /// <summary> 删除 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("delete")]
        public ActionResult Delete(int id)
        {
            var result = ManagementContract.DeleteSubject(id);
            if (result.Status)
            {
                SystemCache.Instance.Clear();
            }
            return DeyiJson(result, true);
        }

        /// <summary> 添加/修改 </summary>
        [HttpPost]
        [Route("add")]
        public ActionResult Add(SubjectDto dto)
        {
            var result = ManagementContract.InsertOrUpdateSubject(dto);
            if (result.Status)
            {
                SystemCache.Instance.Clear();
            }
            return DeyiJson(result);
        }

        #endregion
    }
}