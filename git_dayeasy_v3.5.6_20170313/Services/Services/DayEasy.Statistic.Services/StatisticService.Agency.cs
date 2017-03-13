using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Statistic.Agency;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Enum.Statistic;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.Statistic.Services.Helper;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Statistic.Services
{
    /// <summary> 教务管理相关数据分析 </summary>
    public partial class StatisticService
    {
        #region 注入 & 私有
        public IVersion3Repository<TS_Agency> AgencyRepository { private get; set; }
        public IDayEasyRepository<TU_UserAgency> UserAgencyRepository { private get; set; }
        public IDayEasyRepository<TU_User, long> UserRepository { private get; set; }
        public IVersion3Repository<TG_Group> GroupRepository { private get; set; }
        public IVersion3Repository<TG_Class> ClassRepository { private get; set; }
        public IVersion3Repository<TG_Colleague> ColleagueRepository { private get; set; }
        public IVersion3Repository<TG_Member> MemberRepository { private get; set; }

        public IDayEasyRepository<TL_UserLog, long> UserLogRepository { private get; set; }
        public IDayEasyRepository<TU_Visit> VisitRepository { private get; set; }
        public IDayEasyRepository<TU_Impression> ImpressionRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorQuestion> ErrorQuestionRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorReason> ErrorReasonRepository { private get; set; }
        public IVersion3Repository<TE_Examination> ExaminationRepository { private get; set; }
        public IDayEasyRepository<TP_Variant> VariantRepository { private get; set; }
        public IDayEasyRepository<TT_TutorRecord> TutorRecordRepository { private get; set; }
        public IDayEasyRepository<TS_DownloadLog> DownloadLogRepository { private get; set; }

        private DateTime GetTimeArea(TimeArea timeArea)
        {
            var now = Clock.Now;
            switch (timeArea)
            {
                case TimeArea.Month:
                    return now.AddMonths(-1).Date;
                case TimeArea.ThreeMonth:
                    return now.AddMonths(-3).Date;
                case TimeArea.SixMonth:
                    return now.AddMonths(-6).Date;
                default:
                    return now.AddDays(-7).Date;
            }
        }
        #endregion

        #region 学校概况

        /// <summary> 学校概况 </summary>
        public AgencySurveyDto AgencySurvey(string agencyId, bool refresh = false)
        {
            var dto = new AgencySurveyDto { LastRefresh = Clock.Now };
            if (string.IsNullOrWhiteSpace(agencyId))
                return dto;
            var key = $"survey_{agencyId}";
            if (!refresh)
            {
                var cacheDto = StatisticCacheHelper.Instance.Get<AgencySurveyDto>(key);
                if (cacheDto != null)
                    return cacheDto;
            }
            var users =
                UserAgencyRepository.Where(a => a.AgencyId == agencyId && a.Status == (byte)UserAgencyStatus.Current)
                    .Join(UserRepository.Where(u => u.Status != (byte)UserStatus.Delete), ua => ua.UserId, u => u.Id,
                        (ua, u) => ua)
                    .GroupBy(a => a.Role)
                    .Select(t => new { role = t.Key, count = t.Count() })
                    .ToList()
                    .ToDictionary(k => k.role, v => v.count);
            //注册人数
            if (users.Any())
            {
                if (users.ContainsKey((byte)UserRole.Student))
                    dto.StudentCount = users[(byte)UserRole.Student];
                if (users.ContainsKey((byte)UserRole.Teacher))
                    dto.TeacherCount = users[(byte)UserRole.Teacher];
            }

            //圈子数
            var groups =
                GroupRepository.Table.Where(
                    g =>
                        g.Status == (byte)NormalStatus.Normal &&
                        g.CertificationLevel == (byte)GroupCertificationLevel.Normal);
            var members = MemberRepository.Where(m => m.Status == (byte)CheckStatus.Normal);
            var classModels = groups.Where(g => g.GroupType == (byte)GroupType.Class)
                .Join(ClassRepository.Where(c => c.AgencyId == agencyId), g => g.Id, c => c.Id,
                    (g, c) => new { g.Id, c.Stage, c.GradeYear, g.GroupName })
                .GroupJoin(members.Where(m => m.MemberRole == (byte)UserRole.Student), g => g.Id, m => m.GroupId,
                    (g, m) => new
                    {
                        g.Id,
                        g.GradeYear,
                        g.Stage,
                        g.GroupName,
                        count = m.Count()
                    }).ToList();
            if (classModels.Any())
            {
                dto.ClassCount = classModels.Count();
                dto.ClassList = classModels.GroupBy(g => new { g.Stage, g.GradeYear })
                    .OrderBy(g => g.Key.Stage)
                    .ThenByDescending(g => g.Key.GradeYear)
                    .Select(g => new AgencyGroupServeyDto
                    {
                        Key =
                            g.Key.Stage.GetEnumText<StageEnum, byte>() + (g.Key.GradeYear + (g.Key.Stage == 1 ? 6 : 3)) +
                            "级",
                        Users =
                            g.Select(t => new Core.Domain.DKeyValue<string, int>(t.GroupName, t.count))
                                .OrderBy(t => t.Key.ClassIndex())
                                .ToList()
                    }).ToList();
            }

            var colleagueModels = groups.Where(g => g.GroupType == (byte)GroupType.Colleague)
                .Join(ColleagueRepository.Where(c => c.AgencyId == agencyId), g => g.Id, c => c.Id,
                    (g, c) => new { g.Id, c.SubjectId, g.GroupName })
                .GroupJoin(members.Where(m => m.MemberRole == (byte)UserRole.Teacher), g => g.Id, m => m.GroupId,
                    (g, m) => new
                    {
                        g.Id,
                        g.SubjectId,
                        g.GroupName,
                        count = m.Count()
                    }).ToList();
            if (colleagueModels.Any())
            {
                dto.ColleagueCount = colleagueModels.Count();
                dto.ColleagueList = colleagueModels.GroupBy(g => g.SubjectId)
                    .OrderBy(g => g.Key)
                    .Select(g => new AgencyGroupServeyDto
                    {
                        Key = SystemCache.Instance.SubjectName(g.Key),
                        Users = g.Select(t => new DKeyValue<string, int>(t.GroupName, t.count)).ToList()
                    }).ToList();
            }

            dto.VisitCount = AgencyRepository.Where(a => a.Id == agencyId).Select(a => a.VisitCount).FirstOrDefault();

            dto.TargetCount =
                UserAgencyRepository.Where(a => a.AgencyId == agencyId && a.Status == (byte)UserAgencyStatus.Target)
                    .Count();
            //5分钟缓存
            StatisticCacheHelper.Instance.SetByRefreshTime(key, dto, TimeSpan.FromMinutes(15));
            return dto;
        }

        #endregion

        #region 师生画像
        /// <summary> 学校师生画像 </summary>
        public AgencyPortraitDto AgencyPortrait(string agencyId, TimeArea timeArea = TimeArea.Week, bool refresh = false)
        {
            var dto = new AgencyPortraitDto();
            if (string.IsNullOrWhiteSpace(agencyId))
                return dto;
            var key = $"portrait_{agencyId}_{timeArea}";
            if (!refresh)
            {
                var cacheDto = StatisticCacheHelper.Instance.Get<AgencyPortraitDto>(key);
                if (cacheDto != null)
                    return cacheDto;
            }
            var lastTime = GetTimeArea(timeArea);
            const int size = 4;

            //登录情况
            var users = UserAgencyRepository.Where(
                a => a.AgencyId == agencyId && a.Status == (byte)UserAgencyStatus.Current)
                .Join(UserRepository.Where(u => u.Status != (byte)UserStatus.Delete), ua => ua.UserId, u => u.Id,
                    (ua, u) => ua);
            var logs = UserLogRepository.Where(l => l.LogLevel == 1 && l.AddedAt >= lastTime)
                .Join(users, l => l.UserId, u => u.UserId, (l, u) => new { u.UserId, u.Role, l.AddedAt }).ToList();

            dto.Logins = logs.GroupBy(t => t.AddedAt.Date)
                .Select(t => new AgencyLoginDto
                {
                    Time = t.Key,
                    Student = t.Count(l => (l.Role & (byte)UserRole.Student) > 0),
                    Teacher = t.Count(l => (l.Role & (byte)UserRole.Teacher) > 0)
                }).ToList();
            for (var date = lastTime.Date; date <= Clock.Now.Date; date = date.AddDays(1))
            {
                if (dto.Logins.Exists(t => t.Time == date))
                    continue;
                dto.Logins.Add(new AgencyLoginDto
                {
                    Time = date
                });
            }
            dto.Logins = dto.Logins.OrderBy(t => t.Time).ToList();

            //画像
            dto.Teacher = new TeacherPortraitDto();
            dto.Student = new StudentPortraitDto();
            var teacherLogs = logs.Where(t => (t.Role & (byte)UserRole.Teacher) > 0).ToList();

            if (teacherLogs.Any())
            {
                dto.Teacher.AverageLogin = (float)Math.Round(teacherLogs.Count() / (float)teacherLogs.GroupBy(t => t.UserId).Count(), 2, MidpointRounding.AwayFromZero);
            }
            //平均批阅班次
            var markedList = UsageRepository.Where(t => t.AddedAt >= lastTime && t.MarkingStatus == (byte)MarkingStatus.AllFinished)
                .Join(users.Where(t => t.Role == (byte)UserRole.Teacher), m => m.UserId, u => u.UserId, (m, u) => m.UserId).ToList();

            if (markedList.Any())
            {
                dto.Teacher.AverageMarked = (float)Math.Round(markedList.Count() / (float)markedList.Distinct().Count(), 2, MidpointRounding.AwayFromZero);
            }
            var impressions = ImpressionRepository.Where(t => t.CreationTime >= lastTime)
                .Join(users.Where(t => t.Role == (byte)UserRole.Teacher), i => i.UserId, u => u.UserId, (i, u) => i.Impression)
                .GroupBy(t => t)
                .Select(t => new { key = t.Key, count = t.Count() })
                .OrderByDescending(t => t.count)
                .Take(6)
                .ToList();
            if (impressions.Any())
            {
                dto.Teacher.Portraits = impressions.ToDictionary(k => k.key, v => v.count);
            }

            var studentLogs = logs.Where(t => (t.Role & (byte)UserRole.Student) > 0).ToList();

            if (studentLogs.Any())
            {
                dto.Student.AverageLogin = (float)Math.Round(studentLogs.Count() / (float)studentLogs.GroupBy(t => t.UserId).Count(), 2, MidpointRounding.AwayFromZero);
            }
            //平均考试次数
            var scoreList = StuScoreStatisticsRepository.Where(t => t.AddedAt >= lastTime)
                .Join(users.Where(t => t.Role == (byte)UserRole.Student), s => s.StudentId, u => u.UserId,
                    (s, u) => s.StudentId);
            if (scoreList.Any())
            {
                dto.Student.AverageExam = (float)Math.Round(scoreList.Count() / (float)scoreList.Distinct().Count(), 2, MidpointRounding.AwayFromZero);
            }
            //平均新增错题
            var errorList = ErrorQuestionRepository.Where(t => t.AddedAt >= lastTime)
                .Join(users.Where(t => t.Role == (byte)UserRole.Student), e => e.StudentID, u => u.UserId, (e, u) => e.StudentID);
            if (errorList.Any())
            {
                dto.Student.AverageError = (float)Math.Round(errorList.Count() / (float)errorList.Distinct().Count(), 2, MidpointRounding.AwayFromZero);
            }
            //错因分析
            var reasonList = ErrorReasonRepository.Where(t => t.AddedAt >= lastTime && t.Tags != null && t.Tags != "")
                .Join(users.Where(t => t.Role == (byte)UserRole.Student), r => r.StudentId, u => u.UserId, (r, u) => r.Tags).ToList();
            if (reasonList.Any())
            {
                dto.Student.Portraits = reasonList.SelectMany(t => JsonHelper.JsonList<NameDto>(t))
                    .GroupBy(t => t.Name)
                    .Select(t => new { key = t.Key, count = t.Count() })
                    .OrderByDescending(t => t.count)
                    .Take(6)
                    .ToDictionary(k => k.key, v => v.count);
            }

            #region 排名
            //登录次数排名
            var loginRank = logs.GroupBy(t => t.UserId).Select(t => new AgencyUserRankingDto
            {
                Id = t.Key,
                Count = t.Count()
            })
            .OrderByDescending(t => t.Count)
            .Take(size)
            .ToList();

            //批阅班次
            var markingRank = markedList.GroupBy(t => t)
                .OrderByDescending(t => t.Count())
                .Take(size)
                .Select(t => new AgencyUserRankingDto
                {
                    Id = t.Key,
                    Count = t.Count()
                }).ToList();

            //访问次数
            var visitRank = VisitRepository.Where(t => t.VisitTime >= lastTime)
                .Join(users, v => v.UserId, u => u.UserId, (v, u) => v.UserId)
                .GroupBy(t => t)
                .OrderByDescending(t => t.Count())
                .Take(size)
                .Select(t => new AgencyUserRankingDto
                {
                    Id = t.Key,
                    Count = t.Count()
                }).ToList();

            var userIds = loginRank.Select(t => t.Id)
                .Union(visitRank.Select(t => t.Id))
                .Union(markingRank.Select(t => t.Id))
                .ToList();
            var userDict = UserContract.LoadListDictUser(userIds);

            loginRank.ForEach(t =>
            {
                t.Rank = loginRank.IndexOf(t) + 1;
                if (!userDict.ContainsKey(t.Id))
                    return;
                var user = userDict[t.Id];
                t.Name = user.Name;
                t.Avatar = user.Avatar;
            });
            visitRank.ForEach(t =>
            {
                t.Rank = visitRank.IndexOf(t) + 1;
                if (!userDict.ContainsKey(t.Id))
                    return;
                var user = userDict[t.Id];
                t.Name = user.Name;
                t.Avatar = user.Avatar;
            });
            markingRank.ForEach(t =>
            {
                t.Rank = markingRank.IndexOf(t) + 1;
                if (!userDict.ContainsKey(t.Id))
                    return;
                var user = userDict[t.Id];
                t.Name = user.Name;
                t.Avatar = user.Avatar;
            });
            dto.LoginRank = loginRank;
            dto.VisitRank = visitRank;
            dto.MarkingRank = markingRank;
            #endregion

            StatisticCacheHelper.Instance.SetByRefreshTime(key, dto, TimeSpan.FromMinutes(5));

            return dto;
        }
        #endregion

        #region 考试地图
        /// <summary> 学校考试地图 </summary>
        public AgencyExaminationMapDto AgencyExaminationMap(string agencyId, TimeArea timeArea = TimeArea.Week, bool refresh = false)
        {
            var dto = new AgencyExaminationMapDto();
            if (string.IsNullOrWhiteSpace(agencyId))
                return dto;
            var key = $"examination_{agencyId}_{timeArea}";
            if (!refresh)
            {
                var cacheDto = StatisticCacheHelper.Instance.Get<AgencyExaminationMapDto>(key);
                if (cacheDto != null)
                    return cacheDto;
            }
            var lastTime = GetTimeArea(timeArea);

            //考试情况
            var users = UserAgencyRepository.Where(
                a => a.AgencyId == agencyId && a.Status == (byte)UserAgencyStatus.Current)
                .Join(UserRepository.Where(u => u.Status != (byte)UserStatus.Delete), ua => ua.UserId, u => u.Id,
                    (ua, u) => ua);
            var examList = StuScoreStatisticsRepository.Where(t => t.AddedAt >= lastTime)
                .Join(users, s => s.StudentId, u => u.UserId, (s, u) => new { s.Batch, s.PaperId, s.SubjectId, s.ClassId }).ToList();
            if (examList.Any())
            {
                dto.UserCount = examList.Count();
                dto.ClassCount = examList.DistinctBy(t => t.Batch).Count();
                dto.PaperCount = examList.DistinctBy(t => t.PaperId).Count();
                var knowledges = PaperRepository.Table
                    .Join(examList.DistinctBy(t => t.PaperId).Select(t => t.PaperId), p => p.Id, s => s, (p, s) => p.KnowledgeIDs)
                    .ToList()
                    .Select(t => JsonHelper.Json<Dictionary<string, string>>(t ?? string.Empty));
                dto.KnowledgeCount = knowledges.Sum(t => t.Count());
                dto.ReportCount = dto.ClassCount;
                //协同考试
                var jointCount = UsageRepository.Table
                    .Join(examList.DistinctBy(t => t.Batch).Select(t => t.Batch), uc => uc.Id, s => s,
                        (uc, s) => uc.JointBatch)
                    .Distinct()
                    .Count(t => t != null);
                dto.ReportCount += jointCount;

                //大型考试
                var examCount =
                    ExaminationRepository.Count(
                        t =>
                            t.AgencyId == agencyId && t.CreationTime >= lastTime &&
                            (t.Status & (byte)ExamStatus.Sended) > 0);
                dto.ReportCount += examCount;
                //科目分布
                var subjects = examList.GroupBy(t => t.SubjectId).Select(t => new { id = t.Key, count = t.GroupBy(s => s.Batch).Count() }).ToList();
                dto.Subjects = subjects.OrderBy(t => t.id).ToDictionary(k => SystemCache.Instance.SubjectName(k.id), v => v.count);

                //班次分布
                var classList = examList.GroupBy(t => t.ClassId).Select(t => new { id = t.Key, count = t.Count() }).ToList().ToDictionary(k => k.id, v => v.count);
                var groups = GroupRepository.Table.Where(g => g.Status == (byte)NormalStatus.Normal && g.CertificationLevel == (byte)GroupCertificationLevel.Normal)
                    .Join(ClassRepository.Where(c => c.AgencyId == agencyId), g => g.Id, c => c.Id,
                    (g, c) => new { g.Id, c.Stage, c.GradeYear, g.GroupName })
                    .ToList()
                    .GroupBy(g => new { g.Stage, g.GradeYear })
                    .OrderBy(g => g.Key.Stage)
                    .ThenByDescending(g => g.Key.GradeYear)
                    .Select(g => new
                    {
                        grade = g.Key.Stage.GetEnumText<StageEnum, byte>() + (g.Key.GradeYear + (g.Key.Stage == 1 ? 6 : 3)) + "级",
                        groups = g.Select(t => new { t.Id, t.GroupName }).ToList()
                    }).ToList();
                dto.ClassList = new List<AgencyGroupServeyDto>();
                if (groups.Any())
                {
                    foreach (var grade in groups)
                    {
                        var item = new AgencyGroupServeyDto
                        {
                            Key = grade.grade,
                            Users = new List<Core.Domain.DKeyValue<string, int>>()
                        };
                        if (grade.groups.IsNullOrEmpty())
                            continue;
                        foreach (var group in grade.groups.OrderBy(t => t.GroupName.ClassIndex()))
                        {
                            var count = 0;
                            if (classList.ContainsKey(group.Id))
                                count = classList[group.Id];
                            item.Users.Add(new Core.Domain.DKeyValue<string, int>(group.GroupName, count));
                        }
                        dto.ClassList.Add(item);
                    }
                }
            }
            StatisticCacheHelper.Instance.SetByRefreshTime(key, dto, TimeSpan.FromMinutes(5));
            return dto;
        }
        #endregion

        #region 学情补救
        /// <summary> 学校学情补救 </summary>
        public AgencyRemedyDto AgencyRemedy(string agencyId, TimeArea timeArea = TimeArea.Week, bool refresh = false)
        {
            var dto = new AgencyRemedyDto();
            if (string.IsNullOrWhiteSpace(agencyId))
                return dto;
            var key = $"remedy_{agencyId}_{timeArea}";
            if (!refresh)
            {
                var cacheDto = StatisticCacheHelper.Instance.Get<AgencyRemedyDto>(key);
                if (cacheDto != null)
                    return cacheDto;
            }
            var lastTime = GetTimeArea(timeArea);
            //下载情况
            var types = new List<byte>
            {
                (byte) DownloadType.ErrorQuestion,
                (byte) DownloadType.Paper,
                (byte) DownloadType.Variant
            };
            var downloadDict = DownloadLogRepository.Where(
                t => t.AgencyId == agencyId && t.AddedAt >= lastTime && types.Contains(t.Type))
                .GroupBy(d => d.Type)
                .Select(d => new { type = d.Key, count = d.Sum(t => t.Count) })
                .ToList()
                .ToDictionary(k => k.type, v => v.count);

            if (downloadDict.ContainsKey((byte)DownloadType.Paper))
                dto.PaperDownload = downloadDict[(byte)DownloadType.Paper];
            if (downloadDict.ContainsKey((byte)DownloadType.Variant))
                dto.VariantDownload = downloadDict[(byte)DownloadType.Variant];

            //错题情况
            var users = UserAgencyRepository.Where(
                a => a.AgencyId == agencyId && a.Status == (byte)UserAgencyStatus.Current)
                .Join(UserRepository.Where(u => u.Status != (byte)UserStatus.Delete), ua => ua.UserId, u => u.Id,
                    (ua, u) => ua);
            var errors = ErrorQuestionRepository.Where(t => t.AddedAt >= lastTime)
                .Join(users, e => e.StudentID, u => u.UserId, (e, u) => new { e.QuestionID, e.SubjectID, e.Status, e.Id });
            if (!errors.Any())
                return dto;
            dto.Errors = errors.Count();
            //查询慢
            var knowledges = errors.Join(QuestionRepository.Table, e => e.QuestionID, q => q.Id, (e, q) => (q.KnowledgeIDs.Length - q.KnowledgeIDs.Replace(",", "").Length + 1));
            if (knowledges.Any())
                dto.ErrorKnowledge = knowledges.Sum();

            dto.ErrorDetail = errors.GroupBy(t => t.SubjectID).Select(t => new { id = t.Key, count = t.Count() }).OrderBy(t => t.id)
                .ToList()
                .ToDictionary(k => SystemCache.Instance.SubjectName(k.id), v => v.count);

            dto.ErrorAnalysis = new AgencyRemedyItemDto
            {
                Total = dto.Errors,
                Count = errors.Count(t => (t.Status & (byte)ErrorQuestionStatus.Marked) > 0)
            };
            dto.ErrorDownload = new AgencyRemedyItemDto
            {
                Total = dto.Errors,
                Count = 0
            };
            if (downloadDict.ContainsKey((byte)DownloadType.ErrorQuestion))
                dto.ErrorDownload.Count = downloadDict[(byte)DownloadType.ErrorQuestion];
            dto.ErrorPass = new AgencyRemedyItemDto
            {
                Total = dto.Errors,
                Count = errors.Count(t => (t.Status & (byte)ErrorQuestionStatus.Pass) > 0)
            };
            dto.Variants = dto.Errors;

            var variants = VariantRepository.Where(t => t.AddedAt >= lastTime)
                .Join(users.Where(u => (u.Role & (byte)UserRole.Teacher) > 0), v => v.AddedBy, u => u.UserId, (v, u) => (v.VIDs.Length - v.VIDs.Replace(",", "").Length + 1));

            if (variants.Any())
            {
                dto.Variants += variants.Sum();
            }
            dto.Tutors = TutorRecordRepository.Where(t => t.AddedAt >= lastTime)
                .Join(users, t => t.UserId, u => u.UserId, (t, u) => t.Id).Count();

            //科目错因分析
            dto.SubjectAnalysis = new List<AgencyGroupServeyDto>();
            var reasons = ErrorReasonRepository.Where(t => t.AddedAt >= lastTime && t.Tags != null && t.Tags != "")
                .Join(errors, r => r.ErrorId, e => e.Id, (r, e) => new { r.Tags, e.SubjectID })
                .ToList();
            var subjects = errors.GroupBy(t => t.SubjectID).Select(t => t.Key).OrderBy(t => t).ToList();
            foreach (var subject in subjects)
            {
                var item = new AgencyGroupServeyDto
                {
                    Id = subject,
                    Key = SystemCache.Instance.SubjectName(subject),
                    Users = new List<DKeyValue<string, int>>()
                };
                var reason = reasons.Where(t => t.SubjectID == subject);
                if (reason.Any())
                {
                    item.Users = reason.SelectMany(t => JsonHelper.JsonList<NameDto>(t.Tags))
                        .GroupBy(t => t.Name)
                        .OrderByDescending(t => t.Count())
                        .Take(5)
                        .Select(t => new DKeyValue<string, int>(t.Key, t.Count()))
                        .ToList();
                }
                dto.SubjectAnalysis.Add(item);
            }

            StatisticCacheHelper.Instance.SetByRefreshTime(key, dto, TimeSpan.FromMinutes(5));
            return dto;
        }
        #endregion
    }
}
