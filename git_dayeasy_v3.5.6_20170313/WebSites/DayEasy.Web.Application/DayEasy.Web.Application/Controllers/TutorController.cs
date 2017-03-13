using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Services.Helper;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 辅导 </summary>
    [RoleAuthorize(UserRole.Teacher, "/")]
    public class TutorController : DController
    {
        private readonly ITutorContract _tutorContract;
        private const string AppName = "辅导中心";

        public TutorController(IUserContract userContract, ITutorContract tutorContract)
            : base(userContract)
        {
            _tutorContract = tutorContract;
            ViewBag.AppName = AppName;
        }

        #region 辅导列表
        /// <summary>
        /// 辅导列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var pageIndex = "pageIndex".Query(1);
            var result = _tutorContract.GetTutorsByUserId(CurrentUser.Id, DPage.NewPage(pageIndex - 1, 10));

            if (!result.Status || result.Data == null)
                return View();

            ViewData["totalCount"] = result.TotalCount;

            return View(result.Data.ToList());
        }
        #endregion

        #region 更改状态
        /// <summary>
        /// 更改状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateStatus()
        {
            var tutorId = "tutorId".Form("");
            var status = "status".Form(-1);

            var result = _tutorContract.UpdateStatus(tutorId, (byte)status);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 增加/修改辅导--显示
        /// <summary>
        /// 增加/修改辅导--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult Add(string id)
        {
            var videoId = "videoId".Form("");//上传视频处理
            var newQuestionId = "newQId".Form("");//添加的新问题
            var paperBaseStr = "paperBase".Form("");//处理选题界面的数据传递
            var tutorDataStr = "tutorData".Form("");

            var result = _tutorContract.AddTutorData(id, paperBaseStr, videoId, newQuestionId, tutorDataStr);

            if (!result.Status || result.Data == null)
                return MessageView(result.Message);
            result.Data.Author = CurrentUser.Name;
            result.Data.Subject = CurrentUser.SubjectId;
            ViewData["diffLevels"] = HTMLHelper.EnumToDropDownList(typeof(DiffLevel), result.Data.Diff);
            ViewData["grades"] = HTMLHelper.EnumToDropDownList(typeof(Grade), result.Data.Grade);
            ViewData["subjects"] = SystemCache.Instance.Subjects()
                .ToSlectListItemList(u => u.Value, u => u.Key, result.Data.Subject);

            return View(result.Data);
        }
        #endregion

        #region 增加/修改辅导--保存
        /// <summary>
        /// 增加/修改辅导--保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save()
        {
            var draft = "draft".Form("");
            var tutorDataStr = "tutorData".Form("");

            var result = _tutorContract.SaveTutor(tutorDataStr, CurrentUser.Id, draft);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 辅导使用记录
        /// <summary>
        /// 辅导使用记录
        /// </summary>
        /// <returns></returns>
        public ActionResult Records(string id)
        {
            if (string.IsNullOrEmpty(id))
                return View();

            var tutorModel = _tutorContract.GetTutorById(id);
            if (!tutorModel.Status || tutorModel.Data == null)
                return View();

            var pageIndex = "pageIndex".Query(1);
            var result = _tutorContract.Records(id, DPage.NewPage(pageIndex - 1, 10));
            if (!result.Status || result.Data == null)
                return View();

            ViewData["tutorModel"] = tutorModel.Data;
            ViewData["totalCount"] = result.TotalCount;

            return View(result.Data.ToList());
        }
        #endregion

        #region 反馈
        /// <summary>
        /// 反馈
        /// </summary>
        /// <returns></returns>
        public ActionResult FeedBack(string id)
        {
            if (string.IsNullOrEmpty(id))
                return View();

            var tutorModel = _tutorContract.GetTutorById(id);
            if (!tutorModel.Status || tutorModel.Data == null)
                return View();

            var pageIndex = "pageIndex".Query(1);
            var result = _tutorContract.GetFeedBackData(id, DPage.NewPage(pageIndex - 1, 10));
            if (!result.Status || result.Data == null)
                return View();

            ViewData["tutorName"] = tutorModel.Data.Title;
            ViewData["tutorId"] = tutorModel.Data.Id;
            ViewData["totalCount"] = result.TotalCount;

            return View(result.Data.ToList());
        }
        #endregion

        #region 获取评论统计
        /// <summary>
        /// 获取评论统计
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCommentStatistics()
        {
            var tutorId = "tutorId".Form("");

            var result = _tutorContract.GetFeedBackStatistics(tutorId);
            if (!result.Status || result.Data == null)
                return null;

            return Json(result.Data.ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传视频--显示
        /// <summary>
        /// 上传视频--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadVideo()
        {
            ViewData["baseData"] = "paperBase".Form("");
            ViewData["GradeList"] = HTMLHelper.EnumToDropDownList(typeof(Grade));

            return View();
        }
        #endregion

        #region 上传视频--保存视频
        /// <summary>
        /// 上传视频--保存视频
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveVideo()
        {
            var videoName = "videoName".Form("");
            var videoUrl = "videoUrl".Form("");
            var videoDesc = "videoDesc".Form("");
            var faceImg = "faceImg".Form("");
            var min = "min".Form(0);
            var sec = "sec".Form(0);
            var grade = "grade".Form(-1);

            var time = min + (decimal)sec / 60;

            var result = _tutorContract.AddVideo(videoName, videoUrl, videoDesc, faceImg, time, grade,
                CurrentUser.Id, CurrentUser.SubjectId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}