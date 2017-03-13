using DayEasy.Application.Services;
using DayEasy.Application.Services.Dto;
using DayEasy.Application.Services.Helper;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Enum.Statistic;
using DayEasy.Core.Domain;
using DayEasy.Office;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using DayEasy.Web.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 教务管理 </summary>
    [DAuthorize]
    [RoutePrefix("ea")]
    [Route("{action=index}")]
    public class EducationalController : DController
    {
        private readonly IExaminationContract _examinationContract;
        private readonly IGroupContract _groupContract;
        private readonly IStatisticContract _statisticContract;
        private readonly IApplicationContract _applicationContract;
        private const string AppName = "教务管理";

        public EducationalController(IUserContract userContract, IExaminationContract examinationContract,
            IGroupContract groupContract, IStatisticContract statisticContract, IApplicationContract applicationContract)
            : base(userContract)
        {
            _examinationContract = examinationContract;
            _groupContract = groupContract;
            _statisticContract = statisticContract;
            _applicationContract = applicationContract;
            ViewBag.AppName = AppName;
        }

        private DResult<AgencyDto> _appliactionAgency;

        private DResult<AgencyDto> ApplicationAgency
        {
            get { return _appliactionAgency ?? (_appliactionAgency = UserContract.ApplicationAgency(UserId, 1012)); }
        }

        #region Views
        /// <summary> 学校概况 </summary>
        public ActionResult Index()
        {
            var agency = ApplicationAgency;
            if (!agency.Status)
                return MessageView(agency.Message);
            ViewBag.AgencyId = agency.Data.Id;
            ViewBag.Agency = agency.Data.Name;
            return View();
        }

        /// <summary> 师生画像 </summary>
        [Route("portrait")]
        public ActionResult Portrait()
        {
            var agency = ApplicationAgency;
            if (!agency.Status)
                return MessageView(agency.Message);
            ViewBag.AgencyId = agency.Data.Id;
            return View();
        }

        /// <summary> 考试地图 </summary>
        [Route("exam-map")]
        public ActionResult ExaminationMap()
        {
            var agency = ApplicationAgency;
            if (!agency.Status)
                return MessageView(agency.Message);
            ViewBag.AgencyId = agency.Data.Id;
            return View();
        }

        /// <summary> 学情补救 </summary>
        [Route("remedy")]
        public ActionResult Remedy()
        {
            var agency = ApplicationAgency;
            if (!agency.Status)
                return MessageView(agency.Message);
            ViewBag.AgencyId = agency.Data.Id;
            return View();
        }

        /// <summary> 单科分析 </summary>
        [HttpGet]
        [Route("subject")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Subject(int subject = -1, int pageindex = 1, int pagesize = 10)
        {
            var agencyResult = ApplicationAgency;
            if (!agencyResult.Status)
                return MessageView(agencyResult.Message);
            var result = _examinationContract.JointList(new JointSearchDto
            {
                AgencyId = agencyResult.Data.Id,
                Subject = subject,
                Page = pageindex - 1,
                Size = pagesize
            });
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.TotalCount = result.TotalCount;
            ViewData["subjects"] = SystemCache.Instance.Subjects();
            return View((result.Data ?? new List<ExamSubjectDto>()).ToList());
        }

        /// <summary> 年级报表 </summary>
        [HttpGet]
        [Route("exam")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Examinations(int pageindex = 1, int pagesize = 10)
        {
            var agencyResult = ApplicationAgency;
            if (!agencyResult.Status)
                return MessageView(agencyResult.Message);
            var result = _examinationContract.Examinations(-1, agencyResult.Data.Id, DPage.NewPage(pageindex - 1, pagesize));
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.TotalCount = result.TotalCount;
            return View(result.Data.ToList());
        }

        //[HttpGet]
        //[Route("rankings/{id:regex([0-9a-z]{32})}")]
        //[RoleAuthorize(UserRole.Teacher, "/")]
        //public ActionResult Rankings(string id)
        //{
        //    var rankResult = _examinationContract.Rankings(id);
        //    if (!rankResult.Status)
        //        return MessageView(rankResult.Message);
        //    ViewData["subjects"] = _examinationContract.ExamSubjects(id);
        //    ViewBag.ExamId = id;
        //    return View(rankResult.Data);
        //}

        /// <summary> 学生成绩汇总 </summary>
        [Route("summary/{id}")]
        [RoleAuthorize((UserRole.Student | UserRole.Parents), "/")]
        public ActionResult Summary(string id)
        {
            var result = _examinationContract.Summary(id, ChildOrUserId);
            if (!result.Status) return MessageView(result.Message);
            return View(result.Data);
        }

        /// <summary> 大型考试 - 图表报表 </summary>
        [HttpGet]
        [Route("charts/{id:regex([0-9a-z]{32})}")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Charts(string id)
        {
            ViewBag.ExamId = id;
            return View();
        }

        [HttpGet]
        [Route("u-charts/{id:regex([0-9a-z]{32})}")]
        public ActionResult UnionCharts(string id)
        {
            var result = _applicationContract.UnionCharts(id);
            if (!result.Status)
                return MessageView(result.Message, returnUrl: "ea/exam", returnText: "年级报表");
            return View(result.Data);
        }

        /// <summary> 批阅进行中 </summary>
        [HttpGet]
        [Route("markings")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Markings(int subject = -1, int pageIndex = 1, int pageSize = 10)
        {
            var agencyResult = ApplicationAgency;
            if (!agencyResult.Status)
                return MessageView(agencyResult.Message);
            var result = _examinationContract.JointList(new JointSearchDto
            {
                AgencyId = agencyResult.Data.Id,
                Subject = subject,
                Status = JointStatus.Normal,
                Page = pageIndex - 1,
                Size = pageSize
            });
            if (!result.Status || result.Data == null)
                return MessageView(result.Message);
            ViewBag.TotalCount = result.TotalCount;
            ViewData["subjects"] = SystemCache.Instance.Subjects();
            return View(result.Data.ToList());
        }

        /// <summary> 在校教师 </summary>
        [Route("teacher-list")]
        public ActionResult TeacherList(int subject = 0, int pageindex = 1, int pagesize = 10)
        {
            var subjects = SystemCache.Instance.Subjects();
            ViewData["subjects"] = subjects;
            var agencyResult = ApplicationAgency;
            if (!agencyResult.Status)
            {
                return MessageView("当前机构不存在");
            }
            var result = UserContract.LoadTeacher(subject, agencyResult.Data.Id, DPage.NewPage(pageindex - 1, pagesize));
            if (!result.Status)
            {
                return MessageView(result.Message);
            }

            var teachers = result.Data == null ? new List<BeTeacherDto>() : result.Data.ToList();
            teachers.ForEach(dto =>
            {
                if (subjects.ContainsKey(dto.SubjectId))
                    dto.SubjectName = subjects[dto.SubjectId];
            });
            ViewBag.TeacherCont = result.TotalCount;
            return View(teachers);
        }
        private DResults<GroupDto> GroupList(GroupType type, int page, int size)
        {
            var agencyResult = ApplicationAgency;
            if (!agencyResult.Status)
                return DResult.Errors<GroupDto>(agencyResult.Message);
            return _groupContract.SearchGroups(new SearchGroupDto
            {
                AgencyId = agencyResult.Data.Id,
                Types = new List<int> { (int)type },
                CertificationLevels = new byte?[] { null, (byte)GroupCertificationLevel.Normal },
                Page = page - 1,
                Size = size
            });
        }

        [HttpGet]
        [Route("class-list")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult ClassList(int pageIndex = 1, int pageSize = 15)
        {
            var groupResult = GroupList(GroupType.Class, pageIndex, pageSize);
            if (!groupResult.Status)
                return MessageView(groupResult.Message);
            ViewBag.TotalCount = groupResult.TotalCount;
            var result = ApplicationAgency;
            ViewData["Stage"] = result.Data.Stage;
            return View(groupResult.Data ?? new List<GroupDto>());
        }

        [HttpGet]
        [Route("colleague-list")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult ColleagueList(int pageIndex = 1, int pageSize = 15)
        {
            var groupResult = GroupList(GroupType.Colleague, pageIndex, pageSize);
            if (!groupResult.Status)
                return MessageView(groupResult.Message);
            ViewBag.TotalCount = groupResult.TotalCount;
            var result = ApplicationAgency;
            ViewData["Stage"] = result.Data.Stage;
            return View(groupResult.Data ?? new List<GroupDto>());
        }
        #endregion

        #region Ajax

        /// <summary> 发布考试 </summary>
        [HttpPost]
        [Route("publish-exam")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult PublishExamination(string examId, int role)
        {
            if (role != (byte)UserRole.Student && role != (byte)UserRole.Teacher)
                return DeyiJson(DResult.Error("发布的角色异常"), true);
            var result = _examinationContract.PublishExamination(examId, UserId, (UserRole)role);
            return DeyiJson(result, true);
        }
        /// <summary> 排名信息 </summary>
        [AjaxOnly]
        [HttpGet]
        [Route("ranks/{id:regex([0-9a-z]{32})}")]
        public ActionResult Ranks(string id)
        {
            var result = _examinationContract.Rankings(id);
            return DeyiJson(result);
        }

        /// <summary> 科目信息 </summary>
        [AjaxOnly]
        [HttpGet]
        [Route("subjects/{id:regex([0-9a-z]{32})}")]
        public ActionResult Subjects(string id)
        {
            var result = _examinationContract.ExamSubjects(id);
            return DeyiJson(result);
        }

        /// <summary> 重点率分析 </summary>
        [HttpGet]
        [Route("class-analysis-keys")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult ClassAnalysisKeys(AnalysisInputDto inputDto)
        {
            var result = _examinationContract.ClassAnalysisKey(inputDto);
            return DeyiJson(result);
        }

        /// <summary> 班级分层分析 </summary>
        [HttpGet]
        [Route("class-analysis-layers")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult ClassAnalysisLayers(ClassAnalysisLayerInputDto inputDto)
        {
            var result = _examinationContract.ClassAnalysisLayer(inputDto);
            return DeyiJson(result);
        }

        /// <summary> 学科分析 </summary>
        [HttpGet]
        [Route("subject-analysis")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult SubjectAnalysis(SubjectAnalysisInputDto inputDto)
        {
            var result = _examinationContract.SubjectAnalysis(inputDto);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpGet]
        [Route("subject-score-rates")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult SubjectScoreRates(string id)
        {
            var result = _examinationContract.SubjectScoreRates(id);
            return DeyiJson(result);
        }

        /// <summary> 下载 - 学生排名信息 </summary>
        [HttpPost]
        [Route("ranking-download")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public void RankingDownload(string examId)
        {
            var rankResult = _examinationContract.Rankings(examId);
            if (!rankResult.Status)
                return;
            //DownloadLogger.LogAsync(UserId, "教务管理-年级排名");
            var log = UserId.CreateLog(DownloadType.GradeRank, CurrentAgency);
            ExaminationExportHelper.Rankings(rankResult.Data, () =>
             {
                 log.Complete();
             });
        }

        /// <summary> 学生 分数段统计 </summary>
        [Route("score-sections")]
        [RoleAuthorize((UserRole.Parents | UserRole.Student), "/")]
        public ActionResult ScoreSections(string id)
        {
            return DeyiJson(_examinationContract.ScoreSections(id, ChildOrUserId));
        }

        /// <summary> 下载 - 班级分析 </summary>
        [HttpPost]
        [Route("class-analysis-download")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public void ClassAnalysisDownload()
        {
            var keys = JsonHelper.Json<AnalysisInputDto>("keys".Form(string.Empty));
            var layer = JsonHelper.Json<ClassAnalysisLayerInputDto>("layer".Form(string.Empty));
            var keysList = _examinationContract.ClassAnalysisKey(keys);
            var layerList = _examinationContract.ClassAnalysisLayer(layer);
            //DownloadLogger.LogAsync(UserId, "教务管理-班级分析");
            var log = UserId.CreateLog(DownloadType.ClassAnalysis, CurrentAgency);
            ExaminationExportHelper.ClassAnalysis("name".Form(string.Empty), keysList, layerList, () =>
             {
                 log.Complete();
             });
        }

        /// <summary> 下载 - 学科分析 </summary>
        [HttpPost]
        [Route("subject-analysis-download")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public void SubjectAnalysisDownload()
        {
            var inputDto = JsonHelper.Json<SubjectAnalysisInputDto>("keys".Form(string.Empty));
            if (inputDto == null || string.IsNullOrWhiteSpace(inputDto.ExamSubjectId))
            {
                Response.Write("考试科目Id不能为空~");
                return;
            }
            var result = _examinationContract.SubjectAnalysis(inputDto);
            //DownloadLogger.LogAsync(UserId, "教务管理-学科分析");
            var log = UserId.CreateLog(DownloadType.SubjectAnalysis, CurrentAgency);
            ExaminationExportHelper.SubjectAnalysis("name".Form(string.Empty), result, () =>
             {
                 log.Complete();
             });
        }

        /// <summary> 下载 - 关联报表信息 </summary>
        [HttpPost]
        [Route("union-download")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public void UnionDownload(string unionBatch)
        {
            var rankResult = _examinationContract.UnionSource(unionBatch);
            if (!rankResult.Status)
                return;
            var log = UserId.CreateLog(DownloadType.UnionReport, CurrentAgency);
            ExaminationExportHelper.UnionRanksExport(rankResult.Data, () =>
            {
                log.Complete();
            });
        }

        /// <summary>
        /// 加载科目列表
        /// </summary>
        /// <returns></returns>
        [AjaxOnly]
        [HttpPost]
        public ActionResult LoadSubjects()
        {
            //DOTO学段未处理 CurrentAgency.Stage
            var subjects = SystemCache.Instance.Subjects();
            return DeyiJson(subjects);
        }
        /// <summary>
        /// 认证圈子
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [RoleAuthorize(UserRole.Teacher, "/")]
        [Route("auth-group")]
        public ActionResult AuthGroup(string Id, bool isAuth)
        {
            var result = _groupContract.GroupCertificate(Id, isAuth);
            if (!result.Status)
                MessageView(result.Message);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpGet]
        [RoleAuthorize(UserRole.Teacher, "/")]
        [Route("union-data")]
        public ActionResult UnionSource(string unionBatch)
        {
            var result = _examinationContract.UnionSource(unionBatch);
            if (!result.Status)
                MessageView(result.Message);
            return DeyiJson(result);
        }

        #endregion

        #region 教务看板
        [AjaxOnly]
        [Route("agency-survey")]
        public ActionResult AgencySurvey(string id, bool refresh = false)
        {
            var dto = _statisticContract.AgencySurvey(id, refresh);
            return DeyiJson(dto);
        }

        [AjaxOnly]
        [Route("agency-portrait")]
        public ActionResult AgencyPortrait(string id, int area = 0, bool refresh = false)
        {
            var dto = _statisticContract.AgencyPortrait(id, (TimeArea)area, refresh);
            return DeyiJson(dto);
        }

        [AjaxOnly]
        [Route("examination-map")]
        public ActionResult ExaminationMap(string id, int area = 0, bool refresh = false)
        {
            var dto = _statisticContract.AgencyExaminationMap(id, (TimeArea)area, refresh);
            return DeyiJson(dto);
        }

        [AjaxOnly]
        [Route("agency-remedy")]
        public ActionResult AgencyRemedy(string id, int area = 0, bool refresh = false)
        {
            var dto = _statisticContract.AgencyRemedy(id, (TimeArea)area, refresh);
            return DeyiJson(dto);
        }
        #endregion

        /// <summary>
        /// 教务管理员创建圈子
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateGroupForManage(CreateGroupDto createDto)
        {

            var agencyData = ApplicationAgency;
            if (!agencyData.Status)
            {
                return DeyiJson(agencyData.Message, true);
            }
            var group = new GroupDto
            {
                Type = createDto.Type,
                Name = createDto.Name,
                Capacity = 200,
                CreationTime = Clock.Now,
                ManagerId = UserId,
                AgencyId = agencyData.Data.Id
            };
            DResult<GroupDto> result;
            switch (createDto.Type)
            {
                case (int)GroupType.Class:
                    var classGroup = @group.MapTo<ClassGroupDto>();
                    classGroup.GradeYear = createDto.GradeYear;
                    classGroup.AgencyId = agencyData.Data.Id;
                    classGroup.Capacity = 200;
                    classGroup.Stage = agencyData.Data.Stage;
                    result = _groupContract.CreateGroup(classGroup, (byte)UserRole.Teacher, null, true);
                    break;
                case (int)GroupType.Colleague:
                    var colleagueGroup = @group.MapTo<ColleagueGroupDto>();
                    colleagueGroup.Stage = agencyData.Data.Stage;
                    colleagueGroup.Capacity = 100;
                    colleagueGroup.AgencyId = agencyData.Data.Id;
                    colleagueGroup.SubjectId = createDto.SubjectId;
                    result = _groupContract.CreateGroup(colleagueGroup, (byte)UserRole.Teacher, null, true);
                    break;
                default:
                    result = DResult.Error<GroupDto>("圈子类型异常！");
                    break;
            }
            return DeyiJson(result);
        }
        /// <summary>
        /// 批量导入学生
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BatchImportStudents(BatchUsersDto data)
        {
            data.users = data.users.Where(w => !string.IsNullOrWhiteSpace(w) && Regex.IsMatch(w, "^[\u4e00-\u9fa5]{2,5}$")).ToArray();
            var agencyData = ApplicationAgency;
            if (!agencyData.Status)
            {
                return DeyiJson(DResult.Error("当前用户无权限"));
            }
            if (data != null && (int)UserRole.Student == data.Role)
            {
                var users = data.users.Distinct().ToArray();
                var result = _groupContract.BatchImportStudent(users, data.GroupId, agencyData.Data.Id, agencyData.Data.Stage);
                return DeyiJson(result);
            }
            return DeyiJson(DResult.Error("数据格式错误"));
        }
        /// <summary>
        /// Excel批量导入学生
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExcelBatchImportStudents(BatchUsersDto data)
        {
            HttpPostedFileBase filexls = Request.Files["fileExcel"];
            if (filexls.ContentLength < 0)
            {
                return DeyiJson(DResult.Error("请上传office2003格式的excel文件"));
            }
            string filedName = "学生名字";
            string str = string.Empty;
            // var flag = Base64ToString(data.Path, out str);
            //if (!flag)
            //    return DeyiJson(DResult.Error(str));
            var agencyData = ApplicationAgency;
            if (!agencyData.Status)
            {
                return DeyiJson(DResult.Error("当前用户无权限"));
            }
            List<string> stuNames = GetStudentNamesByDatatable(filexls.InputStream, filedName);
            if (!stuNames.Any())
                return DeyiJson(DResult.Error(string.Format("{0},{1},{2}", "excel文件不包含", filedName, "的列名")));
            stuNames = stuNames.Where(w => !w.Contains(filedName) && Regex.IsMatch(w, "^[\u4e00-\u9fa5]{2,5}$")).ToList();
            data.users = stuNames.Distinct().ToArray();
            if ((int)UserRole.Student == data.Role)
            {
                var result = _groupContract.BatchImportStudent(data.users, data.GroupId, agencyData.Data.Id, agencyData.Data.Stage);
                return DeyiJson(result);
            }
            return DeyiJson(DResult.Error("数据格式错误"));
        }
        /// <summary>
        /// 解析Excel数据并返回学生名字
        /// </summary>
        /// <param name="path">excel路径只支持office2003</param>
        /// <param name="fiedName">学生名字列名称</param>
        /// <returns></returns>
        private List<string> GetStudentNamesByDatatable(Stream bytes, string fiedName = "学生名字")
        {
            //找到当前table学生名字所在列
            Func<DataRowCollection, DataColumnCollection, string, DataColumn> funGetDicName = (rows, columns, cname) =>
            {
                DataColumn dc = new DataColumn();
                string coluName = string.Empty;
                for (int i = 0; i < rows.Count; i++)
                {
                    for (int j = 0; j < columns.Count; j++)
                    {
                        coluName = rows[i][columns[j]].ToString();
                        if (coluName.Equals(cname))
                            dc = columns[j];
                    }
                }
                return dc;
            };
            List<string> nameList = new List<string>();
            DataSet ds = ExcelHelper.Read(bytes);
            if (ds == null)
                return nameList;
            var tableCount = ds.Tables.Count;
            for (int i = 0; i < tableCount; i++)
            {
                var rows = ds.Tables[i].Rows;
                var dcs = ds.Tables[i].Columns;
                DataColumn column = funGetDicName(rows, dcs, fiedName);
                if (column == null)
                    continue;
                foreach (DataRow dr in rows)
                {
                    var stuName = dr[column];
                    if (stuName.GetType() == typeof(string))
                    {
                        if (!string.IsNullOrEmpty(stuName.ToString()))
                            nameList.Add(dr[column].ToString());
                    };
                }
            }
            return nameList;
        }
        /// <summary>
        /// 解码Base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool Base64ToString(string str, out string Tvalue)
        {
            string value = string.Empty;
            if (string.IsNullOrEmpty(str))
            {
                Tvalue = string.Empty;
                return false;
            }
            try
            {
                byte[] bytes = Convert.FromBase64String(str);
                value = Encoding.UTF8.GetString(bytes);
                Tvalue = value;
                return true;
            }
            catch (Exception ex)
            {
                Tvalue = ex.Message;
                return false;
            }
        }
        /// <summary>
        /// 班级圈导入教师
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BatchTeachers(long[] ids, string groupId)
        {
            if (ids == null || ids.Length == 0 || string.IsNullOrEmpty(groupId))
            {
                return DeyiJson(DResult.Error("教师或者圈子不存在"));
            }
            var result = _groupContract.BatchImportTeacher(ids, groupId);
            if (!result.Status)
            {
                return DeyiJson(DResult.Error("圈子不存在"));
            }
            return DeyiJson(result);
        }
        /// <summary>
        /// 同事圈导入教师
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ColleagueBatchTeacher(long[] ids, string groupId)
        {
            if (ids == null || ids.Length == 0 || string.IsNullOrEmpty(groupId))
            {
                return DeyiJson(DResult.Error("教师或者圈子不存在"));
            }
            var result = _groupContract.ColleagueBatchTeacher(ids, groupId);
            return DeyiJson(result);
        }

        /// <summary>
        /// 批量创建圈子
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BatchCreateGroups(BatchCreateGroupsDto dtos)
        {
            if (dtos == null)
                return MessageView("没有选择任何圈子");
            var agency = ApplicationAgency;
            if (!agency.Status)
            {
                return DeyiJson(DResult.Error(agency.Message), true);
            }
            var id = agency.Data.Id;
            var stage = agency.Data.Stage;
            //var stage = 

            dtos.ClassGroups.ForEach(dto =>
            {
                dto.Stage = stage;
            });
            dtos.ColleagueGroups.ForEach(dto =>
            {
                dto.Stage = stage;
            });
            var result = _groupContract.BatchCreateGroups(dtos, id);
            return DeyiJson(result);
        }
        /// <summary>
        /// 加载当前机构老师
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadUsersByAgencyId(string groupId, int subjectId = -1, int groupType = 1)
        {
            var data = ApplicationAgency;
            if (!data.Status)
            {
                return DeyiJson(data.Status, true);
            }
            var result = UserContract.LoadUsersByAgencyId(data.Data.Id, (int)UserRole.Teacher, subjectId);
            if (result.Status)
            {
                var members = _groupContract.GroupMembers(groupId, UserRole.Teacher).Data;
                var memberIDs = members.Select(w => w.Id);
                var memberSubjectIds = members.Select(w => w.SubjectId);
                var userData = result.Data.Where(w => !memberIDs.Contains(w.Id));
                if (groupType == (int)GroupType.Class)
                {
                    userData = userData.Where(w => !memberSubjectIds.Contains(w.SubjectId));
                }
                return DeyiJson(userData);
            }

            return DeyiJson(DResult.Error("没有找到任何老师"));
        }
        /// <summary>
        /// 获取重复的圈子信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadRepeatMsg(BatchCreateGroupsDto dto)
        {
            var agency = ApplicationAgency;
            if (!agency.Status)
            {
                return DeyiJson(DResult.Error(agency.Message), true);
            }
            if (dto.ClassGroups.Any())
            {
                dto.ClassGroups.ForEach(d =>
                {
                    d.Stage = agency.Data.Stage;
                });
            }
            if (dto.ColleagueGroups.Any())
            {
                dto.ColleagueGroups.ForEach(d =>
                {
                    d.Stage = agency.Data.Stage;
                });
            }
            var result = _groupContract.GetGroupRepeatMsg(dto, agency.Data.Id);
            return DeyiJson(result);
        }
        /// <summary>
        /// 删除任教教师
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("teacher-delAgency")]
        public ActionResult DelTeacherAgency(long userID)
        {
            if (userID == 0)
            {
                return DeyiJson(DResult.Error("当前用户不存在"));
            }
            var result = UserContract.UpdateTeacherAgency(userID);
            if (result.Status)
            {
                return DeyiJson(result);
            }
            return DeyiJson(DResult.Error("当前用户不存在"));
        }
    }
}