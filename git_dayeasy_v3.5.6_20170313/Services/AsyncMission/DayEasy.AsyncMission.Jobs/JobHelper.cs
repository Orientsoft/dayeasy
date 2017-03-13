using DayEasy.AsyncMission.Models;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Cache;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using Shoy.Backgrounder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.AsyncMission.Jobs
{
    internal enum ManagerStatus
    {
        Sleep,
        Running,
        Dispose
    }

    internal class JobManagerItem
    {
        public JobManager Manager { get; set; }
        public ManagerStatus Status { get; set; }
    }
    public static class JobHelper
    {
        private const string AreaCacheName = "joint_area_{0}";
        private const string CacheRegion = "marking";
        private static readonly ILogger Logger = LogManager.Logger(typeof(JobHelper));
        private static ConcurrentDictionary<MissionType, JobManagerItem> _jobManagers;

        internal static void AddManager(MissionType type, JobManager manager)
        {
            if (_jobManagers == null)
                _jobManagers = new ConcurrentDictionary<MissionType, JobManagerItem>();
            JobManagerItem managerItem;
            if (_jobManagers.ContainsKey(type))
                _jobManagers.TryRemove(type, out managerItem);
            managerItem = new JobManagerItem
            {
                Manager = manager,
                Status = ManagerStatus.Sleep
            };
            _jobManagers.TryAdd(type, managerItem);
        }

        internal static ConcurrentDictionary<MissionType, JobManagerItem> Managers()
        {
            return _jobManagers;
        }

        internal static void UpdateManager(MissionType type, ManagerStatus status)
        {
            if (!_jobManagers.ContainsKey(type))
                return;
            var manager = _jobManagers[type];
            if (status == ManagerStatus.Sleep)
                manager.Manager.Stop();
            if (status == ManagerStatus.Running)
                manager.Manager.Start();
            if (status == ManagerStatus.Dispose)
                manager.Manager.Dispose();
            manager.Status = status;
        }

        internal static double Interval(this MissionType type)
        {
            var jobs = ConfigUtils<JobsConfig>.Instance.Get();
            if (jobs == null || jobs.Jobs == null)
                return 30D;
            var job = jobs.Jobs.FirstOrDefault(t => t.Type == type);
            if (job == null || job.Interval <= 0)
                return 30D;
            return job.Interval;
        }

        internal static JobManager CreateManager(this IJob job, bool restartOnFail = true)
        {
            return CreateManager(new[] { job }, restartOnFail);
        }

        internal static JobManager CreateManager(this IEnumerable<IJob> jobs, bool restartOnFail = true)
        {
            //初始化后台任务
            var manager = new JobManager(jobs, s => Logger.Debug(s));
            //异常处理
            manager.Fail(e => { Logger.Error(e.Message, e); });
            //异常后是否重启
            manager.RestartSchedulerOnFailure = restartOnFail;
            return manager;
        }

        /// <summary> 学生答案 </summary>
        public static void StudentAnswers(this TP_MarkingDetail detail, int[] sheet, IEnumerable<AnswerDto> answers)
        {
            if (sheet == null || sheet.Any(t => t < 0 || t > 25))
                return;
            detail.AnswerContent = sheet.Aggregate(string.Empty, (c, t) => c + Consts.OptionWords[t]);
            var answerIds = answers.OrderBy(t => t.Sort).Select(t => t.Id).ToList();
            var studentAnswers =
                sheet.Where(t => t < answerIds.Count).Select(t => answerIds[t]).ToList();
            detail.AnswerIDs = studentAnswers.ToJson();
        }

        /// <summary> 自动批阅 </summary>
        public static void AutoMarking(this TP_MarkingDetail detail, ICollection<string> rightAnswers, bool halfScore)
        {
            if (string.IsNullOrWhiteSpace(detail.AnswerIDs) || rightAnswers == null || !rightAnswers.Any())
                return;
            var studentAnswers = detail.AnswerIDs.JsonToObject<List<string>>();
            if (studentAnswers == null || !studentAnswers.Any())
                return;
            var correct = studentAnswers.ArrayEquals(rightAnswers);
            if (correct)
            {
                detail.CurrentScore = detail.Score;
            }
            else
            {
                if (halfScore && studentAnswers.All(rightAnswers.Contains))
                    detail.CurrentScore = detail.Score / 2M;
                else
                    detail.CurrentScore = 0;
            }
            detail.IsCorrect = correct;
            detail.IsFinished = true;
            detail.MarkingBy = 0;
            detail.MarkingAt = Clock.Now;
        }

        public static Dictionary<string, MkQuestionAreaDto> QuestionArea(this string batch)
        {
            var key = AreaCacheName.FormatWith(batch);
            var cache = CacheManager.GetCacher(CacheRegion);
            var dict = cache.Get<Dictionary<string, MkQuestionAreaDto>>(key);
            if (dict != null)
                return dict;
            dict = new Dictionary<string, MkQuestionAreaDto>();
            var markRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingMark>>();
            var marks = markRepository.Where(t => t.BatchNo == batch).Select(t => t.Mark).ToList();
            if (!marks.Any())
                return dict;
            foreach (var mark in marks)
            {
                if (string.IsNullOrWhiteSpace(mark))
                    continue;
                var list = JsonHelper.JsonList<MkQuestionAreaDto>(mark);
                foreach (var dto in list)
                {
                    if (!dict.ContainsKey(dto.Id))
                        dict.Add(dto.Id, dto);
                }
            }
            cache.Set(key, dict, TimeSpan.FromDays(3));
            return dict;
        }
    }
}
