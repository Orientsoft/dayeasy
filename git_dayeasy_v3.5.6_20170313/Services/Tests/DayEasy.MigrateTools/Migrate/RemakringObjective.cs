
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Dependency;
using DayEasy.Marking.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;

namespace DayEasy.MigrateTools.Migrate
{
    public class RemakringObjective : MigrateBase
    {
        private readonly IDayEasyRepository<TP_MarkingResult> _markingResultRepository;
        private readonly IDayEasyRepository<TP_MarkingDetail> _markingDetailRepository;
        private readonly IDayEasyRepository<TP_ErrorQuestion> _errorQuestionRepository;
        private readonly IDayEasyRepository<TP_MarkingPicture> _pictureRepository;
        private readonly IDayEasyRepository<TC_Usage> _usageRepository;
        private readonly IDayEasyRepository<TS_StuScoreStatistics> _stuScoreRepository;
        private readonly IDayEasyRepository<TS_ClassScoreStatistics> _classScoreRepository;
        private readonly IPaperContract _paperContract;
        private readonly IDayEasyRepository<TP_Paper> _paperRepository;
        private readonly ILogger _logger = LogManager.Logger<RemakringObjective>();
        public RemakringObjective()
        {
            _markingResultRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>();
            _markingDetailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            _errorQuestionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>();
            _pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
            _paperContract = CurrentIocManager.Resolve<IPaperContract>();
            _paperRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>();
            _usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
            _stuScoreRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_StuScoreStatistics>>();
            _classScoreRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_ClassScoreStatistics>>();
        }

        public void ReCalcScores()
        {
            var batches = TxtFileHelper.Batches();
            if (!batches.Any())
            {
                Console.WriteLine("没有批次号数据");
                return;
            }
            foreach (var batch in batches)
            {
                Console.WriteLine("开始更新[{0}]的统计信息", batch);
                var result = UpdateScores(batch.Trim());
                Console.WriteLine(result);
            }
            Console.WriteLine("完成！");
        }

