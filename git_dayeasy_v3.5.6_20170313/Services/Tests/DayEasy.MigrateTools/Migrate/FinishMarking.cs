
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Marking.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.MigrateTools.Migrate
{
    public class FinishMarking : MigrateBase
    {
        private readonly ILogger _logger = LogManager.Logger<FinishMarking>();

        public void Finish()
        {
            try
            {
                var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
                var list = TxtFileHelper.Batches();
                Console.WriteLine("共{0}条批次号", list.Count());
                foreach (var str in list)
                {
                    Console.WriteLine("批次号[{0}]", str);
                    var usage = usageRepository.Load(str);
                    if (usage == null)
                        continue;
                    Console.WriteLine("开始完成阅卷！");
                    //CurrentIocManager.Resolve<IMarkingContract>()
                    //    .Finished(str, usage.SourceID, MarkingStatus.AllFinished, false, false, usage.UserId);
                    //                    MarkingTask.FinishMission(usage.Id, usage.SourceID, usage.ClassId, usage.UserId, usage.SubjectId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            Console.WriteLine("完成");
        }

        /// <summary> 根据标记重新批阅 </summary>
        public void ReMarkingPicture()
        {
            try
            {
                var pictureIds = TxtFileHelper.Pictures();
                var pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
                var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
                var resultRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>();
                var errorRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>();
                var paperRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>();
                var questionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();

                foreach (var pictureId in pictureIds)
                {
                    Console.WriteLine("开始重新批阅图片ID[{0}]", pictureId);
                    var updateDetails = new List<TP_MarkingDetail>();
                    var errorQuestions = new List<TP_ErrorQuestion>();
                    var picture = pictureRepository.Load(pictureId);
                    if (picture == null || string.IsNullOrWhiteSpace(picture.RightAndWrong))
                        continue;
                    var marks = picture.RightAndWrong.JsonToObject2<List<MkSymbol>>();
                    if (marks == null || !marks.Any())
                        continue;
                    var result =
                        resultRepository.FirstOrDefault(
                            t => t.Batch == picture.BatchNo && t.StudentID == picture.StudentID);
                    var paper = paperRepository.FirstOrDefault(t => t.Id == picture.PaperID);
                    if (result == null)
                        continue;
                    var totalScore = result.TotalScore;
                    var sectionScore = result.SectionScores.JsonToObject<MakePaperScoresDto>();
                    var details =
                        detailRepository.Where(d => d.Batch == picture.BatchNo && d.StudentID == picture.StudentID);
                    foreach (var mark in marks)
                    {
                        if (mark.Score <= 0)
                            continue;
                        Console.WriteLine("批阅题目:{0}", mark.QuestionId);
                        //记录批阅分数
                        var score = (decimal)mark.Score;
                        var detail = details.FirstOrDefault(t => t.QuestionID == mark.QuestionId);
                        if (detail == null || detail.Score == (detail.CurrentScore - score))
                            continue;
                        //计算分数
                        var currentScore = detail.Score - score;
                        var diffScore = (detail.CurrentScore - currentScore);

                        totalScore -= diffScore;
                        sectionScore.TScore = totalScore;
                        if (picture.AnswerImgType == 0 || picture.AnswerImgType == 1)
                        {
                            sectionScore.TScoreA -= diffScore;
                        }
                        else
                        {
                            sectionScore.TScoreB -= diffScore;
                        }

                        detail.CurrentScore = currentScore;
                        if (detail.IsCorrect.HasValue && detail.IsCorrect.Value)
                        {
                            detail.IsCorrect = false;
                            var question = questionRepository.FirstOrDefault(t => t.Id == detail.QuestionID);
                            //错题库
                            errorQuestions.Add(new TP_ErrorQuestion
                            {
                                Id = IdHelper.Instance.Guid32,
                                PaperID = paper.Id,
                                Batch = detail.Batch,
                                QuestionID = detail.QuestionID,
                                StudentID = detail.StudentID,
                                PaperTitle = paper.PaperTitle,
                                SubjectID = paper.SubjectID,
                                Stage = paper.Stage,
                                QType = question.QType,
                                AddedAt = picture.AddedAt,
                                Status = (byte)ErrorQuestionStatus.Normal,
                                VariantCount = 0
                            });
                            result.ErrorQuestionCount++;
                        }
                        updateDetails.Add(detail);
                    }
                    //更新detail
                    detailRepository.Update(d => new
                    {
                        d.CurrentScore,
                        d.IsCorrect
                    }, updateDetails.ToArray());

                    result.TotalScore = totalScore;
                    result.SectionScores = sectionScore.ToJson();
                    //错题数
                    resultRepository.Update(t => new
                    {
                        t.ErrorQuestionCount,
                        t.TotalScore,
                        t.SectionScores
                    }, result);

                    //错题库
                    if (errorQuestions.Any())
                    {
                        errorRepository.Insert(errorQuestions);
                    }
                    Console.WriteLine("批阅[{0}]成功", pictureId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            Console.WriteLine("完成");
        }

        public void SetRightIconAndScoreMark()
        {
            var list = TxtFileHelper.Batches();
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var batchList = usageRepository.Where(t => list.Contains(t.Id))
                .Select(t => new { t.Id, t.SourceID })
                .ToList();
            foreach (var batch in batchList)
            {
                Console.WriteLine($"开始更新{batch}");
                MarkingTask.UpdateRightIconAndErrorObjMission(batch.Id, batch.SourceID, true, true, (Console.WriteLine)).Wait();
            }
            Console.WriteLine("完成");
        }

        public void SendMessage()
        {
            var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            var batches = TxtFileHelper.Batches();
            var models = usageRepository.Where(t => batches.Contains(t.Id))
                .Select(t => new { t.Id, t.ClassId, t.UserId }).ToList();

            foreach (var model in models)
            {
                Console.WriteLine($"{model.Id},{model.ClassId}");
                SendMessage(model.Id, model.ClassId, model.UserId);
            }
            Console.WriteLine("完成");
        }

        private void SendMessage(string batch, string classId, long userId)
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

    }
}
