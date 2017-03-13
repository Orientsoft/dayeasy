using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Statistic.Services.Model;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using PointDto = DayEasy.Contracts.Dtos.Statistic.PointDto;
using PointInfoDto = DayEasy.Contracts.Dtos.Statistic.PointInfoDto;
using SeriesDto = DayEasy.Contracts.Dtos.Statistic.SeriesDto;

namespace DayEasy.Statistic.Services
{
    /// <summary> 统计服务 </summary>
    public partial class StatisticService : DayEasyService, IStatisticContract
    {
        #region 注入

        private readonly ILogger _logger = LogManager.Logger<StatisticService>();
        public StatisticService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        public ISystemContract SystemContract { private get; set; }
        public IGroupContract GroupContract { get; set; }
        public IPaperContract PaperContract { get; set; }
        public IDayEasyRepository<TS_StudentStatistic, long> StudentStatisticRepository { private get; set; }
        public IDayEasyRepository<TS_TeacherStatistic, long> TeacherStatisticRepository { private get; set; }
        public IDayEasyRepository<TS_TeacherKpStatistic, string> TeacherKpStatisticRepository { private get; set; }
        public IDayEasyRepository<TS_StudentKpStatistic, string> StudentKpStatisticRepository { private get; set; }
        public IDayEasyRepository<TS_ClassScoreStatistics, string> ClassScoreStatisticsRepository { private get; set; }
        public IDayEasyRepository<TS_StuScoreStatistics, string> StuScoreStatisticsRepository { private get; set; }

        #endregion

        #region Private Method

        #region 获取第几周的星期一
        /// <summary>
        /// 获取第几周的星期一
        /// </summary>
        /// <param name="weekNum"></param>
        /// <returns></returns>
        private DateTime GetMondayDate(int weekNum = 0)
        {
            var nowDate = Clock.Now.Date;
            var mondayDate = nowDate.AddDays(1 - (int)nowDate.DayOfWeek);
            if (weekNum != 0)
            {
                mondayDate = mondayDate.AddDays(weekNum * 7);
            }
            return mondayDate;
        }
        #endregion

        #region 组装知识点层级统计
        /// <summary>
        /// 组装知识点层级统计
        /// </summary>
        private List<KpDataDto> MakeKpStatistic(List<Kps> kps)
        {
            if (kps == null) return null;

            //当前知识点
            var currentCodes = kps.Where(u => !string.IsNullOrEmpty(u.KpLayerCode) && u.KpLayerCode.Length >= 5).Select(u => u.KpLayerCode).Distinct().ToList();
            if (currentCodes.Count <= 0) return null;

            var resultList = FindParentsKps(currentCodes);
            if (resultList == null || resultList.Count <= 0) return null;

            var topList = resultList.Where(u => u.PID == 0).DistinctBy(u => u.Id).ToList();
            if (topList.Count <= 0) return null;

            var result = new List<KpDataDto>();
            foreach (var top in topList)
            {
                var kp = new KpDataDto();

                var kpModels = kps.Where(u => u.KpLayerCode.StartsWith(top.Code)).ToList();
                if (kpModels.Count <= 0) continue;

                kp.AnswerCount = kpModels.Sum(u => u.AnswerCount);
                kp.ErrorCount = kpModels.Sum(u => u.ErrorCount);

                kp.KpId = top.Id;
                kp.KpName = top.Name;
                kp.SonKps = MakeStatistics(kps, resultList, top.Id);

                result.Add(kp);
            }

            return result;
        }
        #endregion

        #region 递归组装子层级实体
        /// <summary>
        /// 递归组装子层级实体
        /// </summary>
        /// <param name="kps"></param>
        /// <param name="resultList"></param>
        /// <param name="topId"></param>
        /// <returns></returns>
        private List<KpDataDto> MakeStatistics(List<Kps> kps, List<TS_Knowledge> resultList, int topId)
        {
            if (kps.Count <= 0 || resultList.Count <= 0 || topId <= 0) return null;

            var sonList = resultList.Where(u => u.PID == topId).DistinctBy(u => u.Id).ToList();
            if (sonList.Count <= 0) return null;

            var temp = new List<KpDataDto>();

            foreach (var knowledge in sonList)
            {
                var kp = new KpDataDto();

                var kpModels = kps.Where(u => u.KpLayerCode.StartsWith(knowledge.Code)).ToList();
                if (kpModels.Count <= 0) continue;

                kp.AnswerCount = kpModels.Sum(u => u.AnswerCount);
                kp.ErrorCount = kpModels.Sum(u => u.ErrorCount);
                kp.KpId = knowledge.Id;
                kp.KpName = knowledge.Name;
                kp.SonKps = MakeStatistics(kps, resultList, knowledge.Id);

                temp.Add(kp);
            }

            return temp;
        }
        #endregion

