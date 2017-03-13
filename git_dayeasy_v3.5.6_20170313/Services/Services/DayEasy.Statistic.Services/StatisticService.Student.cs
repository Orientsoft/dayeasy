
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Statistic.Services
{
    public partial class StatisticService
    {

        public IDayEasyRepository<TP_Paper> PaperRepository { private get; set; }

        #region 辅助方法

        /// <summary> 分数段转换 </summary>
        /// <param name="reportDto"></param>
        /// <param name="scoreGroups"></param>
        /// <param name="score"></param>
        /// <param name="point"></param>
        private static void ConverSegments(StudentReportDto reportDto, IEnumerable<ScoreGroupsDto> scoreGroups, decimal score,
            decimal point = 0.6M)
        {
            var segments = new Dictionary<DKeyValue<decimal, decimal>, int>();
            foreach (var dto in scoreGroups)
            {
                decimal end;
                var scores = dto.ScoreInfo.Split('-');
                var start = end = scores[0].To(0M);
                if (scores.Length == 2)
                {
                    end = scores[1].To(0M);
                }
                segments.Add(new DKeyValue<decimal, decimal>(start, end), dto.Count);
            }
            reportDto.TotalScore = segments.Keys.Max(t => t.Value);
            var low = reportDto.TotalScore * point;
            var lastSegment = low;
            var count = 0;
            foreach (var segment in segments.OrderByDescending(t => t.Key.Key))
            {
                if (segment.Key.Key >= low)
                {
                    reportDto.Segments.Add(new ReportSegmentDto
                    {
                        Segment =
                            segment.Key.Key == segment.Key.Value
                                ? segment.Key.Key.ToString("0.#")
                                : "{0}-{1}".FormatWith(segment.Key.Key, segment.Key.Value),
                        Count = segment.Value,
                        ContainsMe = score >= 0 && score >= segment.Key.Key && score <= segment.Key.Value
                    });
                    lastSegment = segment.Key.Key;
                }
                else
                {
                    count += segment.Value;
                }
            }
            reportDto.Segments.Add(new ReportSegmentDto
            {
                Segment = "{0:0.#}以下".FormatWith(lastSegment),
                Count = count,
                ContainsMe = score >= 0 && score <= low
            });
        }

        /// <summary> 班级排名变化 </summary>
        /// <param name="classScore"></param>
        /// <param name="studentId"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        private int? RankChange(TS_ClassScoreStatistics classScore, long studentId, int rank)
        {
            if (classScore == null || rank < 0)
                return null;
            var prevBatch =
                ClassScoreStatisticsRepository.Where(
                    t =>
                        t.SubjectId == classScore.SubjectId
                        && t.ClassId == classScore.ClassId
                        && t.AddedAt < classScore.AddedAt)
                    .OrderByDescending(t => t.AddedAt)
                    .Select(t => t.Batch)
                    .FirstOrDefault();
            if (prevBatch == null)
                return null;
            var prevRank = StuScoreStatisticsRepository.Where(t => t.Batch == prevBatch && t.StudentId == studentId)
                .Select(t => t.CurrentSort)
                .FirstOrDefault();
            if (prevRank > 0)
            {
                return prevRank - rank;
            }
            return null;
        }

        /// <summary> 年级排名 </summary>
        /// <param name="batch"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        private ReportRankDto GradeRank(string batch, long studentId)
        {
            var dto = new ReportRankDto();
            var usage = UsageRepository.Load(batch);
            if (usage == null || string.IsNullOrWhiteSpace(usage.JointBatch))
                return dto;
            var isAb = PaperRepository.Where(t => t.Id == usage.SourceID)
                .Select(t => t.PaperType)
                .FirstOrDefault() == (byte)PaperType.AB;
            var batches = UsageRepository.Where(
                t => t.JointBatch == usage.JointBatch && t.MarkingStatus == (byte)MarkingStatus.AllFinished)
                .Select(t => t.Id)
                .ToList();
            var ranks = StuScoreStatisticsRepository.Where(t => batches.Contains(t.Batch))
                .Select(t => new
                {
                    t.StudentId,
                    t.CurrentSort,
                    t.CurrentScore,
                    t.SectionAScore,
                    t.SectionBScore
                }).ToList();
            dto.Average = Math.Round(ranks.Average(t => t.CurrentScore), 2, MidpointRounding.AwayFromZero);
            if (isAb)
            {
                dto.AverageA = Math.Round(ranks.Average(t => t.SectionAScore), 2, MidpointRounding.AwayFromZero);
                dto.AverageB = Math.Round(ranks.Average(t => t.SectionBScore), 2, MidpointRounding.AwayFromZero);
            }
            var rank = ranks.FirstOrDefault(t => t.StudentId == studentId);
            if (rank != null)
            {
                dto.Rank = ranks.Count(t => t.CurrentScore > rank.CurrentScore) + 1;
                if (ranks.Count == 1)
                    dto.Percent = 100M;
                else
                {
                    dto.Percent = Math.Round(
                        ranks.Count(t => t.CurrentScore < rank.CurrentScore) * 100M / (ranks.Count - 1),
                        MidpointRounding.AwayFromZero);
                }
                //:todo 规则？
                dto.Change = null;
            }
            return dto;
        }

        #endregion

        public DResult<StudentScoreDto> StudentScore(long studentId, string batch)
        {
            if (studentId <= 0 || string.IsNullOrWhiteSpace(batch))
                return DResult.Error<StudentScoreDto>("学生Id或考试批次异常！");
            var usage = UsageRepository.Load(batch);
            if (usage == null)
                return DResult.Error<StudentScoreDto>("考试批次不存在！");
            var dto = new StudentScoreDto
            {
                Batch = batch,
                PaperId = usage.SourceID
            };
            var isAb = PaperRepository.Where(t => t.Id == dto.PaperId).Select(t => t.PaperType).FirstOrDefault() ==
                       (byte)PaperType.AB;
            var model = StuScoreStatisticsRepository.FirstOrDefault(t => t.StudentId == studentId && t.Batch == batch);
            if (model == null)
                return DResult.Succ(dto);
            dto.Rank = model.CurrentSort;
            dto.Score = model.CurrentScore;
            if (!isAb)
                return DResult.Succ(dto);
            dto.ScoreA = model.SectionAScore;
            dto.ScoreB = model.SectionBScore;
            return DResult.Succ(dto);
        }

        public DResult<StudentReportDto> StudentReport(long studentId, string batch)
        {
            if (studentId <= 0 || string.IsNullOrWhiteSpace(batch))
                return DResult.Error<StudentReportDto>("学生Id或考试批次异常！");
            var report = new StudentReportDto();
            var ranks = StuScoreStatisticsRepository.Where(t => t.Batch == batch)
                .Select(t => new { t.StudentId, t.CurrentSort, t.CurrentScore })
                .OrderBy(t => t.CurrentSort)
                .ToList();
            if (!ranks.Any())
                return DResult.Succ(report);
            //排名详情
            report.Ranks = ranks.Select(t => new ReportRankDetailDto
            {
                Id = t.StudentId,
                Rank = t.CurrentSort,
                Score = t.CurrentScore,
                IsMine = t.StudentId == studentId
            }).ToList();

            var classRank = new ReportRankDto();

            var rank = ranks.FirstOrDefault(t => t.StudentId == studentId);
            if (rank != null)
            {
                classRank.Rank = rank.CurrentSort;
                classRank.Percent = 0;
                if (ranks.Count > 1)
                {
                    var count = ranks.Count(t => t.CurrentScore < rank.CurrentScore);
                    var total = ranks.Count - 1;
                    classRank.Percent = Math.Round(count * 100M / total, MidpointRounding.AwayFromZero);
                }
            }
            var classScore = ClassScoreStatisticsRepository.FirstOrDefault(t => t.Batch == batch);
            if (classScore != null)
            {
                classRank.Average = classScore.AverageScore;
                classRank.Change = RankChange(classScore, studentId, classRank.Rank);
                if (!string.IsNullOrWhiteSpace(classScore.SectionScores))
                {
                    var sectionAverage = JsonHelper.Json<ReportSectionScoresDto>(classScore.SectionScores);
                    if (sectionAverage != null)
                    {
                        classRank.AverageA = sectionAverage.AAv;
                        classRank.AverageB = sectionAverage.BAv;
                    }
                }
                if (!string.IsNullOrWhiteSpace(classScore.ScoreGroups))
                {
                    var scoreGroups = JsonHelper.JsonList<ScoreGroupsDto>(classScore.ScoreGroups).ToList();
                    if (!scoreGroups.IsNullOrEmpty())
                    {
                        //分数段
                        ConverSegments(report, scoreGroups, rank == null ? -1 : rank.CurrentScore);
                    }
                }
            }
            report.ClassRank = classRank;

            report.GradeRank = GradeRank(batch, studentId);

            return DResult.Succ(report);
        }

        public DResult<StudentCompareDto> StudentCompare(long userId, long compareId, string batch, string paperId)
        {
            if (string.IsNullOrWhiteSpace(batch))
                return DResult.Error<StudentCompareDto>("考试批次号不能为空！");
            if (userId <= 0 || compareId <= 0)
                return DResult.Error<StudentCompareDto>("学生ID或对比的Id无效！");
            var dto = new StudentCompareDto();
            var details =
                MarkingDetailRepository.Where(
                    d => d.Batch == batch && (d.StudentID == userId || d.StudentID == compareId))
                    .Select(d => new
                    {
                        d.StudentID,
                        d.QuestionID,
                        d.SmallQID,
                        d.IsCorrect,
                        d.Score,
                        d.CurrentScore
                    }).ToList();
            var myDetails = details.Where(d => d.StudentID == userId).ToList();
            var compareDetails = details.Where(d => d.StudentID == compareId).ToList();
            if (!myDetails.Any() || !compareDetails.Any())
                return DResult.Succ(dto);
            var paperResult = PaperContract.PaperDetailById(paperId);
            if (!paperResult.Status || paperResult.Data == null)
                return DResult.Error<StudentCompareDto>("试卷未找到！");
            var paper = paperResult.Data;
            var isAb = paper.PaperBaseInfo.IsAb;
            var questions = paper.PaperSections.SelectMany(s => s.Questions).ToList();
            #region 错题对比

            //错题对比
            foreach (var detail in myDetails)
            {
                var compare =
                    compareDetails.FirstOrDefault(
                        d => d.QuestionID == detail.QuestionID && d.SmallQID == detail.SmallQID);
                if (compare == null)
                    continue;
                var compareState = (byte)((compare.IsCorrect.HasValue && compare.IsCorrect.Value)
                    ? 2
                    : (compare.CurrentScore > 0 ? 1 : 0));
                var state = (byte)((detail.IsCorrect.HasValue && detail.IsCorrect.Value)
                    ? 2
                    : (detail.CurrentScore > 0 ? 1 : 0));
                if (compareState != state)
                {
                    var item = new ErrorCompareDto
                    {
                        Id = string.IsNullOrWhiteSpace(detail.SmallQID) ? detail.QuestionID : detail.SmallQID,
                        Mine = state,
                        Other = compareState
                    };
                    var question = questions.FirstOrDefault(q => q.Question.Id == detail.QuestionID);
                    if (question != null)
                    {
                        var prefix = isAb
                            ? (question.PaperSectionType == (byte)PaperSectionType.PaperA ? "A" : "B")
                            : string.Empty;
                        if (detail.SmallQID.IsNullOrEmpty() || !question.Question.HasSmall)
                        {
                            if (question.Question.HasSmall && paper.PaperBaseInfo.SubjectId == 3 &&
                                question.PaperSectionType == (byte)PaperSectionType.PaperA)
                            {
                                var dlist = question.Question.Details;
                                item.Index = dlist.Max(d => d.Sort) + question.PaperSectionType * 100;
                                item.Sort = string.Format("{0}{1}-{2}", prefix, dlist.Min(d => d.Sort),
                                    dlist.Max(d => d.Sort));
                            }
                            else
                            {
                                item.Index = question.Sort + question.PaperSectionType * 100;
                                item.Sort = string.Concat(prefix, question.Sort);
                            }
                        }
                        else
                        {
                            var dItem = question.Question.Details.FirstOrDefault(d => d.Id == detail.SmallQID);
                            if (dItem != null)
                            {
                                item.Index = dItem.Sort + question.PaperSectionType * 100;
                                item.Sort = string.Concat(prefix, dItem.Sort);
                            }
                        }
                    }
                    dto.Errors.Add(item);
                }
            }

            #endregion

            dto.Errors = dto.Errors.OrderBy(e => e.Index).ToList();

            #region 知识点得分率对比

            var knowledgeResult = PaperContract.KnowledgeQuestions(paper);
            if (knowledgeResult.Status)
            {
                var knowledges = knowledgeResult.Data;
                foreach (var knowledge in knowledges)
                {
                    var questionIds = knowledge.Questions.Select(q => q.Id).Distinct().ToList();
                    var knowledgeDetails = myDetails.Where(d => questionIds.Contains(d.QuestionID)).ToList();
                    int rate, compareRate;
                    decimal total, score;
                    if (knowledgeDetails.IsNullOrEmpty())
                    {
                        rate = 0;
                    }
                    else
                    {
                        total = knowledgeDetails.Sum(d => d.Score);
                        score = knowledgeDetails.Sum(d => d.CurrentScore);
                        if (total > 0)
                        {
                            rate = (int)Math.Round((score / total) * 100M, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    var compareKnowledgeDetails = compareDetails.Where(d => questionIds.Contains(d.QuestionID)).ToList();
                    if (compareKnowledgeDetails.IsNullOrEmpty())
                    {
                        compareRate = 0;
                    }
                    else
                    {
                        total = compareKnowledgeDetails.Sum(d => d.Score);
                        score = compareKnowledgeDetails.Sum(d => d.CurrentScore);
                        if (total > 0)
                        {
                            compareRate = (int)Math.Round((score / total) * 100M, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            compareRate = 0;
                        }
                    }
                    dto.Knowledges.Add(new KnowledgeCompareDto
                    {
                        Code = knowledge.Code,
                        Knowledge = knowledge.Name,
                        Mine = rate,
                        Other = compareRate
                    });
                }
            }

            #endregion

            dto.Knowledges = dto.Knowledges.OrderBy(t => t.Mine).ToList();
            return DResult.Succ(dto);
        }
    }
}
