using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Facade.Models;
using Dayeasy.Facade.Paper.Models;
using DayEasy.Office.Models;

namespace DayEasy.Office
{
    /// <summary>
    /// WordHelper实体类转换
    /// </summary>
    public static class ClassConvertHelper
    {
        /// <summary>
        /// WordHelper 类转换
        /// </summary>
        /// <param name="questions"></param>
        /// <param name="id">题目序号</param>
        /// <returns></returns>
        public static List<WdQuestion> ToWdQuestion(this List<VqQuestion> questions, int id = 1)
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

        /// <summary>
        /// WordHelper 类转换
        /// </summary>
        /// <param name="paper"></param>
        /// <returns></returns>
        public static WdPaper ToWdPaper(this VpPaperInfo paper)
        {
            if (paper == null) return null;
            return new WdPaper
            {
                Num = paper.PaperBaseInfo.PaperNo,
                Score = 0,
                Title = paper.PaperBaseInfo.PaperTitle,
                Sections = paper.PaperSections.Select(s => new WdSection
                {
                    Sort = s.Sort,
                    Name = s.Description,
                    Type = s.SectionQuType,
                    Questions = s.Questions.Select(q => new WdQuestion
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
                            ? q.Question.Details.Select(sq => new WdSmallQuestion
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