        #region 根据知识点Id找到所有的父级知识点

        /// <summary>
        /// 根据知识点Id找到所有的父级知识点
        /// </summary>
        /// <param name="kpCodes"></param>
        /// <returns></returns>
        private List<TS_Knowledge> FindParentsKps(List<string> kpCodes)
        {
            if (kpCodes.Count <= 0) return null;

            //第一级code
            var firstLevelCodes = kpCodes.Select(u => u.Substring(0, 5)).Distinct().ToList();
            if (firstLevelCodes.Count < 1) return null;

            var first = firstLevelCodes.First();
            Expression<Func<TS_Knowledge, bool>> condition = u => u.Code.StartsWith(first);

            firstLevelCodes.RemoveAt(0);
            firstLevelCodes.ForEach(c =>
            {
                condition = condition.Or(u => u.Code.StartsWith(c));
            });

            var kps = CurrentIocManager.Resolve<IDayEasyRepository<TS_Knowledge, int>>().Where(condition).ToList();
            if (kps.Count < 1) return null;

            var result = new List<TS_Knowledge>();
            kpCodes.ForEach(k =>
            {
                var code = k;
                while (code.Length >= 5)
                {
                    if (result.Exists(u => u.Code == code)) return;

                    var kpModel = kps.SingleOrDefault(u => u.Code == code);
                    if (kpModel == null) return;

                    result.Add(kpModel);

                    code = code.Remove(code.Length - 1 - 1, 2);
                }
            });

            return result;
        }

        #endregion

        #region 获取一段时间内 top10 错题

        /// <summary>
        /// 获取一段时间内top10错题
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="subject"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns>
        /// 问题ID，错误次数
        /// </returns>
        private Dictionary<string, int> GetTop10ErrorQu(string groupId, int subject, DateTime startTime, DateTime endTime)
        {
            //学生IDs
            var students = GroupContract.GroupMembers(groupId, UserRole.Student);
            if (students == null || students.Data == null)
                return new Dictionary<string, int>();

            //错误问题列表
            var studentIds = students.Data.Select(u => u.Id).ToList();
            var errorDict = CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>().Where(u =>
                        u.Status != (byte)ErrorQuestionStatus.Delete && u.SubjectID == subject && u.AddedAt >= startTime &&
                        u.AddedAt < endTime && studentIds.Contains(u.StudentID))
                    .GroupBy(u => u.QuestionID)
                    .Select(t => new { t.Key, count = t.Select(e => e.StudentID).Distinct().Count() })
                    .OrderByDescending(u => u.count)
                    .Take(10)
                    .ToDictionary(k => k.Key, v => v.count);

            return errorDict;
        }

        #endregion

        #region 获取最近的错题的时间
        /// <summary>
        /// 获取最近的错题的时间
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        private DateTime GetLastErrorQu(string groupId, int subject)
        {
            //学生IDs
            var students = GroupContract.GroupMembers(groupId, UserRole.Student);
            if (students == null || students.Data == null)
                return Clock.Now;

            //错误问题列表
            var studentIds = students.Data.Select(u => u.Id).ToList();
            var errorDict = CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>().Where(u => u.Status != (byte)ErrorQuestionStatus.Delete && u.SubjectID == subject && studentIds.Contains(u.StudentID)).OrderByDescending(u => u.AddedAt).Take(1).ToList();

            if (!errorDict.Any()) return Clock.Now;

            var tpErrorQuestion = errorDict.FirstOrDefault();
            return tpErrorQuestion == null ? Clock.Now : tpErrorQuestion.AddedAt;
        }
        #endregion

        #region Convert2UTC
        /// <summary>
        /// Convert2UTC
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private double Convert2UTC(System.DateTime time)
        {
            double intResult = 0;
            var ticks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            intResult = new TimeSpan(time.Ticks).Subtract(ticks).Duration().TotalMilliseconds;
            return intResult;
        }
        #endregion

