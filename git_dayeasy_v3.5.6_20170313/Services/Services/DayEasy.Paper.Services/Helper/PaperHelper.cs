using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Cache;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Core.Domain;

namespace DayEasy.Paper.Services.Helper
{
    /// <summary> 试卷排序 </summary>
    public enum PaperSortType : byte
    {
        /// <summary> 普通排序,A卷/B卷单独大题通排 </summary>
        Normal = 0,

        /// <summary> 整卷大题通排 </summary>
        PassRow = 1,

        /// <summary> AB卷小问分别通排 </summary>
        SmallPassRowAb = 2,

        /// <summary> A卷小问通排 </summary>
        SmallPassRowA = 3,

        /// <summary> 整卷小问通排 </summary>
        SmallPassRow = 4
    }

    public static class PaperHelper
    {
        private const string CacheRegion = "paper";
        public static PaperSortType SortType(this PaperDto dto)
        {
            switch (dto.SubjectId)
            {
                case 1:
                    //语文，AB卷分别小问通排
                    return PaperSortType.SmallPassRowAb;
                case 3:
                    //英语，A卷小问通排，B卷大题重排
                    return PaperSortType.SmallPassRowA;
                case 4:
                    //物理，AB卷分别大题通排
                    return PaperSortType.Normal;
                default:
                    return PaperSortType.PassRow;
            }
        }

        /// <summary> 是否小问通排 </summary>
        /// <param name="sortType"></param>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        public static bool SmallRow(this PaperSortType sortType, byte sectionType)
        {
            return (sortType == PaperSortType.SmallPassRowAb ||
                    sortType == PaperSortType.SmallPassRow ||
                    (sortType == PaperSortType.SmallPassRowA &&
                     sectionType == (byte)PaperSectionType.PaperA));
        }

        /// <summary> B卷是否重新排序 </summary>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public static bool IsResetRow(this PaperSortType sortType)
        {
            return sortType.In(PaperSortType.Normal, PaperSortType.SmallPassRowA, PaperSortType.SmallPassRowAb);
        }

        /// <summary> 试卷排序 </summary>
        /// <param name="paperDto"></param>
        /// <param name="sortType"></param>
        public static void LoadQuestions(this PaperDetailDto paperDto, PaperSortType sortType = PaperSortType.Normal)
        {
            var id = paperDto.PaperBaseInfo.Id;
            var cacheManager = CacheManager.GetCacher(CacheRegion);
            var dto = cacheManager.Get<List<PaperSectionDto>>(id);
            if (dto != null)
            {
                paperDto.PaperSections = dto;
                return;
            }
            var sectionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_PaperSection>>();
            var contentRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_PaperContent>>();
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var sectionList = sectionRepository.Where(t => t.PaperID == id)
                .Select(t => new { t.Id, t.Description, t.SectionQuType, t.PaperSectionType, t.Sort, t.SectionScore })
                .ToList()
                .GroupBy(t => t.PaperSectionType)
                .OrderBy(t => t.Key);
            if (!sectionList.Any())
                return;
            var sectionQuestions = contentRepository.Where(t => t.PaperID == id)
                .Select(t => new { t.SectionID, t.QuestionID, t.Sort, t.Score })
                .ToList()
                .GroupBy(t => t.SectionID)
                .ToDictionary(k => k.Key, v => v.OrderBy(t => t.Sort).ToList());
            var questions = paperContract.LoadQuestions(
                sectionQuestions.Values.SelectMany(t => t.Select(q => q.QuestionID)).ToList())
                .ToDictionary(k => k.Id, v => v);
            //试卷答案
            List<PaperAnswerDto> paperAnswers = null;
            var paperAnswersResult = paperContract.GetPaperAnswers(id);
            if (paperAnswersResult.Status && paperAnswersResult.Data != null && paperAnswersResult.Data.Any())
                paperAnswers = paperAnswersResult.Data.ToList();
            var sections = new List<PaperSectionDto>();
            int sort = 1, sectionSort;
            foreach (var section in sectionList)
            {
                var list = section.Select(t => new PaperSectionDto
                {
                    SectionID = t.Id,
                    Description = t.Description,
                    Sort = t.Sort,
                    SectionQuType = t.SectionQuType,
                    PaperSectionType = t.PaperSectionType,
                    SectionScore = t.SectionScore,
                    Questions = new List<PaperQuestionDto>()
                }).OrderBy(t => t.Sort).ToList();
                foreach (var item in list)
                {
                    if (!sectionQuestions.ContainsKey(item.SectionID))
                        continue;
                    var qids = sectionQuestions[item.SectionID];
                    sectionSort = 1;
                    foreach (var qItem in qids)
                    {
                        var questionDto = new PaperQuestionDto
                        {
                            PaperSectionType = item.PaperSectionType,
                            Score = qItem.Score,
                            Sort = qItem.Sort,
                            Question = questions[qItem.QuestionID]
                        };
                        questionDto.Question.LoadPaperAnswers(paperAnswers);
                        var smallRow = sortType.SmallRow(item.PaperSectionType);
                        questionDto.SortQuestion(ref sort, ref sectionSort, smallRow);
                        item.Questions.Add(questionDto);
                    }
                }
                sections.AddRange(list);
                if (sortType.IsResetRow())
                    sort = 1;
            }
            cacheManager.Set(id, sections, TimeSpan.FromDays(30));
            paperDto.PaperSections = sections;
        }

