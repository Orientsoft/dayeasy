using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Office.Models;
using DayEasy.Paper.Services.Helper;

namespace DayEasy.Application.Services.Helper
{
    /// <summary> Word下载实体类转换 </summary>
    public static class WdPaperConvert
    {
        public static WdPaper ToWdPaper(this PaperDetailDto paper)
        {
            if (paper == null) return null;
            var sortType = paper.PaperBaseInfo.SortType();
            return new WdPaper
            {
                Num = paper.PaperBaseInfo.PaperNo,
                SubjectId = paper.PaperBaseInfo.SubjectId,
                Score = 0,
                Title = paper.PaperBaseInfo.PaperTitle,
                Sections = paper.PaperSections.OrderBy(u => u.Sort).Select(s => new WdSection
                {
                    Sort = s.Sort,
                    Name = s.Description,
                    Type = s.SectionQuType,
                    PSectionType = s.PaperSectionType,
                    SmallRow = sortType.SmallRow(s.PaperSectionType),
                    Questions = s.Questions.OrderBy(u => u.Sort).Select(q => new WdQuestion
                    {
                        Sort = q.Sort,
                        Score = q.Score,
                        Type = q.Question.Type,
                        IsObjective = q.Question.IsObjective,
                        ShowOption = q.Question.ShowOption,
                        Body = q.Question.Body,
                        Images = q.Question.Images,
                        Answers = q.Question.Answers.Select(a => new WdAnswer
                        {
                            Sort = a.Sort,
                            Images = a.Images,
                            Body = a.Body,
                            IsCorrect = a.IsCorrect
                        }).ToList(),
                        SmallQuestions = q.Question.HasSmall && q.Question.Details != null
                            ? q.Question.Details.OrderBy(u => u.Sort).Select(sq => new WdSmallQuestion
                            {
                                Sort = sq.Sort,
                                Body = sq.Body,
                                Images = sq.Images,
                                IsObjective = sq.IsObjective,
                                Answers = sq.Answers.Select(sa => new WdAnswer
                                {
                                    Sort = sa.Sort,
                                    Images = sa.Images,
                                    Body = sa.Body,
                                    IsCorrect = sa.IsCorrect
                                }).ToList()
                            }).ToList()
                            : null
                    }).ToList()
                }).ToList()
            };
        }

        public static List<WdQuestion> ToWdQuestion(this List<QuestionDto> questions, int id = 1)
        {
            if (questions == null || !questions.Any())
                return null;
            return questions.Select(q => new WdQuestion
            {
                Sort = id++,
                Type = q.Type,
                IsObjective = q.IsObjective,
                ShowOption = q.ShowOption,
                Body = q.Body,
                Images = q.Images,
                Answers = q.Answers.Select(a => new WdAnswer
                {
                    Sort = a.Sort,
                    Images = a.Images,
                    Body = a.Body,
                    IsCorrect = a.IsCorrect
                }).ToList(),
                SmallQuestions = q.HasSmall && q.Details != null
                    ? q.Details.Select(sq => new WdSmallQuestion
                    {
                        Sort = sq.Sort,
                        Body = sq.Body,
                        Images = sq.Images,
                        IsObjective = sq.IsObjective,
                        Answers = sq.Answers.Select(sa => new WdAnswer
                        {
                            Sort = sa.Sort,
                            Images = sa.Images,
                            Body = sa.Body,
                            IsCorrect = sa.IsCorrect
                        }).ToList()
                    }).ToList()
                    : null
            }).ToList();
        }
    }
}