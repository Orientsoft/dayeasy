using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Statistic.Services
{
    public partial class StatisticService
    {
        #region 注入
        public IErrorBookContract ErrorBookContrace { private get; set; }
        public IUserContract UserContract { private get; set; }
        public ISmsRecordContract SmsRecordContract { private get; set; }
        public ISmsScoreNoticeContract SmsScoreNoticeContract { private get; set; }
        public IDayEasyRepository<TC_Usage, string> UsageRepository { private get; set; }
        public IDayEasyRepository<TP_MarkingDetail, string> MarkingDetailRepository { private get; set; }
        public IDayEasyRepository<TP_PaperAnswer, string> PaperAnswerRepository { private get; set; }
        public IDayEasyRepository<TQ_Question, string> QuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_Answer, string> QuestionAnswerRepository { private get; set; }
        public IDayEasyRepository<TS_Subject, int> SubjectRepository { private get; set; }
        #endregion

        /// <summary>
        /// 客观题选项统计
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="questionId"></param>
        /// <param name="smallQuestionId"></param>
        /// <returns></returns>
        public DResult<StatisticsQuestionDto> StatisticsQuestionDetail(string batch, string paperId,
            string questionId, string smallQuestionId = "")
        {
            //题目参考答案
            var result = new StatisticsQuestionDto
            {
                SystemAnswer = GetQuestionAnswer(questionId, smallQuestionId, paperId),
                Answers = new List<StatisticsQuestionAnswerDto>()
            };

            Expression<Func<TP_MarkingDetail, bool>> condition = d =>
                d.Batch == batch && d.PaperID == paperId && d.QuestionID == questionId && d.IsFinished;
            if (smallQuestionId.IsNotNullOrEmpty())
                condition = condition.And(d => d.SmallQID == smallQuestionId);

            var details = MarkingDetailRepository.Where(condition).ToList();
            if (!details.Any()) return DResult.Succ(result);

            var total = details.Count();
            result.Answers = details.GroupBy(t => t.AnswerContent)
                .Select(t => new StatisticsQuestionAnswerDto
                {
                    Name = t.Key.IsNullOrEmpty() ? "未答" : t.Key,
                    Y = Math.Round(t.Count() / (double)total * 100, 2),
                    Students = t.Select(s => s.StudentID).Distinct().ToList()
                }).ToList();

            return DResult.Succ(result);
        }

        /// <summary>
        /// 查询问题答案
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="smallQuestionId"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        string GetQuestionAnswer(string questionId, string smallQuestionId = "", string paperId = "")
        {
            if (paperId.IsNotNullOrEmpty())
            {
                Expression<Func<TP_PaperAnswer, bool>> condition = a =>
                    a.PaperId == paperId && a.QuestionId == questionId;
                if (smallQuestionId.IsNotNullOrEmpty())
                    condition = condition.And(a => a.SmallQuId == smallQuestionId);
                var item = PaperAnswerRepository.FirstOrDefault(condition);
                if (item != null) return item.AnswerContent;
            }
            var isObjective = QuestionRepository.Exists(q => q.Id == questionId && q.IsObjective);
            Expression<Func<TQ_Answer, bool>> condition1 = a => a.IsCorrect;
            condition1 = smallQuestionId.IsNotNullOrEmpty()
                ? condition1.And(a => a.QuestionID == smallQuestionId)
                : condition1.And(a => a.QuestionID == questionId);
            var answers = QuestionAnswerRepository.Where(condition1).OrderBy(a => a.Sort).ToList();
            if (!isObjective)
                return string.Join(" ", answers.Select(a => a.QContent));
            return string.Join("", answers.Select(a =>
                Convert.ToChar((a.Sort) + 65).ToString(CultureInfo.InvariantCulture)));
        }

        //单科成绩 学生排名升降趋势
        internal Dictionary<long, int> TrendRanks(string batch, string paperId, string classId)
        {
            var thisCs =
                ClassScoreStatisticsRepository.Where(
                    s => s.Batch == batch && s.PaperId == paperId && s.ClassId == classId)
                    .Join(UsageRepository.Table, c => c.Batch, u => u.Id,
                        (c, u) => new { c.SubjectId, c.AddedAt, u.JointBatch }).FirstOrDefault();
            if (thisCs == null)
                return null;
            //add by shay 协同/非协同分开比较
            var isJoint = !string.IsNullOrWhiteSpace(thisCs.JointBatch);

            var usages = UsageRepository.Where(u => u.ClassId == classId);
            usages = isJoint
                ? usages.Where(u => u.JointBatch != null && u.JointBatch.Length == 32)
                : usages.Where(u => u.JointBatch == null);
            var lastCs =
                ClassScoreStatisticsRepository.Where(
                    s => s.ClassId == classId && s.SubjectId == thisCs.SubjectId && s.AddedAt < thisCs.AddedAt)
                    .Join(usages, c => c.Batch, u => u.Id, (c, u) => new { c.Batch, c.PaperId, c.AddedAt })
                    .OrderByDescending(s => s.AddedAt)
                    .Select(t => new { t.Batch, t.PaperId })
                    .FirstOrDefault();
            if (lastCs == null)
                return null;

            return StuScoreStatisticsRepository.Where(
                s =>
                    s.Batch == lastCs.Batch && s.PaperId == lastCs.PaperId && s.ClassId == classId &&
                    s.SubjectId == thisCs.SubjectId)
                .Select(s => new { s.StudentId, s.CurrentSort })
                .ToList()
                .ToDictionary(k => k.StudentId, v => v.CurrentSort);
        }

        /// <summary>
        /// 考试统计概况 - 排名分析
        /// </summary>
        public DResult<SurveyAnalysis> GetSurveyAnalysis(string batch, string paperId)
        {
            var rankResult = GetStatisticsRank(batch, paperId);
            if (!rankResult.Status) return DResult.Error<SurveyAnalysis>(rankResult.Message);

            var ranks = rankResult.Data.ToList();
            var result = new SurveyAnalysis();

            #region 前后十名

            result.TopTen = ranks.Where(r => r.Rank != -1 && r.Rank <= 10)
                .Select(r => new DUserDto
                {
                    Id = r.StudentId,
                    Name = r.StudentName
                }).ToList();
            var tmpRank = ranks.Max(r => r.Rank) - 10;
            if (tmpRank > 0)
            {
                if (tmpRank < 10) tmpRank = 10;
                result.LastTen = ranks.Where(r => r.Rank > tmpRank)
                    .Select(r => new DUserDto
                    {
                        Id = r.StudentId,
                        Name = r.StudentName
                    }).ToList();
            }

            #endregion

            #region 排名对比升降

            if (ranks.Any(r => r.LastRank.HasValue && r.Rank != -1))
            {
                var trends = ranks.Where(r => r.LastRank.HasValue && r.Rank != -1)
                    .Select(r => new SurveyTrendDto
                    {
                        UserBase = new DUserDto
                        {
                            Id = r.StudentId,
                            Name = r.StudentName
                        },
                        Trend = (r.LastRank.Value - r.Rank)
                    }).ToList();
                var upList = trends.Where(t => t.Trend > 0).OrderByDescending(t => t.Trend).ToList();
                if (upList.Any())
                {
                    result.Progress = upList.Take(10).ToList();
                    if (upList.Count > 10)
                    {
                        var lastProgress = result.Progress.LastOrDefault();
                        result.Progress.AddRange(upList.Skip(10).Where(t => t.Trend == lastProgress.Trend));
                    }
                }
                var downList = trends.Where(t => t.Trend < 0).OrderBy(t => t.Trend).ToList();
                if (downList.Any())
                {
                    result.BackSlide = downList.Take(10).ToList();
                    if (downList.Count > 10)
                    {
                        var lastBackSlide = result.BackSlide.LastOrDefault();
                        result.BackSlide.AddRange(downList.Skip(10).Where(t => t.Trend == lastBackSlide.Trend));
                    }
                }
            }

            #endregion

            #region 不及格

            var paperResult = PaperContract.PaperDetailById(paperId, false);
            if (paperResult.Status && paperResult.Data != null)
            {
                var paper = paperResult.Data;
                result.IsAb = paper.PaperBaseInfo.IsAb;
                if (paper.PaperBaseInfo.PaperScores != null)
                {
                    var totalScore = paper.PaperBaseInfo.PaperScores.TScore;
                    if (totalScore > 0)
                    {
                        var passScore = totalScore * 0.6M;
                        result.Fails = ranks.Where(r => r.TotalScore < passScore)
                            .Select(r => new DUserDto
                            {
                                Id = r.StudentId,
                                Name = r.StudentName
                            }).ToList();
                        if (paper.PaperBaseInfo.IsAb)
                        {
                            passScore = paper.PaperBaseInfo.PaperScores.TScoreA * 0.6M;
                            result.FailsA = ranks.Where(t => t.SectionAScore < passScore)
                                .Select(t => new DUserDto
                                {
                                    Id = t.StudentId,
                                    Name = t.StudentName
                                }).ToList();
                        }
                    }
                }
            }

            #endregion

            #region 未提交

            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null) return DResult.Succ(result);

            var studentResult = GroupContract.GroupMembers(usage.ClassId, UserRole.Student);
            if (!studentResult.Status)
                return DResult.Succ(result);

            var submitIds = ErrorBookContrace.IsSubmitStudentIds(batch, paperId);
            result.UnSubmits = studentResult.Data.Where(s => !submitIds.Contains(s.Id))
                .Select(s => new DUserDto
                {
                    Id = s.Id,
                    Avatar = s.Avatar,
                    Name = s.Name,
                    Nick = s.Nick
                }).ToList();

            #endregion

            return DResult.Succ(result);
        }

        /// <summary> 排名统计 </summary>
        /// <returns></returns>
        public DResults<StudentRankInfoDto> GetStatisticsRank(string batch, string paperId, string jointBatch = null,
            string groupId = null)
        {
            if ((batch.IsNullOrEmpty() && jointBatch.IsNullOrEmpty()) || paperId.IsNullOrEmpty())
                return DResult.Errors<StudentRankInfoDto>("参数错误");

            //发布记录
            Expression<Func<TC_Usage, bool>> condition;
            if (jointBatch.IsNotNullOrEmpty())
            {
                condition = u => u.JointBatch == jointBatch;
                if (groupId.IsNotNullOrEmpty())
                    condition = condition.And(u => u.ClassId == groupId);
            }
            else
            {
                condition = u => u.Id == batch;
            }
            var usages = UsageRepository.Where(condition).ToList();
            var thisUsage = usages.FirstOrDefault();
            if (!usages.Any() || thisUsage == null) return DResult.Errors<StudentRankInfoDto>("没有查询到发布记录");

            // update by epc 16/03/28
            // 之前方案，默认生成该班级下所有学生的答卷统计，分数为0
            // 现，不生成答卷统计，查询出所有所学显示分数为-，当教师编辑后生成统计
            ////推送的试卷需要初始化分数统计
            //if (usages.Any(u => u.SourceType == (byte)PublishType.Test))
            //{
            //    if (usages.Any(usage => !CheckGenerateScoreStatistics(usage, paperId)))
            //    {
            //        return DResult.Errors<StudentRankInfoDto>("分数统计初始化失败，请刷新重试");
            //    }
            //}

            //学生资料
            var students = new List<MemberDto>();
            usages.ForEach(usage =>
            {
                var studentResult = GroupContract.GroupMembers(usage.ClassId, UserRole.Student, true);
                if (studentResult.Status) students.AddRange(studentResult.Data);
            });

            //班级圈资料
            var groupIds = usages.Select(u => u.ClassId).Distinct().ToList();
            var groupDict = GroupContract.GroupDict(groupIds);

            //统计资料
            var batchs = usages.Select(u => u.Id).ToList();
            var statistics = StuScoreStatisticsRepository
                .Where(d => d.PaperId == paperId && batchs.Contains(d.Batch) && groupIds.Contains(d.ClassId))
                .OrderByDescending(d => d.CurrentScore)
                .ToList();

            //协同统计 需 重新计算排名
            if (jointBatch.IsNotNullOrEmpty() && usages.Any() && statistics.Any())
            {
                statistics.ForEach(s =>
                {
                    s.CurrentSort = statistics.Count(t => t.CurrentScore > s.CurrentScore) + 1;
                });
            }

            //返回数据
            //协同必然存在统计数据，so 推送试卷时，usages[0] 为当前班级
            //var rank = statistics.Any() ? (statistics.Max(s => s.CurrentSort) + 1) : -1;
            var result = new List<StudentRankInfoDto>();
            students.ForEach(u =>
            {
                var s = statistics.FirstOrDefault(i => i.StudentId == u.Id);
                //if (s == null) return;
                // StudentRankInfoDto decimal 变更为 decimal?
                if (s == null && (jointBatch.IsNotNullOrEmpty() ||
                                  thisUsage.SourceType != (byte)PublishType.Test)) return;
                var item = new StudentRankInfoDto
                {
                    Id = string.Empty,
                    StudentId = u.Id,
                    Rank = -1,
                    LastRank = null,
                    StudentNum = u.StudentNum,
                    StudentName = u.Name,
                    GroupId = thisUsage.ClassId,
                    Mobiles = new List<string>()
                };
                if (s != null)
                {
                    item.Id = s.Id;
                    item.TotalScore = s.CurrentScore;
                    item.ErrorQuestionCount = s.ErrorQuCount;
                    item.SectionAScore = s.SectionAScore;
                    item.SectionBScore = s.SectionBScore;
                    item.Rank = s.CurrentSort;
                    item.GroupId = s.ClassId;
                }
                if (u.Mobile.IsNotNullOrEmpty() && (u.ValidationType & (byte)ValidationType.Mobile) > 0)
                    item.Mobiles.Add(u.Mobile);
                if (u.Parents != null && u.Parents.Any())
                    u.Parents.Foreach(p =>
                    {
                        var tmp = (RelationUserDto)p;
                        if (tmp.Mobile.IsNotNullOrEmpty()) item.Mobiles.Add(tmp.Mobile);
                    });
                result.Add(item);
            });
            if (!result.Any()) return DResult.Errors<StudentRankInfoDto>("没有任何统计数据");
            result = result.OrderBy(r => r.Rank < 0 ? 999 : r.Rank).ToList();

            //查询排名升降变化
            var trends = jointBatch.IsNullOrEmpty()
                ? TrendRanks(batch, paperId, thisUsage.ClassId)
                : null;
            trends = trends ?? new Dictionary<long, int>();
            result.ForEach(r =>
            {
                //班级圈名称
                if (groupDict.ContainsKey(r.GroupId))
                {
                    r.GroupName = groupDict[r.GroupId];
                }
                //排名升降
                if (trends.ContainsKey(r.StudentId))
                    r.LastRank = trends[r.StudentId];
            });

            return DResult.Succ(result, result.Count);
        }

        /// <summary>
        /// 发送成绩通知短信
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        public DResult SendScoreSms(string batch, string paperId, List<long> studentIds)
        {
            if (batch.IsNullOrEmpty() || paperId.IsNullOrEmpty())
                return DResult.Error("参数错误：批次号或试卷ID为空");
            if (studentIds == null || !studentIds.Any())
                return DResult.Error("参数错误：未勾选待发送成绩的学生");

            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null) return DResult.Error("没有查询到发布记录");
            var studentsResult = GroupContract.GroupMembers(usage.ClassId, UserRole.Student, true);
            if (!studentsResult.Status || studentsResult.Data == null)
                return DResult.Error("没有查询到学生资料");
            var students = studentsResult.Data.Where(u => studentIds.Contains(u.Id)).ToList();
            if (!students.Any()) return DResult.Error("没有查询到学生资料");
            var subjtct = SubjectRepository.FirstOrDefault(s => s.Id == usage.SubjectId);
            if (subjtct == null) return DResult.Error("没有查询到学科");
            var paperResult = PaperContract.PaperDetailById(paperId, false);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error("没有查询到试卷资料");
            var classStatistics = ClassScoreStatisticsRepository
                .FirstOrDefault(s => s.Batch == batch && s.PaperId == paperId);
            if (classStatistics == null) return DResult.Error("没有查询到班级分数统计");
            var studentStatistics = StuScoreStatisticsRepository
                .Where(s => s.Batch == batch && s.PaperId == paperId && studentIds.Contains(s.StudentId));
            if (!studentStatistics.Any()) return DResult.Error("没有查询到学生分数统计");

            //已发送的短信记录
            var exists = SmsScoreNoticeContract.FindByBatch(batch, paperId);
            const byte mobileValidation = (byte)ValidationType.Mobile;

            var isAb = paperResult.Data.PaperBaseInfo.IsAb;
            var time = usage.StartTime.ToString("M月dd日");
            var subjectName = subjtct.SubjectName;
            List<string> mobiles = new List<string>(), messages = new List<string>();
            string
                messageTemplate = "【得一教育】{0}，在" + time + subjectName +
                                  "考试中得分为{1}，排名{2}，最高分" + classStatistics.TheHighestScore +
                                  "，平均分" + classStatistics.AverageScore + "，详见www.dayeasy.net",
                messageAbTemplate = "【得一教育】{0}，在" + time + subjectName +
                                    "考试中得分为{1}，A卷{3}，B卷{4}，排名{2}，最高分" +
                                    classStatistics.TheHighestScore +
                                    "，平均分" + classStatistics.AverageScore + "，详见www.dayeasy.net";
            students.ForEach(stu =>
            {
                var ts = studentStatistics.FirstOrDefault(s => s.StudentId == stu.Id);
                if (ts == null) return;

                if (stu.Mobile.IsNotNullOrEmpty() &&
                    (stu.ValidationType & mobileValidation) > 0 &&
                    !exists.Exists(m => m.Mobile == stu.Mobile))
                {
                    mobiles.Add(stu.Mobile);
                    messages.Add(isAb
                        ? messageAbTemplate.FormatWith(stu.Name, ts.CurrentScore, ts.CurrentSort, ts.SectionAScore, ts.SectionBScore)
                        : messageTemplate.FormatWith(stu.Name, ts.CurrentScore, ts.CurrentSort));
                }
                if (stu.Parents != null && stu.Parents.Any())
                {
                    stu.Parents.Foreach(p =>
                    {
                        var rel = (RelationUserDto)p;
                        if (rel.Mobile.IsNullOrEmpty() || exists.Exists(m => m.Mobile == rel.Mobile)) return;
                        mobiles.Add(rel.Mobile);
                        messages.Add(isAb
                         ? messageAbTemplate.FormatWith(stu.Name, ts.CurrentScore, ts.CurrentSort, ts.SectionAScore, ts.SectionBScore)
                         : messageTemplate.FormatWith(stu.Name, ts.CurrentScore, ts.CurrentSort));
                    });
                }
            });
            if (!mobiles.Any()) return DResult.Success;
            if (mobiles.Count > 1000) return DResult.Error("短信数量过多，请分次勾选发送");

            //批量发短信
            var result = SmsRecordContract.SendLotSizeAndGetResult(mobiles, messages);
            if (!result.Status) return DResult.Error(result.Message);
            if (result.Data.All(r => r.Status != 0)) return DResult.Error("短信发送失败");

            //记录已成功通知成绩的手机号码
            var dt = Clock.Now;
            var list = new List<MongoSmsScoreNotice>();
            var data = result.Data.Where(r => r.Status == 0).ToList();
            data.ForEach(i => list.Add(new MongoSmsScoreNotice
            {
                Id = IdHelper.Instance.GetGuid32(),
                Batch = batch,
                PaperId = paperId,
                SmsRecordId = i.Id,
                Mobile = i.Mobile,
                Time = dt
            }));
            SmsScoreNoticeContract.Add(list);

            return DResult.Success;
        }

        /// <summary>
        /// 初始化推送试卷的分数统计
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="paperId"></param>
        /// <returns></returns>
        bool CheckGenerateScoreStatistics(TC_Usage usage, string paperId)
        {
            var exist = StuScoreStatisticsRepository.Exists(
                s => s.Batch == usage.Id && s.PaperId == paperId && s.ClassId == usage.ClassId);
            if (exist) return true;

            return UnitOfWork.Transaction(() =>
            {
                #region TS_StuScoreStatistics

                var studentsResult = GroupContract.GroupMembers(usage.ClassId, UserRole.Student);
                if (!studentsResult.Status) return;
                var dt = Clock.Now;

                var list = new List<TS_StuScoreStatistics>();
                var sort = 1;
                studentsResult.Data.Foreach(stu => list.Add(new TS_StuScoreStatistics
                {
                    Id = IdHelper.Instance.GetGuid32(),
                    Batch = usage.Id,
                    PaperId = paperId,
                    ClassId = usage.ClassId,
                    AddedAt = dt,
                    AddedBy = usage.UserId,
                    CurrentScore = 0M,
                    CurrentSort = sort++,
                    ErrorQuCount = 0,
                    SectionAScore = 0M,
                    SectionBScore = 0M,
                    Status = (byte)StuStatisticsStatus.Normal,
                    StudentId = stu.Id,
                    SubjectId = usage.SubjectId
                }));
                StuScoreStatisticsRepository.Insert(list.ToArray());

                #endregion

                #region TS_ClassScoreStatistics

                var exists = ClassScoreStatisticsRepository.Exists(
                    c => c.Batch == usage.Id && c.PaperId == paperId && c.ClassId == usage.ClassId);
                if (!exists)
                {
                    ClassScoreStatisticsRepository.Insert(new TS_ClassScoreStatistics
                    {
                        Id = IdHelper.Instance.GetGuid32(),
                        Batch = usage.Id,
                        PaperId = paperId,
                        ClassId = usage.ClassId,
                        AddedBy = usage.UserId,
                        AddedAt = dt,
                        AverageScore = 0M,
                        ScoreGroups = string.Empty,
                        SectionScoreGroups = string.Empty,
                        SectionScores = string.Empty,
                        SubjectId = usage.SubjectId,
                        Status = (byte)ClassStatisticsStatus.Normal,
                        TheHighestScore = 0M,
                        TheLowestScore = 0M
                    });
                }

                #endregion
            }) > 0;
        }

        #region 分数段统计

        List<StatisticsScoreDto> ConverToStatisticsScoreDtos(List<TS_ClassScoreStatistics> statisticses, List<GroupDto> groups)
        {
            if (statisticses == null || !statisticses.Any() || groups == null || !groups.Any())
                return new List<StatisticsScoreDto>();

            return (from s in statisticses
                    from g in groups
                    where s.ClassId == g.Id
                    select new StatisticsScoreDto
                    {
                        GroupId = g.Id,
                        GroupName = g.Name,
                        AgencyId = g.AgencyId,
                        AgencyName = g.AgencyName,
                        Time = s.AddedAt.ToString("yyyyMMdd"),
                        Max = s.TheHighestScore,
                        Min = s.TheLowestScore,
                        Avg = s.AverageScore,
                        AbAvg = (s.SectionScores.IsNullOrEmpty()
                            ? new ReportSectionScoresDto()
                            : JsonHelper.Json<ReportSectionScoresDto>(s.SectionScores)),
                        ScoreGroupes = (s.ScoreGroups.IsNullOrEmpty()
                            ? new List<ScoreGroupsDto>()
                            : JsonHelper.JsonList<ScoreGroupsDto>(s.ScoreGroups).ToList()),
                        AbScoreGroupes = (s.SectionScoreGroups.IsNullOrEmpty()
                            ? new List<List<ScoreGroupsDto>>()
                            : JsonHelper.JsonList<List<ScoreGroupsDto>>(s.SectionScoreGroups).ToList())
                    }).OrderBy(g => g.AgencyId).ThenBy(g => g.GroupName.ClassIndex()).ToList();
        }

        /// <summary>
        /// 分数段统计
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="keepSixty">保留60%以下</param>
        /// <param name="single">不查询年级统计</param>
        /// <returns></returns>
        public DResults<StatisticsScoreDto> GetStatisticsAvges(string batch, string paperId, bool keepSixty = false, bool single = false)
        {
            if (batch.IsNullOrEmpty() || paperId.IsNullOrEmpty())
                return DResult.Errors<StatisticsScoreDto>("参数错误");
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null)
                return DResult.Errors<StatisticsScoreDto>("没有查询到发布记录");

            //当前圈子统计
            var item = ClassScoreStatisticsRepository.FirstOrDefault(s => s.Batch == batch && s.PaperId == paperId);
            if (item == null)
                return DResult.Errors<StatisticsScoreDto>("没有查询到分数统计");

            //统计列表
            var list = new List<TS_ClassScoreStatistics>();
            if (usage.ColleagueGroupId.IsNotNullOrEmpty() && !single)
            {
                //前后2周内年级各班考试统计
                var dt = Clock.Now;
                DateTime st = dt.AddDays(-14), et = dt.AddDays(14);
                var usages = UsageRepository.Where(u => u.ColleagueGroupId == usage.ColleagueGroupId);
                list = ClassScoreStatisticsRepository
                    .Where(s => s.PaperId == paperId && s.Id != item.Id && s.AddedAt >= st && s.AddedAt <= et)
                    .Join(usages, s => s.Batch, u => u.Id, (s, u) => s)
                    .ToList();
                list.Insert(0, item);
            }
            else
            {
                list.Add(item);
            }

            //圈子列表
            var groupResult = GroupContract.SearchGroups(list.Select(s => s.ClassId).ToList());
            if (!groupResult.Status || !groupResult.Data.Any())
                return DResult.Errors<StatisticsScoreDto>("没有查询圈子资料");

            //转换数据
            var result = ConverToStatisticsScoreDtos(list, groupResult.Data.ToList());

            //查询及格率
            if (single)
            {
                var ssItem = result.FirstOrDefault();
                var paperResult = PaperContract.PaperDetailById(paperId, false);
                if (ssItem != null && paperResult.Status && paperResult.Data != null)
                {
                    var totalScore = paperResult.Data.PaperBaseInfo.PaperScores != null
                        ? paperResult.Data.PaperBaseInfo.PaperScores.TScore
                        : 0;
                    ssItem.Count = StuScoreStatisticsRepository.Count(i => i.Batch == batch && i.PaperId == paperId);
                    if (ssItem.Count > 0 && totalScore > 0)
                    {
                        var passScore = totalScore * 0.6M;
                        var passCount = StuScoreStatisticsRepository
                            .Count(i => i.Batch == batch && i.PaperId == paperId && i.CurrentScore >= passScore);
                        ssItem.PassRate = Math.Round((decimal)passCount / ssItem.Count * 100, 2,
                            MidpointRounding.AwayFromZero);
                    }
                }
            }

            if (!keepSixty)
            {
                //舍去60%以下
                result.ForEach(r =>
                {
                    SixtyPercent(r.ScoreGroupes);
                    if (r.AbScoreGroupes == null || r.AbScoreGroupes.Count != 2) return;
                    SixtyPercent(r.AbScoreGroupes[0]);
                    SixtyPercent(r.AbScoreGroupes[1]);
                });
            }

            return DResult.Succ(result, result.Count);
        }

        /// <summary>
        /// 分数段统计 - 协同
        /// </summary>
        /// <param name="jointBatch"></param>
        /// <param name="paperId"></param>
        /// <param name="keepSixty"></param>
        /// <returns></returns>
        public DResults<StatisticsScoreDto> GetJointStatisticsAvges(string jointBatch, string paperId, bool keepSixty = false)
        {
            if (jointBatch.IsNullOrEmpty() || paperId.IsNullOrEmpty())
                return DResult.Errors<StatisticsScoreDto>("参数错误");

            //统计列表
            var usages = UsageRepository.Where(u => u.JointBatch == jointBatch);
            var list = ClassScoreStatisticsRepository.Where(s => s.PaperId == paperId)
                .Join(usages, s => s.Batch, u => u.Id, (s, u) => s).ToList();

            if (!list.Any()) return DResult.Errors<StatisticsScoreDto>("没有统计资料");

            //圈子列表
            var groupResult = GroupContract.SearchGroups(list.Select(s => s.ClassId).ToList());
            if (!groupResult.Status || !groupResult.Data.Any())
                return DResult.Errors<StatisticsScoreDto>("没有查询圈子资料");

            //转换数据
            var result = ConverToStatisticsScoreDtos(list, groupResult.Data.ToList());

            if (!keepSixty)
            {
                //舍去60%以下
                result.ForEach(r =>
                {
                    SixtyPercent(r.ScoreGroupes);
                    if (r.AbScoreGroupes == null || r.AbScoreGroupes.Count != 2) return;
                    SixtyPercent(r.AbScoreGroupes[0]);
                    SixtyPercent(r.AbScoreGroupes[1]);
                });
            }

            return DResult.Succ(result, result.Count);
        }

        #endregion

        /// <summary>
        /// 共享、取消共享试卷统计到同事圈
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="teacherId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public DResult StatisticsShare(string batch, long teacherId, string groupId = "")
        {
            if (batch.IsNullOrEmpty())
                return DResult.Error("参数错误");
            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null)
                return DResult.Error("没有查询到发布记录");
            if (usage.UserId != teacherId)
                return DResult.Error("没有权限，您不是发布该考试的教师");

            if (groupId.IsNullOrEmpty())
            {
                if (usage.ColleagueGroupId.IsNullOrEmpty())
                    return DResult.Success;
                usage.ColleagueGroupId = string.Empty;
            }
            else
            {
                var groupResult = GroupContract.Groups(teacherId, (byte)GroupType.Colleague);
                if (!groupResult.Status || groupResult.Data == null || !groupResult.Data.Any() ||
                    !groupResult.Data.Select(g => g.Id).Contains(groupId))
                    return DResult.Error("没有查询到该同事圈资料");
                usage.ColleagueGroupId = groupId;
            }
            return UsageRepository.Update(u => new { u.ColleagueGroupId }, usage) > 0
                ? DResult.Success
                : DResult.Error("操作失败，请刷新重试");
        }

        /// <summary> 每题得分 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public DResult<QuestionScoresDto> QuestionScores(string batch)
        {
            var paperId = UsageRepository.Where(u => u.Id == batch).Select(u => u.SourceID).FirstOrDefault();
            if (paperId.IsNullOrEmpty())
                return DResult.Error<QuestionScoresDto>("发布批次号不正确！");
            //题目序号
            var dto = new QuestionScoresDto();
            var paper = PaperContract.PaperDetailById(paperId);
            LoadQuestionSorts(paper.Data, dto);
            var qids = dto.QuestionSorts.Keys.ToList();

            var students = MarkingDetailRepository.Where(d => d.Batch == batch)
                .Select(d => new
                {
                    d.StudentID,
                    d.QuestionID,
                    d.SmallQID,
                    d.CurrentScore
                })
                .ToList()
                .GroupBy(d => d.StudentID)
                .Select(s => new StudentQuestionScoresDto
                {
                    Id = s.Key,
                    Scores = s.Select(t => new
                    {
                        id = t.SmallQID.IsNullOrEmpty() ? t.QuestionID : t.SmallQID,
                        score = t.CurrentScore
                    })
                        .OrderBy(t => qids.IndexOf(t.id))
                        .ToDictionary(k => k.id, v => v.score)
                }).ToList();
            var userIds = students.Select(s => s.Id).ToList();
            var userList = UserContract.LoadList(userIds);
            foreach (var student in students)
            {
                var user = userList.FirstOrDefault(u => u.Id == student.Id);
                if (user == null)
                    continue;
                student.Name = user.Name;
                student.Code = user.Code;
            }
            dto.Students = students;

            return DResult.Succ(dto);
        }

        public DResult<QuestionScoresDto> JointQuestionScores(string joint)
        {
            throw new NotImplementedException();
        }

        private static void LoadQuestionSorts(PaperDetailDto paperDto, QuestionScoresDto dto)
        {
            var sorts = new Dictionary<string, string>();
            var types = new Dictionary<byte, Dictionary<string, string>>();

            foreach (var section in paperDto.PaperSections)
            {
                if (!types.ContainsKey(section.PaperSectionType))
                {
                    types.Add(section.PaperSectionType, new Dictionary<string, string>());
                }
                var type = types[section.PaperSectionType];

                var prefix = string.Empty;
                var sortRange = new List<int>();
                if (paperDto.PaperBaseInfo.IsAb)
                {
                    prefix = (section.PaperSectionType == (byte)PaperSectionType.PaperA ? "A" : "B");
                }
                var isThrough = paperDto.PaperBaseInfo.SubjectId == 3;
                foreach (var question in section.Questions)
                {
                    if (question.Question.IsObjective && question.Question.HasSmall)
                    {
                        if (isThrough)
                        {
                            foreach (var detail in question.Question.Details)
                            {

                                sortRange.Add(detail.Sort);
                                sorts.Add(detail.Id, string.Concat(prefix, detail.Sort));
                            }
                        }
                        else
                        {
                            sortRange.Add(question.Sort);
                            foreach (var detail in question.Question.Details)
                            {
                                sorts.Add(detail.Id,
                                    string.Concat(prefix, string.Format("{0}({1})", question.Sort, detail.Sort)));
                            }
                        }
                    }
                    else
                    {
                        if (question.Question.HasSmall && isThrough &&
                            section.PaperSectionType == (byte)PaperSectionType.PaperA)
                        {
                            var detailSorts = question.Question.Details.Select(d => d.Sort).ToList();
                            sortRange.AddRange(detailSorts);
                            sorts.Add(question.Question.Id,
                                string.Format("{0}({1}-{2})", prefix, detailSorts.Min(), detailSorts.Max()));
                        }
                        else
                        {
                            sortRange.Add(question.Sort);
                            sorts.Add(question.Question.Id, string.Concat(prefix, question.Sort));
                        }
                    }
                }
                if (sortRange.Any())
                {
                    type.Add(section.Description,
                        sortRange.Count() > 1
                            ? string.Format("{0}-{1}", sortRange.Min(), sortRange.Max())
                            : sortRange[0].ToString());
                }
            }
            dto.QuestionSorts = sorts;
            dto.QuestionTypes = types;
        }

        //舍去分数60%以下的数据，并计算60%以下的和
        private void SixtyPercent(List<ScoreGroupsDto> list)
        {
            if (list == null || list.Count < 10) return;
            var count = (int)Math.Floor((list.Count - 1) * 0.6);
            var lowCount = list.GetRange(0, count).Sum(c => c.Count);
            var strs = list[count].ScoreInfo.Split('-');
            list.RemoveRange(0, count);
            list.Insert(0, new ScoreGroupsDto { Count = lowCount, ScoreInfo = (strs.Length > 0 ? strs[0] + "分以下" : "") });
        }

    }
}
