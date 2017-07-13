
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Marking.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Timing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.Marking.Services
{
    /// <summary> 完成阅卷 </summary>
    public partial class MarkingService
    {
        #region 完成阅卷业务逻辑

        /// <summary>
        /// 完成阅卷业务逻辑
        /// 1、更新状态；
        /// 2、生成错题；
        /// 3、更新题目统计；
        /// 4、更新知识点统计；
        /// 5、发送完成阅卷消息。
        /// </summary>
        private DResult FinishedMarking(TC_Usage usage, long teacherId, MarkingStatus status)
        {
            if (usage == null)
                return DResult.Error(MarkingConsts.MsgBatchNotFind);

            usage.MarkingStatus += (byte)status;
            if (usage.MarkingStatus != (byte)MarkingStatus.AllFinished)
                return DResult.Error(MarkingConsts.MsgMarkingNotFinished);

            //1、更新状态
            var sw = new Stopwatch();
            sw.Start();

            var result = FinishMarking(usage, teacherId, (byte)MarkingStatus.AllFinished);

            sw.Stop();
            _logger.Info("完成阅卷耗时：" + sw.Elapsed.TotalMilliseconds);

            if (result <= 0)
                return DResult.Error(MarkingConsts.MsgCommitError);

            //启动完成阅卷后台任务
            Task.Run(() => MarkingTask.FinishMission(usage.Id, usage.SourceID, usage.ClassId, usage.UserId, usage.SubjectId));

            return MarkingConsts.Success;
        }

        /// <summary>
        /// 完成阅卷任务
        /// 1、更新阅卷状态
        /// 2、生成统计数据：个人、班级
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="isJoint"></param>
        /// <returns></returns>
        private DResult CompleteMarking(string batch, bool isJoint)
        {
            if (isJoint)
                return CompleteJointMarking(batch);
            var model = UsageRepository.Load(batch);
            if (model == null || model.MarkingStatus == (byte)MarkingStatus.AllFinished)
                return DResult.Error("批阅状态异常");
            //计算Result，总分/错题数/模块得分
            var results = CalcResults(model.SourceID, batch, model.UserId);
            //处理报表统计数据
            var scoreStatistics = ReportStatisticsMission(model, results);
            var result = UnitOfWork.Transaction(unitWork =>
            {
                //更新发布记录状态
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction,
                    "update TC_Usage set MarkingStatus=@status  where Batch=@batch",
                    new SqlParameter("@status", (byte)MarkingStatus.AllFinished), new SqlParameter("@batch", batch));
                //更新Details
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction,
                    "update TP_MarkingDetail set IsFinished=1 where Batch=@batch and PaperID=@paperId and IsFinished=0",
                    new SqlParameter("@batch", batch), new SqlParameter("@paperId", model.SourceID));
                //更新
                if (results != null && results.Count > 0)
                {
                    MarkingResultRepository.Update(t => new
                    {
                        t.ErrorQuestionCount,
                        t.TotalScore,
                        t.SectionScores,
                        t.IsFinished,
                        t.MarkingBy,
                        t.MarkingTime
                    }, results.ToArray());
                }
                //报表数据插入
                if (scoreStatistics == null)
                    return;
                if (scoreStatistics.ClassScoreStatisticses != null)
                {
                    ClassScoreStatisticsRepository.Insert(scoreStatistics.ClassScoreStatisticses);
                }
                if (scoreStatistics.StuScoreStatisticses.Count > 0)
                {
                    StuScoreStatisticsRepository.Insert(scoreStatistics.StuScoreStatisticses);
                }
            });
            if (result > 0)
            {
                //:todo 
            }
            return DResult.FromResult(result);
        }

        /// <summary>
        /// 完成协同阅卷任务
        /// 1、更新阅卷状态
        /// 2、生成统计数据：个人、班级
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        private DResult CompleteJointMarking(string batch)
        {
            var model = JointMarkingRepository.Load(batch);
            if (model == null || model.Status != (byte)JointStatus.Normal)
                return DResult.Error("协同批阅状态异常");
            var usages = UsageRepository.Where(t => t.JointBatch == batch);
            if (!usages.Any())
                return DResult.Error("该协同没有任何发布记录");
            var updateResults = new List<TP_MarkingResult>();
            var classStatisticList = new List<TS_ClassScoreStatistics>();
            var studentStatisticList = new List<TS_StuScoreStatistics>();
            foreach (var usage in usages)
            {
                //计算Result，总分/错题数/模块得分
                var results = CalcResults(usage.SourceID, usage.Id, model.AddedBy);
                if (results == null || results.Count <= 0)
                    continue;
                updateResults.AddRange(results);
                //处理报表统计数据
                var scoreStatistics = ReportStatisticsMission(usage, results);
                if (scoreStatistics == null)
                    continue;
                if (scoreStatistics.ClassScoreStatisticses != null)
                {
                    classStatisticList.Add(scoreStatistics.ClassScoreStatisticses);
                }
                if (scoreStatistics.StuScoreStatisticses != null && scoreStatistics.StuScoreStatisticses.Count > 0)
                {
                    studentStatisticList.AddRange(scoreStatistics.StuScoreStatisticses);
                }
            }
            var result = UnitOfWork.Transaction(unitWork =>
            {
                //更新发布记录状态
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction,
                    "update [TC_Usage] set MarkingStatus=@status where JointBatch=@batch",
                    new SqlParameter("@status", (byte)MarkingStatus.AllFinished), new SqlParameter("@batch", batch));
                //更新Details
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction,
                    "update [TP_MarkingDetail] set IsFinished=1 where Batch in (select uc.Batch from [TC_Usage] as uc where uc.JointBatch=@batch) and PaperID=@paperId and IsFinished=0",
                    new SqlParameter("@batch", batch), new SqlParameter("@paperId", model.PaperId));
                //更新
                if (updateResults.Any())
                {
                    MarkingResultRepository.Update(t => new
                    {
                        t.ErrorQuestionCount,
                        t.TotalScore,
                        t.SectionScores,
                        t.IsFinished,
                        t.MarkingBy,
                        t.MarkingTime
                    }, updateResults.ToArray());
                }
                //报表数据插入
                if (classStatisticList.Any())
                {
                    ClassScoreStatisticsRepository.Insert(classStatisticList);
                }
                if (studentStatisticList.Any())
                {
                    StuScoreStatisticsRepository.Insert(studentStatisticList);
                }
                model.Status = (byte)JointStatus.Finished;
                model.FinishedTime = Clock.Now;
                JointMarkingRepository.Update(t => new { t.Status, t.FinishedTime }, model);
            });
            if (result > 0)
            {
                //:todo 
            }
            return DResult.FromResult(result);
        }

        #endregion
    }
}
