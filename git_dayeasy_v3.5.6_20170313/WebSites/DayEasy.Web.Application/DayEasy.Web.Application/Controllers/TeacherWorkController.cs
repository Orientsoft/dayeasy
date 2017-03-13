using DayEasy.Application.Services;
using DayEasy.Application.Services.Helper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Office;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    [RoleAuthorize(UserRole.Teacher, "/")]
    [RoutePrefix("work/teacher")]
    public class TeacherWorkController : WorkController
    {
        #region 注入
        private readonly IErrorBookContract _errorBookContract;
        private readonly IMarkingContract _markingContract;
        private readonly IMessageContract _messageContract;
        private readonly ISmsScoreNoticeContract _smsScoreNoticeContract;
        private readonly IApplicationContract _applicationContract;
        private const string AppName = "报表中心";

        public TeacherWorkController(
            IUserContract userContract,
            IApplicationContract applicationContract,
            IPublishContract publishContract,
            IStatisticContract statisticContract,
            IPaperContract paperContract,
            IGroupContract groupContract,
            IErrorBookContract errorBookContract,
            IMarkingContract markingContract,
            IMessageContract messageContract,
            ISmsScoreNoticeContract smsScoreNoticeContract)
            : base(userContract, paperContract, publishContract, groupContract, statisticContract)
        {
            _errorBookContract = errorBookContract;
            _markingContract = markingContract;
            _messageContract = messageContract;
            _smsScoreNoticeContract = smsScoreNoticeContract;
            _applicationContract = applicationContract;
            ViewBag.AppName = AppName;
        }

        #endregion

        #region Views

        /// <summary> 批阅中心 </summary>
        [Route("v1")]
        public ActionResult Index()
        {
            ViewBag.AppName = "批阅中心";
            var pageIndex = "pageIndex".Query(1);
            var key = "key".Query("");

            var result = PublishContract.GetTeacherPubWorks(CurrentUser.Id, key, DPage.NewPage(pageIndex - 1, 10));

            ViewData["totalCount"] = result.TotalCount;

            //查询试卷总数量
            var countResult = PaperContract.GetCount(CurrentUser.Id, CurrentUser.SubjectId, PaperStatus.Normal);
            if (countResult.Status)
                ViewData["paperCount"] = countResult.Data;

            return result.Status && result.Data != null ? View(result.Data.ToList()) : View();
        }

        /// <summary> 批阅中心 </summary>
        [Route("")]
        public ActionResult MarkingCenter()
        {
            ViewBag.AppName = "批阅中心";
            LastMarkingDateHelper.Set(UserId);
            return View();
        }

        /// <summary> 题目统计 </summary>
        [Route("statistics-question")]
        public ActionResult StatisticsQuestion(string batch, string paper_id)
        {
            //发布信息
            var publishModel = PublishContract.GetUsageDetail(batch);
            if (!publishModel.Status || publishModel.Data == null)
                return MessageView("没有查询到考试、推送记录", returnUrl: "/work/teacher", returnText: "批阅中心");

            if (publishModel.Data.SourceType == (byte)PublishType.Print &&
                publishModel.Data.MarkingStatus != (byte)MarkingStatus.AllFinished)
                return MessageView("该考试尚未结束阅卷，请于结束阅卷后查看统计", returnUrl: "/work/teacher", returnText: "批阅中心");

            //试卷信息
            var paperInfo = PaperContract.PaperDetailById(publishModel.Data.SourceId);
            if (!paperInfo.Status || paperInfo.Data == null)
                return MessageView("没有查询到试卷资料", returnUrl: "/work/teacher", returnText: "批阅中心");
            ViewData["paper"] = paperInfo.Data;

            var dto = GetWorkDto(batch, paper_id, 1);
            return View(dto);
        }

        /// <summary> 考试概况 </summary>
        [Route("statistics-survey")]
        public ActionResult StatisticsSurvey(string batch, string paper_id)
        {
            var dto = GetWorkDto(batch, paper_id, 0);
            return View(dto);
        }

        /// <summary> 分数排名 </summary>
        [Route("statistics-score")]
        public ActionResult StatisticsScore(string batch, string paper_id)
        {
            var dto = GetWorkDto(batch, paper_id, 2);
            var rankResult = StatisticContract.GetStatisticsRank(batch, paper_id);
            if (rankResult.Status && rankResult.Data != null)
                ViewData["ranks"] = rankResult.Data.ToList();

            var colleagueResult = GroupContract.Groups(CurrentUser.Id, (byte)GroupType.Colleague);
            if (colleagueResult.Status && colleagueResult.Data != null)
                ViewData["groups"] = colleagueResult.Data.ToList();

            //已通知的电话号码
            ViewData["noticeMobiles"] =
                _smsScoreNoticeContract.FindByBatch(batch, paper_id).Select(m => m.Mobile).ToList();
            return View(dto);
        }

        [Route("statistics-score-joint")]
        public ActionResult StatisticsScoreJoint(string joint_batch, string paper_id, string group_id = "")
        {
            string paperTitle = string.Empty, batch = string.Empty;
            var isAb = false;
            var ranks = new List<StudentRankInfoDto>();
            var groups = new List<ClassGroupDto>();
            decimal scoreA = 0M, scoreB = 0M, scoreT = 0M;

            var paperResult = PaperContract.PaperDetailById(paper_id, false);
            if (paperResult.Status && paperResult.Data != null)
            {
                paperTitle = paperResult.Data.PaperBaseInfo.PaperTitle;
                isAb = paperResult.Data.PaperBaseInfo.PaperType == (byte)PaperType.AB;
                scoreT = paperResult.Data.PaperBaseInfo.PaperScores.TScore;
                scoreA = paperResult.Data.PaperBaseInfo.PaperScores.TScoreA;
                scoreB = paperResult.Data.PaperBaseInfo.PaperScores.TScoreB;
            }

            var usagesResult = PublishContract.GetUsageDetailByJointBatch(joint_batch);
            if (usagesResult.Status)
            {
                var usages = usagesResult.Data.ToList();
                ViewData["batches"] = usages.ToDictionary(k => k.ClassId, v => v.Batch);
                var usage = usages.FirstOrDefault();
                if (usage != null)
                    batch = usage.Batch;

                usages.ForEach(u =>
                {
                    var groupResult = GroupContract.LoadById(u.ClassId);
                    if (groupResult.Status && groupResult.Data != null)
                        groups.Add(groupResult.Data as ClassGroupDto);
                });
                groups = groups.OrderBy(t => t.AgencyId).ThenBy(g => g.Name.ClassIndex()).ToList();
            }

            var rankResult = StatisticContract.GetStatisticsRank(string.Empty, paper_id, joint_batch, group_id);
            if (rankResult.Status && rankResult.Data != null)
                ranks = rankResult.Data.ToList();

            return View(new ViewDataDictionary
            {
                {"jointBatch", joint_batch},
                {"batch", batch},
                {"paperId", paper_id},
                {"paperTitle", paperTitle},
                {"scoreT", scoreT},
                {"scoreA", scoreA},
                {"scoreB", scoreB},
                {"isAb", isAb},
                {"ranks", ranks},
                {"groups", groups},
                {"groupId", group_id}
            });
        }

        [Route("statistics-unsubmit")]
        public ActionResult StatisticsUnSubmit(string batch, string paper_id)
        {
            var dto = GetWorkDto(batch, paper_id, 2);
            //未提交的学生名单
            var unsubmits = new List<NameDto>();
            //圈内所有学生
            var studentResult = GroupContract.GroupMembers(dto.ClassId, UserRole.Student);
            if (studentResult.Status)
            {
                var submitStuIds = _errorBookContract.IsSubmitStudentIds(batch, paper_id);
                unsubmits = studentResult.Data.Where(s => !submitStuIds.Contains(s.Id))
                    .Select(s => new NameDto(s.Avatar, s.Name)).ToList();
            }
            ViewData["unsubmits"] = unsubmits;
            return View(dto);
        }

        [Route("paper-variant/{batch:regex([a-z0-9]{32})}/{paper_id:regex([a-z0-9]{32})}")]
        public ActionResult PaperVariant(string batch, string paper_id)
        {
            var dto = GetWorkDto(batch, paper_id, 3);
            ViewData["hasVariant"] = PublishContract.IsSendVariant(batch, paper_id);
            var weaks = PublishContract.PaperWeak(batch, paper_id);
            if (weaks.Status)
            {
                ViewData["paperWeak"] = weaks.Data;
            }
            return View(dto);
        }


        /// <summary> 链接到试卷答题详情 - 对应学生错题详细 </summary>
        [Route("detail/{batch}/{paperId}/{questionId}")]
        public ActionResult LinkErrorDetail(string batch, string paperId, string questionId)
        {
            const string path = "~/Views/ErrorBook/Detail.cshtml";
            ViewData["isTeacher"] = true;
            ViewData["currentUser"] = CurrentUser;

            var result = _errorBookContract.ErrorQuestion(batch, paperId, questionId);
            if (!result.Status)
                return View(path);
            var item = result.Data;
            var groupResult = GroupContract.LoadById(item.GroupId);
            ViewData["groupId"] = item.GroupId;
            ViewData["groupName"] = groupResult.Status && groupResult.Data != null
                ? groupResult.Data.Name
                : string.Empty;

            //圈内错因标签统计
            var statisResult = _errorBookContract.TagStatistics(batch, questionId);
            if (statisResult.Status)
                ViewData["statics"] = statisResult.Data.ToList();

            //查询班级内所有同学的错因分析
            var reasonResult = _errorBookContract.Reasons(string.Empty, item.Batch, item.Question.Id);
            if (reasonResult.Status)
                ViewData["reasons"] = reasonResult.Data;

            //答错的学生ID
            var hasErrorStudentIds = _errorBookContract.HasErrorStudentIds(batch, paperId, questionId);
            if (hasErrorStudentIds == null) return View(path, item);
            //圈子内的所有学生
            var studentResult = GroupContract.GroupMembers(item.GroupId, UserRole.Student);
            if (!studentResult.Status) return View(path, item);
            //已分析错因的学生ID
            var hasReasonStudentIds = !reasonResult.Status
                ? new List<long>()
                : reasonResult.Data.Select(r => r.StudentId).ToList();
            //未分析错因的学生名单
            ViewData["unReasons"] = studentResult.Data
                .Where(s => hasErrorStudentIds.Contains(s.Id) && !hasReasonStudentIds.Contains(s.Id))
                //.Select(s => s.Name)
                .ToList();
            //出错的学生名单
            var errorResult = _errorBookContract.ErrorStudents(batch, paperId, questionId, true);
            if (errorResult.Status)
                ViewData["questionErrors"] = errorResult.Data.ToList();

            return View(path, item);
        }

        #endregion

        #region Ajax

        [HttpGet]
        [Route("marking-count")]
        public JsonpResult MarkingCount()
        {
            var result = _applicationContract.NewMarking(UserId);
            return new JsonpResult(result, "callback".Query(string.Empty));
        }

        [AjaxOnly]
        [HttpGet]
        [Route("marking-list")]
        public ActionResult MarkingList(int page = 0, int size = 10)
        {
            var result = _applicationContract.MarkingList(UserId, DPage.NewPage(page, size));
            return DeyiJson(result);
        }

        /// <summary> 撤回试卷 </summary>
        [HttpPost]
        public ActionResult DeletePaper()
        {
            var batch = "batch".Form("");

            var result = PublishContract.DeletePubPaper(batch, CurrentUser.Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary> 圈子内的所有学生 </summary>
        [Route("class-students")]
        public ActionResult ClassStudents(string group_id)
        {
            return DJson.Json(GroupContract.GroupMembers(group_id, UserRole.Student), namingType: NamingType.UrlCase);
        }

        #region 题目统计

        /// <summary>
        /// 题目统计错因分析数量
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        [Route("statistics-question-reasons")]
        public ActionResult StatisticsQuestionReasons(string batch)
        {
            return DJson.Json(_errorBookContract.ReasonCountDict(batch).ToList(), namingType: NamingType.UrlCase);
        }

        /// <summary> 客观题各选项统计 </summary>
        /// <param name="batch"></param>
        /// <param name="paper_id"></param>
        /// <param name="question_id"></param>
        /// <param name="small_question_id"></param>
        /// <returns></returns>
        [Route("statistics-question-detail")]
        public ActionResult StatisticsQuestionDetail(string batch, string paper_id,
            string question_id, string small_question_id)
        {
            var json = StatisticContract.StatisticsQuestionDetail(batch, paper_id, question_id, small_question_id);
            return DJson.Json(json, namingType: NamingType.UrlCase);
        }

        #endregion

        #region 试卷统计

        [Route("statistics-avges")]
        public ActionResult StatisticsAvges(string batch, string paper_id, bool single = false)
        {
            return DJson.Json(StatisticContract.GetStatisticsAvges(batch, paper_id, single: single), namingType: NamingType.UrlCase);
        }

        [Route("statistics-avges-joint")]
        public ActionResult StatisticsAvgesJoint(string joint_batch, string paper_id)
        {
            return DJson.Json(StatisticContract.GetJointStatisticsAvges(joint_batch, paper_id), namingType: NamingType.UrlCase);
        }

        [HttpGet]
        [AjaxOnly]
        [Route("question-scores")]
        public ActionResult QuestionScores(string batch)
        {
            return DeyiJson(StatisticContract.QuestionScores(batch));
        }

        /// <summary>
        /// 共享、取消共享试卷统计
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="group_id"></param>
        /// <returns></returns>
        [Route("statistics-share")]
        public ActionResult StatisticsShare(string batch, string group_id)
        {
            return DJson.Json(
                StatisticContract.StatisticsShare(batch, CurrentUser.Id, group_id),
                namingType: NamingType.UrlCase);
        }

        /// <summary>
        /// 编辑分数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("statistics-edit-socre")]
        public ActionResult StatisticsEditScore(string batch, string paper_id, string data)
        {
            if (batch.IsNullOrEmpty() || paper_id.IsNullOrEmpty() || data.IsNullOrEmpty())
                return DJson.Json(DResult.Error("提交数据错误，请刷新重试！"));
            try
            {
                var list = JsonHelper.JsonList<StudentRankInfoDto>(data).ToList();
                if (!list.Any())
                    return DJson.Json(DResult.Error("提交数据错误，请刷新重试！"));
                return DJson.Json(_markingContract.UpdateScoreStatistics(batch, paper_id, CurrentUser.Id, list));
            }
            catch
            {
                return DJson.Json(DResult.Error("提交数据错误，请刷新重试！"));
            }
        }

        /// <summary> 表扬 </summary>
        /// <param name="batch"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Route("statistics-praise")]
        public ActionResult StatisticsPraise(string batch, string message)
        {
            if (batch.IsNullOrEmpty() || message.IsNullOrEmpty())
                return DJson.Json(DResult.Error("提交数据错误，请刷新重试！"));
            var groupId = string.Empty;
            var groupResult = PublishContract.LoadByBatch(batch);
            if (groupResult.Status && groupResult.Data != null)
                groupId = groupResult.Data.Id;
            var json = _messageContract.SendDynamic(new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Praise,
                GroupId = groupId,
                Title = "特别表扬",
                ReceivRole = (UserRole.Student | UserRole.Teacher),
                Message = HttpUtility.HtmlDecode(message),
                UserId = CurrentUser.Id
            });
            return DJson.Json(json);
        }

        /// <summary>
        /// 发送成绩通知短信
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paper_id"></param>
        /// <param name="student_ids"></param>
        /// <returns></returns>
        [Route("statistics-sendsms")]
        public ActionResult StatisticsSendSms(string batch, string paper_id, string student_ids)
        {
            var ids = student_ids.JsonToObject<List<long>>();
            var result = StatisticContract.SendScoreSms(batch, paper_id, ids);
            return DeyiJson(result);
        }

        #endregion

        #region 考后补救

        /// <summary> 变式列表 </summary>
        [HttpGet]
        [Route("variant-list")]
        public ActionResult VariantList(string batch, string paper_id)
        {
            return DeyiJson(PublishContract.VariantList(batch, paper_id));
        }

        /// <summary> 试卷变式系统推荐 </summary>
        [HttpGet]
        [Route("variant-paper")]
        public ActionResult VariantFromPaper(string batch, string paper_id)
        {
            var result = PublishContract.VariantListFromSystem(batch, paper_id, max: 10, pre: 0);
            return DeyiJson(result);
        }

        /// <summary> 获取变式 </summary>
        [HttpPost]
        [Route("variant")]
        public ActionResult Variant(string qid, int count = 1, string excepts = null, string isdata= "1")
        {
            var exceptList = (excepts ?? string.Empty).Split(',').ToList();
            var result = PublishContract.Variant(qid, count, exceptList);
            if (!result.Data.Any()&& isdata.Equals("0"))
            {
                result = PublishContract.Variant(qid, count, exceptList, true);
            }
            return DeyiJson(result, true);
        }

        [HttpGet]
        [Route("usage-list")]
        public ActionResult UsageList(string paper_id)
        {
            var dict = PublishContract.UsageList(paper_id, UserId);
            if (dict.Any())
                return DeyiJson(DResult.Succ(dict));
            return DeyiJson(DResult.Error("未找到任何班级信息！"));
        }

        /// <summary> 推送变式 </summary>
        [HttpPost]
        [Route("send-variant")]
        public ActionResult SendVariants(string paper_id, string class_ids, string variants)
        {
            var classList = class_ids.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var vlist = JsonHelper.Json<Dictionary<string, List<string>>>(variants);
            if (!classList.Any())
            {
                return DeyiJson(DResult.Error("请选择要推送的班级！"));
            }
            if (vlist == null || !vlist.Any())
            {
                return DeyiJson(DResult.Error("请选择要推送变式题！"));
            }
            var result = PublishContract.SendVariant(UserId, paper_id, vlist, classList);
            return DeyiJson(result, true);
        }

        #endregion

        #region 考试概况

        //分数段概况
        [Route("survey-analysis")]
        public ActionResult SurveyAnalysis(string batch, string paper_id)
        {
            return DJson.Json(StatisticContract.GetSurveyAnalysis(batch, paper_id), namingType: NamingType.CamelCase);
        }

        //各题作答、知识点概况
        [Route("survey-graspings")]
        public ActionResult SurveyGraspings(string batch, string paper_id)
        {
            return DJson.Json(StatisticContract.Graspings(batch, paper_id), namingType: NamingType.CamelCase);
        }

        #endregion

        #endregion

        #region 导出统计资料

        /// <summary>
        /// 导出统计资料
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paper_id"></param>
        [Route("export")]
        public void Export(string batch, string paper_id)
        {
            string paperTitle = string.Empty, groupName = string.Empty;
            var isAb = false;
            var paperResult = PaperContract.PaperDetailById(paper_id, false);
            if (paperResult.Status)
            {
                paperTitle = paperResult.Data.PaperBaseInfo.PaperTitle;
                isAb = paperResult.Data.PaperBaseInfo.PaperType == (byte)PaperType.AB;
            }
            var groupResult = PublishContract.LoadByBatch(batch);
            if (groupResult.Status)
                groupName = groupResult.Data.Name;
            var log = UserId.CreateLog(DownloadType.ClassStatistic, CurrentAgency);
            var ds = new DataSet();

            #region 排名

            var rankResult = StatisticContract.GetStatisticsRank(batch, paper_id);
            if (rankResult.Status)
            {
                var dtRank = new DataTable("班内排名");
                dtRank.Columns.Add("Index", typeof(string));
                dtRank.Columns.Add("Num", typeof(string));
                dtRank.Columns.Add("Name", typeof(string));
                dtRank.Columns.Add("Score", typeof(string));
                if (isAb)
                {
                    dtRank.Columns.Add("ScoreA", typeof(string));
                    dtRank.Columns.Add("ScoreB", typeof(string));
                    dtRank.Rows.Add(new object[] { "序号", "学号", "姓名", "总分", "A卷总分", "B卷总分" });
                }
                else
                    dtRank.Rows.Add(new object[] { "序号", "学号", "姓名", "总分" });
                var index = 1;
                var ranks = rankResult.Data.OrderByDescending(r => r.TotalScore).ToList();
                ranks.ForEach(r => dtRank.Rows.Add(isAb
                    ? new object[]
                    {
                        index++,
                        (r.StudentNum ?? string.Empty),
                        (r.StudentName ?? string.Empty),
                        (r.TotalScore.HasValue?r.TotalScore.Value.ToString("0.0"):"0"),
                        (r.SectionAScore.HasValue?r.SectionAScore.Value.ToString("0.0"):"0"),
                        (r.SectionBScore.HasValue?r.SectionBScore.Value.ToString("0.0"):"0")
                    }
                    : new object[]
                    {
                        index++,
                        (r.StudentNum ?? string.Empty),
                        (r.StudentName ?? string.Empty),
                        (r.TotalScore.HasValue?r.TotalScore.Value.ToString("0.0"):"0")
                    }));
                ds.Tables.Add(dtRank);
            }

            #endregion

            #region 分数段

            var scoresResult = StatisticContract.GetStatisticsAvges(batch, paper_id, single: true);
            if (scoresResult.Status)
            {
                var score = scoresResult.Data.FirstOrDefault();
                if (score != null)
                {
                    var scoreGroups = score.ScoreGroupes;
                    scoreGroups.Reverse();
                    var dtSub = new DataTable("分数段统计");
                    dtSub.Columns.Add("Key", typeof(string));
                    dtSub.Columns.Add("Value", typeof(string));
                    dtSub.Rows.Add(new object[] { "分数段", "人数" });
                    scoreGroups.ForEach(s => dtSub.Rows.Add(new object[] { s.ScoreInfo, s.Count.ToString(CultureInfo.InvariantCulture) }));
                    dtSub.Rows.Add(new object[] { "最高分", score.Max.ToString("0.0") });
                    dtSub.Rows.Add(new object[] { "最低分", score.Min.ToString("0.0") });
                    dtSub.Rows.Add(new object[] { "平均分", score.Avg.ToString("0.0") });
                    ds.Tables.Add(dtSub);
                }
            }

            #endregion

            #region 每题得分

            var questionScores = StatisticContract.QuestionScores(batch);
            if (questionScores.Status)
            {
                var scores = questionScores.Data;
                var dt = new DataTable("每题得分");
                var row = new List<object> { "姓名", "得一号" };
                dt.Columns.Add("name", typeof(string));
                dt.Columns.Add("code", typeof(string));
                foreach (var sort in scores.QuestionSorts)
                {
                    dt.Columns.Add(sort.Key, typeof(string));
                    row.Add(sort.Value);
                }
                dt.Rows.Add(row.ToArray());
                foreach (var student in scores.Students)
                {
                    row = new List<object>
                    {
                        student.Name,
                        student.Code
                    };
                    foreach (var qid in scores.QuestionSorts.Keys)
                    {
                        if (student.Scores.ContainsKey(qid))
                            row.Add(student.Scores[qid]);
                        else
                            row.Add(-1);
                    }
                    dt.Rows.Add(row.ToArray());
                }
                ds.Tables.Add(dt);
            }

            #endregion

            log.Complete();
            ExcelHelper.Export(ds, groupName + "-" + paperTitle + ".xls");
        }

        [Route("export-joint")]
        public void ExportJoint(string joint_batch, string paper_id)
        {
            var paperResult = PaperContract.PaperDetailById(paper_id, false);
            if (!paperResult.Status) return;
            var log = UserId.CreateLog(DownloadType.JointStatistic, CurrentAgency);
            var paperTitle = paperResult.Data.PaperBaseInfo.PaperTitle;
            var isAb = paperResult.Data.PaperBaseInfo.PaperType == (byte)PaperType.AB;

            var ds = new DataSet();

            #region 排名

            var rankResult = StatisticContract.GetStatisticsRank("", paper_id, joint_batch);
            if (rankResult.Status)
            {
                //班级列表
                var groups = new List<GroupDto>();
                var groupIds = rankResult.Data.Select(r => r.GroupId).ToList();
                var groupsResult = GroupContract.SearchGroups(groupIds);
                if (groupsResult.Status && groupsResult.Data != null)
                    groups = groupsResult.Data.ToList();

                var dtRank = new DataTable("排名统计");
                dtRank.Columns.Add("Sort", typeof(string));
                dtRank.Columns.Add("GroupName", typeof(string));
                dtRank.Columns.Add("Agency", typeof(string));
                dtRank.Columns.Add("Num", typeof(string));
                dtRank.Columns.Add("Name", typeof(string));
                dtRank.Columns.Add("Score", typeof(string));
                if (isAb)
                {
                    dtRank.Columns.Add("ScoreA", typeof(string));
                    dtRank.Columns.Add("ScoreB", typeof(string));
                    dtRank.Rows.Add(new object[] { "年级排名", "班级", "学校", "学号", "姓名", "总分", "A卷总分", "B卷总分" });
                }
                else
                    dtRank.Rows.Add(new object[] { "年级排名", "班级", "学校", "学号", "姓名", "总分" });

                var ranks = rankResult.Data.OrderByDescending(r => r.TotalScore).ToList();
                ranks.ForEach(r =>
                {
                    var group = groups.FirstOrDefault(g => g.Id == r.GroupId);
                    dtRank.Rows.Add(isAb
                        ? new object[]
                        {
                            r.Rank,
                            (group != null ? group.Name : string.Empty),
                            (group != null ? group.AgencyName : string.Empty),
                            (r.StudentNum ?? string.Empty),
                            (r.StudentName ?? string.Empty),
                            (r.TotalScore.HasValue ? r.TotalScore.Value.ToString("0.0") : "0"),
                            (r.SectionAScore.HasValue ? r.SectionAScore.Value.ToString("0.0") : "0"),
                            (r.SectionBScore.HasValue ? r.SectionBScore.Value.ToString("0.0") : "0")
                        }
                        : new object[]
                        {
                            r.Rank,
                            (group != null ? group.Name : string.Empty),
                            (group != null ? group.AgencyName : string.Empty),
                            (r.StudentNum ?? string.Empty),
                            (r.StudentName ?? string.Empty),
                            (r.TotalScore.HasValue ? r.TotalScore.Value.ToString("0.0") : "0")
                        });
                });
                ds.Tables.Add(dtRank);
            }

            #endregion

            #region 分数段

            var scoresResult = StatisticContract.GetJointStatisticsAvges(joint_batch, paper_id, true);
            if (!scoresResult.Status || !scoresResult.Data.Any())
            {
                ExcelHelper.Export(ds, paperTitle + "-各班分数段排名统计.xls");
                return;
            }

            #region 解析数据
            var list = scoresResult.Data.ToList();
            var firstItem = list.FirstOrDefault();
            if (firstItem == null) return;

            int i, x = list.Count + 1, y = firstItem.ScoreGroupes.Count + 4;
            //string[,] array2d = new string[colC, rowC];
            var array2D = new string[y][];
            for (i = 0; i < y; i++)
            {
                array2D[i] = new string[x];
            }

            //首列
            i = 1;
            array2D[0][0] = "分数段";
            firstItem.ScoreGroupes.Reverse();
            firstItem.ScoreGroupes.ForEach(group =>
            {
                array2D[i++][0] = @group.ScoreInfo;
            });
            array2D[i++][0] = "最高分";
            array2D[i++][0] = "最低分";
            array2D[i][0] = "平均分";

            //数据列
            var j = 1;
            list.ForEach(item =>
            {
                i = 0;
                array2D[i++][j] = item.GroupName;

                if (j > 1)
                {
                    item.ScoreGroupes.Reverse();
                }

                item.ScoreGroupes.ForEach(group =>
                {
                    array2D[i++][j] = @group.Count.ToString(CultureInfo.InvariantCulture);
                });
                array2D[i++][j] = item.Max.ToString("0.0");
                array2D[i++][j] = item.Min.ToString("0.0");
                array2D[i][j++] = item.Avg.ToString("0.0");
            });
            #endregion

            #region 制表

            var dt = new DataTable("分数段统计");
            for (i = 1; i <= x; i++)
            {
                dt.Columns.Add("Col" + i, typeof(string));
            }
            for (i = 0; i < y; i++)
            {
                dt.Rows.Add(array2D[i]);
            }
            ds.Tables.Add(dt);

            #endregion

            #endregion

            log.Complete();
            ExcelHelper.Export(ds, paperTitle + "-各班分数段排名统计.xls");
        }

        #endregion

    }
}