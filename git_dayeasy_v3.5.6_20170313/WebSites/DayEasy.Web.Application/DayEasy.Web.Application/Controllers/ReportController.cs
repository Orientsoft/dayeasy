using DayEasy.Application.Services;
using DayEasy.Application.Services.Dto;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 报表 </summary>
    [DAuthorize]
    [RoutePrefix("report")]
    public class ReportController : DController
    {
        private readonly IStatisticContract _statisticContract;
        private readonly IGroupContract _groupContract;
        private readonly IApplicationContract _applicationContract;
        private const string AppName = "报表";


        public ReportController(IUserContract userContract, IStatisticContract statisticContract, IGroupContract groupContract, IApplicationContract applicationContract)
            : base(userContract)
        {
            _statisticContract = statisticContract;
            _groupContract = groupContract;
            _applicationContract = applicationContract;
            ViewBag.AppName = AppName;
        }

        public ActionResult Index()
        {
            if (CurrentUser.IsTeacher())
                return RedirectToAction("ClassReports");
            return RedirectToAction("KpStatistic");
        }

        [Route("class-reports")]
        public ActionResult ClassReports(int pageIndex = 1, int pageSize = 24)
        {
            var result = _applicationContract.ClassReports(UserId, DPage.NewPage(pageIndex - 1, pageSize));
            ViewData["totalCount"] = result.TotalCount;
            return View((result.Data ?? new List<VReportDto>()).ToList());
        }

        [Route("grade-reports")]
        public ActionResult GradeReports(int pageIndex = 1, int pageSize = 10)
        {
            var result = _applicationContract.GradeReports(UserId, CurrentUser.Role, DPage.NewPage(pageIndex - 1, pageSize));
            ViewData["totalCount"] = result.TotalCount;
            return View((result.Data ?? new List<VGradeReportDto>()).ToList());
        }

        #region 知识点掌握情况统计
        /// <summary>
        /// 知识点掌握情况统计
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [Route("learn/kp")]
        public ActionResult KpStatistic()
        {
            var subjectId = "sub".Query(-1);
            string groupId = "cid".Query(string.Empty),
                startTimeStr = "start".Query(string.Empty),
                endTimeStr = "end".Query(string.Empty);

            #region 处理圈子和学科

            if (CurrentUser.IsStudent() || CurrentUser.IsParents())
            {
                var subjects = SystemCache.Instance.Subjects();
                if (subjects.Count > 0)
                {
                    if (subjectId < 0)
                    {
                        subjectId = subjects.Keys.First();
                    }

                    ViewData["subjects"] = subjects.ToSlectListItemList(u => u.Value, u => u.Key, subjectId);
                }
            }
            else if (CurrentUser.IsTeacher())
            {
                var groups = _groupContract.Groups(ChildOrUserId, (int)GroupType.Class);
                if (groups.Status && groups.Data != null && groups.Data.Any())
                {
                    if (string.IsNullOrEmpty(groupId))
                    {
                        var firstGroup = groups.Data.ToList().FirstOrDefault();
                        if (firstGroup != null)
                            groupId = firstGroup.Id;
                    }

                    ViewData["groups"] = groups.Data.ToList().ToSlectListItemList(u => u.Name, u => u.Id, groupId);
                }

                subjectId = CurrentUser.SubjectId;
            }
            #endregion

            var result = _statisticContract.GetKpStatistic(new SearchKpStatisticDataDto()
            {
                EndTimeStr = endTimeStr,
                GroupId = groupId,
                Role = (CurrentUser.IsStudent() || CurrentUser.IsParents()) ? UserRole.Student : UserRole.Teacher,
                StartTimeStr = startTimeStr,
                SubjectId = subjectId,
                UserId = ChildOrUserId,
                RegistTime = ChildOrUserRegistTime
            });

            KpStatisticDataDto resultDto = null;

            if (result.Status)
                resultDto = result.Data;

            return View(resultDto);
        }
        #endregion

        #region 错题top 10
        /// <summary>
        /// 错题top 10 
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [Route("errortop10")]
        public ActionResult ErrorTopTen()
        {
            var weekNum = "wn".Query(1);
            var subject = "sub".Query(-1);
            var groupId = "cid".Query("");

            var groups = _groupContract.Groups(ChildOrUserId, (int)GroupType.Class);
            if (groups.Status && groups.Data != null && groups.Data.Any())
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    var firstGroup = groups.Data.ToList().FirstOrDefault();
                    if (firstGroup != null)
                        groupId = firstGroup.Id;
                }

                ViewData["groups"] = groups.Data.ToList().ToSlectListItemList(u => u.Name, u => u.Id, groupId);
            }

            if (CurrentUserRoles.Contains(UserRole.Student) || CurrentUserRoles.Contains(UserRole.Parents))
            {
                var subjects = SystemCache.Instance.Subjects();
                if (subjects.Count > 0)
                {
                    if (subject < 0)
                    {
                        subject = subjects.Keys.First();
                    }

                    ViewData["subjects"] = subjects.ToSlectListItemList(u => u.Value, u => u.Key, subject);
                }
            }
            else if (CurrentUserRoles.Contains(UserRole.Teacher))
            {
                subject = CurrentUser.SubjectId;

                ViewData["studentCount"] = _groupContract.GroupMemberCount(groupId);
            }


            var result = _statisticContract.ErrorTopTen(new SearchErrorTopTenDto()
            {
                GroupId = groupId,
                RegisTime = ChildOrUserRegistTime,
                SubjectId = subject,
                UserId = ChildOrUserId,
                WeekNum = weekNum
            });

            ErrorTopTenDataDto data = null;
            if (result.Status)
            {
                data = result.Data;
            }
            else if (int.TryParse(result.Message, out weekNum))
            {
                return RedirectToAction("ErrorTopTen", "Report", new { sub = subject, wn = weekNum, cid = groupId });
            }

            return View(data);
        }
        #endregion

        #region 班级综合成绩

        #region 班级综合成绩--显示
        /// <summary>
        /// 班级综合成绩--显示
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Teacher, "/report/learn/kp")]
        [Route("learn/class-score")]
        public ActionResult ClassScores()
        {
            ViewData["subjectName"] = CurrentUser.SubjectName;

            //教师所教班级年级
            var teachGroupes = _groupContract.GroupGradeYears(CurrentUser.Id);
            if (teachGroupes.Status && teachGroupes.Data != null)
            {
                if (teachGroupes.Data != null && teachGroupes.Data.Any())
                {
                    ViewData["graduateYears"] = teachGroupes.Data;
                }
            }

            return View();
        }
        #endregion

        #region 异步获取班级综合成绩统计数据
        /// <summary>
        /// 异步获取班级综合成绩统计数据
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Teacher, "/report/learn/kp")]
        [HttpPost]
        [Route("learn/class-scoredata")]
        public ActionResult GetClassScores()
        {
            var startDateStr = "startDate".Form("");
            var endDateStr = "endDate".Form("");
            var year = "year".Form("");

            var result = _statisticContract.GetClassScores(new SearchClassScoresDto()
            {
                EndDateStr = endDateStr,
                StartDateStr = startDateStr,
                SubjectId = CurrentUser.SubjectId,
                UserId = CurrentUser.Id,
                GradeYear = year
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region 学生成绩排名升降图
        /// <summary>
        /// 学生成绩排名升降图
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Teacher, "/report/learn/kp")]
        [Route("learn/stu-rank/{classId?}")]
        [OutputCache(Duration = 120)]
        public ActionResult StudentRank(string classId)
        {
            //老师所教的班级
            var teachGroupes = _groupContract.Groups(CurrentUser.Id, (int)GroupType.Class);
            if (teachGroupes.Status && teachGroupes.Data != null && teachGroupes.Data.Any())
            {
                if (string.IsNullOrEmpty(classId))
                {
                    var first = teachGroupes.Data.FirstOrDefault();
                    if (first != null)
                        classId = first.Id;//默认第一个班级
                }

                ViewData["teachGroupes"] = teachGroupes.Data.ToList();
            }

            ViewData["subjectName"] = CurrentUser.SubjectName;
            ViewData["groupId"] = classId;

            if (string.IsNullOrEmpty(classId)) return View();

            var rankResult = _statisticContract.StudentRank(classId, CurrentUser.SubjectId);
            if (rankResult.Status && rankResult.Data != null)
            {
                ViewData["studentRank"] = rankResult.Data;

                if (rankResult.Data.StudentRankList != null)//查询学生
                {
                    var studentIds = rankResult.Data.StudentRankList.Select(u => u.StudentId).ToList();
                    if (studentIds.Count > 0)
                    {
                        ViewData["student"] = UserContract.LoadList(studentIds);
                    }
                }
            }

            return View();
        }
        #endregion

        #region 教师端学生排名折线图

        #region 教师端学生排名折线图--显示
        /// <summary>
        /// 教师端学生排名折线图--显示
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Teacher, "/report/learn/kp")]
        [Route("learn/stu-rank/detail/{cid}/{uid}")]
        public ActionResult StudentRankDetail(string cid, string uid)
        {
            long studentId;
            if (!long.TryParse(uid, out studentId))
            {
                studentId = 0;
            }

            if (studentId <= 0 || string.IsNullOrEmpty(cid))
                return MessageView("该学生不存在！");

            var student = UserContract.Load(studentId);
            if (student != null)
            {
                //查询学生的班级
                var status = _groupContract.IsGroupMember(studentId, cid);
                if (status == CheckStatus.Normal)
                {
                    ViewData["student"] = student;
                    ViewData["groupId"] = cid;

                    return View();
                }

                return MessageView("请查看自己班上的学生！");
            }
            return MessageView("该学生不存在！");
        }
        #endregion

        #region 获取教师端学生排名折线图数据
        /// <summary>
        /// 获取教师端学生排名折线图数据
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Teacher, "/report/learn/kp")]
        [HttpPost]
        [Route("learn/stu-rankdata")]
        public ActionResult GetStuRankDetail()
        {
            var userIdStr = "userId".Form("");
            var classId = "classId".Form("");
            var startDateStr = "startDate".Form("");
            var endDateStr = "endDate".Form("");

            long userId;
            if (!long.TryParse(userIdStr, out userId))
            {
                userId = 0;
            }
            if (userId < 1 || string.IsNullOrEmpty(classId)) return MessageView("该学生不存在！");

            //查询学生的班级
            var status = _groupContract.IsGroupMember(userId, classId);
            if (status != CheckStatus.Normal) return MessageView("请查看自己班上的学生！");

            var result = _statisticContract.GetStuRankDetail(new SearchStudentRankDto()
            {
                EndTimeStr = endDateStr,
                StartTimeStr = startDateStr,
                StudentId = userId,
                SubjectId = CurrentUser.SubjectId,
                TeacherId = CurrentUser.Id
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region 学生个人成绩统计

        #region 学生个人成绩统计--显示
        /// <summary>
        /// 学生个人成绩统计--显示
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Student, "/report/learn/kp")]
        [Route("learn/stu-scores")]
        public ActionResult StudentScores()
        {
            var result = _statisticContract.GetStudentSubject(new SearchStudentRankDto()
            {
                StudentId = ChildOrUserId
            });

            if (result.Status)
                ViewData["subjects"] = result.Data;

            return View();
        }
        #endregion

        #region 获取学生成绩统计
        /// <summary>
        /// 获取学生成绩统计
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Student, "/report/learn/kp")]
        [Route("learn/student-scoredata")]
        [HttpPost]
        public ActionResult GetStudentScores()
        {
            int subjectId = "subjectId".Form(-1);
            var startDateStr = "startDate".Form("");
            var endDateStr = "endDate".Form("");

            if (subjectId < 0) return Json(DResult.Error("没有找到相关数据!"), JsonRequestBehavior.AllowGet);

            var result = _statisticContract.GetStudentScores(new SearchStudentRankDto()
            {
                EndTimeStr = endDateStr,
                StartTimeStr = startDateStr,
                StudentId = ChildOrUserId,
                SubjectId = subjectId
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取学生时间段内考过的科目
        /// <summary>
        /// 获取学生时间段内考过的科目
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Student, "/report/learn/kp")]
        [Route("learn/student-subjectdata")]
        [HttpPost]
        public ActionResult GetStudentSubject()
        {
            var startDateStr = "startDate".Form("");
            var endDateStr = "endDate".Form("");

            var result = _statisticContract.GetStudentSubject(new SearchStudentRankDto()
            {
                EndTimeStr = endDateStr,
                StartTimeStr = startDateStr,
                StudentId = ChildOrUserId
            });

            if (result.Status && result.Data != null)
            {
                var subjectList = result.Data.Select(u => new
                {
                    Id = u.Key,
                    Name = u.Value
                }).ToList();

                return Json(DResult.Succ(subjectList), JsonRequestBehavior.AllowGet);
            }

            return Json(DResult.Error("没有找到相关数据！"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        //[AjaxOnly]
        //[HttpGet]
        //[Route("class-reports")]
        //public ActionResult ClassReports(DPage page = null)
        //{
        //    var result = _applicationContract.ClassReports(UserId, page ?? DPage.NewPage(0, 10));
        //    return DeyiJson(result);
        //}

        //[AjaxOnly]
        //[HttpGet]
        //[Route("grade-reports")]
        //public ActionResult GradeReports(DPage page = null)
        //{
        //    var result = _applicationContract.GradeReports(UserId, CurrentUser.Role, page ?? DPage.NewPage(0, 10));
        //    return DeyiJson(result);
        //}
    }
}