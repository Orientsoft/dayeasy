using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Marking.Services.Helper;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace DayEasy.Marking.Services
{
    public partial class MarkingService
    {
        /// <summary>
        /// 阅卷的事务处理
        /// 1、2不能同时为空
        /// 1.更新阅卷信息
        /// 2.更新批阅痕迹
        /// </summary>
        int UpdateMarking(List<TP_MarkingDetail> details, long teacherId, TP_MarkingPicture picture = null)
        {
            if ((details == null || !details.Any()) && picture == null)
                return 1;
            try
            {
                var now = Clock.Now;
                const string sql = @"update [TP_MarkingResult] set [MarkingTime] = @time where [MarkingID] = @id ";

                return UnitOfWork.Transaction(() =>
                {
                    //更新批阅痕迹
                    if (picture != null)
                    {
                        MarkingPictureRepository.Update(p => new { p.LastMarkingTime, p.RightAndWrong, p.Marks }, picture);
                        var dist = JointPictureDistributionRepository.FirstOrDefault(t => t.PictureId == picture.Id
                                                                                          && t.TeacherId == teacherId);
                        if (dist != null)
                        {
                            dist.LastMarkingTime = Clock.Now;
                            JointPictureDistributionRepository.Update(p => new
                            {
                                p.LastMarkingTime
                            }, dist);
                        }
                    }

                    //更新阅卷信息
                    if (details != null && details.Any())
                    {
                        //批阅时间
                        var rids = details.Select(t => t.MarkingID).Distinct();
                        rids.Foreach(id => MarkingResultRepository.UnitOfWork.SqlExecute(
                            TransactionalBehavior.DoNotEnsureTransaction,
                            sql, new SqlParameter("@time", now), new SqlParameter("@id", id)));

                        //批阅详情
                        MarkingDetailRepository.Update(t => new
                        {
                            t.IsFinished,
                            t.IsCorrect,
                            t.CurrentScore,
                            t.MarkingBy,
                            t.MarkingAt,
                            t.AnswerIDs,
                            t.AnswerContent
                        }, details.ToArray());
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return -1;
            }
        }

        /// <summary>
        /// 提交试卷的事务处理
        /// 1.生成答卷记录
        /// </summary>
        /// <param name="result"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        int GenerateResult(IEnumerable<TP_MarkingDetail> details, TP_MarkingResult result = null)
        {
            try
            {
                if (result != null)
                {
                    //判断重复提交
                    var exist = MarkingResultRepository.Exists(t =>
                            t.Batch == result.Batch && t.PaperID == result.PaperID &&
                            t.StudentID == result.StudentID);
                    if (exist) return -1;
                }
                return UnitOfWork.Transaction(() =>
                {
                    if (result != null)
                        MarkingResultRepository.Insert(result);
                    MarkingDetailRepository.Insert(details);
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return -1;
            }
        }

        /// <summary>
        /// 结束阅卷，更改usage/result/detail状态
        /// </summary>
        /// <returns></returns>
        int FinishMarking(TC_Usage usage, long teacherId, byte status)
        {
            try
            {
                //计算Result，总分/错题数/模块得分
                var results = CalcResults(usage.SourceID, usage.Id, teacherId);

                //处理报表统计数据
                var scoreStatistics = ReportStatisticsMission(usage, results);

                return UnitOfWork.Transaction(() =>
                {
                    //更新发布记录状态
                    UsageRepository.UnitOfWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, "update TC_Usage set MarkingStatus=@status  where Batch=@batch", new SqlParameter("@status", status), new SqlParameter("@batch", usage.Id));
                    //更新Details
                    MarkingDetailRepository.UnitOfWork.SqlExecute(TransactionalBehavior.DoNotEnsureTransaction, "update TP_MarkingDetail set IsFinished=1 where Batch=@batch and PaperID=@paperId and IsFinished=0", new SqlParameter("@batch", usage.Id), new SqlParameter("@paperId", usage.SourceID));
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
                    if (scoreStatistics != null)
                    {
                        if (scoreStatistics.ClassScoreStatisticses != null)
                        {
                            ClassScoreStatisticsRepository.Insert(scoreStatistics.ClassScoreStatisticses);
                        }
                        if (scoreStatistics.StuScoreStatisticses.Count > 0)
                        {
                            StuScoreStatisticsRepository.Insert(scoreStatistics.StuScoreStatisticses);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return -1;
            }
        }

        #region 计算Result，总分/错题数/模块得分
        /// <summary>
        /// 计算Result，总分/错题数/模块得分
        /// </summary>
        /// <returns></returns>
        List<TP_MarkingResult> CalcResults(string paperId, string batch, long teacherId = 0)
        {
            try
            {
                //阅卷结果表
                var markResults = MarkingResultRepository.Where(u => u.Batch == batch && u.PaperID == paperId).ToList();

                if (markResults.Count < 1)
                    return null;

                const string sql = @"select tempa.StudentID,isnull(Score,0)as Score,isnull(ErrorCount,0)as ErrorCount,isnull(ScoreA,0)as ScoreA,isnull(ScoreB,0)as ScoreB from 
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

                var scores = MarkingDetailRepository.UnitOfWork.SqlQuery<SearchDetailDataDto>(sql, new SqlParameter("@batch", batch)).ToList();
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
                _logger.Error(ex.Message, ex);
                return null;
            }
        }
        #endregion

        #region 报表统计信息处理 - Statistic.UpdateScoreStatistics 含生成统计数据
        /// <summary>
        /// 报表统计信息处理
        /// </summary>
        ScoreStatistics ReportStatisticsMission(TC_Usage usage, List<TP_MarkingResult> markResults)
        {
            if (markResults == null || markResults.Count < 1)
                return null;
            var paper = PaperRepository.SingleOrDefault(u => u.Id == usage.SourceID);
            if (paper == null) return null;
            //重复判断
            if (ClassScoreStatisticsRepository.Exists(c => c.Batch == usage.Id && c.PaperId == paper.Id))
                return null;

            var result = new ScoreStatistics();

            #region 处理班级统计信息

            var paperScores = JsonHelper.Json<PaperScoresDto>(paper.PaperScores);
            if (paperScores != null && paperScores.TScore > 0) //有分才记录班级统计信息
            {
                var classStatistics = new TS_ClassScoreStatistics
                {
                    Id = IdHelper.Instance.Guid32,
                    Batch = usage.Id,
                    PaperId = paper.Id,
                    ClassId = usage.ClassId,
                    SubjectId = usage.SubjectId,
                    AddedAt = usage.AddedAt,
                    AddedBy = usage.UserId,
                    Status = (byte)ClassStatisticsStatus.Normal
                };

                #region Init TS_ClassScoreStatistics

                List<decimal> currentScores = markResults.Select(m => m.TotalScore).ToList(),
                    aScores = new List<decimal>(),
                    bScores = new List<decimal>();
                decimal scoreA = 0M, scoreB = 0M;

                if (paper.PaperType == (byte)PaperType.AB)
                {
                    var sections = markResults.Select(u => JsonHelper.Json<PaperScoresDto>(u.SectionScores)).ToList();
                    if (sections.Any())
                    {
                        aScores = sections.Select(s => s.TScoreA).ToList();
                        bScores = sections.Select(s => s.TScoreB).ToList();
                    }
                    scoreA = paperScores.TScoreA;
                    scoreB = paperScores.TScoreB;
                }

                InitClassScoreStatistics(classStatistics,
                    currentScores, aScores, bScores,
                    paperScores.TScore, scoreA, scoreB,
                    paper.PaperType == (byte)PaperType.AB);

                #endregion

                result.ClassScoreStatisticses = classStatistics;
            }
            #endregion

            #region 处理个人统计信息

            var stuScoreStatisticsList = new List<TS_StuScoreStatistics>();
            markResults.ForEach(u =>
            {
                var perScoreStatistics = new TS_StuScoreStatistics
                {
                    Id = IdHelper.Instance.Guid32,
                    Batch = usage.Id,
                    PaperId = paper.Id,
                    ClassId = usage.ClassId,
                    StudentId = u.StudentID,
                    SubjectId = usage.SubjectId,
                    CurrentScore = u.TotalScore,
                    ErrorQuCount = u.ErrorQuestionCount,
                    AddedAt = usage.AddedAt,
                    AddedBy = usage.UserId,
                    Status = (byte)StuStatisticsStatus.Normal
                };

                var scores = JsonHelper.Json<PaperScoresDto>(u.SectionScores);
                if (scores != null)
                {
                    perScoreStatistics.SectionAScore = scores.TScoreA;
                    perScoreStatistics.SectionBScore = scores.TScoreB;
                }

                var sort = markResults.Count(s => s.TotalScore > u.TotalScore);
                perScoreStatistics.CurrentSort = sort + 1;

                stuScoreStatisticsList.Add(perScoreStatistics);
            });

            result.StuScoreStatisticses = stuScoreStatisticsList;

            #endregion

            return result;
        }

        //构造分数段
        List<ScoreGroupsDto> MakeScoreGroup(int[] scoreGroup, decimal score)
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

        #endregion

        #region 初始化班级统计数据
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

        #endregion
    }

}
