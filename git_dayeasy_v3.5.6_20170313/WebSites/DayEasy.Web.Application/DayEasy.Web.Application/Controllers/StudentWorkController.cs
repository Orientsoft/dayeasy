using DayEasy.Application.Services.Dto;
using DayEasy.Application.Services.Helper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Publish;
using DayEasy.Contracts.Enum;
using DayEasy.Office;
using DayEasy.Office.Enum;
using DayEasy.Office.Models;
using DayEasy.Services.Helper;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using DayEasy.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 作业中心 - 学生版 </summary>
    [DAuthorize]
    [RoutePrefix("work")]
    public class StudentWorkController : WorkController
    {
        #region 接口注入
        private readonly ISystemContract _systemContract;
        private readonly IErrorBookContract _errorBookContract;
        private readonly IMarkingContract _markingContract;
        private readonly IAnswerShareContract _answerShareContract;
        private readonly ITutorContract _tutorContract;
        private readonly IPublishContract _publishContract;

        public StudentWorkController(
            IUserContract userContract,
            IPublishContract publishContract,
            IStatisticContract statisticContract,
            ISystemContract systemContract,
            IPaperContract paperContract,
            IErrorBookContract errorBookContract,
            IMarkingContract markingContract,
            IAnswerShareContract answerShareContract,
            IGroupContract groupContract,
            ITutorContract tutorContract)
            : base(userContract, paperContract, publishContract, groupContract, statisticContract)
        {
            _systemContract = systemContract;
            _errorBookContract = errorBookContract;
            _markingContract = markingContract;
            _answerShareContract = answerShareContract;
            _tutorContract = tutorContract;
            _publishContract = publishContract;
        }

        #endregion

        #region Views

        /// <summary> 学生考试中心--列表页 </summary>
        [Route("")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult Index()
        {
            var pageIndex = "pageIndex".Query(1);
            var searchKey = "key".Query("");
            var subjectId = "sj".Query(-1);

            //查询科目
            var subjects = SystemCache.Instance.Subjects();
            ViewData["subjects"] = subjects.ToSlectListItemList(u => u.Value, u => u.Key, subjectId, true, "全部");

            var result = PublishContract.GetStudentHomeworks(new SearchStuWorkDto
            {
                Key = searchKey,
                Page = pageIndex - 1,
                Size = 10,
                SubjectId = subjectId,
                UserId = ChildOrUserId
            });

            if (result == null || result.Data == null)
                return View();

            //查询学生交卷情况
            var batchNos =
                result.Data.Where(u => u.IsFinished || u.SourceType == (byte)PublishType.Test)
                    .Select(u => u.Batch)
                    .Distinct()
                    .ToList();
            if (batchNos.Any())
            {
                var studentScores = StatisticContract.GetStudentScores(batchNos, ChildOrUserId);
                if (studentScores.Status)
                    ViewData["studentScores"] = studentScores.Data.ToList();

                var classScores = StatisticContract.GetGroupAvgScores(batchNos);
                if (classScores.Status)
                    ViewData["classScores"] = classScores.Data;
            }
            ViewData["totalCount"] = result.TotalCount;

            return View(result.Data.ToList());
        }

        /// <summary> 试卷详情--推送 </summary>
        [HttpGet]
        [Route("pub-paper/{batch}")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult PubPaperDetail(string batch)
        {
            if (string.IsNullOrEmpty(batch))
                return RedirectToAction("Index");

            //查询发布信息
            var publishModel = PublishContract.GetUsageDetail(batch);
            if (!publishModel.Status || publishModel.Data == null)
                return RedirectToAction("Index");

            if (publishModel.Data.SourceType == (byte)PublishType.Print)
                return RedirectToAction("AnswerPaperDetail", new { batchNo = publishModel.Data.Batch });

            //查询试卷详情
            var paperInfo = PaperContract.PaperDetailById(publishModel.Data.SourceId);
            if (!paperInfo.Status || paperInfo.Data == null) return View();

            //查询试卷里面的错题
            var errorQuestion = _errorBookContract.GetPaperErrorQIds(batch, paperInfo.Data.PaperBaseInfo.Id,
                ChildOrUserId);
            if (errorQuestion.Status)
                ViewData["errorQuestions"] = errorQuestion.Data.ToList();

            ViewData["workDto"] = new VWorkDto(batch, publishModel.Data.SourceId)
            {
                PaperTitle = paperInfo.Data.PaperBaseInfo.PaperTitle,
                PublishType = publishModel.Data.SourceType,
                ClassId = publishModel.Data.ClassId,
                ClassName = publishModel.Data.ClassName,
                IsAb = paperInfo.Data.PaperBaseInfo.IsAb
            };

            return View(paperInfo.Data);
        }

        /// <summary> 试卷详情--考试 </summary>
        [HttpGet]
        [Route("answer-paper/{batch}")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult AnswerPaperDetail(string batch)
        {
            if (string.IsNullOrEmpty(batch))
                return RedirectToAction("Index");

            //查询发布信息 
            var publishModel = PublishContract.GetUsageDetail(batch);
            if (!publishModel.Status || publishModel.Data == null)
                return RedirectToAction("Index");

            if (publishModel.Data.SourceType != (byte)PublishType.Print)
                return RedirectToAction("PubPaperDetail", new { batchNo = publishModel.Data.Batch });

            //查询试卷详情
            var paperInfo = PaperContract.PaperDetailById(publishModel.Data.SourceId);
            if (!paperInfo.Status || paperInfo.Data == null) return View();

            if (publishModel.Data.MarkingStatus == (byte)MarkingStatus.AllFinished)//已经阅卷完成
            {
                //答题详情
                var markDetails = _markingContract.GetMarkedDetail(publishModel.Data.Batch, publishModel.Data.SourceId, ChildOrUserId);
                if (markDetails.Status && markDetails.Data != null)
                    ViewData["markDetails"] = markDetails.Data.ToList();

                //我分享的答案
                var shareQuResult = _answerShareContract.SharedQuestions(batch, paperInfo.Data.PaperBaseInfo.Id, ChildOrUserId);
                if (shareQuResult.Status && shareQuResult.Data != null)
                {
                    ViewData["sharedQuestions"] = shareQuResult.Data.ToList();
                }

                ViewData["hadMarked"] = true;
            }
            ViewData["workDto"] = new VWorkDto(batch, publishModel.Data.SourceId)
            {
                PaperTitle = paperInfo.Data.PaperBaseInfo.PaperTitle,
                PublishType = publishModel.Data.SourceType,
                ClassId = publishModel.Data.ClassId,
                ClassName = publishModel.Data.ClassName,
                IsAb = paperInfo.Data.PaperBaseInfo.PaperType == (byte)PaperType.AB
            };

            return View(paperInfo.Data);
        }

        /// <summary> 成绩报表 </summary>
        [HttpGet]
        [Route("score-analysis/{batch}")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult ScoreAnalysis(string batch)
        {
            if (string.IsNullOrEmpty(batch))
                return MessageView("考试批次号异常！");

            var result = StatisticContract.StudentScore(ChildOrUserId, batch);
            if (result.Status)
            {
                ViewData["workDto"] = GetWorkDto(batch, result.Data.PaperId, 1);
                return View(result.Data);
            }
            return View();
        }

        /// <summary> 变式过关 </summary>
        [HttpGet]
        [Route("variant-pass/{batch}")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult VariantPass(string batch)
        {
            if (string.IsNullOrEmpty(batch))
                return RedirectToAction("Index");

            var result = PublishContract.VariantQuestions(batch, ChildOrUserId);
            if (result.Status)
            {
                ViewData["workDto"] = GetWorkDto(batch, result.Data.PaperId, 2);
                return View(result.Data);
            }
            return View();
        }

        /// <summary> 相关辅导 </summary>
        [HttpGet]
        [Route("tutors/{batch}")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult Tutors(string batch)
        {
            if (string.IsNullOrEmpty(batch))
                return RedirectToAction("Index");

            var result = PublishContract.GetRecommendTutors(batch, ChildOrUserId);
            if (!result.Status)
            {
                return MessageView(result.Message);
            }
            ViewData["workDto"] = GetWorkDto(batch, result.Data.PaperId, 3);
            return View(result.Data);
        }

        /// <summary> 辅导详细 </summary>
        [HttpGet]
        [Route("tutor-item/{id}")]
        [RoleAuthorize(UserRole.Student | UserRole.Teacher, "/work/teacher")]
        public ActionResult TutorItem(string id)
        {
            var result = _tutorContract.GetTutorDetail(id);
            if (!result.Status || result.Data == null)
                return MessageView("没有查询到辅导详细资料");

            //教师预览无须后续操作
            if (CurrentUser.IsTeacher())
                return View(result.Data);

            ViewData["isStudent"] = true;

            //增加当前学生的使用记录
            _tutorContract.AddTutorRecord(id, CurrentUser.Id);

            //当前学生的评价内容 - 为空未评价
            var comment = _tutorContract.GetTutorStudentRecord(id, ChildOrUserId);
            if (comment.Status && comment.Data != null)
                ViewData["comment"] = comment.Data;

            return View(result.Data);
        }

        /// <summary> 查看批阅结果 图片+图标 展示 </summary>
        [Route("marking-detail")]
        [RoleAuthorize(UserRole.Student | UserRole.Teacher, "/")]
        public ActionResult MarkingDetail(string batch = "", string paperId = "", long studentId = 0, string type = "")
        {
            if (CurrentUser.Role == (byte)UserRole.Student || CurrentUser.Role == (byte)UserRole.Parents)
                studentId = ChildOrUserId;

            if (batch.IsNullOrEmpty() || paperId.IsNullOrEmpty() || studentId < 1)
                return View();

            var paperResult = PaperContract.PaperDetailById(paperId);
            if (paperResult.Status)
            {
                ViewData["paper"] = paperResult.Data.PaperBaseInfo;
                ViewData["batch"] = batch;
                ViewData["studentId"] = studentId;
                ViewData["type"] = type;
            }

            var paperType = (type.IsNotNullOrEmpty() && type.ToLower().Trim() == "b")
                ? MarkingPaperType.PaperB
                : MarkingPaperType.Normal;

            var pictureResult = _markingContract.LoadPictureDto(batch, paperId, studentId, paperType);
            if (!pictureResult.Status || pictureResult.Data == null)
                return View();

            var picture = pictureResult.Data;
            if (!_markingContract.IsFinished(batch))
                return View(picture);
            var sectionType =
                (byte)(paperType == MarkingPaperType.PaperB ? PaperSectionType.PaperB : PaperSectionType.PaperA);
            var hasObjective = paperResult.Data.PaperSections.Where(s => s.PaperSectionType == sectionType)
                .Any(s => s.Questions.Any(q => q.Question.IsObjective));
            if (hasObjective)
            {
                var message = string.IsNullOrWhiteSpace(picture.ObjectiveError)
                    ? "全对"
                    : picture.ObjectiveError;
                if (message == "全对" || message == "全错")
                    message = "客观题" + message;
                else
                    message = "客观错题：" + message;
                if (picture.ObjectiveScore.HasValue)
                    message += "\t得分：" + picture.ObjectiveScore.Value;
                ViewData["objerror"] = message;
            }

            var markResult = _markingContract.LoadResultDto(batch, paperId, studentId);
            if (markResult.Status && markResult.Data != null && !string.IsNullOrEmpty(markResult.Data.SectionScores))
            {
                var paperScores = JsonHelper.Json<PaperScoresDto>(markResult.Data.SectionScores);
                ViewData["paperScores"] = paperScores;
            }
            return View(picture);
        }
        /// <summary>点击查看我的答案</summary>
        [HttpGet]
        [RoleAuthorize(UserRole.Student | UserRole.Teacher, "/")]
        public ActionResult InterceptPicture(string batch, string paperId, int type = 0, string questionid = "")
        {
            var paperType = (MarkingPaperType)type;
            var picData = _markingContract.InterceptPicture(batch, paperId, UserId, paperType, questionid);
            if (picData == null)
            {
                return DeyiJson("未找到该答案");
            }
            return DeyiJson(picData);
        }
        #endregion

        #region Ajax

        #region 获取试卷中题目的错误人数
        [HttpGet]
        [Route("error-count")]
        [RoleAuthorize(UserRole.Student | UserRole.Teacher, "/")]
        public ActionResult GetErrorCount(string batch)
        {
            var result = PublishContract.GetPaperErrorQuCount(batch);

            return DeyiJson(result);
        }
        #endregion

        #region 加入错题库

        [HttpPost]
        [Route("add-error")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult AddErrorQuestion(string batch, string paperId, string qid)
        {
            var result = _errorBookContract.MarkErrorQuestion(batch, paperId, qid, ChildOrUserId);

            return DeyiJson(result, true);
        }

        #endregion

        #region 分享答案
        [HttpPost]
        [Route("share-answer")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult ShareAnswer(string batch, string paperId, string qid, string groupId)
        {
            var result = _answerShareContract.Add(new AnswerShareAddModelDto
            {
                Batch = batch,
                GroupId = groupId,
                PaperId = paperId,
                QuestionId = qid,
                UserId = CurrentUser.Id,
                UserName = CurrentUser.Name,
                Status = AnswerShareStatus.Normal
            });

            return DeyiJson(result, true);
        }
        #endregion

        #region 同学分享的答案
        [HttpGet]
        [Route("share-answers")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult AnswerShares(string qid, string groupId)
        {
            var result = _answerShareContract.Shares(qid, groupId);
            return DeyiJson(result);
        }
        #endregion

        #region 获取分享的答案的详情
        [HttpGet]
        [Route("share-detail")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult ShareAnswerDetail(string id)
        {
            var result = _answerShareContract.Detail(id, ChildOrUserId);

            return DeyiJson(result);
        }
        #endregion

        #region 膜拜
        [HttpPost]
        [Route("worship")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult Worship(string id)
        {
            var result = _answerShareContract.Worship(id, CurrentUser.Id, CurrentUser.Name);
            if (result.Status)
                result.Message = CurrentUser.Name;
            return DeyiJson(result, true);
        }
        #endregion

        #region 查询未提交试卷类型
        [HttpGet]
        [Route("not-submit")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult GetNotSubmitted(string batch)
        {
            var result = PublishContract.GetNotSubmitted(batch, ChildOrUserId);
            return DeyiJson(result);
        }
        #endregion

        #region 添加辅导评价
        [HttpPost]
        [Route("add-tutor-comment")]
        [RoleAuthorize(UserRole.Student, "/work/teacher")]
        public ActionResult AddTutorComment(string id, string comment, byte? type)
        {
            return Json(_tutorContract.AddTutorComment(id, CurrentUser.Id, comment, type, null));
        }
        #endregion

        [AjaxOnly]
        [HttpGet]
        [Route("student-reports")]
        public ActionResult StudentReports(string batch)
        {
            var result = StatisticContract.StudentReport(ChildOrUserId, batch);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("student-compare")]
        public ActionResult StudentCompare(string batch, string paperId, long compareId)
        {
            var result = StatisticContract.StudentCompare(ChildOrUserId, compareId, batch, paperId);
            return DeyiJson(result, true);
        }

        #endregion

        #region 下载

        //const string Msg = "<div style='text-align:center;font-size:18px;color:red;'>{0}</div>";
        const string Msg = "{0}";

        /// <summary>
        /// 试卷下载
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="type"></param>
        /// <param name="eq"></param>
        /// <returns></returns>
        [Route("dowload")]
        public ActionResult Dowload(string batch, string paperId, int type = 6, int eq = 0)
        {
            //把家长当作学生
            var isStudent = !CurrentUserRoles.Contains(UserRole.Teacher);

            if (paperId.IsNullOrEmpty() || (isStudent && batch.IsNullOrEmpty()))
                return MessageView(string.Format(Msg, "参数错误"));

            if ((type & (byte)DowloadType.PaperA) == 0 && (type & (byte)DowloadType.PaperB) == 0)
                type = (byte)DowloadType.PaperA | (byte)DowloadType.PaperB;

            var list = new List<WdPaper>();

            var paperResult = PaperContract.PaperDetailById(paperId);
            if (!paperResult.Status || paperResult.Data == null || paperResult.Data.PaperBaseInfo.Status != (byte)PaperStatus.Normal)
                return MessageView(string.Format(Msg, "未查询到试卷资料"));
            var paper = paperResult.Data;

            var fileName = Regex.Replace(paper.PaperBaseInfo.PaperTitle, "\\s+", "-", RegexOptions.Compiled);

            if (isStudent)
            {
                var usageResult = PublishContract.GetUsageDetail(batch);
                if (!usageResult.Status || usageResult.Data.SourceId != paperId)
                    return MessageView(string.Format(Msg, "没有找到试卷发布资料"));
                var usage = usageResult.Data;

                var groupIds = new List<string>();
                var groupsResult = GroupContract.Groups(ChildOrUserId, (int)GroupType.Class);
                if (groupsResult.Status)
                    groupIds = groupsResult.Data.Select(g => g.Id).ToList();
                if (!groupIds.Contains(usage.ClassId))
                    return MessageView(string.Format(Msg, "没有权限"));

                if (eq == 1)
                {
                    //非推送试卷
                    if (usage.SourceType != (byte)PublishType.Test)
                    {
                        if (usage.MarkingStatus != (byte)MarkingStatus.AllFinished)
                            return MessageView(string.Format(Msg, "老师还没有完成批阅，请于批阅结束后下载错题"));
                        var markingResult = _markingContract.LoadResultDto(batch, paperId, ChildOrUserId);
                        if (!markingResult.Status || markingResult.Data == null)
                            return MessageView(string.Format(Msg, "你没有参与本次考试，没有可下载的错题"));
                    }

                    var errorIdsResult = _errorBookContract.GetPaperErrorQIds(batch, paperId, ChildOrUserId);
                    if (!errorIdsResult.Status || errorIdsResult.Data == null || !errorIdsResult.Data.Any())
                        return MessageView(string.Format(Msg, "本次考试你的答题全部正确，没有可下载的错题"));

                    //return DowloadEqByQIds(errorIdsResult.Data.ToList(), true, fileName + "-仅含错题");
                    //update by epc 2016.04.12
                    //更该题号与试卷对应
                    fileName += "-仅含错题";
                    paper.PaperSections.ForEach(s =>
                    {
                        s.Questions = s.Questions.Where(q => errorIdsResult.Data.Contains(q.Question.Id)).ToList();
                    });
                    paper.PaperSections = paper.PaperSections.Where(s => s.Questions.Any()).ToList();
                }
            }
            var log = UserId.CreateLog(DownloadType.Paper, CurrentAgency);
            if (paper.PaperBaseInfo.PaperType != (byte)PaperType.AB || (isStudent && eq == 1))
            {
                paper.PaperBaseInfo.PaperTitle = fileName;
                var wdPaper = paper.ToWdPaper();
                wdPaper.Simplify = (isStudent && eq == 1);
                list.Add(wdPaper);
            }
            else
            {
                List<PaperSectionDto>
                    sectionA = paper.PaperSections
                        .Where(s => s.PaperSectionType == (byte)PaperSectionType.PaperA).ToList(),
                    sectionB = paper.PaperSections
                        .Where(s => s.PaperSectionType == (byte)PaperSectionType.PaperB).ToList();
                if ((type & (byte)DowloadType.PaperA) > 0)
                {
                    paper.PaperBaseInfo.PaperTitle = fileName + "[A卷]";
                    paper.PaperSections = sectionA;
                    list.Add(paper.ToWdPaper());
                }
                if ((type & (byte)DowloadType.PaperB) > 0)
                {
                    paper.PaperBaseInfo.PaperTitle = fileName + "[B卷]";
                    paper.PaperSections = sectionB;
                    list.Add(paper.ToWdPaper());
                }
            }

            using (var wh = new WordHelper())
            {
                //生成压缩文件
                type |= (byte)DowloadType.Original;
                type |= (byte)DowloadType.Answer;
                if (!isStudent)
                    type |= (byte)DowloadType.Card;
               // list[0].Sections.Remove(list[0].Sections[1]);
                var zip = wh.DownLoadZip(list, type);
                Console.Write(list);
                if (zip == null)
                    return MessageView(string.Format(Msg, "生成压缩包失败"));
                log.Complete();
                return File(zip.ToArray(), "application/x-zip-compressed", fileName + ".zip");
            }
        }

        /// <summary> 学生错题库下载 </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Route("dowload_eq")]
        public ActionResult DowloadEq(string data)
        {
            if (data.IsNullOrEmpty())
                return MessageView(string.Format(Msg, "参数错误"));
            try
            {
                data = HttpUtility.UrlDecode(data) ?? "[]";
                if (data.EndsWith("%5"))
                    data = data.Replace("%5", "]");
                var eids = data.JsonToObject<List<string>>();
                var result = _errorBookContract.GetErrorQIdsByEIds(eids);
                if (!result.Status || !result.Data.Any())
                    return MessageView(string.Format(Msg, "没有错题可下载"));
                var log = UserId.CreateLog(DownloadType.ErrorQuestion, CurrentAgency, result.TotalCount);
                var actionResult = DowloadEqByQIds(result.Data.ToList());
                log.Complete();
                return actionResult;
            }
            catch (Exception ex)
            {
                return MessageView(string.Format(Msg, "参数错误：" + ex.Message));
            }
        }

        /// <summary>
        /// 变式题下载
        /// </summary>
        /// <param name="data"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        [Route("dowload_variant")]
        public ActionResult DowloadVariant(string data, string title)
        {
            if (data.IsNullOrEmpty())
                return MessageView(string.Format(Msg, "参数错误"));
            try
            {
                data = HttpUtility.UrlDecode(data) ?? "[]";
                if (data.EndsWith("%5"))
                    data = data.Replace("%5", "]");
                var ids = data.JsonToObject<List<string>>();
                var log = UserId.CreateLog(DownloadType.Variant, CurrentAgency, ids.Count());
                var result = DowloadEqByQIds(ids, title: title);
                log.Complete();
                return result;
            }
            catch (Exception ex)
            {
                return MessageView(string.Format(Msg, "参数错误：" + ex.Message));
            }
        }

        /// <summary>
        /// 问题下载
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="isPure"></param>
        /// <param name="fileName"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public ActionResult DowloadEqByQIds(ICollection<string> ids, bool isPure = false, string fileName = "", string title = "“我的得一”错题库下载-")
        {
            if (ids == null || !ids.Any())
                return MessageView(string.Format(Msg, "参数错误"));
            if (ids.Count > 50)
                return MessageView(string.Format(Msg, "下载的错题数量太多了"));

            #region 构造数据格式

            var questions = PaperContract.LoadQuestions(ids.ToArray());
            if (questions == null || !questions.Any())
                return MessageView(string.Format(Msg, "没有查询到题目资料"));

            //从试卷中筛选的错题不需要科目
            IDictionary<int, string> subjects = new Dictionary<int, string>();
            if (!isPure)
            {
                var subjectIds = questions.Select(q => q.SubjectId).Distinct().ToList();
                subjects =
                    SystemCache.Instance.Subjects()
                        .Where(t => subjectIds.Contains(t.Key))
                        .ToDictionary(k => k.Key, v => v.Value);
            }
            var qtypeIds = questions.Select(q => q.Type).Distinct().ToList();
            var qtypes = _systemContract.GetQuestionTypes(qtypeIds);

            var wdSubjects = new List<WdSubject>();
            questions.OrderBy(s => s.SubjectId).GroupBy(s => s.SubjectId).Select(s => new
            {
                subjectId = s.Key,
                types = s.OrderBy(t => t.Type).GroupBy(t => t.Type).Select(t => new
                {
                    type = t.Key,
                    data = t.ToList()
                }).ToList()
            }).ToList()
            .ForEach(s =>
            {
                var wdSubject = new WdSubject
                {
                    SubjectId = s.subjectId,
                    Sections = new List<WdSection>(),
                    SubjectName = subjects.Keys.Contains(s.subjectId) ? subjects[s.subjectId] : string.Empty
                };
                s.types.ForEach(t =>
                {
                    var wdSection = new WdSection { Type = t.type };
                    var qtype = qtypes.FirstOrDefault(i => i.Id == t.type);
                    if (qtype != null)
                        wdSection.Name = qtype.Name;
                    wdSection.Questions = t.data.ToWdQuestion();
                    wdSubject.Sections.Add(wdSection);
                });
                wdSubjects.Add(wdSubject);
            });
            #endregion

            if (fileName.IsNullOrEmpty())
                fileName = title + CurrentUser.Name + "-" + Clock.Now.ToString("yyyy-MM-dd");
            var group = new WdQuestionGroup { Title = fileName, Subjects = wdSubjects };

            using (var wh = new WordHelper())
            {
                //生成压缩文件
                var zip = wh.DownLoadZip(group);
                if (zip == null)
                    return MessageView(string.Format(Msg, "生成压缩包失败"));
                return File(zip.ToArray(), "application/x-zip-compressed", group.Title + ".zip");
            }
        }

        #endregion
    }
}