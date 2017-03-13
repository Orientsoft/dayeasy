using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Marking.Joint;
using DayEasy.Contracts.Enum;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.Portal.Controllers
{
    /// <summary> 阅卷相关 </summary>
    [RoleAuthorize(UserRole.Teacher, "/")]
    [RoutePrefix("marking")]
    public class MarkingController : DController
    {
        #region 注入

        private readonly IMarkingContract _markingContract;
        private readonly IAnswerShareContract _answerShareContract;
        public MarkingController(
            IUserContract userContract,
            IMarkingContract markingContract,
            IAnswerShareContract answerShareContract)
            : base(userContract)
        {
            _markingContract = markingContract;
            _answerShareContract = answerShareContract;
            ViewBag.PageNav = 2;
        }

        #endregion

        #region View

        /// <summary> 批阅 </summary>
        [Route("")]
        public ActionResult Index(string batch, int type)
        {
            //基本验证
            var checkResult = _markingContract.MkCheck(batch, type);
            if (!checkResult.Status)
                return MessageView(checkResult.Message, "在线阅卷");

            //未标记区域
            if (checkResult.Data)
                return RedirectToAction("MarkingArea", new { batch, type });

            return View();
        }

        //[HttpPost]
        [Route("combine/{jointBatch}")]
        public ActionResult Combine(string jointBatch)
        {
            if (!string.Equals(Request.HttpMethod, "post", StringComparison.CurrentCultureIgnoreCase))
                return RedirectToAction("Mission", new { jointBatch });
            var str = "qList".Form(string.Empty).UrlDecode();
            var qList = JsonHelper.JsonList<string[]>(str).ToList();
            if (!qList.Any())
                return MessageView("请选择要批阅的题目", returnUrl: "/marking/mission_v2/" + jointBatch, returnText: "协同任务页");
            ViewBag.JointBatch = jointBatch;
            return View(qList);
        }

        /// <summary> 标记区域 </summary>
        [Route("marking-area")]
        public ActionResult MarkingArea(string batch, int type, bool isJoint = false)
        {
            return View();
        }

        /// <summary> 协同任务分配 </summary>
        [Route("allot/{jointBatch}")]
        public ActionResult Allot(string jointBatch)
        {
            var result = _markingContract.JointAllot(jointBatch);
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.JointBatch = jointBatch;
            return View(result.Data);
        }

        /// <summary> 批阅任务 </summary>
        [Route("mission_v2/{jointBatch}")]
        public ActionResult Mission(string jointBatch)
        {
            var result = _markingContract.JointMission(jointBatch, UserId);
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.JointBatch = jointBatch;
            return View(result.Data);
        }

        #endregion

        #region 阅卷相关Ajax

        #region 加载基本数据

        [Route("load")]
        public ActionResult Load(string batch, int type)
        {
            return DeyiJson(_markingContract.MkPictureList(batch, type));
        }

        #endregion

        #region 加载答题详细

        [Route("picture")]
        public ActionResult Picture(string picture_id, int type)
        {
            return DJson.Json(_markingContract.MkPictureDetail(picture_id, type),
                namingType: NamingType.CamelCase);
        }

        #endregion

        #region 阅卷提交

        [Route("submit")]
        public ActionResult Submit(MkSubmitDto data)
        {
            if (data == null) return DJson.Json(new { status = false, message = "参数错误，请刷新重试" });
            data.UserId = CurrentUser.Id;
            if (data.DetailData.IsNotNullOrEmpty())
                data.Details = JsonHelper.JsonList<MkDetailDto>(data.DetailData, NamingType.UrlCase).ToList();
            return DJson.Json(_markingContract.Submit(data), namingType: NamingType.UrlCase);
        }

        #endregion

        #region 分享学生答案

        [Route("add-share")]
        public ActionResult AddShare(string batch, string paper_id, string group_id,
            string question_id, long student_id, string student_name)
        {
            var json = _answerShareContract.Add(new AnswerShareAddModelDto
            {
                Batch = batch,
                GroupId = group_id,
                PaperId = paper_id,
                QuestionId = question_id,
                UserId = student_id,
                UserName = student_name,
                Status = AnswerShareStatus.PreShare
            });
            return DJson.Json(json);
        }

        #endregion

        #endregion

        #region 协同相关Ajax

        #region 发起协同

        [AjaxOnly]
        [HttpPost]
        [Route("publish-joint")]
        public ActionResult PublishJoint(string groupId, string paperNo)
        {
            var result = _markingContract.PublishJoint(UserId, groupId, paperNo);
            return DeyiJson(result, true);
        }

        #endregion

        [AjaxOnly]
        [HttpPost]
        [Route("allot-submit")]
        public ActionResult AllotSubmit(string jointBatch, string missions)
        {
            var details = JsonHelper.JsonList<DistributionDetailDto>(missions.UrlDecode()) ??
                          new List<DistributionDetailDto>();
            var dto = new JDistributionDto
            {
                UserId = UserId,
                JointBatch = jointBatch,
                Details = details.ToList()
            };
            var result = _markingContract.DistributionJoint(dto);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("allot-add")]
        public ActionResult AllotAdd(string id, string teachers)
        {
            var teacherIds = teachers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.To(0L)).ToList();
            var result = _markingContract.AddDistribution(id, teacherIds);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpGet]
        [Route("combine-data")]
        public ActionResult CombineData(string joint)
        {
            var groupsParam = "groups".Query(string.Empty).UrlDecode();
            var groups = JsonHelper.JsonList<string[]>(groupsParam).ToList();
            var result = _markingContract.JointCombine(joint, groups, UserId);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpGet]
        [Route("change-picture")]
        public ActionResult ChangePicture(string joint)
        {
            var groupsParam = "groups".Query(string.Empty).UrlDecode();
            var groups = JsonHelper.JsonList<JGroupStepDto>(groupsParam).ToList();
            var result = _markingContract.ChangePictures(joint, groups, UserId);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("report-exception")]
        public ActionResult ReportException(string data)
        {
            var dto = JsonHelper.Json<JExceptionDto>(data.UrlDecode());
            dto.TeacherId = UserId;
            var result = _markingContract.ReportException(dto);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("combine-submit")]
        public ActionResult CombineSubmit(string data)
        {
            var dto = JsonHelper.Json<JSubmitDto>(data.UrlDecode());
            dto.TeacherId = UserId;
            var result = _markingContract.JointSubmit(dto);
            return DeyiJson(result);
        }
        /// <summary>
        /// 协同-获得客观题得分率
        /// </summary>
        /// <param name="joinBatch"></param>
        /// <returns></returns>
        [HttpGet]
        [AjaxOnly]
        [Route("object_ScoreRate")]
        public ActionResult GetObjectiveQuestionScoreRate(string joinBatch)
        {
            var result = _markingContract.ObjectiveQuestionScore(joinBatch);
            return DeyiJson(result);
        }

        #region 结束阅卷

        [HttpPost]
        [Route("complete")]
        public ActionResult Complete(CompleteMarkingInputDto inputDto)
        {
            inputDto.UserId = UserId;
            return DeyiJson(_markingContract.CompleteMarking(inputDto), true);
        }

        [HttpPost]
        [Route("update-status")]
        public ActionResult UpdateStatus(string batch, byte status)
        {
            return DeyiJson(_markingContract.UpdateMarkingStatus(batch, status), true);
        }

        #endregion

        #endregion

        #region 标记区域相关Ajax

        [HttpGet]
        [Route("marking-init")]
        public ActionResult MarkingInit(string batch, int type, bool isJoint = false)
        {
            return DeyiJson(_markingContract.MkAreaCheck(batch, type, UserId, isJoint));
        }

        //        [Route("marking-area-questions")]
        //        public ActionResult MarkingAreaQuestions(string paper_id, int type)
        //        {
        //            return DJson.Json(_markingContract.MkAreaQuestions(paper_id, type),
        //                namingType: NamingType.UrlCase);
        //        }

        [HttpPost]
        [Route("picture-change")]
        public ActionResult ChangeMarkingPicture(string batch, int type, bool isJoint = false)
        {
            return DJson.Json(_markingContract.MkAreaChangePicture(batch, type, isJoint),
                namingType: NamingType.UrlCase);
        }

        [HttpPost]
        [Route("marking-area-save")]
        public ActionResult SaveMarkingArea(string batch, int type, string areas)
        {
            return DJson.Json(_markingContract.MkAreaSave(batch, type, areas),
                namingType: NamingType.UrlCase);
        }

        #endregion

    }
}