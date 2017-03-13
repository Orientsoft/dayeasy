using System.Linq;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Office.Models;

namespace DayEasy.UnitTest
{
    public static class OfficModelConvert
    {
        public static WdPaper ToWdPaper(this PaperDetailDto paper)
        {
            if (paper == null) return null;
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
    }
}
