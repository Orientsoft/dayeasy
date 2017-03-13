using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.SchoolBook;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.SystemManager)]
    [RoutePrefix("sys/sbooks")]
    public class ScBookChapterController : AdminController
    {
        private readonly ISystemContract _systemContract;
        public ScBookChapterController(
            IUserContract userContract,
            IManagementContract managementContract,
            ISystemContract systemContract)
            : base(userContract, managementContract)
        {
            _systemContract = systemContract;
        }

        #region View

        [Route("")]
        public ActionResult Index()
        {
            ViewData["subjects"] = _systemContract.SubjectDict();
            return View();
        }
        
        [Route("chapters")]
        public ActionResult Chapters (string code)
        {
            //自动截取5位教材编号
            var sbResult = _systemContract.GetSchoolBookByCode(code);
            if (!sbResult.Status)
            {
                ViewData["errorMsg"] = sbResult.Message;
                return View();
            }
            //章节列表
            var result = _systemContract.SbChapters(code);
            if (!result.Status)
            {
                ViewData["errorMsg"] = result.Message;
                return View();
            }
            //导航列表
            var navResult = _systemContract.SbChapterNav(code);
            if (!navResult.Status)
            {
                ViewData["errorMsg"] = navResult.Message;
                return View();
            }
            ViewData["sbook"] = sbResult.Data;
            ViewData["navs"] = navResult.Data;
            return View(result.Data.ToList());
        }
        
        #endregion

        #region Ajax

        [Route("sbook-list")]
        public ActionResult SBookList(byte stage,int subject)
        {
            return DeyiJson(_systemContract.SchoolBooks(stage, subject));
        }

        [Route("sbook-edit")]
        public ActionResult SBookEdit(byte stage,int subject,string title, string id = "")
        {
            var dto = new SchoolBookDto
            {
                Id = id,
                Stage = stage,
                SubjectId = subject,
                Title = title,
                Status = (byte) NormalStatus.Normal
            };
            return DeyiJson(id.IsNullOrEmpty()
                ? _systemContract.AddSchoolBook(dto)
                : _systemContract.EditSchoolBook(dto));
        }

        [Route("sbook-del")]
        public ActionResult SBookDelete(string id)
        {
            var dto = new SchoolBookDto
            {
                Id = id,
                Status = (byte) NormalStatus.Delete
            };
            return DeyiJson(_systemContract.EditSchoolBook(dto));
        }

        [Route("sbook-status")]
        public ActionResult SBookStatus(string id, byte status)
        {
            var dto = new SchoolBookDto
            {
                Id = id,
                Status = status
            };
            return DeyiJson(_systemContract.EditSchoolBook(dto));
        }

        [Route("chapter-edit")]
        public ActionResult SbChapterEdit(string code,string title, int sort,string id="")
        {
            var dto = new SchoolBookChapterDto
            {
                Id = id,
                Code = code,
                Title = title,
                Sort = sort
            };
            return DeyiJson(id.IsNullOrEmpty()
                ? _systemContract.AddSbChapter(dto)
                : _systemContract.EditSbChapter(dto));
        }

        [Route("chapter-del")]
        public ActionResult SbChapterDelete(string id)
        {
            var dto = new SchoolBookChapterDto
            {
                Id = id,
                Status = (byte) NormalStatus.Delete
            };
            return DeyiJson(_systemContract.EditSbChapter(dto));
        }

        [Route("search-kps")]
        public ActionResult SearchKps(byte stage, int subject, string key)
        {
            var kps = _systemContract.Knowledges(new SearchKnowledgeDto
            {
                Page = 0,
                Size = 10,
                Keyword = key,
                Stage = stage,
                SubjectId = subject,
                ParentId = -1,
                LoadPath = true,
                IsLast = true
            }).Select(t => new
            {
                code = t.Code,
                name = t.Name,
                path =
                    (t.Parents.Count > 0
                        ? t.Parents.Values.Aggregate(string.Empty, (c, v) => c + v + ">") + t.Name
                        : t.Name)
            });
            return DeyiJson(DResult.Succ(kps));
        }

        [Route("save-kps")]
        public ActionResult SaveKps(string id, string kps)
        {
            return DeyiJson(_systemContract.UpdateSbChapterKps(id, kps));
        }

        #endregion

    }
}