        /// <summary> 清空缓存 </summary>
        /// <param name="paperIds"></param>
        internal static void ClearPaperCache(params string[] paperIds)
        {
            var cache = CacheManager.GetCacher(CacheRegion);
            cache.Remove(paperIds);
        }

        /// <summary> 加载试卷答案 </summary>
        /// <param name="qItem"></param>
        /// <param name="paperAnswers"></param>
        public static void LoadPaperAnswers(this QuestionDto qItem, IReadOnlyCollection<PaperAnswerDto> paperAnswers)
        {
            if (qItem == null || paperAnswers == null || paperAnswers.Count == 0)
                return;
            var answers = paperAnswers.Where(u => u.QuestionId == qItem.Id).ToList();
            foreach (var answerItem in answers)
            {
                var content = answerItem.AnswerContent.FormatBody();
                if (string.IsNullOrWhiteSpace(content))
                    continue;

                if (qItem.IsObjective)
                {
                    if (qItem.Details == null || qItem.Details.Count < 1)
                    {
                        foreach (var answer in qItem.Answers)
                        {
                            answer.IsCorrect = content.Contains(Consts.OptionWords[answer.Sort]);
                        }
                    }
                    else
                    {
                        var detail = qItem.Details.FirstOrDefault(d => d.Id == answerItem.SmallQuId);
                        if (detail == null || !detail.IsObjective)
                            continue;
                        foreach (var answer in detail.Answers)
                        {
                            answer.IsCorrect = content.Contains(Consts.OptionWords[answer.Sort]);
                        }
                    }
                }
                else
                {
                    if (qItem.Answers == null || !qItem.Answers.Any())
                    {
                        qItem.Answers = new List<AnswerDto>
                        {
                            new AnswerDto {IsCorrect = true, Sort = 0, Id = answerItem.QuestionId}
                        };
                    }

                    qItem.Answers[0].Body = content;
                    qItem.Answers[0].Images = null;
                }

            }
        }

        /// <summary> 试卷排序 </summary>
        /// <param name="dto"></param>
        /// <param name="startSort"></param>
        /// <param name="smallRow"></param>
        /// <param name="sectionSort"></param>
        public static void SortQuestion(this PaperQuestionDto dto, ref int startSort, ref int sectionSort, bool smallRow = false)
        {
            var hasDetail = dto.Question.HasSmall && !dto.Question.Details.IsNullOrEmpty();
            if (!smallRow || !hasDetail)
            {
                dto.Sort = startSort++;
                return;
            }
            dto.Sort = sectionSort++;
            foreach (var detail in dto.Question.Details.OrderBy(t => t.Sort))
            {
                detail.Sort = startSort++;
            }
        }

        public static Dictionary<string, DKeyValue<byte, List<int>>> PaperSorts(string paperId)
        {
            var sorts = new Dictionary<string, DKeyValue<byte, List<int>>>();
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var paper = paperContract.PaperDetailById(paperId);
            if (!paper.Status || paper.Data == null)
                return sorts;
            var sortType = paper.Data.PaperBaseInfo.SortType();
            foreach (var section in paper.Data.PaperSections)
            {
                var smallRow = sortType.SmallRow(section.PaperSectionType);

                foreach (var dto in section.Questions)
                {
                    var item = new DKeyValue<byte, List<int>>(section.PaperSectionType, new List<int>());
                    if (dto.Question.IsObjective && !dto.Question.Details.IsNullOrEmpty())
                    {
                        foreach (var detail in dto.Question.Details)
                        {
                            sorts.Add(detail.Id,
                                new DKeyValue<byte, List<int>>(section.PaperSectionType, new List<int> { detail.Sort }));
                            item.Value.Add(detail.Sort);
                        }
                    }
                    else
                    {
                        if (smallRow && !dto.Question.Details.IsNullOrEmpty())
                        {
                            var detailSorts = dto.Question.Details.Select(d => d.Sort).ToList();
                            item.Value.AddRange(detailSorts);
                        }
                        else
                        {
                            item.Value.Add(dto.Sort);
                        }
                    }
                    sorts.Add(dto.Question.Id, item);
                }
            }
            return sorts;
        }
    }
}