        public void Remarking(bool isFinished = false)
        {
            var pictures = TxtFileHelper.Pictures();
            if (!pictures.Any())
            {
                Console.WriteLine("没有待批阅的题目数据");
                return;
            }
            var papers = new Dictionary<string, PaperDetailDto>();
            foreach (var picture in pictures)
            {
                if (string.IsNullOrWhiteSpace(picture))
                    continue;
                var item = _pictureRepository.FirstOrDefault(t => t.Id == picture);
                if (item == null || string.IsNullOrWhiteSpace(item.SheetAnswers))
                    continue;
                Console.WriteLine("开始重新批阅[{0}]", picture);
                var errorList = new List<TP_ErrorQuestion>();
                var correctList = new List<string>();
                PaperDetailDto paper;
                if (!papers.ContainsKey(item.PaperID))
                {
                    paper = _paperContract.PaperDetailById(item.PaperID).Data;
                    papers.Add(item.PaperID, paper);
                }
                else
                {
                    paper = papers[item.PaperID];
                }
                var sheets = item.SheetAnswers.JsonToObject<List<int[]>>();
                var type = item.AnswerImgType == 0 ? 1 : item.AnswerImgType;

                var questions = new List<PaperQuestionDto>();
                paper.PaperSections.Where(t => t.PaperSectionType == type).OrderBy(s => s.Sort).Foreach(t =>
                {
                    questions.AddRange(t.Questions.Where(q => q.Question.IsObjective).OrderBy(q => q.Sort));
                });

                var details =
                    _markingDetailRepository.Where(t => t.Batch == item.BatchNo && t.StudentID == item.StudentID)
                        .ToList();
                var currentIndex = 0;
                foreach (var question in questions)
                {
                    TP_MarkingDetail detail;
                    var qid = question.Question.Id;
                    var isCorrent = true;
                    if (question.Question.HasSmall && question.Question.Details.Count > 0)
                    {
                        foreach (var smallQuestionDto in question.Question.Details)
                        {
                            detail =
                                details.FirstOrDefault(
                                    t => t.QuestionID == question.Question.Id && t.SmallQID == smallQuestionDto.Id);
                            if (detail == null)
                                continue;
                            var sheet = sheets[currentIndex];
                            if (sheet.All(t => t < 0))
                            {
                                detail.AnswerIDs = null;
                                detail.AnswerContent = null;
                                detail.IsCorrect = false;
                                detail.CurrentScore = 0M;
                            }
                            else
                            {
                                var answerIds = smallQuestionDto.Answers.Where(t => sheet.Contains(t.Sort))
                                    .Select(t => t.Id);
                                detail.AnswerIDs = answerIds.ToJson();

                                detail.AnswerContent = sheet.Aggregate(string.Empty, (c, t) => c + Consts.OptionWords[t]);
                                var answers =
                                    smallQuestionDto.Answers.Where(t => t.IsCorrect).Select(t => t.Sort).ToList();
                                detail.IsCorrect = answers.ArrayEquals(sheet);
                                if (detail.IsCorrect.Value)
                                {
                                    detail.CurrentScore = detail.Score;
                                }
                                else
                                {
                                    isCorrent = false;
                                    if (sheet.All(t => answers.Contains(t)) && question.Question.Type == 3)
                                        detail.CurrentScore = detail.Score / 2M;
                                    else
                                        detail.CurrentScore = 0;
                                }
                            }
                            currentIndex++;
                        }
                    }
                    else
                    {
                        detail = details.FirstOrDefault(t => t.QuestionID == question.Question.Id);
                        if (detail == null)
                            continue;
                        var sheet = sheets[currentIndex];
                        if (sheet.All(t => t < 0))
                        {
                            detail.AnswerIDs = null;
                            detail.AnswerContent = null;
                            detail.IsCorrect = false;
                            detail.CurrentScore = 0M;
                        }
                        else
                        {
                            var answerIds = question.Question.Answers.Where(t => sheet.Contains(t.Sort))
                                .Select(t => t.Id);
                            detail.AnswerIDs = answerIds.ToJson();
                            detail.AnswerContent = sheet.Aggregate(string.Empty, (c, t) => c + Consts.OptionWords[t]);
                            var answers = question.Question.Answers.Where(t => t.IsCorrect).Select(t => t.Sort).ToList();
                            detail.IsCorrect = answers.ArrayEquals(sheet);
                            if (detail.IsCorrect.Value)
                            {
                                detail.CurrentScore = detail.Score;
                            }
                            else
                            {
                                isCorrent = false;
                                if (sheet.All(t => answers.Contains(t)) && question.Question.Type == 3)
                                    detail.CurrentScore = detail.Score / 2M;
                                else
                                    detail.CurrentScore = 0;
                            }
                        }
                        currentIndex++;
                    }
                    if (isCorrent)
                    {
                        correctList.Add(qid);
                    }
                    else
                    {
                        if (isFinished)
                        {
                            if (!_errorQuestionRepository.Exists(t =>
                                t.Batch == item.BatchNo && t.StudentID == item.StudentID &&
                                t.QuestionID == qid))
                            {
                                errorList.Add(new TP_ErrorQuestion
                                {
                                    Id = IdHelper.Instance.Guid32,
                                    AddedAt = Clock.Now,
                                    Batch = item.BatchNo,
                                    PaperID = item.PaperID,
                                    QuestionID = question.Question.Id,
                                    QType = question.Question.Type,
                                    PaperTitle = paper.PaperBaseInfo.PaperTitle,
                                    SubjectID = paper.PaperBaseInfo.SubjectId,
                                    StudentID = item.StudentID,
                                    Stage = paper.PaperBaseInfo.Stage,
                                    Status = (byte)ErrorQuestionStatus.Normal,
                                    VariantCount = 0
                                });
                            }
                        }
                    }
                }
                Console.WriteLine("提交数据");
                _markingDetailRepository.Update(d => new
                {
                    d.AnswerIDs,
                    d.AnswerContent,
                    d.IsCorrect,
                    d.CurrentScore
                }, details.ToArray());
                if (isFinished)
                {
                    // 错题
                    if (errorList.Any())
                        _errorQuestionRepository.Insert(errorList);
                    if (correctList.Any())
                    {
                        _errorQuestionRepository.Delete(
                            t =>
                                t.Batch == item.BatchNo && t.StudentID == item.StudentID &&
                                correctList.Contains(t.QuestionID));
                    }
                }
            }
            Console.WriteLine("完成");
        }

