using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace DayEasy.AsyncMission.Jobs.JobTasks
{
    /// <summary> 协同阅卷任务相关逻辑 </summary>
    public partial class FinishMarkingTask
    {
        #region SQL

        /// <summary> 更新题目统计 </summary>
        private const string JointQuestionStatisticSql = @"
                IF Object_id('Tempdb..#batches') IS NOT NULL
                DROP TABLE #batches
                select Batch into #batches from [TC_Usage] where JointBatch=@batch
                update tq
                set tq.AnswerCount=tq.AnswerCount+t.answer,tq.ErrorCount=tq.ErrorCount+t.error
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
                update tss set tss.FinishPaperCount=tss.FinishPaperCount+1,tss.FinishQuestionCount=tss.FinishQuestionCount+t.answer,tss.ErrorQuestionCount=tss.ErrorQuestionCount+t.error
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

        /// <summary> 生成协同阅卷错题库 </summary>
        private const string JointErrorQuestionSql = @"
                IF Object_id('Tempdb..#batches') IS NOT NULL
                DROP TABLE #batches
                select Batch into #batches from [TC_Usage] where JointBatch=@batch
                delete [TP_ErrorQuestion] where Batch in (select * from #batches)
                insert into [TP_ErrorQuestion] (ErrorQID,PaperID,Batch,QuestionID,StudentID,PaperTitle,SubjectID,Stage,QType,AddedAt,[Status],[VariantCount],Importance,SourceType)  
	                select
		                lower(replace(newid(),'-','')) as id,
		                p.PaperID,
		                error.Batch,
		                error.QuestionID,
		                error.StudentID,
		                p.PaperTitle,
		                p.SubjectID,
		                p.Stage,
		                q.QType,
		                getdate() as AddedAt,
		                cast(0 as tinyint) as [Status],
	                    0 as [VariantCount],
	                    0 as Importance,
	                    cast(0 as tinyint) as SourceType
	                from (
		                select 
			                md.StudentID,
			                md.QuestionID,
			                md.PaperID,
			                md.Batch
		                from [TP_MarkingDetail] as md
		                where md.Batch in (select * from #batches)
		                and isnull(md.IsCorrect,0)=0
		                group by md.StudentID,md.QuestionID,md.PaperID,md.Batch
	                ) as error
	                inner join [TP_Paper] as p on p.PaperID=error.paperId
	                inner join [TQ_Question] as q on q.QID=error.QuestionID
                DROP TABLE #batches";

        #endregion

        /// <summary>
        /// 完成协同阅卷 
        /// 1、客观题错题、默认标记 & 分数标记; 
        /// 2、更新题目答题统计;
        /// 3、生成错题库;
        /// 4、更新知识点统计; 
        /// 5、发送完成阅卷消息。
        /// </summary>
        private DResult JointFinish()
        {
            var jointRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>();
            var model = jointRepository.Load(Param.Batch);
            if (model == null || model.Status != (byte)JointStatus.Finished)
                return DResult.Error("协同状态不匹配");
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            var pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var usages = usageRepository.Where(t => t.JointBatch == Param.Batch).ToList();
            if (!usages.Any())
                return DResult.Error("该协同还没有任何发布记录");
            var paperResult = paperContract.PaperDetailById(model.PaperId);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error("发布试卷不存在");
            var watcher = new Stopwatch();
            watcher.Start();
            var paper = paperResult.Data;
            var dict = usages.ToDictionary(k => k.Id, v => v.ClassId);
            var kpStatistics = InitKpStatistics(paper, dict);
            watcher.Stop();
            LogAction($"初始化知识点统计，耗时：{watcher.ElapsedMilliseconds}ms");
            watcher.Restart();
            var batches = dict.Keys;
            //客观题错题、默认标记 & 分数标记
            var pictures = pictureRepository.Where(t => batches.Contains(t.BatchNo)).ToList();
            var updatePictures = new List<TP_MarkingPicture>();
            if (pictures.Any())
            {
                //图片标记处理
                updatePictures = UpdatePictures(paper, pictures);
                watcher.Stop();
                LogAction($"图片标记处理,{updatePictures.Count},耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
            }
            var result = detailRepository.UnitOfWork.Transaction(unitWork =>
            {
                if (updatePictures.Any())
                {
                    var paramList = new List<string>
                    {
                        nameof(TP_MarkingPicture.ObjectiveError),
                        nameof(TP_MarkingPicture.ObjectiveScore)
                    };
                    if (Param.SetIcon)
                        paramList.Add(nameof(TP_MarkingPicture.RightAndWrong));
                    if (Param.SetMarks)
                        paramList.Add(nameof(TP_MarkingPicture.Marks));
                    updatePictures.ForEach(p =>
                    {
                        pictureRepository.Update(p, paramList.ToArray());
                    });
                }
                //更新题目答题统计
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, JointQuestionStatisticSql,
                    new SqlParameter("@batch", Param.Batch));
                watcher.Stop();
                LogAction($"更新题目答题统计，耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
                //更新学生答题统计
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, JointStudentStatisticSql,
                    new SqlParameter("@batch", Param.Batch));
                watcher.Stop();
                LogAction($"更新学生答题统计，耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
                //生成错题库
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, JointErrorQuestionSql,
                    new SqlParameter("@batch", Param.Batch));
                watcher.Stop();
                LogAction($"生成错题库，耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
                //知识点统计
                if (kpStatistics == null)
                    return;
                var teacherKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_TeacherKpStatistic>>();
                var studentKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StudentKpStatistic>>();

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
                watcher.Stop();
                LogAction($"知识点统计，耗时：{watcher.ElapsedMilliseconds}ms");
            });
            if (result <= 0)
                return DResult.Error("提交数据异常");
            watcher.Restart();
            //班级圈动态消息
            var messages = usages.Select(t => new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Exam,
                ContentType = (byte)ContentType.Publish,
                ContentId = t.Id,
                GroupId = t.ClassId,
                ReceivRole = (UserRole.Student | UserRole.Teacher),
                UserId = t.UserId
            }).ToList();
            CurrentIocManager.Resolve<IMessageContract>().SendDynamics(messages);
            watcher.Stop();
            LogAction($"发送动态消息，耗时：{watcher.ElapsedMilliseconds}ms");
            return DResult.Success;
        }
    }
}
