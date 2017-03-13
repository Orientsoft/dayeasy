using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.AsyncMission.Jobs.JobTasks
{
    /// <summary> 修改客观题答案任务 </summary>
    internal class ChangeAnswerTask : DTask<ChangeAnswerParam>
    {
        public ChangeAnswerTask(ChangeAnswerParam param, Action<string> logAction = null)
            : base(param, logAction)
        {
        }

        public override DResult Execute()
        {
            if (Param == null || string.IsNullOrWhiteSpace(Param.PaperId) || Param.Answers.IsNullOrEmpty())
                return DResult.Error("参数异常");
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var usages =
                usageRepository.Where(u => u.Status == (byte)NormalStatus.Normal && u.SourceID == Param.PaperId);
            if (!Param.ContainsFinished)
            {
                usages = usages.Where(t => t.MarkingStatus != (byte)MarkingStatus.AllFinished);
            }
            if (!usages.Any())
                return DResult.Success;
            var batches = usages.Select(t => t.Id).ToList();
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var paperResult = paperContract.PaperDetailById(Param.PaperId);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error("试卷未找到");
            var updateDetails = new List<TP_MarkingDetail>();
            var halfList = paperResult.Data.PaperSections.SelectMany(s => s.Questions).Where(q => q.Question.Type == 3)
                .Select(q => q.Question.Id).ToList();
            foreach (var answer in Param.Answers)
            {
                var details = detailRepository.Where(
                    d =>
                        d.PaperID == Param.PaperId && batches.Contains(d.Batch) && d.QuestionID == answer.QuestionId &&
                        (d.SmallQID == null || d.SmallQID == answer.SmallId))
                    .Select(d => new
                    {
                        d.Id,
                        d.IsCorrect,
                        d.AnswerContent,
                        d.CurrentScore,
                        d.Score
                    }).ToList();
                var halfModel = halfList.Contains(answer.QuestionId);
                foreach (var detail in details)
                {
                    if (string.IsNullOrWhiteSpace(detail.AnswerContent))
                        continue;
                    var correct = (detail.AnswerContent == answer.Answer);
                    var score = correct ? detail.Score : 0M;
                    if (!correct && halfModel)
                    {
                        if (detail.AnswerContent.All(t => answer.Answer.Contains(t)))
                            score = detail.Score / 2M;
                    }
                    if (detail.IsCorrect != correct || detail.CurrentScore != score)
                    {
                        updateDetails.Add(new TP_MarkingDetail
                        {
                            Id = detail.Id,
                            IsCorrect = correct,
                            CurrentScore = score
                        });
                    }
                }
            }
            LogAction($"共修改{Param.Answers.Count}道题目的答案,{updateDetails.Count}条记录");
            if (!updateDetails.Any())
                return DResult.Success;
            var result = detailRepository.UnitOfWork.Transaction(() =>
            {
                if (updateDetails.Any())
                {
                    detailRepository.Update(d => new
                    {
                        d.IsCorrect,
                        d.CurrentScore
                    }, updateDetails.ToArray());
                }
            });
            if (result > 0 && Param.ContainsFinished)
            {
                //:todo 计算分数
                //var calcBatches =
                //    usages.Where(t => t.MarkingStatus == (byte)MarkingStatus.AllFinished).Select(t => t.Id).ToList();
            }
            return DResult.FromResult(result);
        }
    }
}
