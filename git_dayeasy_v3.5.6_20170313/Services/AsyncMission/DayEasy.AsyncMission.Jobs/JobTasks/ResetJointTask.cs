using DayEasy.AsyncMission.Models;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Timing;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace DayEasy.AsyncMission.Jobs.JobTasks
{
    /// <summary> 重置协同阅卷 </summary>
    internal class ResetJointTask : DTask<ResetJointParam>
    {
        private const string JointQuestionStatisticSql = @"
                IF Object_id('Tempdb..#batches') IS NOT NULL
                DROP TABLE #batches
                select Batch into #batches from [TC_Usage] where JointBatch=@batch
                update tq
                set tq.AnswerCount=tq.AnswerCount-t.answer,tq.ErrorCount=tq.ErrorCount-t.error
                from [TQ_Question] as tq
	            inner join (
	                select a.QuestionID as id,isnull(a.answer,0) as answer,isnull(b.error,0) as error
	                from (
		                select md.QuestionID,count(distinct md.StudentID) as answer
		                from [TP_MarkingDetail] as md
		                where md.Batch in (select * from #batches)
		                group by md.QuestionID
	                ) as a 
	                left join (
		                select md.QuestionID,count(distinct md.StudentID) as error
		                from [TP_MarkingDetail] as md
		                where md.Batch in (select * from #batches)
		                and isnull(md.IsCorrect,0)=0
		                group by md.QuestionID
	                ) as b on b.QuestionID=a.QuestionID
                ) as t on t.id=tq.QID
                DROP TABLE #batches";
        /// <summary> 更新学生答题统计 </summary>
        private const string JointStudentStatisticSql = @"
                IF Object_id('Tempdb..#batches') IS NOT NULL
                DROP TABLE #batches
                select Batch into #batches from [TC_Usage] where JointBatch=@batch
                update tss set tss.FinishPaperCount=tss.FinishPaperCount-1,tss.FinishQuestionCount=tss.FinishQuestionCount-t.answer,tss.ErrorQuestionCount=tss.ErrorQuestionCount-t.error
                from [TS_StudentStatistic] as tss
                inner join (
	                select a.StudentID,isnull(a.answer,0) as answer,isnull(b.error,0) as error
	                from (
		                select md.StudentID,count(distinct md.QuestionID) as answer from [TP_MarkingDetail] as md
		                where md.Batch in (select * from #batches)
		                group by md.StudentID
	                ) as a
	                left join (
		                select md.StudentID,count(distinct md.QuestionID) as error from [TP_MarkingDetail] as md
		                where md.Batch in (select * from #batches)
		                and isnull(md.IsCorrect,0)=0
		                group by md.StudentID
	                ) as b on b.StudentID=a.StudentID
                ) as t on t.StudentID=tss.UserID
                DROP TABLE #batches";

        public ResetJointTask(ResetJointParam param, Action<string> logAction = null)
            : base(param, logAction)
        {
        }

        public override DResult Execute()
        {
            if (Param == null || string.IsNullOrWhiteSpace(Param.JointBatch))
                return DResult.Error("任务参数异常");
            var jointRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>();
            var jointModel =
                jointRepository.Where(t => t.Id == Param.JointBatch)
                    .Select(t => new { t.Status, t.FinishedTime })
                    .FirstOrDefault();
            //只能还原10天内的协同
            var lastTime = Clock.Now.AddDays(-10);
            if (jointModel == null || jointModel.Status != (byte)JointStatus.Finished)
                return DResult.Error("协同状态无效");
            if (jointModel.FinishedTime < lastTime)
                return DResult.Error("协同已超过可还原的日期(10天内)");
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var list =
                usageRepository.Where(t => t.JointBatch == Param.JointBatch)
                    .Select(t => new { t.Id, t.ClassId, t.SubjectId })
                    .ToList();
            if (!list.Any())
                return DResult.Error("该协同没有任何考试信息");
            var batches = list.Select(t => t.Id).ToList();
            var classIds = list.Select(t => t.ClassId).ToList();
            var subjectId = list.Select(t => t.SubjectId).First();
            if (subjectId <= 0)
                return DResult.Error("协同科目异常");
            var stuScoreRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StuScoreStatistics>>();
            var classScoreRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_ClassScoreStatistics>>();
            var errorRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>();
            var resultRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>();
            var variantRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_VariantQuestion>>();
            var studentKpRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StudentKpStatistic>>();
            var teacherKpRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_TeacherKpStatistic>>();
            var pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();


            var result = jointRepository.UnitOfWork.Transaction(unitWork =>
            {
                //重置题目答题统计
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, JointQuestionStatisticSql,
                    new SqlParameter("@batch", Param.JointBatch));
                //重置学生答题统计
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, JointStudentStatisticSql,
                    new SqlParameter("@batch", Param.JointBatch));
                //学生成绩
                stuScoreRepository.Delete(t => batches.Contains(t.Batch));
                //班级成绩
                classScoreRepository.Delete(t => batches.Contains(t.Batch));
                //变式
                var errorIds = errorRepository.Where(t => batches.Contains(t.Batch)).Select(t => t.Id).ToList();
                variantRepository.Delete(t => errorIds.Contains(t.ErrorQID));
                //错题库
                errorRepository.Delete(t => errorIds.Contains(t.Id));

                //时间
                var startTime = jointModel.FinishedTime.Value.AddMinutes(2);
                var endTime = jointModel.FinishedTime.Value;

                studentKpRepository.Delete(
                    t =>
                        t.SubjectID == subjectId && t.EndTime > startTime && t.EndTime < endTime);

                teacherKpRepository.Delete(
                    t =>
                        classIds.Contains(t.ClassID) && t.SubjectID == subjectId && t.EndTime > startTime &&
                        t.EndTime < endTime);

                usageRepository.Update(new TC_Usage
                {
                    MarkingStatus = (int)MarkingStatus.NotMarking
                }, t => batches.Contains(t.Id), "MarkingStatus");

                resultRepository.Update(new TP_MarkingResult
                {
                    IsFinished = false,
                    TotalScore = 0,
                    ErrorQuestionCount = 0,
                    SectionScores = null
                }, t => batches.Contains(t.Batch), "IsFinished", "TotalScore", "ErrorQuestionCount", "SectionScores");

                jointRepository.Update(new TP_JointMarking
                {
                    Status = (byte)JointStatus.Normal,
                    FinishedTime = null
                }, t => t.Id == Param.JointBatch, "Status", "FinishedTime");

                pictureRepository.Update(new TP_MarkingPicture
                {
                    Marks = null,
                    ObjectiveError = null,
                    ObjectiveScore = null
                }, t => batches.Contains(t.BatchNo), "Marks", "ObjectiveError", "ObjectiveScore");
            });
            if (result > 0)
            {
                //删除圈子动态
                CurrentIocManager.Resolve<IVersion3Repository<TM_GroupDynamic>>()
                    .Delete(t => batches.Contains(t.ContentId));
            }
            return DResult.Success;
        }
    }
}
