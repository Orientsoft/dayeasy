using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Cache;
using DayEasy.Core.Config;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    [RoleAuthorize(UserRole.Teacher, "/")]
    [RoutePrefix("paper")]
    public class PaperController : DController
    {
        private readonly IPaperContract _paperContract;
        private readonly IGroupContract _groupContract;
        private readonly ISystemContract _systemContract;
        private readonly IPublishContract _publishContract;
        private const string AppName = "试卷库";

        public PaperController(IUserContract userContract, IPaperContract paperContract, IGroupContract groupContract,
            ISystemContract systemContract, IPublishContract publishContract)
            : base(userContract)
        {
            _paperContract = paperContract;
            _groupContract = groupContract;
            _systemContract = systemContract;
            _publishContract = publishContract;
            ViewBag.AppName = AppName;
        }

        #region 试卷库--列表页

        /// <summary>
        /// 试卷库--列表页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var grades = StageList.SelectMany(t => ((byte)t.Key).Grades().ToSlectListItemList(u => u.Value, u => u.Key)).ToList();
            grades.Insert(0, new SelectListItem() { Text = "选择年级", Value = "-1" });
            ViewData["gradeList"] = grades;

            //todo:YBG 试卷分派
            //var hasAllot = false;
            //if (!string.IsNullOrWhiteSpace(CommonHelper.Config.DeyiAccount))
            //{
            //    hasAllot = CommonHelper.Config.DeyiAccount.To(new long[] { }).Contains(CurrentUser.UserId);
            //}

            ////是否具有分派试卷的权限
            //ViewData["hasAllot"] = hasAllot;
            ////收到的分派试卷数量
            //ViewData["allotCount"] = _allotFacade.GetCount(a =>
            //    a.ReceiveId == CurrentUser.UserId && a.Status == (byte)PaperAllotStatus.Sended);

            return View();
        }

        #endregion

        #region 试卷数据列表
        /// <summary>
        /// 试卷数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PaperList()
        {
            var pageIndex = "pageIndex".Form(1);
            //var share = "sr".Form((byte)ShareRange.Self);
            var key = "key".Form("");
            var grade = "g".Form(-1);
            var status = "s".Form((byte)PaperStatus.Normal);

            var result = _paperContract.PaperList(new SearchPaperDto()
            {
                Page = pageIndex - 1,
                Size = 10,
                Grade = grade,
                Key = key,
                Share = (byte)ShareRange.Self,
                //Stage = stage,
                Status = status,
                SubjectId = CurrentUser.SubjectId,
                UserId = CurrentUser.Id
            });

            ViewData["totalCount"] = result.TotalCount;
            ViewData["pageindex"] = pageIndex;

            return PartialView(result.Data);
        }
        #endregion

        #region 试卷库--详情页

        /// <summary>
        /// 试卷库--详情页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(string id)
        {
            PaperDetailDto paperModel = null;
            if (!string.IsNullOrEmpty(id))
            {
                var result = _paperContract.PaperDetailById(id);
                if (result.Status)
                    paperModel = result.Data;
            }
            return View(paperModel);
        }

        #endregion

        #region 删除试卷
        /// <summary>
        /// 删除试卷
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PaperDelete()
        {
            var pid = "pid".Form("");

            var result = _paperContract.DeletePaper(pid, CurrentUser.Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 试卷答案--显示
        /// <summary>
        /// 试卷答案--显示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("answer/{id}")]
        public ActionResult Answer(string id, string edit)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var result = _paperContract.PaperDetailById(id);
                if (!result.Status)
                    return View();

                var paperModel = result.Data;
                if (paperModel != null && paperModel.PaperBaseInfo.AddedBy == CurrentUser.Id && paperModel.PaperBaseInfo.Status == (byte)PaperStatus.Normal)
                {
                    ViewData["isMyself"] = true;

                    if (!string.IsNullOrEmpty(edit) && edit.Trim() == "edit")
                    {
                        ViewData["canEdit"] = true;
                    }
                }

                return View(paperModel);
            }

            return View();
        }
        #endregion

        #region 试卷答案--保存
        /// <summary>
        /// 试卷答案--保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("save-answer")]
        public ActionResult SaveAnswer()
        {
            var answers = "answers".Form("");
            var paperId = "paperId".Form("");
            var finished = "finished".Form(false);

            answers = Server.HtmlDecode(answers);
            var answerList = JsonHelper.JsonList<MakePaperAnswerDto>(answers).ToList();
            if (!answerList.Any())
            {
                return Json(new DResult(false, "参数错误，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }

            var result = _paperContract.EditPaperAnswer(paperId, answerList, CurrentUser.Id, finished);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 发布试卷

        #region 发布试卷的显示页面
        /// <summary>
        /// 发布试卷的显示页面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PublishPaper()
        {
            //我的圈子
            var groups = new List<GroupDto>();
            var groupList = _groupContract.Groups(CurrentUser.Id);
            if (groupList.Status)
            {
                groups = groupList.Data.ToList();
            }

            //试卷
            ViewData["paperId"] = "paperId".Form("");
            //同事圈Id
            ViewData["groupId"] = "groupId".Form("");

            return PartialView(groups);
        }
        #endregion

        #region 发布试卷
        /// <summary>
        /// 发布试卷
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PubPaper()
        {
            var pId = "pId".Form("");
            var groupIds = "groupIds".Form("");
            var sendMsg = "sendMsg".Form("");
            var sourceGId = "sourceGId".Form("");

            var result = _publishContract.PulishPaper(pId, groupIds, sourceGId, CurrentUser.Id, sendMsg);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 发布试卷选择列表
        /// <summary>
        /// 发布试卷选择列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PublishPapers()
        {
            var pageIndex = "pageIndex".Form(1);
            //试卷
            var paperId = "paperId".Form("");

            var result = _paperContract.PaperPublishList(paperId, new SearchPaperDto()
            {
                Page = pageIndex - 1,
                Size = 4,
                SubjectId = CurrentUser.SubjectId,
                UserId = CurrentUser.Id
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region 在线出卷

        #region 在线出卷--选择试卷类型

        /// <summary>
        /// 在线出卷--选择试卷类型
        /// </summary>
        /// <returns></returns>
        public ActionResult ChoosePaperType()
        {
            ViewData["Stages"] = StageList.ToSlectListItemList(u => u.Value, u => u.Key);

            return View();
        }

        #endregion

        #region 在线出卷--显示

        /// <summary>
        /// 在线出卷--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult CreatePaper()
        {
            var paperBaseStr = "paperBase".Form("");
            var id = "id".Form("a");
            var stage = "stage".Form((byte)StageEnum.JuniorMiddleSchool);

            //出卷信息处理公共方法
            var result = _paperContract.PaperBaseAction(paperBaseStr, id, CurrentUser.SubjectId, stage);
            if (result.Status)
            {
                stage = (byte)result.Data.PaperBaseDto.Stage;
                ViewData["chooseQuestionData"] = result.Data;
            }

            ViewData["GradeList"] = stage.Grades().ToSlectListItemList(u => u.Value, u => u.Key);

            return View();
        }

        #endregion

        #region 自动出卷--显示
        /// <summary>
        /// 自动出卷--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult AutoCreatePaper()
        {
            var autoData = "autoData".Form(""); ;//判断是否操作了自动出卷
            var paperBaseStr = "paperBase".Form("");
            var paperType = "id".Form("a");
            var stage = "stage".Form((byte)StageEnum.JuniorMiddleSchool);

            var result = string.IsNullOrEmpty(autoData) ? _paperContract.PaperBaseAction(paperBaseStr, paperType, CurrentUser.SubjectId, stage) : _paperContract.AutoPaperBaseAction(autoData, paperType, CurrentUser.SubjectId, stage);

            if (result.Status)
            {
                stage = (byte)result.Data.PaperBaseDto.Stage;
                if (string.IsNullOrEmpty(autoData))
                {
                    result.Data.HasAll = true;
                }

                ViewData["chooseQuestionData"] = result.Data;
            }

            if (string.IsNullOrEmpty(autoData) && string.IsNullOrEmpty(paperBaseStr))
            {
                ViewData["isFirstLoad"] = true;
            }

            ViewData["GradeList"] = stage.Grades().ToSlectListItemList(u => u.Value, u => u.Key);

            return View();
        }
        #endregion

        #region 在线出卷--保存试卷数据

        /// <summary>
        /// 在线出卷--保存试卷数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SavePaperData()
        {
            var paperId = "paperId".Form("");
            var paperData = "paperData".Form("");
            var isTemp = "isTemp".Form("");
            //var shareSchool = "shareSchool".Form("");
            var answers = "answers".Form("");

            var data = JsonHelper.Json<MakePaperDto>(paperData);
            if (data == null)
                return Json(new DResult(false, "数据错误，请稍后重试！"), JsonRequestBehavior.AllowGet);
            var isDraft = true;

            if (!bool.TryParse(isTemp, out isDraft))
            {
                isDraft = true;
            }
            data.IsTemp = isDraft;
            data.UserId = CurrentUser.Id;
            data.SubjectId = CurrentUser.SubjectId;

            if (!isDraft && string.IsNullOrEmpty(answers))
            {
                return Json(new DResult(false, "参数错误，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }

            List<MakePaperAnswerDto> answerList = null;
            if (!isDraft && !string.IsNullOrEmpty(answers))
            {
                answers = Server.HtmlDecode(answers);

                answerList = JsonHelper.JsonList<MakePaperAnswerDto>(answers).ToList();
                if (answerList.Count < 1)
                {
                    return Json(new DResult(false, "参数错误，请稍后重试！"), JsonRequestBehavior.AllowGet);
                }
            }

            var result = string.IsNullOrEmpty(paperId)
                ? _paperContract.SavePaper(data, answerList) :
                _paperContract.EditPaper(paperId, data, answerList);
            if (result.Status && !string.IsNullOrWhiteSpace(paperId))
            {
                CacheManager.GetCacher("paper").Remove(paperId);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region 选择题目

        #region 选择题目--显示
        /// <summary>
        /// 选择题目--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult ChooseQu()
        {
            var paperBase = new PaperBaseDto();
            var paperBaseStr = "paperBase".Form("");
            if (!string.IsNullOrEmpty(paperBaseStr))
            {
                var temp = JsonHelper.Json<PaperBaseDto>(paperBaseStr);
                if (temp != null)
                {
                    paperBase = temp;
                }
            }
            paperBase.SubjectId = CurrentUser.SubjectId;

            ViewData["paperBase"] = paperBase;

            //根据用户科目 获取题目类型
            var qtypeList = SystemCache.Instance.SubjectQuestionTypes(CurrentUser.SubjectId).ToList();
            //_systemContract.GetQuTypeBySubjectId(CurrentUser.SubjectId);
            ViewData["qTypeList"] = qtypeList;

            ViewData["gradeList"] = ((byte)paperBase.Stage).Grades().ToSlectListItemList(u => u.Value, u => u.Key);

            //是否存在章节关联
            var sbookResult = _systemContract.SchoolBooks((byte)paperBase.Stage, CurrentUser.SubjectId, false);
            if (sbookResult.Status && sbookResult.Data != null && sbookResult.Data.Any())
                ViewData["sbooks"] = sbookResult.Data.ToList();

            return View();
        }
        #endregion

        #region 选择题目--列表

        /// <summary>
        /// -选择题目--列表
        /// </summary>
        /// <returns></returns>
        public ActionResult QuestionList()
        {
            var qtype = "qtype".Form(-1);
            //var shareRange = RequestHelper.GetFormInt32("share", (byte)ShareRange.Public);
            var sortBy = "sort".Form(-1);
            var star = "star".Form(-1);
            var year = "year".Form(-1);
            var kp = "kp".Form(string.Empty);
            var keyword = "queryKey".Form(string.Empty);
            var qSourceType = "qSourceType".Form(string.Empty).Trim();
            var area = "area".Form(string.Empty).Trim();
            var stage = "stage".Form((byte)StageEnum.JuniorMiddleSchool);

            var pageIndex = "pageIndex".Form(1);
            const int pageSize = 15;

            var tags = new List<string>();
            if (!string.IsNullOrEmpty(qSourceType) && qSourceType != "-1" && qSourceType != "1")
            {
                tags.Add(qSourceType);
            }
            if (year > 0)
            {
                tags.Add(year.ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrEmpty(area) && area != "-1")
            {
                tags.Add(area);
            }
            List<string> kps = null;
            if (kp.IsNotNullOrEmpty())
            {
                kps = kp.IndexOf(',') > 0 ? kp.Split(',').ToList() : new List<string> { kp };
            }

            var query = new SearchQuestionDto
            {
                UserId = CurrentUser.Id,
                QuestionType = qtype,
                Stages = new List<int> { stage },
                Keyword = keyword,
                SubjectId = CurrentUser.SubjectId,
                IsHighLight = false,
                Page = pageIndex - 1,
                Size = pageSize,
                ShareRange = (qSourceType == "1" ? (int)ShareRange.Self : -1),
                Tags = tags,
                Points = kps,
                LoadCreator = true,
                //Points = string.IsNullOrEmpty(kp) ? null : new[] { kp },
                Difficulties = star < 1 ? null : new[] { (double)star }
            };

            switch (sortBy)
            {
                case 1://1:错误率倒序  
                    query.Order = QuestionOrder.ErrorRateDesc;
                    break;
                case 2://2:使用次数倒序
                    query.Order = QuestionOrder.UsedCountDesc;
                    break;
                default://0:时间倒序 
                    query.Order = QuestionOrder.AddedAtDesc;
                    break;
            }

            var result = _paperContract.SearchQuestion(query);

            //ViewData["shareRange"] = shareRange;
            ViewData["totalCount"] = result.TotalCount;

            return PartialView(result.Data);
        }

        #endregion

        #region 选择题目--试卷列表
        /// <summary>
        /// 选择题目--试卷列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PaperData()
        {
            var grade = "grade".Form(-1);
            var source = "source".Form(-1);
            var key = "key".Form("");
            var kp = "kp".Form("");
            var pageIndex = "pageIndex".Form(1);
            var stage = "stage".Form((byte)StageEnum.JuniorMiddleSchool);

            var result = _paperContract.TopicsPaper(new SearchTopicDto()
            {
                Grade = grade,
                Stage = stage,
                Key = key,
                Kp = kp,
                Source = source,
                Page = pageIndex - 1,
                Size = 20,
                SubjectId = CurrentUser.SubjectId,
                UserId = CurrentUser.Id
            });

            if (!result.Status) return PartialView();

            ViewData["totalCount"] = result.TotalCount;
            if (!string.IsNullOrEmpty(kp) && result.TotalCount > 0)
            {
                ViewData["paperIds"] = result.Data.Select(u => u.PaperId).ToList().ToJson();
                ViewData["kpData"] = kp;
            }

            return PartialView(result.Data);
        }
        #endregion

        #region 获取试卷问题包含知识点的数量
        /// <summary>
        /// 获取试卷问题包含知识点的数量
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPaperKpCount()
        {
            var paperIds = "paperIds".Form("");
            var kp = "kp".Form("");
            if (!string.IsNullOrEmpty(paperIds) && !string.IsNullOrEmpty(kp))
            {
                var paperIdList = paperIds.JsonToObject<List<string>>();
                if (paperIdList != null)
                {
                    var result = _paperContract.GetPaperKpCount(paperIdList, kp);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new List<string>(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 试卷选择--试卷详情
        /// <summary>
        /// 试卷选择--试卷详情
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ShowPaperDetail()
        {
            var paperId = "paperId".Form("");
            if (!string.IsNullOrEmpty(paperId))
            {
                var paperModel = _paperContract.PaperDetailById(paperId);

                ViewData["questionTypes"] =
                    SystemCache.Instance.SubjectQuestionTypes(CurrentUser.SubjectId);
                //_systemContract.GetQuTypeBySubjectId(CurrentUser.SubjectId);
                ViewData["kp"] = "kp".Form("");

                return PartialView(paperModel.Data);
            }
            return PartialView();
        }
        #endregion

        #endregion

        #region 试卷编辑
        /// <summary>
        /// 试卷编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return View();

            PaperDetailDto paperModel = null;
            var paperBaseStr = "paperBase".Form("");//判断是否操作了找题

            if (string.IsNullOrEmpty(paperBaseStr))
            {
                var result = _paperContract.PaperDetailById(id);
                if (result.Status)
                    paperModel = result.Data;
            }
            else
            {
                var result = _paperContract.PaperDetailById(id);
                if (result.Status)
                    paperModel = result.Data;

                if (paperModel != null)
                {
                    var pType = paperModel.PaperBaseInfo.PaperType == (byte)PaperType.AB ? "ab" : "a";

                    var paperBaseResult = _paperContract.PaperBaseAction(paperBaseStr, pType, CurrentUser.SubjectId, paperModel.PaperBaseInfo.Stage);//出卷信息处理公共方法
                    if (paperBaseResult.Status)
                        ViewData["ChooseQuestionDataDto"] = paperBaseResult.Data;

                    ViewData["hasAdded"] = true;
                }
            }

            if (paperModel != null)
            {
                ViewData["GradeList"] = paperModel.PaperBaseInfo.Stage.Grades().ToSlectListItemList(u => u.Value, u => u.Key);

                if (paperModel.PaperBaseInfo.AddedBy != CurrentUser.Id)
                {
                    paperModel = null;
                }
            }

            //是否有修改权限
            var hasEdit = CurrentUser.Code.HasSpecialAuth(SpecialAccountType.EditPaperQuestion);
            if (hasEdit)
            {
                //是否具有分派试卷的权限
                ViewData["hasEdit"] = true;
            }

            return View(paperModel);
        }

        #endregion

        #region 试卷录入问题

        #region 试卷添加问题--显示

        /// <summary>
        /// 试卷添加问题--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult AddQuestion()
        {
            ViewData["stage"] = "stage".Form((byte)StageEnum.JuniorMiddleSchool);
            var types = SystemCache.Instance.SubjectQuestionTypes(CurrentUser.SubjectId).ToList();
            ViewData["qTypes"] = types;
            //_systemContract.GetQuTypeBySubjectId(CurrentUser.SubjectId);
            //有选项的题型
            var optionTypes =
                types.Where(t => (t.Style & (byte)QuestionTypeStyle.Option) > 0).Select(t => t.Id).ToList();
            ViewData["optionTypes"] = optionTypes;
            //有小问的题型
            List<int> smallTypes;
            if (CurrentUser.SubjectId == 3)
            {
                smallTypes =
                    types.Where(t => (t.Style & (byte)QuestionTypeStyle.Detail) > 0).Select(t => t.Id).ToList();
            }
            else
            {
                smallTypes =
                    types.Where(
                        t =>
                            (t.Style & (byte)QuestionTypeStyle.Detail) > 0 &&
                            (t.Style & (byte)QuestionTypeStyle.Option) > 0 &&
                            (t.Style & (byte)QuestionTypeStyle.Answer) == 0).Select(t => t.Id).ToList();
            }
            ViewData["smallTypes"] = smallTypes;
            return PartialView();
        }

        #endregion

        #region 试卷添加问题--保存
        /// <summary>
        /// 试卷添加问题--保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveQuestion()
        {
            var qType = "qtype".Form(-1);
            var kps = "kps".Form("");
            var qContent = Server.HtmlDecode("qContent".Form(""));
            var optionNum = "optionNum".Form(0);
            var smallQuNum = "smallQuNum".Form(0);
            var stage = "stage".Form((byte)StageEnum.JuniorMiddleSchool);

            if (Consts.HasSmallQType.Contains(qType) && optionNum > 0 && smallQuNum < 1)
            {
                return Json(DResult.Error("请输入小问数量！"), JsonRequestBehavior.AllowGet);
            }

            var result = _paperContract.AddPaperQuestion(new AddQuestionDto()
            {
                Kps = kps,
                OptionNum = optionNum,
                QContent = qContent,
                QType = qType,
                SmallQuNum = smallQuNum,
                RealName = CurrentUser.Name,
                SubjectId = CurrentUser.SubjectId,
                UserId = CurrentUser.Id,
                Stage = stage
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取知识点
        /// <summary>
        /// 获取知识点
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetKps()
        {
            var kp = "kp".Form("");
            var stage = "stage".Form((byte)StageEnum.JuniorMiddleSchool);
            if (!string.IsNullOrEmpty(kp))
            {
                var list = _systemContract.Knowledges(new SearchKnowledgeDto()
                {
                    Keyword = kp,
                    SubjectId = CurrentUser.SubjectId,
                    Stage = stage,
                    Page = 0,
                    Size = 10
                });

                if (!list.Any())
                    return Json(new List<string>(), JsonRequestBehavior.AllowGet);

                var kps = list.Select(u => new { id = u.Code, name = u.Name }).ToList();
                return Json(kps, JsonRequestBehavior.AllowGet);
            }
            return Json(new List<string>(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region 录入答案--显示
        /// <summary>
        /// 录入答案--显示
        /// </summary>
        /// <returns></returns>
        public ActionResult InputAnswer()
        {
            var paperData = "paperData".Form("");
            var data = JsonHelper.Json<MakePaperDto>(paperData);

            var paperModel = _paperContract.MakePreviewData(data);

            return PartialView(paperModel);
        }
        #endregion

        #region 在线出卷，按章节知识点选题

        public ActionResult SchoolBooks(byte stage, int subject)
        {
            return DeyiJson(_systemContract.SchoolBooks(stage, subject, false));
        }

        public ActionResult SbChapters(string code)
        {
            return DeyiJson(_systemContract.SbChapterKnowledges(code));
        }

        #endregion

        [Route("answer-sheet/{paperId}")]
        public ActionResult AnswerSheet(string paperId)
        {
            var paperResult = _paperContract.PaperDetailById(paperId);
            if (!paperResult.Status)
                return MessageView(paperResult.Message);
            var paper = paperResult.Data;

            var sections = paper.PaperSections.GroupBy(s => s.PaperSectionType);
            foreach (var section in sections)
            {
                var questions = section.SelectMany(s => s.Questions.Where(q => q.Question.IsObjective)).ToList();
                if (!questions.Any())
                    continue;
                var data = new Dictionary<string, int>();
                var sorts = _paperContract.PaperSorts(paper, true, section.Key);
                foreach (var question in questions)
                {
                    if (!question.Question.HasSmall)
                    {
                        //无小问
                        data.Add(sorts[question.Question.Id], question.Question.Answers.Count);
                    }
                    else
                    {
                        //小问
                        foreach (var small in question.Question.Details)
                        {
                            data.Add(sorts[small.Id], small.Answers.Count);
                        }
                    }
                }
                ViewData["objective_" + section.Key] = data;
            }
            return View(paper);
        }
    }
}