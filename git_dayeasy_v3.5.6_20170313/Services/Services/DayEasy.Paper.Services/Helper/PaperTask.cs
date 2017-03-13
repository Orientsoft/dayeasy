using DayEasy.AsyncMission;
using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Paper.Services.Helper.Question;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.Paper.Services.Helper
{
    internal class PaperTask
    {
        private readonly ILogger _logger = LogManager.Logger<PaperTask>();

        internal static PaperTask Instance
        {
            get { return Singleton<PaperTask>.Instance ?? (Singleton<PaperTask>.Instance = new PaperTask()); }
        }

        /// <summary> 出卷成功后的异步操作 </summary>
        /// <returns></returns>
        internal Task GeneratePaperTaskAsync(List<TP_PaperAnswer> answerList, List<string> qids,
            long teacherId, string paperTags)
        {
            EditMyselfAnswerAsync(answerList, teacherId);
            return Task.Factory.StartNew(() =>
            {
                Utils.WatchAction("异步任务[生成试卷]", () =>
                {
                    var teacherStatisticRepository =
                    CurrentIocManager.Resolve<IDayEasyRepository<TS_TeacherStatistic, long>>();
                    //出卷统计
                    var statistic = teacherStatisticRepository.Load(teacherId);
                    if (statistic != null)
                    {
                        statistic.AddPaperCount++;
                        teacherStatisticRepository.Update(t => new
                        {
                            t.AddPaperCount
                        }, statistic);
                    }
                    if (!string.IsNullOrWhiteSpace(paperTags))
                    {
                        //更新tag表
                        var tags = JsonHelper.JsonList<string>(paperTags);
                        var systemContract = CurrentIocManager.Resolve<ISystemContract>();
                        systemContract.UpdateTags(TagType.Paper, tags);
                    }

                    //修改问题的状态
                    if (!qids.Any())
                        return;
                    var questionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();
                    questionRepository.Update(new TQ_Question
                    {
                        Status = (byte)NormalStatus.Normal
                    }, q => qids.Contains(q.Id), "Status");

                    QuestionManager.Instance.UpdateAsync(qids);
                });
            });
        }

        #region 修改自己的问题的答案

        /// <summary> 异步任务修改自己的问题的答案 </summary>
        /// <returns></returns>
        internal Task EditMyselfAnswerAsync(IReadOnlyCollection<TP_PaperAnswer> answerList, long currentUserId)
        {
            return Task.Factory.StartNew(() =>
            {
                Utils.WatchAction("异步任务[修改自己的答案]", () =>
                {
                    if (answerList == null || answerList.Count <= 0)
                        return;

                    var paperContract = CurrentIocManager.Resolve<IPaperContract>();

                    //查询试卷中自己的问题
                    var qids = answerList.Select(u => u.QuestionId).Distinct().ToArray();
                    var myQuestions = paperContract.LoadQuestions(qids).Where(q => q.UserId == currentUserId).ToList();

                    if (!myQuestions.Any())
                        return;
                    var answerRepository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Answer>>();
                    var cleanCacheList = new List<string>();
                    var updateOptions = new List<TQ_Answer>();
                    var updateAnswers = new List<TQ_Answer>();
                    foreach (var dto in answerList)
                    {
                        var qItem = myQuestions.FirstOrDefault(q => q.Id == dto.QuestionId);
                        if (qItem == null)
                            continue;
                        List<AnswerDto> answers;
                        if (!string.IsNullOrWhiteSpace(dto.SmallQuId))
                        {
                            var detail = qItem.Details.FirstOrDefault(t => t.Id == dto.SmallQuId);
                            if (detail == null)
                                continue;
                            answers = detail.Answers;
                        }
                        else
                        {
                            answers = qItem.Answers;
                        }
                        if (answers.IsNullOrEmpty())
                            continue;
                        if (qItem.IsObjective)
                        {
                            var oldAnswer = string.Empty;
                            if (answers.Any(a => a.IsCorrect))
                            {
                                oldAnswer = string.Join(string.Empty,
                                    answers.Where(t => t.IsCorrect).OrderBy(t => t.Sort).Select(t => t.Tag));
                            }
                            if (oldAnswer.Equals(dto.AnswerContent, StringComparison.CurrentCultureIgnoreCase))
                                continue;
                            foreach (var answer in answers)
                            {
                                var correct = dto.AnswerContent.Contains(answer.Tag);
                                if (answer.IsCorrect == correct)
                                    continue;
                                answer.IsCorrect = correct;
                                updateOptions.Add(new TQ_Answer
                                {
                                    Id = answer.Id,
                                    IsCorrect = answer.IsCorrect
                                });
                            }
                            cleanCacheList.Add(qItem.Id);
                        }
                        else
                        {
                            var oldAnswer = answers.First();
                            if (oldAnswer.Body == dto.AnswerContent)
                                continue;
                            updateAnswers.Add(new TQ_Answer
                            {
                                Id = oldAnswer.Id,
                                QContent = dto.AnswerContent
                            });
                            cleanCacheList.Add(qItem.Id);
                        }
                    }

                    //更新数据
                    var result = answerRepository.UnitOfWork.Transaction(() =>
                    {
                        if (updateAnswers.Any())
                        {
                            answerRepository.Update(a => new
                            {
                                a.QContent
                            }, updateAnswers.ToArray());
                        }
                        if (updateOptions.Any())
                        {
                            answerRepository.Update(a => new
                            {
                                a.IsCorrect
                            }, updateOptions.ToArray());
                        }
                    });
                    if (result <= 0)
                        return;
                    foreach (var id in cleanCacheList.Distinct())
                    {
                        //清除缓存
                        QuestionCache.Instance.Remove(id);
                    }
                });
            });
        }

        #endregion

        #region 修改当前正在阅卷的阅卷结果

        /// <summary> 修改当前正在阅卷的阅卷结果 </summary>
        /// <param name="updateAnswers"></param>
        /// <param name="paperId"></param>
        /// <param name="userId"></param>
        /// <param name="containsFinished"></param>
        internal Task ChangeMarkingAsync(List<TP_PaperAnswer> updateAnswers, string paperId, long userId, bool containsFinished = false)
        {
            return Task.Factory.StartNew(() =>
            {
                var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
                var usages =
                    usageRepository.Where(
                        c =>
                            c.SourceID == paperId && c.SourceType == (byte)PublishType.Print &&
                            c.Status != (byte)NormalStatus.Delete);
                if (!containsFinished)
                {
                    usages = usages.Where(u => u.MarkingStatus != (byte)MarkingStatus.AllFinished);
                }
                //没有正在批阅的试卷
                if (!usages.Any())
                    return;
                var paperContract = CurrentIocManager.Resolve<IPaperContract>();
                var paperResult = paperContract.PaperDetailById(paperId);
                if (!paperResult.Status)
                    return;
                var paper = paperResult.Data;
                //查找试卷中的客观题
                var objecctiveIds =
                    paper.PaperSections.SelectMany(
                        t => t.Questions.Where(q => q.Question.IsObjective).Select(q => q.Question.Id)).ToList();

                if (!objecctiveIds.Any())
                    return;
                var objectiveAnswers = updateAnswers.Where(t => objecctiveIds.Contains(t.QuestionId)).ToList();
                if (!objectiveAnswers.Any())
                    return;
                var modifyAnswers = objectiveAnswers.Select(t => new QuestionAnswer
                {
                    QuestionId = t.QuestionId,
                    SmallId = t.SmallQuId,
                    Answer = t.AnswerContent
                }).ToList();
                MissionHelper.PushMission(MissionType.ChangeAnswer, new ChangeAnswerParam
                {
                    PaperId = paperId,
                    Answers = modifyAnswers,
                    ContainsFinished = containsFinished
                }, userId);
            });
        }

        #endregion
    }
}