        /// <summary> 已结束阅卷，重新批阅客观题 </summary>
        public void RemarkingObjective()
        {
            var batches = TxtFileHelper.Batches();
            if (!batches.Any())
            {
                Console.WriteLine("没有发布批次");
                return;
            }
            var paperDict = new Dictionary<string, PaperDetailDto>();
            var batchList = _usageRepository.Where(t => batches.Contains(t.Id))
                .Select(t => new { t.Id, t.SourceID })
                .ToList()
                .ToDictionary(k => k.Id, v => v.SourceID);
            foreach (var batch in batches)
            {
                if (!batchList.ContainsKey(batch))
                    continue;
                var paperId = batchList[batch];
                PaperDetailDto paper;
                if (!paperDict.ContainsKey(paperId))
                {
                    var result = _paperContract.PaperDetailById(paperId);
                    if (!result.Status || result.Data == null)
                        continue;
                    paper = result.Data;
                    paperDict.Add(paperId, paper);
                }
                else
                {
                    paper = paperDict[paperId];
                }
                if (paper == null)
                    continue;
                try
                {
                    RemarkingQuestions(batch, paper);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }
            Console.WriteLine("完成");
        }

        private void RemarkingQuestions(string batch, PaperDetailDto paper)
        {
            var updateDetails = new List<TP_MarkingDetail>();
            var errorList = new List<TP_ErrorQuestion>();
            var deleteList = new List<TP_ErrorQuestion>();
            var questions =
                paper.PaperSections.SelectMany(s => s.Questions.Where(q => q.Question.IsObjective).ToList())
                    .ToList();
            foreach (var question in questions)
            {
                var qid = question.Question.Id;
                var details = _markingDetailRepository.Where(t => t.Batch == batch && t.QuestionID == qid).ToList();

                if (question.Question.HasSmall && question.Question.Details.Count > 0)
                {
                    foreach (var smallQuestionDto in question.Question.Details)
                    {
                        var list = details.Where(t => t.SmallQID == smallQuestionDto.Id);
                        foreach (var detail in list)
                        {
                            if (detail == null)
                                continue;
                            var answers = (JsonHelper.JsonList<string>(detail.AnswerIDs) ?? new List<string>()).ToList();
                            var correctAnswers = smallQuestionDto.Answers.Where(t => t.IsCorrect).Select(t => t.Id).ToList();
                            var correct = answers.Any() && answers.ArrayEquals(correctAnswers);
                            var score = correct
                                ? detail.Score
                                : (question.Question.Type == 3 && answers.All(t => correctAnswers.Contains(t))
                                    ? detail.Score / 2M
                                    : 0);
                            if (detail.IsCorrect != correct || detail.CurrentScore != score)
                            {
                                detail.IsCorrect = correct;
                                detail.CurrentScore = score;
                                updateDetails.Add(detail);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var detail in details)
                    {
                        if (detail == null)
                            continue;
                        var studentId = detail.StudentID;
                        var answers = (JsonHelper.JsonList<string>(detail.AnswerIDs) ?? new List<string>()).ToList();
                        var correctAnswers = question.Question.Answers.Where(t => t.IsCorrect).Select(t => t.Id).ToList();
                        var isCorrect = answers.Any() && answers.ArrayEquals(correctAnswers);
                        var score = isCorrect
                            ? detail.Score
                            : (question.Question.Type == 3 && answers.All(t => correctAnswers.Contains(t))
                                ? detail.Score / 2M
                                : 0);
                        if (detail.IsCorrect != isCorrect || detail.CurrentScore != score)
                        {
                            detail.IsCorrect = isCorrect;
                            detail.CurrentScore = score;
                            updateDetails.Add(detail);
                        }
                        if (detail.IsCorrect.Value && !isCorrect)
                        {
                            //新增
                            errorList.Add(new TP_ErrorQuestion
                            {
                                Id = IdHelper.Instance.Guid32,
                                AddedAt = Clock.Now,
                                Batch = batch,
                                PaperID = paper.PaperBaseInfo.Id,
                                QuestionID = question.Question.Id,
                                QType = question.Question.Type,
                                PaperTitle = paper.PaperBaseInfo.PaperTitle,
                                SubjectID = paper.PaperBaseInfo.SubjectId,
                                StudentID = studentId,
                                Stage = paper.PaperBaseInfo.Stage,
                                Status = (byte)ErrorQuestionStatus.Normal,
                                VariantCount = 0
                            });
                        }
                        else if (!detail.IsCorrect.Value && isCorrect)
                        {
                            //删除
                            var item =
                                _errorQuestionRepository.FirstOrDefault(
                                    t => t.Batch == batch && t.StudentID == studentId && t.QuestionID == qid);
                            if (item != null)
                                deleteList.Add(item);
                        }
                    }
                }
            }
            Console.WriteLine($"更新detail:{updateDetails.Count}条");
            foreach (var detail in updateDetails)
            {
                _markingDetailRepository.Update(d => new
                {
                    d.IsCorrect,
                    d.CurrentScore
                }, detail);
            }
            // 错题
            Console.WriteLine($"新增errorQuestion:{errorList.Count}条");
            if (errorList.Any())
                _errorQuestionRepository.Insert(errorList);

            Console.WriteLine($"删除errorQuestion:{deleteList.Count}条");
            if (deleteList.Any())
            {
                foreach (var item in deleteList)
                {
                    _errorQuestionRepository.Delete(item);
                }
            }
            Console.WriteLine($"{batch}更新完成");
        }

        private void ErrorQuestionMission()
        {
            //if (isCorrect)
            //{
            //    var item = _errorQuestionRepository.FirstOrDefault(t =>
            //        t.Batch == batch && t.StudentID == studentId &&
            //        t.QuestionID == qid);
            //    if (item != null)
            //        deleteList.Add(item);
            //}
            //else
            //{
            //    if (!_errorQuestionRepository.Exists(t =>
            //        t.Batch == batch && t.StudentID == studentId &&
            //        t.QuestionID == qid))
            //    {
            //        var paper = _paperRepository.Load(paperId);
            //        errorList.Add(new TP_ErrorQuestion
            //        {
            //            Id = IdHelper.Instance.Guid32,
            //            AddedAt = Clock.Now,
            //            Batch = batch,
            //            PaperID = paperId,
            //            QuestionID = question.Question.Id,
            //            QType = question.Question.Type,
            //            PaperTitle = paper.PaperTitle,
            //            SubjectID = paper.SubjectID,
            //            StudentID = studentId,
            //            Stage = paper.Stage,
            //            Status = (byte)ErrorQuestionStatus.Normal,
            //            VariantCount = 0
            //        });
            //    }
            //}
        }

        private DResult UpdateScores(string batch)
        {
            var usage = _usageRepository.FirstOrDefault(t => t.Id == batch);
            if (usage == null)
                return DResult.Error("该发布批次不存在！");
            var results = CalcResults(usage.SourceID, batch);
            if (results == null || !results.Any())
                return DResult.Error("阅卷结果集不存在！");
            try
            {
                MarkingTask.UpdateRightIconAndErrorObjMission(batch, usage.SourceID, false, false, (Console.WriteLine))
                    .Wait();
                //UpdateErrorObjective(batch, usage.SourceID);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return DResult.Error(ex.Message);
            }

            var classScore =
                _classScoreRepository.FirstOrDefault(
                    c => c.Batch == batch && c.PaperId == usage.SourceID && c.ClassId == usage.ClassId);
            var stuScores = _stuScoreRepository
                .Where(s => s.Batch == batch && s.PaperId == usage.SourceID && s.ClassId == usage.ClassId).ToList();
            if (classScore == null || !stuScores.Any())
                return DResult.Error("没有查询到分数统计资料");

            var paperResult = _paperContract.PaperDetailById(usage.SourceID, false);
            if (!paperResult.Status)
                return DResult.Error("没有查询到试卷资料");
            var paper = paperResult.Data.PaperBaseInfo;

            decimal scoreA = 0M, scoreB = 0M;

            if (paper.PaperType == (byte)PaperType.AB)
            {
                scoreA = paper.PaperScores.TScoreA;
                scoreB = paper.PaperScores.TScoreB;
            }

            #region TS_StuScoreStatistics

            foreach (var result in results)
            {
                var item = stuScores.FirstOrDefault(s => s.StudentId == result.StudentID);
                if (item == null)
                    continue;
                var sectionScores = result.SectionScores.JsonToObject<PaperScoresDto>();

                item.CurrentScore = result.TotalScore;
                item.SectionAScore = sectionScores.TScoreA;
                item.SectionBScore = sectionScores.TScoreB;
            }
            stuScores.ForEach(s =>
            {
                s.CurrentSort = stuScores.Count(t => t.CurrentScore > s.CurrentScore) + 1;
            });

            #endregion

            #region TS_ClassScoreStatistics

            List<decimal> currentScores = stuScores.Select(m => m.CurrentScore).ToList(),
                aScores = new List<decimal>(),
                bScores = new List<decimal>();

            if (paper.PaperType == (byte)PaperType.AB)
            {
                aScores = stuScores.Select(s => s.SectionAScore).ToList();
                bScores = stuScores.Select(s => s.SectionBScore).ToList();
            }
            //班级统计数据
            InitClassScoreStatistics(classScore,
                currentScores, aScores, bScores,
                paper.PaperScores.TScore, scoreA, scoreB,
                paper.PaperType == (byte)PaperType.AB);

            #endregion

            var code = _markingResultRepository.UnitOfWork.Transaction(() =>
            {
                //更新Result
                _markingResultRepository.Update(t => new
                {
                    t.TotalScore,
                    t.SectionScores,
                    t.ErrorQuestionCount
                }, results.ToArray());

                //学生统计
                _stuScoreRepository.Update(s => new
                {
                    s.CurrentScore,
                    s.CurrentSort,
                    s.SectionAScore,
                    s.SectionBScore
                }, stuScores.ToArray());

                //班级统计
                _classScoreRepository.Update(c => new
                {
                    c.AverageScore,
                    c.TheHighestScore,
                    c.TheLowestScore,
                    c.ScoreGroups,
                    c.SectionScores,
                    c.SectionScoreGroups
                }, classScore);
            });
            return DResult.FromResult(code);
        }

        /// <summary>
        /// 计算Result，总分/错题数/模块得分
        /// </summary>
        /// <returns></returns>
        private List<TP_MarkingResult> CalcResults(string paperId, string batch, long teacherId = 0)
        {
            try
            {
                //阅卷结果表
                var markResults = _markingResultRepository.Where(u => u.Batch == batch && u.PaperID == paperId).ToList();

                if (markResults.Count < 1)
                    return null;

                const string sql =
                    @"select tempa.StudentID,isnull(Score,0)as Score,isnull(ErrorCount,0)as ErrorCount,isnull(ScoreA,0)as ScoreA,isnull(ScoreB,0)as ScoreB from 
                        (
                        select StudentID,sum(CurrentScore)as Score from TP_MarkingDetail where Batch=@batch
                        group by StudentID)tempa
                        left join 
                        (select StudentID,count(*) as ErrorCount from TP_MarkingDetail where Batch=@batch 
                        and IsCorrect is not null and IsCorrect=0
                        group by StudentID)tempb
                        on tempa.StudentID=tempb.StudentID
                        left join
                        (select StudentID,sum(CurrentScore)as ScoreA from TP_MarkingDetail
                        join TP_PaperContent
                        on TP_MarkingDetail.PaperID=TP_PaperContent.PaperID and TP_PaperContent.PaperSectionType=1 
                        and TP_MarkingDetail.QuestionID=TP_PaperContent.QuestionID 
                        where Batch=@batch 
                        group by StudentID)tempc
                        on tempc.StudentID=tempa.StudentID
                        left join
                        (select StudentID,sum(CurrentScore)as ScoreB from TP_MarkingDetail
                        join TP_PaperContent
                        on TP_MarkingDetail.PaperID=TP_PaperContent.PaperID and TP_PaperContent.PaperSectionType=2
                        and TP_MarkingDetail.QuestionID=TP_PaperContent.QuestionID 
                        where Batch=@batch 
                        group by StudentID)tempd
                        on tempd.StudentID=tempa.StudentID";

                var scores =
                    _markingDetailRepository.UnitOfWork.SqlQuery<SearchDetailDataDto>(sql,
                        new SqlParameter("@batch", batch)).ToList();
                if (scores.Count < 1)
                    return null;

                foreach (var result in markResults)
                {
                    var tempScore = scores.SingleOrDefault(u => u.StudentID == result.StudentID);
                    if (tempScore == null)
                    {
                        continue;
                    }
                    result.ErrorQuestionCount = tempScore.ErrorCount;
                    result.TotalScore = tempScore.Score;

                    var section = new PaperScoresDto
                    {
                        TScore = tempScore.Score,
                        TScoreA = tempScore.ScoreA,
                        TScoreB = tempScore.ScoreB
                    };
                    result.SectionScores = section.ToJson();
                    if (teacherId <= 0)
                        continue;
                    result.IsFinished = true;
                    result.MarkingBy = teacherId;
                    result.MarkingTime = Clock.Now;
                }

                return markResults;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //初始化班级统计数据
        private void InitClassScoreStatistics(TS_ClassScoreStatistics item, List<decimal> currentScores,
            List<decimal> aScores, List<decimal> bScores, decimal scoreTotal, decimal scoreA, decimal scoreB, bool isAb)
        {
            if (item == null) return;

            //整卷最高、低、平均分
            item.AverageScore = decimal.Round(currentScores.Average(), 2);
            item.TheHighestScore = currentScores.Max();
            item.TheLowestScore = currentScores.Min();

            //整卷分数段
            var scoreGroup = new int[(int)(scoreTotal / 10) + 1];
            currentScores.ForEach(s => { scoreGroup[(int)(s / 10)]++; });
            item.ScoreGroups = JsonHelper.ToJson(MakeScoreGroup(scoreGroup, scoreTotal));

            if (!isAb) return;

            //AB各卷最高、低、平均分
            item.SectionScores = JsonHelper.ToJson(new ReportSectionScoresDto
            {
                AAv = decimal.Round(aScores.Average(), 2),
                Ah = aScores.Max(),
                Al = aScores.Min(),
                BAv = decimal.Round(bScores.Average(), 2),
                Bh = bScores.Max(),
                Bl = bScores.Min()
            });

            //A卷分数段
            var scoreGroupA = new int[(int)(scoreA / 10) + 1];
            aScores.ForEach(s => { scoreGroupA[(int)(s / 10)]++; });
            //B卷分数段
            var scoreGroupB = new int[(int)(scoreB / 10) + 1];
            bScores.ForEach(s => { scoreGroupB[(int)(s / 10)]++; });

            item.SectionScoreGroups = JsonHelper.ToJson(new List<List<ScoreGroupsDto>>
            {
                MakeScoreGroup(scoreGroupA, scoreA),
                MakeScoreGroup(scoreGroupB, scoreB)
            });
        }

        //构造分数段
        private List<ScoreGroupsDto> MakeScoreGroup(int[] scoreGroup, decimal score)
        {
            var scoreGroups = new List<ScoreGroupsDto>();
            for (int i = 0; i < scoreGroup.Length; i++)
            {
                var group = new ScoreGroupsDto();

                var startScore = 10 * i;
                var endScore = startScore + 9 >= score
                    ? score.ToString("0.#")
                    : (startScore + 9).ToString(CultureInfo.InvariantCulture);
                group.ScoreInfo = startScore >= score
                    ? startScore.ToString(CultureInfo.InvariantCulture)
                    : startScore + "-" + endScore;
                group.Count = scoreGroup[i];

                scoreGroups.Add(group);
            }

            return scoreGroups;
        }
    }
}
