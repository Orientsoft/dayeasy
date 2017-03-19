using DayEasy.AsyncMission.Jobs.JobTasks;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.MigrateTools.Migrate
{
    public class StatisticMigrate : MigrateBase
    {
        public void KpStatistic()
        {
            var batches = TxtFileHelper.Batches();
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var jointRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>();
            var resultRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>();
            var teacherKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_TeacherKpStatistic>>();
            var studentKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StudentKpStatistic>>();
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            foreach (var batch in batches)
            {
                Console.WriteLine($"开始：{batch}");
                var usage =
                    usageRepository.Where(t => t.Id == batch && t.MarkingStatus == (byte)MarkingStatus.AllFinished)
                        .Select(t => new { t.Id, t.SourceID, t.ClassId, t.AddedAt, t.JointBatch })
                        .FirstOrDefault();
                if (usage == null)
                    continue;
                DateTime time;
                if (string.IsNullOrWhiteSpace(usage.JointBatch))
                {
                    time =
                        resultRepository.Where(t => t.Batch == batch && t.MarkingTime.HasValue)
                            .Select(t => t.MarkingTime.Value).OrderByDescending(t => t).FirstOrDefault();
                }
                else
                {
                    time =
                        jointRepository.Where(t => t.Id == usage.JointBatch && t.FinishedTime.HasValue)
                            .Select(t => t.FinishedTime.Value)
                            .OrderByDescending(t => t)
                            .FirstOrDefault();
                }
                var paper = paperContract.PaperDetailById(usage.SourceID).Data;
                var kpStatistics = FinishMarkingTask.InitKpStatistics(paper,
                    new Dictionary<string, string> { { usage.Id, usage.ClassId } }, time);
                if (kpStatistics == null)
                    continue;
                if (kpStatistics.AddTeacherKpStatistic != null && kpStatistics.AddTeacherKpStatistic.Count > 0)
                {
                    teacherKpStatisticRepository.Insert(kpStatistics.AddTeacherKpStatistic);
                }
                if (kpStatistics.UpdateTeacherKpStatistic != null && kpStatistics.UpdateTeacherKpStatistic.Count > 0)
                {
                    teacherKpStatisticRepository.Update(s => new
                    {
                        s.AnswerCount,
                        s.ErrorCount
                    }, kpStatistics.UpdateTeacherKpStatistic.ToArray());
                }

                if (kpStatistics.AddStudentKpStatistic != null && kpStatistics.AddStudentKpStatistic.Count > 0)
                {
                    studentKpStatisticRepository.Insert(kpStatistics.AddStudentKpStatistic);
                }
                if (kpStatistics.UpdateStudentKpStatistic != null && kpStatistics.UpdateStudentKpStatistic.Count > 0)
                {
                    studentKpStatisticRepository.Update(s => new
                    {
                        s.AnswerCount,
                        s.ErrorCount
                    }, kpStatistics.UpdateStudentKpStatistic.ToArray());
                }
                Console.WriteLine($"完成:{batch}");
            }
            Console.WriteLine("完成");
        }
    }
}
