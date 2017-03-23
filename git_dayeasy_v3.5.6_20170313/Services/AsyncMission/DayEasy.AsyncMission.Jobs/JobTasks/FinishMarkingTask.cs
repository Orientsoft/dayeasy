using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.AsyncMission.Jobs.JobTasks
{
    /// <summary>  完成阅卷任务  </summary>
    public partial class FinishMarkingTask : DTask<FinishMarkingParam>
    {

        #region SQL

        /// <summary> 更新题目统计 </summary>
        private const string QuestionStatisticSql = @"update tq
                set tq.AnswerCount=tq.AnswerCount+t.answer,tq.ErrorCount=tq.ErrorCount+t.error
                from [TQ_Question] as tq
	            inner join (
	                select a.QuestionID as id,isnull(a.answer,0) as answer,isnull(b.error,0) as error
	                from (
		                select md.QuestionID,count(distinct md.StudentID) as answer
		                from [TP_MarkingDetail] as md
		                where md.Batch = @batch
		                group by md.QuestionID
	                ) as a 
	                left join (
		                select md.QuestionID,count(distinct md.StudentID) as error
		                from [TP_MarkingDetail] as md
		                where md.Batch = @batch
		                and isnull(md.IsCorrect,0)=0
		                group by md.QuestionID
	                ) as b on b.QuestionID=a.QuestionID
                ) as t on t.id=tq.QID";

        private const string StudentStatisticSql = @"
                update tss set tss.FinishPaperCount=tss.FinishPaperCount+1,tss.FinishQuestionCount=tss.FinishQuestionCount+t.answer,tss.ErrorQuestionCount=tss.ErrorQuestionCount+t.error
                from [TS_StudentStatistic] as tss
                inner join (
	                select a.StudentID,isnull(a.answer,0) as answer,isnull(b.error,0) as error
	                from (
		                select md.StudentID,count(distinct md.QuestionID) as answer from [TP_MarkingDetail] as md
		                where md.Batch = @batch
		                group by md.StudentID
	                ) as a
	                left join (
		                select md.StudentID,count(distinct md.QuestionID) as error from [TP_MarkingDetail] as md
		                where md.Batch = @batch
		                and isnull(md.IsCorrect,0)=0
		                group by md.StudentID
	                ) as b on b.StudentID=a.StudentID
                ) as t on t.StudentID=tss.UserID";

        /// <summary> 生成错题库 </summary>
        private const string ErrorQuestionSql = @"
                delete [TP_ErrorQuestion] where Batch=@batch;
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
		                where md.Batch=@batch
		                and isnull(md.IsCorrect,0)=0
		                group by md.StudentID,md.QuestionID,md.PaperID,md.Batch
	                ) as error
	                inner join [TP_Paper] as p on p.PaperID=error.paperId
	                inner join [TQ_Question] as q on q.QID=error.QuestionID";

        private const string AnswerShareSql = @"
                delete tas
                from [TP_AnswerShare] as tas
                where tas.Batch = @batch
                and exists (
	                select md.DetailId 
	                from [TP_MarkingDetail] as md 
	                where md.Batch=tas.Batch 
	                and md.QuestionID=tas.QuestionId 
	                and isnull(md.IsCorrect,0)=0
	                and md.StudentID=tas.AddedBy
                );
                update tas
                set tas.[Status]=0
                from [TP_AnswerShare] as tas
                where tas.Batch = @batch
                and not exists (
	                select md.DetailId 
	                from [TP_MarkingDetail] as md 
	                where md.Batch=tas.Batch 
	                and md.QuestionID=tas.QuestionId 
	                and isnull(md.IsCorrect,0)=0
	                and md.StudentID=tas.AddedBy
                );";

        #endregion

        public FinishMarkingTask(FinishMarkingParam param, Action<string> logAction = null)
            : base(param, logAction)
        {
        }

        public override DResult Execute()
        {
            if (Param == null || string.IsNullOrWhiteSpace(Param.Batch))
                return DResult.Error("任务参数异常");
            return Param.IsJoint ? JointFinish() : Finish();
        }

        /// <summary>
        /// 完成普通阅卷
        /// 1、客观题错题、默认标记 & 分数标记; 
        /// 2、更新题目答题统计、学生答题统计;
        /// 3、生成错题库;
        /// 4、更新知识点统计;
        /// 5、分享答案;
        /// 6、发送完成阅卷消息。
        /// </summary>
        private DResult Finish()
        {
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            var pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();

            var model = usageRepository.Load(Param.Batch);
            if (model == null || model.MarkingStatus != (byte)MarkingStatus.AllFinished)
                return DResult.Error("发布批次状态异常");
            var paperResult = paperContract.PaperDetailById(model.SourceID);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error("发布试卷不存在");
            var paper = paperResult.Data;
            var watcher = new Stopwatch();
            watcher.Start();
            var kpStatistics = InitKpStatistics(paper, new Dictionary<string, string> { { model.Id, model.ClassId } });
            watcher.Stop();
            LogAction($"初始化知识点统计，耗时：{watcher.ElapsedMilliseconds}ms");
            watcher.Restart();

            var pictures = pictureRepository.Where(t => t.BatchNo == Param.Batch).ToList();

            //图片标记处理
            var updatePictures = UpdatePictures(paper, pictures);

            watcher.Stop();
            LogAction($"图片标记处理,{updatePictures.Count},耗时：{watcher.ElapsedMilliseconds}ms");
            watcher.Restart();

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
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, QuestionStatisticSql,
                    new SqlParameter("@batch", Param.Batch));
                watcher.Stop();
                LogAction($"更新题目答题统计，耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
                //更新学生答题统计
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, StudentStatisticSql,
                    new SqlParameter("@batch", Param.Batch));
                watcher.Stop();
                LogAction($"更新学生答题统计，耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
                //生成错题库
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, ErrorQuestionSql,
                    new SqlParameter("@batch", Param.Batch));
                watcher.Stop();
                LogAction($"生成错题库，耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
                //知识点统计
                if (kpStatistics != null)
                {
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
                }
                watcher.Stop();
                LogAction($"知识点统计，耗时：{watcher.ElapsedMilliseconds}ms");
                watcher.Restart();
                //更新分享答案状态
                unitWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, AnswerShareSql,
                    new SqlParameter("@batch", Param.Batch));
                watcher.Stop();
                LogAction($"更新分享答案状态，耗时：{watcher.ElapsedMilliseconds}ms");
            });
            if (result <= 0)
                return DResult.Error("提交数据异常");
            watcher.Restart();
            var messageContract = CurrentIocManager.Resolve<IMessageContract>();
            var shareRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_AnswerShare>>();
            var userIds =
                    shareRepository.Where(t => t.Batch == Param.Batch).Select(t => t.AddedBy).Distinct().ToList();
            if (userIds.Any())
            {
                var userContract = CurrentIocManager.Resolve<IUserContract>();
                var teacher = userContract.Load(model.UserId);
                var msgStr = teacher.Name + "老师 分享了你在试卷 " + paper.PaperBaseInfo.PaperTitle + " 中的答案。";
                messageContract.SendMessage(msgStr, string.Empty, model.UserId, MessageType.DryingAnswer, userIds.ToArray());
            }
            //班级圈动态消息
            messageContract.SendDynamic(new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Exam,
                ContentType = (byte)ContentType.Publish,
                ContentId = Param.Batch,
                GroupId = model.ClassId,
                ReceivRole = (UserRole.Student | UserRole.Teacher),
                UserId = model.UserId
            });
            watcher.Stop();
            LogAction($"发送动态消息，耗时：{watcher.ElapsedMilliseconds}ms");
            return DResult.Success;
        }

        #region 知识点统计

        /// <summary> 初始化知识点统计 </summary>
        /// <param name="paper"></param>
        /// <param name="dict">batch-classId对应关系</param>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public static KpStatisticsDto InitKpStatistics(PaperDetailDto paper, Dictionary<string, string> dict, DateTime? createTime = null)
        {
            if (paper == null || dict == null || dict.Count == 0)
                return null;
            var questions = paper.PaperSections.SelectMany(s => s.Questions.Select(q => q.Question)).ToList();
            var subjectId = paper.PaperBaseInfo.SubjectId;
            Expression<Func<TP_MarkingDetail, bool>> condition = d => d.IsFinished;
            var batches = dict.Keys.ToArray();
            if (batches.Length == 1)
            {
                var batch = batches[0];
                condition = condition.And(d => d.Batch == batch);
            }
            else
            {
                condition = condition.And(d => batches.Contains(d.Batch));
            }
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            var details = detailRepository.Where(condition)
                .GroupBy(d => new { d.StudentID, d.QuestionID, d.Batch }).Select(d => new
                {
                    d.Key.StudentID,
                    d.Key.QuestionID,
                    d.Key.Batch,
                    IsCorrect = d.All(t => t.IsCorrect.HasValue && t.IsCorrect.Value)
                }).ToList();
            if (!details.Any())
                return new KpStatisticsDto();
            //所有知识点
            var kpDicts = questions.ToDictionary(k => k.Id,
                v => string.IsNullOrWhiteSpace(v.KnowledgeIDs)
                    ? new Dictionary<string, string>()
                    : v.KnowledgeIDs.JsonToObject<Dictionary<string, string>>());

            var allKps = kpDicts.SelectMany(u => u.Value.Select(d => new { QId = u.Key, KpName = d.Value, KpCode = d.Key })).ToList();
            //没有知识点
            if (!allKps.Any()) return null;

            //本周星期一
            var time = createTime ?? Clock.Now;
            var mondayDate = time.AddDays(1 - (int)time.DayOfWeek).Date;
            var sundayDate = time; //不能是本周日
            var searchSunday = mondayDate.AddDays(7);//用于查询的本周星期日

            var teacherKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_TeacherKpStatistic>>();
            var studentKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StudentKpStatistic>>();

            var result = new KpStatisticsDto();

            #region 教师知识点统计
            //教师
            var allKpCode = allKps.GroupBy(u => u.KpCode).ToList();
            //该班级这周已经有的知识点
            var kpCodes = allKpCode.Select(u => u.Key).ToList();

            foreach (var item in dict)
            {
                var teacherKps =
                    teacherKpStatisticRepository.Where(
                        u =>
                            kpCodes.Contains(u.KpLayerCode) && u.StartTime >= mondayDate && u.EndTime < searchSunday &&
                            u.SubjectID == subjectId && u.ClassID == item.Value).ToList();

                allKpCode.ForEach(k =>
                {
                    var qids = k.Select(q => q.QId).Distinct().ToList();
                    var teacherStatistic = new TS_TeacherKpStatistic
                    {
                        Id = IdHelper.Instance.Guid32,
                        KpID = -1,
                        KpLayerCode = k.Key,
                        StartTime = mondayDate,
                        EndTime = sundayDate,
                        AnswerCount = details.Count(u => u.Batch == item.Key && qids.Contains(u.QuestionID)),
                        ErrorCount =
                            details.Count(u => u.Batch == item.Key && qids.Contains(u.QuestionID) && !u.IsCorrect),
                        SubjectID = subjectId,
                        ClassID = item.Value
                    };

                    var kpModel = teacherKps.FirstOrDefault(u => u.KpLayerCode == k.Key);
                    if (kpModel != null) //这周已经有了
                    {
                        kpModel.AnswerCount += teacherStatistic.AnswerCount;
                        kpModel.ErrorCount += teacherStatistic.ErrorCount;

                        result.UpdateTeacherKpStatistic.Add(kpModel);
                    }
                    else //这周还没有
                    {
                        result.AddTeacherKpStatistic.Add(teacherStatistic);
                    }
                });
            }

            #endregion

            #region 学生知识点统计
            //学生
            var studentIds = details.Select(u => u.StudentID).Distinct().ToList();
            studentIds.ForEach(u =>
            {
                //学生回答过的知识点
                var studentKp = (from d in details.Where(d => d.StudentID == u)
                                 from k in allKps
                                 where d.QuestionID == k.QId
                                 select k).ToList();

                var studentKpCode = studentKp.GroupBy(s => s.KpCode).ToList();

                //该学生这周已经有的知识点
                var stuCodes = studentKpCode.Select(c => c.Key).ToList();
                var studentKpCodeList =
                    studentKpStatisticRepository.Where(
                        s =>
                            stuCodes.Contains(s.KpLayerCode) && s.StartTime >= mondayDate && s.EndTime < searchSunday &&
                            s.SubjectID == subjectId && s.StudentID == u).ToList();

                studentKpCode.ForEach(k =>
                {
                    var qids = k.Select(q => q.QId).Distinct().ToList();
                    var studentStatistic = new TS_StudentKpStatistic
                    {
                        Id = IdHelper.Instance.Guid32,
                        StudentID = u,
                        KpID = -1,
                        KpLayerCode = k.Key,
                        StartTime = mondayDate,
                        EndTime = sundayDate,
                        AnswerCount = details.Count(s => s.StudentID == u && qids.Contains(s.QuestionID)),
                        ErrorCount = details.Count(s => qids.Contains(s.QuestionID) && !s.IsCorrect && s.StudentID == u),
                        SubjectID = subjectId
                    };

                    var kpModel = studentKpCodeList.FirstOrDefault(s => s.KpLayerCode == k.Key);
                    if (kpModel != null)//这周已经有了
                    {
                        kpModel.AnswerCount += studentStatistic.AnswerCount;
                        kpModel.ErrorCount += studentStatistic.ErrorCount;

                        result.UpdateStudentKpStatistic.Add(kpModel);
                    }
                    else//这周还没有
                    {
                        result.AddStudentKpStatistic.Add(studentStatistic);
                    }
                });
            });
            #endregion

            return result;
        }
        #endregion

        #region 默认标记 & 客观题错题&分数

        /// <summary> 图片处理 </summary>
        /// <param name="paper"></param>
        /// <param name="pictures"></param>
        /// <returns></returns>
        private List<TP_MarkingPicture> UpdatePictures(PaperDetailDto paper, List<TP_MarkingPicture> pictures)
        {
            var updateList = new List<TP_MarkingPicture>();
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var batches = pictures.Select(t => t.BatchNo).Distinct().ToList();
            Expression<Func<TP_MarkingDetail, bool>> detailCondition = d => d.PaperID == paper.PaperBaseInfo.Id;
            if (batches.Count > 1)
            {
                detailCondition = detailCondition.And(d => batches.Contains(d.Batch));
            }
            else
            {
                var batch = batches[0];
                detailCondition = detailCondition.And(d => d.Batch == batch);
            }

            foreach (var pictureGroup in pictures.GroupBy(p => p.AnswerImgType))
            {
                var sectionType = pictureGroup.Key == 0 ? 1 : pictureGroup.Key;
                var questions = paper.PaperSections.Where(s => s.PaperSectionType == sectionType)
                    .SelectMany(s => s.Questions).ToList();
                #region 客观题错题

                var objectives = questions.Where(q => q.Question.IsObjective).Select(q => q.Question.Id).ToList();
                if (objectives.Any())
                {
                    var sorts = paperContract.PaperSorts(paper, true, sectionType);
                    var condition =
                        detailCondition.And(
                            d => objectives.Contains(d.QuestionID) && !(d.IsCorrect.HasValue && d.IsCorrect.Value));
                    //学生错题信息
                    var errorDict = detailRepository.Where(condition)
                        .Select(d => new
                        {
                            d.StudentID,
                            d.QuestionID,
                            d.SmallQID
                        })
                        .ToList()
                        .GroupBy(d => d.StudentID)
                        .ToDictionary(k => k.Key,
                            v =>
                                v.Select(t => string.IsNullOrWhiteSpace(t.SmallQID) ? t.QuestionID : t.SmallQID)
                                    .ToList());
                    var scoreDict = detailRepository.Where(detailCondition.And(d => objectives.Contains(d.QuestionID)))
                        .Select(d => new { d.StudentID, d.CurrentScore })
                        .ToList()
                        .GroupBy(d => d.StudentID)
                        .ToDictionary(k => k.Key, v => v.Sum(t => t.CurrentScore));
                    foreach (var picture in pictureGroup.ToList())
                    {
                        var studentId = picture.StudentID;
                        if (!errorDict.ContainsKey(studentId))
                            picture.ObjectiveError = "全对";
                        else
                        {
                            var errors = errorDict[studentId];
                            if (sorts.Count == errors.Count)
                                picture.ObjectiveError = "全错";
                            else
                            {
                                var errorSorts = sorts.Where(t => errors.Contains(t.Key))
                                    .Select(t => t.Value)
                                    .OrderBy(t => t.To(0M))
                                    .ToList();
                                picture.ObjectiveError = string.Join("，", errorSorts);
                            }
                        }
                        if (scoreDict.ContainsKey(studentId))
                            picture.ObjectiveScore = scoreDict[studentId];
                        else
                            picture.ObjectiveScore = 0M;
                        updateList.Add(picture);
                    }
                }

                #endregion
                if (!Param.SetIcon && !Param.SetMarks)
                    continue;
                //主观题
                var subjectives = questions.Where(q => !q.Question.IsObjective).Select(q => q.Question.Id).ToList();
                if (!subjectives.Any())
                    continue;
                var areas = Param.Batch.QuestionArea();
                var areaList = areas.Values.Where(t => subjectives.Contains(t.Id)).ToList();
                var subCondition = detailCondition.And(d => subjectives.Contains(d.QuestionID));
                var details = detailRepository.Where(subCondition)
                    .Select(d => new { d.StudentID, d.QuestionID, d.IsCorrect, d.CurrentScore })
                    .ToList();
                if (Param.SetMarks)
                {
                    #region 得分标记

                    var scoreDict = details.GroupBy(d => d.StudentID)
                        .ToDictionary(k => k.Key,
                            v =>
                                v.GroupBy(t => t.QuestionID)
                                    .ToDictionary(kk => kk.Key, vv => vv.Sum(t => t.CurrentScore)));
                    var fillAreas = areaList.Where(t => t.T).ToList();
                    var otherAreas = areaList.Where(t => !t.T).ToList();
                    foreach (var picture in pictureGroup.ToList())
                    {
                        var studentId = picture.StudentID;
                        if (!scoreDict.ContainsKey(studentId))
                            continue;
                        var scores = scoreDict[studentId];
                        List<MkComment> marks = picture.Marks.IsNotNullOrEmpty()
                            ? picture.Marks.Replace("'", "\"").JsonToObject2<List<MkComment>>()
                            : new List<MkComment>();
                        //填空题
                        if (fillAreas.Any())
                        {
                            var region = fillAreas.OrderBy(t => t.Y).ThenBy(t => t.X).First();
                            var qids = fillAreas.Select(t => t.Id);
                            var fillScore = scores.Where(t => qids.Contains(t.Key)).Sum(t => t.Value);
                            marks.Add(new MkComment
                            {
                                X = region.X,
                                Y = (region.Y - 26), //字体大小24px 所占高度24px
                                SymbolType = 4,
                                Words = fillScore.ToString(CultureInfo.InvariantCulture)
                            });
                        }
                        foreach (var area in otherAreas)
                        {
                            if (!scores.ContainsKey(area.Id))
                                continue;
                            var score = scores[area.Id];
                            if (marks.Exists(m => Equals(m.X, area.X) && Equals(m.Y, area.Y - 26F)))
                            {
                                //去掉重复的标记
                                var exist = marks.FirstOrDefault(m => Equals(m.X, area.X) && Equals(m.Y, area.Y - 26F));
                                marks.Remove(exist);
                            }
                            marks.Add(new MkComment
                            {
                                X = area.X,
                                Y = (area.Y - 26), //字体大小24px 所占高度24px
                                SymbolType = 4,
                                Words = score.ToString(CultureInfo.InvariantCulture)
                            });
                        }

                        var item = updateList.FirstOrDefault(t => t.Id == picture.Id);
                        if (item == null)
                        {
                            picture.Marks = marks.ToJson2();
                            updateList.Add(picture);
                        }
                        else
                        {
                            item.Marks = marks.ToJson2();
                        }
                    }

                    #endregion
                }
                if (Param.SetIcon)
                {
                    #region 默认打勾
                    //默认标记
                    var rightDict = details.GroupBy(d => d.StudentID)
                        .ToDictionary(k => k.Key,
                            v =>
                                v.GroupBy(t => t.QuestionID)
                                    .ToDictionary(kk => kk.Key, vv => vv.All(t => (t.IsCorrect ?? false))));
                    foreach (var picture in pictureGroup.ToList())
                    {
                        var studentId = picture.StudentID;
                        if (!rightDict.ContainsKey(studentId))
                            continue;
                        var list = rightDict[studentId];
                        var icons = picture.RightAndWrong.IsNotNullOrEmpty()
                            ? picture.RightAndWrong.Replace("'", "\"").JsonToObject2<List<MkSymbol>>()
                            : new List<MkSymbol>();
                        foreach (var qItem in list)
                        {
                            var qid = qItem.Key;
                            if (!qItem.Value || icons.Exists(t => t.QuestionId == qid))
                                continue;
                            if (!areas.ContainsKey(qid))
                                continue;
                            var area = areas[qid];
                            icons.Add(new MkSymbol
                            {
                                QuestionId = qid,
                                Score = 0,
                                SymbolType = (int)SymbolType.Right,
                                X = area.X,
                                Y = area.Y
                            });
                        }
                        var item = updateList.FirstOrDefault(t => t.Id == picture.Id);
                        if (item == null)
                        {
                            picture.RightAndWrong = icons.ToJson2();
                            updateList.Add(picture);
                        }
                        else
                        {
                            item.RightAndWrong = icons.ToJson2();
                        }
                    }
                    #endregion
                }
            }
            return updateList;
        }

        #endregion
    }
}