        #region 组装 PointInfo
        /// <summary>
        /// 组装 PointInfo
        /// </summary>
        /// <returns></returns>
        private PointInfoDto MakePointInfo(TS_ClassScoreStatistics classScore, PaperDto paperModel)
        {
            if (classScore == null)
            {
                if (paperModel != null)
                {
                    return new PointInfoDto()
                    {
                        PName = paperModel.PaperTitle,
                        PType = paperModel.PaperType
                    };
                }
                return new PointInfoDto()
                {
                    PName = string.Empty
                };
            }

            var pointInfo = new PointInfoDto()
            {
                Batch = classScore.Batch,
                PId = classScore.PaperId,
                Average = classScore.AverageScore,
                HScore = classScore.TheHighestScore,
                LScore = classScore.TheLowestScore,
                PName = paperModel == null ? string.Empty : paperModel.PaperTitle,
                PType = paperModel == null ? (byte)PaperType.Normal : paperModel.PaperType,
                AAverage = 0,
                BAverage = 0,
                SGroup = new List<ScoreGroupsDto>()
            };

            if (!string.IsNullOrEmpty(classScore.SectionScores))
            {
                var sectionScores = JsonHelper.Json<ReportSectionScoresDto>(classScore.SectionScores);
                if (sectionScores != null)
                {
                    pointInfo.AAverage = sectionScores.AAv;
                    pointInfo.BAverage = sectionScores.BAv;
                }
            }

            var scoreGroups = JsonHelper.JsonList<ScoreGroupsDto>(classScore.ScoreGroups);
            if (scoreGroups == null) return pointInfo;

            var paperScores = paperModel == null ? null : paperModel.PaperScores;
            var pScore = paperScores == null ? 0 : paperScores.TScore;
            var length = (int)((double)pScore * 0.6 / 10);

            //未及格的数量
            var scoreGroupsDtos = scoreGroups.ToList();
            var lowLevelCount = scoreGroupsDtos.GetRange(0, length).Sum(s => s.Count);

            scoreGroupsDtos.RemoveRange(0, length);//移除未及格的
            scoreGroupsDtos.Reverse();//反转

            scoreGroupsDtos.Add(new ScoreGroupsDto()
            {
                ScoreInfo = length * 10 + " 以下",
                Count = lowLevelCount
            });

            pointInfo.SGroup = scoreGroupsDtos;

            return pointInfo;
        }
        #endregion

        #region 获取学生的成绩统计
        /// <summary>
        /// 获取学生的成绩统计
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="subjectId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        IEnumerable<StudentSeriesDto> GetStudentSeries(DateTime startDate, DateTime endDate, int subjectId, long studentId)
        {
            var studentScores = StuScoreStatisticsRepository.Where(u => u.AddedAt >= startDate && u.AddedAt <= endDate && u.StudentId == studentId && u.Status == (byte)StuStatisticsStatus.Normal && u.SubjectId == subjectId).ToList();

            if (studentScores.Any())
            {
                //查询班级统计
                var batchs = studentScores.Select(u => u.Batch).Distinct().ToList();
                var classStatisticsList = ClassScoreStatisticsRepository.Where(u => batchs.Contains(u.Batch)).ToList();

                //查找试卷
                var paperIds = studentScores.Select(u => u.PaperId).Distinct().ToList();
                var paperList = PaperContract.PaperList(paperIds);

                var studentPoints = new List<StudentPointDto>();
                studentScores = studentScores.OrderBy(u => u.AddedAt).ToList();
                studentScores.ForEach(s =>
                {
                    var classStatistics = classStatisticsList.SingleOrDefault(u => u.Batch == s.Batch);

                    var paperModel = paperList.Data == null ? null : paperList.Data.SingleOrDefault(p => p.Id == s.PaperId);
                    var sPoint = new StudentPointDto
                    {
                        x = Convert2UTC(s.AddedAt),
                        y = s.CurrentSort,
                        Score = s.CurrentScore,
                        AScore = s.SectionAScore,
                        BScore = s.SectionBScore,
                        PointInfo = MakePointInfo(classStatistics, paperModel)
                    };
                    sPoint.PointInfo.Batch = s.Batch;
                    sPoint.PointInfo.PId = s.PaperId;
                    sPoint.PointInfo.IsSelf = true;

                    studentPoints.Add(sPoint);
                });

                var subject = SystemContract.SubjectDict(new List<int> { subjectId });

                var result = new List<StudentSeriesDto>();
                var obj = new StudentSeriesDto()
                {
                    name = subject == null ? string.Empty : subject.FirstOrDefault().Value,
                    data = studentPoints
                };
                result.Add(obj);

                return result;
            }

            return new List<StudentSeriesDto>();
        }

        #endregion

        #endregion

        #region 更新学生统计
        /// <summary>
        /// 更新学生统计
        /// </summary>
        /// <param name="statistic"></param>
        /// <returns></returns>
        public DResult UpdateStatistic(StudentStatisticDto statistic)
        {
            if (statistic == null || statistic.UserID < 1)
                return DResult.Error("参数错误！");
            var statisticModel = statistic.MapTo<TS_StudentStatistic>();
            if (statisticModel == null)
                return DResult.Error("更新失败！");

            var model = StudentStatisticRepository.SingleOrDefault(u => u.Id == statisticModel.Id);
            if (model == null)//第一次更新，为新增操作
            {
                var result = StudentStatisticRepository.Insert(statisticModel);
                if (result > 0)
                    return DResult.Success;
            }
            else//非第一次，为更新操作
            {
                Type t = typeof(TS_StudentStatistic);
                var properties = t.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    if (propertyInfo.Name.ToLower() == "id")
                        continue;
                    var value = propertyInfo.GetValue(statisticModel);
                    var intValue = Convert.ToInt32(value);

                    if (intValue == 0)
                        continue;
                    var currentValue = propertyInfo.GetValue(model);
                    var intCurrentValue = Convert.ToInt32(currentValue);
                    if (intCurrentValue + intValue < 0)
                        continue;
                    propertyInfo.SetValue(model, intCurrentValue + intValue);
                }

                var updateResult = StudentStatisticRepository.Update(model);
                if (updateResult > 0)
                    return DResult.Success;
            }

