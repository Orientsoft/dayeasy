using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Marking.Services.Helper
{
    /// <summary> 完成阅卷后台任务 </summary>
    public class MarkingTask
    {
        private static readonly ILogger Logger = LogManager.Logger<MarkingTask>();

        /// <summary>
        /// 完成阅卷后台任务
        /// 1、计算Result总分、模块得分、错题数...
        /// 2、生成错题、更新题目统计；
        /// 3、更新试卷、班级考试统计信息
        /// 4、更新知识点统计；
        /// 5、发送完成阅卷消息。
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paperId"></param>
        /// <param name="classId"></param>
        /// <param name="teacherId"></param>
        /// <param name="subjectId"></param>
        public static void FinishMission(string batch, string paperId, string classId, long teacherId, int subjectId)
        {
            var sw = new Stopwatch();
            sw.Start();

            var questionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();

            var sb = new StringBuilder();
            try
            {
                const string updateQuSql = @"update TQ_Question set AnswerCount=TQ_Question.AnswerCount+tempResult.AnswerCount,
                            ErrorCount=TQ_Question.ErrorCount+tempResult.ErrorCount from 
                            (
                            select tempa.QuestionID,ISNULL(AnswerCount,0)as AnswerCount,ISNULL(ErrorCount,0)as ErrorCount from 
                            (
                            select QuestionID,count(1)as AnswerCount from TP_MarkingDetail where Batch=@batch and PaperID=@paperId
                            group by QuestionID)tempa
                            left join 
                            (select QuestionID,count(1)as ErrorCount from TP_MarkingDetail where Batch=@batch and PaperID=@paperId
                            and IsCorrect=0
                            group by QuestionID)tempb
                            on tempa.QuestionID=tempb.QuestionID
                            )tempResult
                            where tempResult.QuestionID=TQ_Question.QID";

                const string updateStuSql = @"update TS_StudentStatistic set ErrorQuestionCount=TS_StudentStatistic.ErrorQuestionCount+temp.ErrorCount from
                        (
                        select StudentID,count(1) as ErrorCount from TP_MarkingDetail  
                        where Batch=@batch and PaperID=@paperId and IsCorrect=0
                        group by StudentID)temp
                        where temp.StudentID=TS_StudentStatistic.UserID";

                //查询答题详情
                var details = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>().Where(d => d.Batch == batch && d.PaperID == paperId && d.IsFinished).Select(q => new MarkDetailDto { QuestionId = q.QuestionID, StudentId = q.StudentID, IsCorrect = q.IsCorrect }).ToList();

                //查询问题
                var qids = details.Select(u => u.QuestionId).Distinct().ToArray();
                var tempQus = CurrentIocManager.Resolve<IPaperContract>().LoadQuestions(qids);
                var questions = tempQus == null ? null : tempQus.Select(u => new QuestionDetailDto { Type = u.Type, Id = u.Id, KpIds = u.KnowledgeIDs }).ToList();

                //查询试卷
                var paperModel = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>().FirstOrDefault(p => p.Id == paperId);

                //生成错题任务
                var errorQuTask = Task.Run(() => ErrorQuestionMission(details, questions, paperModel, batch));

                //知识点统计任务
                var kpTask = Task.Run(() => UpdateKpStatistic(details, questions, subjectId, classId));

                //分享答案任务
                var shareTask = Task.Run(() => UpdateAnswerShareStatus(batch, paperId));

                Task.WaitAll(errorQuTask, kpTask, shareTask);//等待执行完毕

                var errorQuestions = errorQuTask.Result;//生成错题结果
                var kpStatistics = kpTask.Result;//知识点统计结果
                var shares = shareTask.Result;//分享答案结果

                var teacherName = string.Empty;//教师姓名
                List<long> userIds = null;//分享的答案的学生Id

                //处理发送分享答案消息 查询老师
                if (shares != null && shares.Count > 0)
                {
                    var teacher = CurrentIocManager.Resolve<IDayEasyRepository<TU_User, long>>().SingleOrDefault(u => u.Id == teacherId);
                    teacherName = teacher == null ? string.Empty : teacher.TrueName;

                    userIds = shares.Where(u => u.IsCorrect.HasValue && u.IsCorrect.Value).Select(u => u.StudentId).ToList();
                }

                questionRepository.UnitOfWork.Transaction(() =>
                {
                    sb.AppendLine(string.Format("完成阅卷 任务开始:batch:{0},paperId:{1}", batch, paperId));

                    //题目答题数,错题数统计更新
                    questionRepository.UnitOfWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, updateQuSql, new SqlParameter("@batch", batch), new SqlParameter("@paperId", paperId));

                    //更新学生错题统计
                    CurrentIocManager.Resolve<IDayEasyRepository<TS_StudentStatistic, long>>().UnitOfWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, updateStuSql, new SqlParameter("@batch", batch), new SqlParameter("@paperId", paperId));

                    //1、生成错题
                    if (errorQuestions != null && errorQuestions.Count > 0)
                    {
                        CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>()
                            .Insert(errorQuestions);
                    }
                    sb.AppendLine("1、生成错题、更新题目统计 完成");

                    //2、更新试卷、班级考试统计信息
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
                            //                            kpStatistics.UpdateTeacherKpStatistic.Foreach(u => teacherKpStatisticRepository.Update(u));
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
                            //                            kpStatistics.UpdateStudentKpStatistic.Foreach(u => studentKpStatisticRepository.Update(u));
                        }
                    }
                    sb.AppendLine("2、更新知识点统计信息 完成");

                    //3、更新答案分享状态
                    if (shares != null)
                    {
                        var answerShareRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_AnswerShare>>();
                        foreach (var share in shares)
                        {
                            if (share.IsCorrect.HasValue && share.IsCorrect.Value)
                            {
                                const string sql = "update TP_AnswerShare set TP_AnswerShare.Status=0 where Id=@id";
                                answerShareRepository.UnitOfWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, sql, new SqlParameter("@id", share.ShareId));
                            }
                            else
                            {
                                const string sql = "delete TP_AnswerShare where Id=@id";
                                answerShareRepository.UnitOfWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, sql, new SqlParameter("@id", share.ShareId));
                            }
                        }

                        if (userIds != null && userIds.Count > 0)//发送分享答案试卷消息
                        {
                            var msgStr = teacherName + "老师 分享了你在试卷 " + paperModel.PaperTitle + " 中的答案。";
                            CurrentIocManager.Resolve<IMessageContract>().SendMessage(msgStr, string.Empty, teacherId, MessageType.DryingAnswer, userIds.ToArray());
                        }
                    }
                    sb.AppendLine("3、更新答案分享状态与发送消息 完成");
                });
                //4、todo 事务处理:发送完成阅卷消息
                MessageMission(batch, paperId, classId, teacherId);
                sb.AppendLine("4、发送完成阅卷消息 完成");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            finally
            {
                sw.Stop();
                sb.AppendLine("异步任务耗时：" + sw.Elapsed.TotalMilliseconds);

                Logger.Info(sb.ToString());
                sb.Clear();
            }
        }

        #region 生成错题
        /// <summary>
        /// 生成错题,更新题目统计
        /// </summary>
        private static List<TP_ErrorQuestion> ErrorQuestionMission(List<MarkDetailDto> details, IEnumerable<QuestionDetailDto> questions, TP_Paper paperModel, string batch)
        {
            if (details == null || details.Count < 1)
                return null;

            //判断重复
            if (CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>().Exists(e => e.Batch == batch && e.PaperID == paperModel.Id))
                return null;

            var errorQuestions = details.Where(u => u.IsCorrect == false).DistinctBy(u => new { u.QuestionId, u.StudentId }).ToList();
            if (errorQuestions.Count < 1)
                return null;

            if (paperModel == null)
                return null;

            var errorQuestionList = new List<TP_ErrorQuestion>();
            errorQuestions.ForEach(q =>
            {
                var question = questions == null ? null : questions.FirstOrDefault(u => u.Id == q.QuestionId);
                if (question == null) return;
                if (errorQuestionList.Exists(u => u.QuestionID == q.QuestionId && u.StudentID == q.StudentId))
                    return;
                errorQuestionList.Add(new TP_ErrorQuestion
                {
                    Id = IdHelper.Instance.Guid32,
                    PaperID = paperModel.Id,
                    Batch = batch,
                    QuestionID = q.QuestionId,
                    StudentID = q.StudentId,
                    PaperTitle = paperModel.PaperTitle,
                    SubjectID = paperModel.SubjectID,
                    Stage = paperModel.Stage,
                    QType = question.Type,
                    AddedAt = Clock.Now,
                    Status = (byte)ErrorQuestionStatus.Normal,
                    VariantCount = 0
                });
            });

            return errorQuestionList;
        }

        #endregion

        #region 更新知识点统计
        /// <summary>
        /// 更新知识点统计
        /// </summary>
        private static KpStatisticsDto UpdateKpStatistic(List<MarkDetailDto> details, List<QuestionDetailDto> questions, int subjectId, string classId)
        {
            if (details == null || details.Count < 1 || questions == null || questions.Count < 1)
                return null;
            //所有知识点
            var kpDicts = questions.ToDictionary(k => k.Id,
                v => string.IsNullOrWhiteSpace(v.KpIds)
                    ? new Dictionary<string, string>()
                    : v.KpIds.JsonToObject<Dictionary<string, string>>());

            var allKps = kpDicts.SelectMany(u => u.Value.Select(d => new { QId = u.Key, KpName = d.Value, KpCode = d.Key })).ToList();
            //没有知识点
            if (!allKps.Any()) return null;

            //本周星期一
            var mondayDate = Clock.Now.AddDays(1 - (int)Clock.Now.DayOfWeek).Date;
            var sundayDate = Clock.Now; //不能是本周日
            var searchSunday = mondayDate.AddDays(7);//用于查询的本周星期日

            var teacherKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_TeacherKpStatistic>>();
            var studentKpStatisticRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StudentKpStatistic>>();

            var result = new KpStatisticsDto();

            #region 教师知识点统计
            //教师
            var allKpCode = allKps.GroupBy(u => u.KpCode).ToList();
            //该班级这周已经有的知识点
            var kpCodes = allKpCode.Select(u => u.Key).ToList();
            var teacherKps =
                teacherKpStatisticRepository.Where(
                    u =>
                        kpCodes.Contains(u.KpLayerCode) && u.StartTime >= mondayDate && u.EndTime < searchSunday &&
                        u.SubjectID == subjectId && u.ClassID == classId).ToList();

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
                    AnswerCount = details.Count(u => qids.Contains(u.QuestionId)),
                    ErrorCount = details.Count(u => qids.Contains(u.QuestionId) && u.IsCorrect == false),
                    SubjectID = subjectId,
                    ClassID = classId
                };

                var kpModel = teacherKps.SingleOrDefault(u => u.KpLayerCode == k.Key);
                if (kpModel != null)//这周已经有了
                {
                    kpModel.AnswerCount += teacherStatistic.AnswerCount;
                    kpModel.ErrorCount += teacherStatistic.ErrorCount;

                    result.UpdateTeacherKpStatistic.Add(kpModel);
                }
                else//这周还没有
                {
                    result.AddTeacherKpStatistic.Add(teacherStatistic);
                }
            });

            #endregion

            #region 学生知识点统计
            //学生
            var studentIds = details.Select(u => u.StudentId).Distinct().ToList();
            studentIds.ForEach(u =>
            {
                //学生回答过的知识点
                var studentKp = (from d in details.Where(d => d.StudentId == u)
                                 from k in allKps
                                 where d.QuestionId == k.QId
                                 select k).ToList();

                var studentKpCode = studentKp.GroupBy(s => s.KpCode).ToList();

                //该学生这周已经有的知识点
                var stuCodes = studentKpCode.Select(c => c.Key).ToList();
                var studentKpCodeList = studentKpStatisticRepository.Where(s => stuCodes.Contains(s.KpLayerCode) && s.StartTime >= mondayDate && s.EndTime < searchSunday && s.SubjectID == subjectId && s.StudentID == u).ToList();

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
                        AnswerCount = details.Count(s => qids.Contains(s.QuestionId) && s.StudentId == u),
                        ErrorCount =
                            details.Count(s => qids.Contains(s.QuestionId) && s.IsCorrect == false && s.StudentId == u),
                        SubjectID = subjectId
                    };

                    var kpModel = studentKpCodeList.SingleOrDefault(s => s.KpLayerCode == k.Key);
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

        #region 阅卷完成消息发送

        private static void MessageMission(string batch, string paperId, string classId, long userId)
        {
            var messageContract = CurrentIocManager.Resolve<IMessageContract>();
            messageContract.SendDynamic(new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Exam,
                ContentType = (byte)ContentType.Publish,
                ContentId = batch,
                GroupId = classId,
                ReceivRole = (UserRole.Student | UserRole.Teacher),
                UserId = userId
            });
        }

        #endregion

        #region 更新答案分享状态

        /// <summary>
        /// 更新答案分享状态
        /// </summary>
        private static List<ShareAnswerDto> UpdateAnswerShareStatus(string batch, string paperId)
        {
            var answerShareRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_AnswerShare>>();
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            var shares = (from a in answerShareRepository.Where(s => s.Batch == batch && s.PaperId == paperId)
                          join d in detailRepository.Table
                              on new
                              {
                                  a.Batch,
                                  a.PaperId,
                                  a.QuestionId,
                                  StudentID = a.AddedBy
                              } equals
                              new
                              {
                                  d.Batch,
                                  PaperId = d.PaperID,
                                  QuestionId = d.QuestionID,
                                  d.StudentID
                              }
                          select new ShareAnswerDto
                          {
                              ShareId = a.Id,
                              IsCorrect = d.IsCorrect,
                              StudentId = d.StudentID
                          }).ToList();
            return shares;
        }

        #endregion

        #region 默认标记 & 客观题错题

        /// <summary> 正确答案默认打上勾、主观题得分标记 、更新错误的客观题的题号 </summary>
        public static Task UpdateRightIconAndErrorObjMission(string batch, string paperId,
            bool autoSetRightIcon, bool autoSetScore, Action<string> logAction = null)
        {
            return Task.Factory.StartNew(() =>
            {
                logAction = logAction ?? (m => { Logger.Info(m); });
                var watch = new Stopwatch();
                watch.Start();
                try
                {
                    var paper = CurrentIocManager.Resolve<IPaperContract>().PaperDetailById(paperId);
                    if (!paper.Status || paper.Data == null)
                        return;
                    var paperModel = paper.Data.PaperBaseInfo;

                    if (paperModel.PaperType == (byte)PaperType.AB)
                    {
                        UpdateErrorObjective(paper.Data, batch, MarkingPaperType.PaperA, logAction);
                        UpdateErrorObjective(paper.Data, batch, MarkingPaperType.PaperB, logAction);

                        if (!autoSetRightIcon && !autoSetScore)
                            return;
                        SetRightIconAndScoreMark(batch, paperId, MarkingPaperType.PaperA, autoSetRightIcon,
                            autoSetScore, logAction);
                        SetRightIconAndScoreMark(batch, paperId, MarkingPaperType.PaperB, autoSetRightIcon,
                            autoSetScore, logAction);
                    }
                    else
                    {
                        UpdateErrorObjective(paper.Data, batch, MarkingPaperType.Normal, logAction);

                        if (!autoSetRightIcon && !autoSetScore)
                            return;
                        SetRightIconAndScoreMark(batch, paperId, MarkingPaperType.Normal, autoSetRightIcon,
                            autoSetScore, logAction);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                }
                finally
                {
                    watch.Stop();
                    logAction("默认标记耗时：" + watch.Elapsed.TotalMilliseconds);
                }
            });
        }

        /// <summary> 正确答案默认打上勾、主观题分数 </summary>
        private static void SetRightIconAndScoreMark(string batch, string paperId, MarkingPaperType imageType, bool setRightIcon, bool setScoreMark, Action<string> logAction)
        {
            try
            {
                //若是协同阅卷则需要协同批次号
                var usage = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>()
                    .FirstOrDefault(u => u.Id == batch);
                if (usage == null) return;
                //查询标记区域
                var tmpBatch = usage.JointBatch.IsNullOrEmpty() ? batch : usage.JointBatch;
                var pMark = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingMark>>()
                    .FirstOrDefault(u => u.BatchNo == tmpBatch && u.PaperType == (byte)imageType);
                if (pMark == null) return;
                var areas = (JsonHelper.JsonList<MkQuestionAreaDto>(pMark.Mark.Replace("'", "\""))
                             ?? new List<MkQuestionAreaDto>()).ToList();
                if (!areas.Any())
                    return;
                //                var areas = JointHelper.QuestionAreaCache(tmpBatch).Values.ToList();
                areas = areas.OrderBy(a => a.Index).ToList();

                //主观题IDs
                var objectiveIds = areas.Select(a => a.Id).ToList();

                //查询Details
                Expression<Func<TP_MarkingDetail, bool>> conditionDetail = d =>
                    d.Batch == batch && d.PaperID == paperId && objectiveIds.Contains(d.QuestionID);
                //只标记正确题目 “ √ ”
                if (!setScoreMark)
                    conditionDetail = conditionDetail.And(d => d.IsCorrect.HasValue && d.IsCorrect.Value);
                var details = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>()
                    .Where(conditionDetail).Select(d => new
                    {
                        d.QuestionID,
                        d.StudentID,
                        d.CurrentScore,
                        d.IsCorrect
                    }).ToList();
                if (!details.Any()) return;

                //学生答卷图片
                var markingPictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
                var pictures = markingPictureRepository.Where(p =>
                    p.BatchNo == batch && p.PaperID == paperId && p.AnswerImgType == (byte)imageType).ToList();
                if (!pictures.Any()) return;

                //数据处理
                var tkList = areas.Where(a => a.T).Select(a => a.Id).ToList(); //填空题ID集合
                var firstTk = areas.Where(a => a.T).OrderBy(a => a.Index).FirstOrDefault(); //第一个填空题区域
                pictures.Foreach(pic =>
                {
                    #region 设置主观题得分

                    if (setScoreMark)
                    {
                        //当前学生的全部答题详细
                        var stuDetails = details.Where(d => d.StudentID == pic.StudentID).ToList();
                        var nMarks = new List<MkComment>(); //新增标记容器
                        //填空题
                        if (tkList.Any() && firstTk != null)
                        {
                            var tkScore =
                                stuDetails.Where(d => tkList.Contains(d.QuestionID)).Sum(d => d.CurrentScore);
                            nMarks.Add(new MkComment
                            {
                                X = firstTk.X,
                                Y = (firstTk.Y - 26), //字体大小24px 所占高度24px
                                SymbolType = 4,
                                Words = tkScore.ToString(CultureInfo.InvariantCulture)
                            });
                            //用完就丢 保留 非填空题
                            stuDetails = stuDetails.Where(d => !tkList.Contains(d.QuestionID)).ToList();
                        }
                        //非填空题
                        stuDetails.ForEach(d =>
                        {
                            var area = areas.FirstOrDefault(a => a.Id == d.QuestionID);
                            if (area == null) return;
                            nMarks.Add(new MkComment
                            {
                                X = area.X,
                                Y = (area.Y - 26),
                                SymbolType = 4,
                                Words = d.CurrentScore.ToString(CultureInfo.InvariantCulture)
                            });
                        });
                        if (nMarks.Any())
                        {
                            var picMarks = pic.Marks.IsNotNullOrEmpty()
                                ? pic.Marks.Replace("'", "\"").JsonToObject2<List<MkComment>>()
                                : new List<MkComment>();
                            picMarks.AddRange(nMarks);
                            pic.Marks = picMarks.ToJson2();
                        }
                    }

                    #endregion

                    #region 正确答题默认标记 “ √ ”

                    if (!setRightIcon) return;
                    var stuRightDetails = details.Where(d =>
                        d.StudentID == pic.StudentID && d.IsCorrect.HasValue && d.IsCorrect.Value).ToList();
                    var picIcons = pic.RightAndWrong.IsNotNullOrEmpty()
                        ? pic.RightAndWrong.Replace("'", "\"").JsonToObject2<List<MkSymbol>>()
                        : new List<MkSymbol>();
                    stuRightDetails.ForEach(d =>
                    {
                        if (picIcons.Any(p => p.QuestionId == d.QuestionID)) return;
                        var area = areas.FirstOrDefault(a => a.Id == d.QuestionID);
                        if (area == null) return;
                        picIcons.Add(new MkSymbol
                        {
                            QuestionId = d.QuestionID,
                            Score = 0,
                            SymbolType = (int)SymbolType.Right,
                            X = area.X,
                            Y = area.Y
                        });
                    });
                    pic.RightAndWrong = picIcons.ToJson2();

                    #endregion
                });
                markingPictureRepository.Update(p => new { p.RightAndWrong, p.Marks }, pictures.ToArray());
                logAction(string.Format("批次号[{0}{1}]设置默认标记完成！", batch, imageType.GetText()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        /// <summary> 更新错误的客观题的题号 </summary>
        private static void UpdateErrorObjective(PaperDetailDto paper, string batch, MarkingPaperType imageType, Action<string> logAction)
        {
            if (paper == null || paper.PaperSections.Count < 1)
                return;

            var markingDetailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();

            var pSectionType = imageType == MarkingPaperType.PaperB
                ? (byte)PaperSectionType.PaperB
                : (byte)PaperSectionType.PaperA;
            var objectiveSorts = CurrentIocManager.Resolve<IPaperContract>().PaperSorts(paper, true, pSectionType);
            //试卷客观题序号
            if (!objectiveSorts.Any())
                return;
            var paperId = paper.PaperBaseInfo.Id;
            //学生错题
            var studentErrors =
                markingDetailRepository.Where(
                    d => d.Batch == batch
                         && d.PaperID == paperId
                         && !(d.IsCorrect.HasValue && d.IsCorrect.Value))
                    .Select(d => new
                    {
                        d.StudentID,
                        d.QuestionID,
                        d.SmallQID
                    })
                    .ToList()
                    .GroupBy(d => d.StudentID)
                    .ToDictionary(k => k.Key,
                        v => v.Select(d => string.IsNullOrWhiteSpace(d.SmallQID) ? d.QuestionID : d.SmallQID));

            //查询学生的阅卷标记
            var markingPictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();

            var studentPics =
                markingPictureRepository.Where(
                    u => u.BatchNo == batch
                         && u.PaperID == paperId
                         && u.AnswerImgType == (byte)imageType).ToList();
            if (studentPics.Count < 1)
                return;

            studentPics.Foreach(s =>
            {
                var errorStr = "全对";
                if (studentErrors.ContainsKey(s.StudentID))
                {
                    var errors = studentErrors[s.StudentID];
                    var errorSortList =
                        objectiveSorts.Where(t => errors.Contains(t.Key))
                            .Select(t => t.Value)
                            .OrderBy(t => t)
                            .ToList();
                    //errorObjSorts.Where(u => u.StudentID == s.StudentID).Select(u => u.Sort).OrderBy(u => u).ToList();
                    if (errorSortList.Any())
                    {
                        if (errorSortList.Count == objectiveSorts.Count)
                            errorStr = "全错";
                        else errorStr = string.Join("，", errorSortList);
                    }
                }
                s.ObjectiveError = errorStr;
            });
            markingPictureRepository.Update(u => new { u.ObjectiveError }, studentPics.ToArray());
            logAction($"{batch}:更新客观题题号完成");
        }

        #endregion
    }
}
