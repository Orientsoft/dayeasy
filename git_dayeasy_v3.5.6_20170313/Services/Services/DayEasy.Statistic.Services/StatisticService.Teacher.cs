using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.Statistic.Services
{
    public partial class StatisticService
    {
        #region 私有方法

        /// <summary> 题目掌握情况 </summary>
        /// <param name="questionScores"></param>
        /// <param name="paper"></param>
        /// <returns></returns>
        private List<QuestionGraspingDto> QuestionGrasping(List<QuestionScoreDto> questionScores, PaperDetailDto paper)
        {
            var dtos = new List<QuestionGraspingDto>();
            var isAb = paper.PaperBaseInfo.IsAb;
            var questions = paper.PaperSections.SelectMany(s => s.Questions).ToList();

            Func<string, string, string, int, QuestionGraspingDto> graspingFunc = (prefix, qid, did, sort) =>
            {
                var dto = new QuestionGraspingDto
                {
                    Id = qid,
                    ScoreRate = 0
                };
                var scores = (string.IsNullOrWhiteSpace(did)
                    ? questionScores.Where(s => s.Id == qid)
                    : questionScores.Where(s => s.SmallId == did)).ToList();
                if (!scores.Any())
                    return null;
                var total = scores.Sum(d => d.Total);
                if (total > 0)
                {
                    var score = scores.Sum(d => d.Score);
                    dto.ScoreRate = (int)Math.Round((score * 100M) / total, MidpointRounding.AwayFromZero);
                }
                dto.Sort = string.Concat(prefix, sort);
                return dto;
            };

            foreach (var question in questions)
            {
                var qid = question.Question.Id;
                var prefix = isAb
                    ? (question.PaperSectionType == (byte)PaperSectionType.PaperA ? "A" : "B")
                    : string.Empty;
                if (question.Question.IsObjective && question.Question.HasSmall)
                {
                    foreach (var detail in question.Question.Details)
                    {
                        var dto = graspingFunc(prefix, qid, detail.Id, detail.Sort);
                        if (dto != null)
                            dtos.Add(dto);
                    }
                }
                else
                {
                    var dto = graspingFunc(prefix, qid, null, question.Sort);
                    if (dto != null)
                        dtos.Add(dto);
                }
            }
            dtos = dtos.OrderBy(d => d.ScoreRate).ThenBy(d => d.Sort.TrimStart('A', 'B').To(0)).ToList();
            return dtos;
        }

        /// <summary> 知识点掌握情况 </summary>
        /// <param name="details"></param>
        /// <param name="paper"></param>
        /// <returns></returns>
        private List<KnowledgeGraspingDto> KnowledgeGrasping(List<QuestionScoreDto> details, PaperDetailDto paper)
        {
            var dtos = new List<KnowledgeGraspingDto>();
            if (!details.Any())
                return dtos;
            var knowledges = PaperContract.KnowledgeQuestions(paper);
            if (!knowledges.Status || !knowledges.Data.Any())
                return dtos;
            foreach (var knowledge in knowledges.Data)
            {
                var dto = new KnowledgeGraspingDto
                {
                    Code = knowledge.Code,
                    Name = knowledge.Name,
                    Questions = knowledge.Questions,
                    ScoreRate = 0
                };
                var qids = dto.Questions.Select(t => t.Id).Distinct().ToList();
                var knowledgeDetails = details.Where(d => qids.Contains(d.Id)).ToList();
                if (!knowledgeDetails.Any())
                    continue;
                var total = knowledgeDetails.Sum(d => d.Total);
                if (total > 0)
                {
                    var score = knowledgeDetails.Sum(d => d.Score);
                    dto.ScoreRate = (int) Math.Round((score*100M)/total, MidpointRounding.AwayFromZero);
                }
                dtos.Add(dto);
            }
            dtos = dtos.OrderBy(k => k.ScoreRate).ToList();
            return dtos;
        }

        #endregion

        public DResult<GraspingDto> Graspings(string batch, string paperId)
        {
            if (string.IsNullOrWhiteSpace(batch) || string.IsNullOrWhiteSpace(paperId))
                return DResult.Error<GraspingDto>("考试批次号不正确！");
            var dto = new GraspingDto();
            var details = MarkingDetailRepository.Where(t => t.Batch == batch && t.PaperID == paperId)
                .Select(t => new QuestionScoreDto
                {
                    Id = t.QuestionID,
                    SmallId = t.SmallQID,
                    Total = t.Score,
                    Score = t.CurrentScore
                }).ToList();
            if (!details.Any())
                return DResult.Succ(dto);
            var paperResult = PaperContract.PaperDetailById(paperId);
            if (paperResult.Status && paperResult.Data != null)
            {
                dto.Questions = QuestionGrasping(details, paperResult.Data);
                dto.Knowledges = KnowledgeGrasping(details, paperResult.Data);
            }
            return DResult.Succ(dto);
        }
    }
}