            return DResult.Error("更新失败！");
        }
        #endregion

        #region 更新老师统计
        /// <summary>
        /// 更新老师统计
        /// </summary>
        /// <param name="statistic"></param>
        /// <returns></returns>
        public DResult UpdateStatistic(TeacherStatisticDto statistic)
        {
            if (statistic == null || statistic.UserID < 1)
                return DResult.Error("参数错误！");
            var statisticModel = statistic.MapTo<TS_TeacherStatistic>();
            if (statisticModel == null)
                return DResult.Error("更新失败！");

            var model = TeacherStatisticRepository.SingleOrDefault(u => u.Id == statisticModel.Id);
            if (model == null)//第一次更新，为新增操作
            {
                var result = TeacherStatisticRepository.Insert(statisticModel);
                if (result > 0)
                    return DResult.Success;
            }
            else//非第一次，为更新操作
            {
                Type t = typeof(TS_TeacherStatistic);
                var properties = t.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    if (propertyInfo.Name.ToLower() == "id")
                        continue;
                    var value = propertyInfo.GetValue(statisticModel);
                    var intValue = Convert.ToInt32(value);
                    if (intValue != 0)
                    {
                        var currentValue = propertyInfo.GetValue(model);
                        var intCurrentValue = Convert.ToInt32(currentValue);
                        if (intValue + intCurrentValue < 0)
                            continue;
                        propertyInfo.SetValue(model, intCurrentValue + intValue);
                    }
                }

                var updateResult = TeacherStatisticRepository.Update(model);
                if (updateResult > 0)
                    return DResult.Success;
            }

