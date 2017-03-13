using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Office;
using DayEasy.Utility.Extend;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Services.Helper;

namespace DayEasy.Web.ManageMent.Controllers
{
    /// <summary> 试卷管理 </summary>
    [ManagerRoles(ManagerRole.OperationManager)]
    public class PaperController : AdminController
    {
        public PaperController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        { }

        public ActionResult Index(PaperSearchDto searchDto)
        {
            searchDto.Page = "pageIndex".Query(1) - 1;
            searchDto.Size = "pageSize".Query(25);
            var subjects = new List<SelectListItem>
            {
                new SelectListItem{Text = "所有科目",Value = "0",Selected = searchDto.Subject<=0}
            };
            subjects.AddRange(SystemCache.Instance.Subjects().Select(t => new SelectListItem
            {
                Text = t.Value,
                Value = t.Key.ToString(),
                Selected = t.Key == searchDto.Subject
            }));
            ViewData["subject"] = subjects;
            ViewData["shareRange"] = MvcHelper.EnumToDropDownList<ShareRange>(searchDto.ShareRange, true, "所有范围");
            ViewData["status"] = MvcHelper.EnumToDropDownList<PaperStatus>(searchDto.Status, true, "所有状态");
            var paperResult = ManagementContract.PaperSearch(searchDto);
            ViewData["totalCount"] = paperResult.TotalCount;
            var uids = paperResult.Data.Select(p => p.AddedBy).Distinct();
            var users = UserContract.LoadList(uids);
            ViewData["creators"] = users;
            return View(paperResult.Data.ToList());
        }

        public ActionResult Sheet()
        {
            return View();
        }

        public void MakeSheet(string word)
        {
            var bmp = WordAssist.PaperSheet(word);
            CurrentContext.Response.ContentType = "image/png";
            bmp.Save(CurrentContext.Response.OutputStream, ImageFormat.Png);
        }

        public void DownloadSheet(string word)
        {
            var stream = WordAssist.DownLoadPaperSheet(word);
            var rep = CurrentContext.Response;
            rep.Clear();
            rep.ContentType = "application/msword";
            rep.Headers.Add("Content-Disposition", "attachment;filename=cart.doc");
            rep.BinaryWrite(stream.ToArray());
            rep.End();
        }
    }
}