            return DResult.Error("更新失败！");
        }
        #endregion

        #region 更新老师知识点统计
        /// <summary>
        /// 更新老师知识点统计
        /// </summary>
        /// <param name="kpStatistics"></param>
        /// <returns></returns>
        public void UpdateKpStatistic(IEnumerable<TeacherKpStatisticDto> kpStatistics)
        {
            if (kpStatistics == null) return;

            var addList = new List<TS_TeacherKpStatistic>();
            foreach (var kpStatistic in kpStatistics)
            {
                //本周星期一
                var mondayDate = Clock.Now.AddDays(1 - (int)Clock.Now.DayOfWeek).Date;
                var sundayDate = mondayDate.AddDays(7);//本周星期日

                var statistic = kpStatistic;
                var kpModel = TeacherKpStatisticRepository.SingleOrDefault(u => u.KpLayerCode == statistic.KpLayerCode && u.StartTime >= mondayDate && u.EndTime < sundayDate && u.ClassID == statistic.GroupId);

                if (kpModel != null)//这周已经有了
                {
                    kpModel.AnswerCount += kpStatistic.AnswerCount;
                    kpModel.ErrorCount += kpStatistic.ErrorCount;

                    TeacherKpStatisticRepository.Update(k => new {k.AnswerCount, k.ErrorCount}, kpModel);
                }
                else//这周还没有
                {
                    //var model = addList.SingleOrDefault(u => u.KpLayerCode == kpStatistic.KpLayerCode);
                    //if (model != null)
                    //{
                    //    var index = addList.IndexOf(model);

                    //    addList[index].ErrorCount += kpStatistic.ErrorCount;
                    //    addList[index].AnswerCount += kpStatistic.AnswerCount;
                    //}
                    //else
                    //{
                    var tempStatistic = kpStatistic.MapTo<TS_TeacherKpStatistic>();
                    if (tempStatistic != null)
                    {
                        addList.Add(tempStatistic);
                    }
                    //}
                }
            }

            //执行新增
            if (addList.Count > 0)
            {
                TeacherKpStatisticRepository.Insert(addList);
            }
        }
        #endregion

        #region 更新学生知识点统计
        /// <summary>
        /// 更新学生知识点统计
        /// </summary>
        /// <param name="kpStatistics"></param>
        /// <returns></returns>
        public void UpdateKpStatistic(IEnumerable<StudentKpStatisticDto> kpStatistics)
        {
            if (kpStatistics == null) return;

            var addList = new List<TS_StudentKpStatistic>();
            foreach (var kpStatistic in kpStatistics)
            {
                //本周星期一
                var mondayDate = Clock.Now.AddDays(1 - (int)Clock.Now.DayOfWeek).Date;
                var sundayDate = mondayDate.AddDays(7);//本周星期日

                var statistic = kpStatistic;
                var kpModel = StudentKpStatisticRepository.SingleOrDefault(u => u.KpLayerCode == statistic.KpLayerCode && u.StartTime >= mondayDate && u.EndTime < sundayDate && u.StudentID == statistic.StudentID);

                if (kpModel != null)//这周已经有了
                {
                    kpModel.AnswerCount += kpStatistic.AnswerCount;
                    kpModel.ErrorCount += kpStatistic.ErrorCount;

                    StudentKpStatisticRepository.Update(k => new {k.AnswerCount, k.ErrorCount}, kpModel);
                }
                else//这周还没有
                {
                    //var model = addList.SingleOrDefault(u => u.KpLayerCode == kpStatistic.KpLayerCode && u.StudentID == kpStatistic.StudentID);
                    //if (model != null)
                    //{
                    //    var index = addList.IndexOf(model);

                    //    addList[index].ErrorCount += kpStatistic.ErrorCount;
                    //    addList[index].AnswerCount += kpStatistic.AnswerCount;
                    //}
                    //else
                    //{
                    var tempStatistic = kpStatistic.MapTo<TS_StudentKpStatistic>();
                    if (tempStatistic != null)
                    {
                        addList.Add(tempStatistic);
                    }
                    //}
                }
            }

            //执行新增
            if (addList.Count > 0)
            {
                StudentKpStatisticRepository.Insert(addList);
            }
        }
        #endregion

        #region 获取知识点统计数据
        /// <summary>
        /// 获取知识点统计数据
        /// </summary>
        /// <returns></returns>
        public DResult<KpStatisticDataDto> GetKpStatistic(SearchKpStatisticDataDto searchDto)
        {
            #region 处理时间

            DateTime startTime = Clock.Now;
            if (string.IsNullOrEmpty(searchDto.StartTimeStr))
            {
                if (searchDto.Role == UserRole.Teacher) //老师
                {
                    var teacherLastKp = TeacherKpStatisticRepository.Where(u => u.ClassID == searchDto.GroupId && u.SubjectID == searchDto.SubjectId).OrderByDescending(u => u.StartTime).Take(1).ToList();

                    var tsTeacherKpStatistic = teacherLastKp.FirstOrDefault();
                    if (tsTeacherKpStatistic != null)
                        startTime = tsTeacherKpStatistic.StartTime;
                }
                else if (searchDto.Role == UserRole.Student)//学生
                {
                    var studentLastKp = StudentKpStatisticRepository.Where(u => u.StudentID == searchDto.UserId && u.SubjectID == searchDto.SubjectId).OrderByDescending(u => u.StartTime).Take(1).ToList();

                    var tsStudentKpStatistic = studentLastKp.FirstOrDefault();
                    if (tsStudentKpStatistic != null)
                        startTime = tsStudentKpStatistic.StartTime;
                }
            }

            //计算日期
            var week = (Clock.Now - startTime).Days / 7;

            startTime = GetMondayDate(-week);
            DateTime endTime = startTime.AddDays(6);//本周星期日

            var outStartTimeStr = startTime.ToString("yyyy-MM-dd");
            var outEndTimeStr = endTime.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(searchDto.StartTimeStr))
            {
                if (DateTime.TryParse(searchDto.StartTimeStr, out startTime))
                {
                    outStartTimeStr = startTime.ToString("yyyy-MM-dd");
                    startTime = startTime.AddDays(1 - (int)startTime.DayOfWeek).Date;
                }
            }

            if (!string.IsNullOrEmpty(searchDto.StartTimeStr) && !string.IsNullOrEmpty(searchDto.EndTimeStr))
            {
                if (DateTime.TryParse(searchDto.EndTimeStr, out endTime))
                {
                    outEndTimeStr = endTime.ToString("yyyy-MM-dd");
                }
            }
            endTime = endTime.AddDays(1);

            #endregion

            List<KpDataDto> resultList = null;//结果集

            if (endTime > searchDto.RegistTime)
            {
                if (searchDto.Role == UserRole.Teacher)//老师
                {
                    #region 老师部分
                    //查询老师的知识点统计数据
                    var teacherKps = TeacherKpStatisticRepository.Where(u => u.StartTime >= startTime && u.StartTime < endTime && u.ClassID == searchDto.GroupId && u.SubjectID == searchDto.SubjectId).ToList();

                    if (teacherKps.Count > 0)
                    {
                        var kps = teacherKps.Select(u => new Kps()
                        {
                            AnswerCount = u.AnswerCount,
                            ErrorCount = u.ErrorCount,
                            KpID = u.KpID,
                            KpLayerCode = u.KpLayerCode
                        }).ToList();

                        resultList = MakeKpStatistic(kps);
                    }
                    #endregion
                }
                else if (searchDto.Role == UserRole.Student)//学生
                {
                    #region 学生部分
                    //查询学生的知识点统计数据
                    var studentKps = StudentKpStatisticRepository.Where(u => u.StartTime >= startTime && u.StartTime < endTime && u.StudentID == searchDto.UserId && u.SubjectID == searchDto.SubjectId).ToList();

                    if (studentKps.Count > 0)
                    {
                        var kps = studentKps.Select(u => new Kps()
                        {
                            AnswerCount = u.AnswerCount,
                            ErrorCount = u.ErrorCount,
                            KpID = u.KpID,
                            KpLayerCode = u.KpLayerCode
                        }).ToList();

                        resultList = MakeKpStatistic(kps);
                    }
                    #endregion
                }
            }

            var result = new KpStatisticDataDto()
            {
                OutEndTimeStr = outEndTimeStr,
                OutStartTimeStr = outStartTimeStr,
                KpData = resultList
            };

            return DResult.Succ(result);
        }
        #endregion

        #region 错题top 10
        /// <summary>
        /// 错题top 10
        /// </summary>
        /// <returns></returns>
        public DResult<ErrorTopTenDataDto> ErrorTopTen(SearchErrorTopTenDto searchDto)
        {
            var startTime = GetMondayDate(searchDto.WeekNum);//星期一的日期
            var endTime = startTime.AddDays(7);

            if (string.IsNullOrEmpty(searchDto.GroupId))//班级ID
            {
                var groups = GroupContract.Groups(searchDto.UserId);
                if (groups == null || groups.Data == null)
                    return DResult.Succ<ErrorTopTenDataDto>(null);

                var firstOrDefault = groups.Data.FirstOrDefault();
                if (firstOrDefault == null)
                    return DResult.Succ<ErrorTopTenDataDto>(null);

                searchDto.GroupId = firstOrDefault.Id;
            }

            if (string.IsNullOrEmpty(searchDto.GroupId) || endTime < searchDto.RegisTime)
                return DResult.Succ<ErrorTopTenDataDto>(null);

            if (searchDto.WeekNum > 0)
            {
                //查找最近的错题的时间
                var dateTime = GetLastErrorQu(searchDto.GroupId, searchDto.SubjectId);

                var week = (Clock.Now - dateTime).Days / 7;

                if (week == 0)
                {
                    searchDto.WeekNum = 0;

                    startTime = GetMondayDate(searchDto.WeekNum);//星期一的日期
                    endTime = startTime.AddDays(7);
                }
                else
                {
                    searchDto.WeekNum = -(week + 1);

                    return DResult.Error<ErrorTopTenDataDto>(searchDto.WeekNum.ToString(CultureInfo.InvariantCulture));

                    //startTime = GetMondayDate(searchDto.WeekNum);//星期一的日期
                    //endTime = startTime.AddDays(7);
                }
            }

            var result = new ErrorTopTenDataDto()
            {
                Questions = null,
                IsClickPrevWeek = startTime > searchDto.RegisTime,
                StartTimeStr = startTime.ToString("yyyy.MM.dd"),
                EndTimeStr = endTime.AddDays(-1).ToString("yyyy.MM.dd")
            };

            //查询错题top10
            var errorQus = GetTop10ErrorQu(searchDto.GroupId, searchDto.SubjectId, startTime, endTime);
            if (errorQus.Count > 0)
            {
                var questionIds = errorQus.Keys.ToList();
                if (questionIds.Count > 0)
                {
                    result.Questions = PaperContract.LoadQuestions(questionIds.ToArray());
                    result.ErrorAndCount = errorQus;
                }
            }

            return DResult.Succ(result);
        }
        #endregion

        #region 获取班级综合成绩统计数据
        /// <summary>
        /// 获取班级综合成绩统计数据
        /// </summary>
        /// <returns></returns>
        public DResults<SeriesDto> GetClassScores(SearchClassScoresDto searchDto)
        {
            DateTime endDate;
            if (!DateTime.TryParse(searchDto.EndDateStr, out endDate))
            {
                endDate = Clock.Now.Date;
            }
            endDate = endDate.AddDays(1);

            DateTime startDate;
            if (!DateTime.TryParse(searchDto.StartDateStr, out startDate))
            {
                startDate = endDate.AddMonths(-3);
            }

            var schoolClasses = new List<string>();
            var myGroupIds = new List<string>();

            //查找我的圈子
            var myGroups = GroupContract.Groups(searchDto.UserId, (int)GroupType.Class, containsAll: true);
            if (myGroups.Status && myGroups.Data != null)
            {
                myGroupIds = myGroups.Data.Select(g => g.Id).ToList();

                var gradeYear = -1;
                var stage = -1;

                if (!string.IsNullOrEmpty(searchDto.GradeYear))
                {
                    var strArr = searchDto.GradeYear.Split('_');
                    if (strArr.Length == 2)
                    {
                        stage = ConvertHelper.StrToInt(strArr[0], -1);
                        gradeYear = ConvertHelper.StrToInt(strArr[1], -1);
                    }

                    if (stage > 0 && gradeYear > 0)
                    {
                        gradeYear -= 3;
                        if (stage == (byte)StageEnum.PrimarySchool)
                            gradeYear -= 3;
                    }
                }

                //毕业年级
                if (gradeYear < 0 || stage < 0 || !Enum.IsDefined(typeof(StageEnum), (byte)stage))
                {
                    var group = myGroups.Data.FirstOrDefault();
                    if (group != null)
                    {
                        var classGroup = group as ClassGroupDto;
                        if (classGroup != null)
                        {
                            gradeYear = classGroup.GradeYear;
                            stage = classGroup.Stage;
                        }
                    }
                }

                myGroups.Data.Foreach(g =>
                {
                    var classGroup = g as ClassGroupDto;
                    if (classGroup != null && classGroup.GradeYear == gradeYear && classGroup.Stage == stage)
                    {
                        schoolClasses.Add(classGroup.Id);
                    }
                });
            }

            //TODO:YBG 查找全校同年级班级

            var classScoreList = ClassScoreStatisticsRepository.Where(u => u.Status == (byte)ClassStatisticsStatus.Normal && schoolClasses.Contains(u.ClassId) && u.SubjectId == searchDto.SubjectId && u.AddedAt >= startDate && u.AddedAt < endDate).ToList();

            if (!classScoreList.Any()) return DResult.Succ<SeriesDto>(null, 0);

            //查找试卷
            var paperIds = classScoreList.Select(u => u.PaperId).Distinct().ToList();
            var paperList = PaperContract.PaperList(paperIds);

            var groupList = classScoreList.GroupBy(u => u.ClassId).ToList();

            //查找班级
            var groupIds = groupList.Select(u => u.Key).Distinct().ToList();
            var groups = GroupContract.SearchGroups(groupIds);

            var seriesList = new List<SeriesDto>();
            groupList.ForEach(u =>
            {
                var groupModel = groups.Data == null ? null : groups.Data.SingleOrDefault(c => c.Id == u.Key);

                var series = new SeriesDto
                {
                    name = groupModel == null ? string.Empty : groupModel.Name,
                    data = new List<PointDto>()
                };

                var dataList = u.OrderBy(o => o.AddedAt).ToList();
                foreach (var classScore in dataList)
                {
                    var paperModel = paperList.Data == null ? null : paperList.Data.SingleOrDefault(p => p.Id == classScore.PaperId);

                    var point = new PointDto()
                    {
                        x = Convert2UTC(classScore.AddedAt),
                        y = classScore.AverageScore,
                        PointInfo = MakePointInfo(classScore, paperModel)
                    };
                    point.PointInfo.IsSelf = myGroupIds != null && myGroupIds.Contains(u.Key);

                    series.data.Add(point);
                }

                seriesList.Add(series);
            });

            return DResult.Succ<SeriesDto>(seriesList);
        }
        #endregion

        #region 学生成绩排名升降图
        /// <summary>
        /// 学生成绩排名升降图
        /// </summary>
        /// <returns></returns>
        public DResult<StudentRankDataDto> StudentRank(string groupId, int subjectId)
        {
            if (string.IsNullOrEmpty(groupId)) return DResult.Error<StudentRankDataDto>("参数错误！");

            //查询最近的6套
            var usageList = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>().Where(u => u.ClassId == groupId && u.SubjectId == subjectId && u.MarkingStatus == (byte)MarkingStatus.AllFinished && u.SourceType != (byte)PublishType.Video).OrderByDescending(u => u.AddedAt).Take(6).Select(u => new UsageDto { Batch = u.Id, SourceId = u.SourceID, AddedTime = u.AddedAt }).ToList();

            if (usageList.Count < 1) return DResult.Succ<StudentRankDataDto>(null);

            var result = new StudentRankDataDto();
            //查询试卷
            var paperIds = usageList.Select(u => u.SourceId).ToList();
            var paperList = PaperContract.PaperList(paperIds);
            if (paperList.Data != null)
            {
                usageList.ForEach(u =>
                {
                    var paper = paperList.Data.SingleOrDefault(p => p.Id == u.SourceId);
                    u.PaperName = paper == null ? string.Empty : paper.PaperTitle;
                });
                result.UsageList = usageList;
            }

            var batchs = usageList.Select(u => u.Batch).ToList();
            var studentResultList = StuScoreStatisticsRepository.Where(u => batchs.Contains(u.Batch)).Select(u => new StudentRankDto { Rank = u.CurrentSort, StudentId = u.StudentId, Batch = u.Batch }).ToList();

            if (studentResultList.Any())
            {
                result.StudentRankList = studentResultList;
            }

            return DResult.Succ(result);
        }
        #endregion

        #region 获取教师端学生排名折线图数据
        /// <summary>
        /// 获取教师端学生排名折线图数据
        /// </summary>
        /// <returns></returns>
        public DResults<StudentSeriesDto> GetStuRankDetail(SearchStudentRankDto searchDto)
        {
            if (searchDto.SubjectId < 0) return DResult.Errors<StudentSeriesDto>("没有找到相关数据!");

            DateTime endDate;
            if (!DateTime.TryParse(searchDto.EndTimeStr, out endDate))
            {
                endDate = Clock.Now.Date;
            }
            endDate = endDate.AddDays(1);

            DateTime startDate;
            if (!DateTime.TryParse(searchDto.StartTimeStr, out startDate))
            {
                startDate = endDate.AddMonths(-3);
            }

            var result = GetStudentSeries(startDate, endDate, searchDto.SubjectId, searchDto.StudentId);

            return DResult.Succ<StudentSeriesDto>(result);
        }
        #endregion

        #region 获取学生时间段内考过的科目
        /// <summary>
        /// 获取学生时间段内考过的科目
        /// </summary>
        /// <returns></returns>
        public DResult<IDictionary<int, string>> GetStudentSubject(SearchStudentRankDto searchDto)
        {
            DateTime endDate;
            if (!DateTime.TryParse(searchDto.EndTimeStr, out endDate))
            {
                endDate = Clock.Now.Date;
            }
            endDate = endDate.AddDays(1);

            DateTime startDate;
            if (!DateTime.TryParse(searchDto.StartTimeStr, out startDate))
            {
                startDate = endDate.AddMonths(-3);
            }

            var subjects = StuScoreStatisticsRepository.Where(u => u.AddedAt >= startDate && u.AddedAt <= endDate && u.StudentId == searchDto.StudentId && u.Status == (byte)StuStatisticsStatus.Normal).Select(u => u.SubjectId).ToList();

            if (subjects.Count < 1)
                return DResult.Succ<IDictionary<int, string>>(null);

            var subjectList = SystemContract.SubjectDict(subjects);

            return DResult.Succ(subjectList);
        }
        #endregion

        #region 获取学生成绩统计
        /// <summary>
        /// 获取学生成绩统计
        /// </summary>
        /// <returns></returns>
        public DResults<StudentSeriesDto> GetStudentScores(SearchStudentRankDto searchDto)
        {
            if (searchDto.SubjectId < 0) return DResult.Errors<StudentSeriesDto>("没有找到相关数据!");

            DateTime endDate;
            if (!DateTime.TryParse(searchDto.EndTimeStr, out endDate))
            {
                endDate = Clock.Now.Date;
            }
            endDate = endDate.AddDays(1);

            DateTime startDate;
            if (!DateTime.TryParse(searchDto.StartTimeStr, out startDate))
            {
                startDate = endDate.AddMonths(-3);
            }

            var result = GetStudentSeries(startDate, endDate, searchDto.SubjectId, searchDto.StudentId);

            return DResult.Succ<StudentSeriesDto>(result);
        }
        #endregion

        #region 获取学生成绩统计
        /// <summary>
        /// 获取学生成绩统计
        /// </summary>
        /// <returns></returns>
        public DResults<StudentScoreDto> GetStudentScores(List<string> batchs, long userId)
        {
            var result = StuScoreStatisticsRepository.Where(u => batchs.Contains(u.Batch) && u.StudentId == userId).Select(u => new StudentScoreDto()
            {
                Batch = u.Batch,
                Rank = u.CurrentSort,
                Score = u.CurrentScore
            }).ToList();

            return DResult.Succ<StudentScoreDto>(result);
        }
        #endregion

        #region 获取学生成绩
        /// <summary>
        /// 获取学生成绩
        /// </summary>
        /// <param name="batchs"></param>
        /// <returns></returns>
        public DResult<Dictionary<string, decimal>> GetGroupAvgScores(List<string> batchs)
        {
            var result = ClassScoreStatisticsRepository.Where(u => batchs.Contains(u.Batch))
                  .Select(u => new { u.Batch, u.AverageScore })
                  .ToList()
                  .ToDictionary(u => u.Batch, u => u.AverageScore);

            return DResult.Succ(result);
        }
        #endregion

    }
}
