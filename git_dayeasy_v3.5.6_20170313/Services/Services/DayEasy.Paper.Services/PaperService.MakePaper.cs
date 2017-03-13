using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Paper.Services.Helper.AutoMakePaper;
using DayEasy.Paper.Services.Helper.Question;
using DayEasy.Paper.Services.Model;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Paper.Services
{
    public partial class PaperService
    {
        #region Private Method

        #region 转换成保存问题的实体
        /// <summary>
        /// 转换成保存问题的实体
        /// </summary>
        /// <returns></returns>
        private QuestionDto ConvertToQuestion(AddQuestionDto addQuestion)
        {
            var newQuestion = new QuestionDto
            {
                UserId = addQuestion.UserId,
                UserName = addQuestion.RealName,
                Type = addQuestion.QType,
                Body = addQuestion.QContent.ConvertImgs(),
                Images = null,
                SubjectId = addQuestion.SubjectId,
                Stage = addQuestion.Stage,
                Difficulty = 3,
                UseCount = 0,
                AnswerCount = 0,
                ErrorCount = 0,
                Range = (byte)ShareRange.Self,
                OptionStyle = (byte)OptionStyle.AddFromPaper,
                Tags = null,
                Analysis = null,
                IsObjective = false,
                HasSmall = false
            };

            //特殊处理
            var kpList = JsonHelper.JsonList<NameDto>(addQuestion.Kps);
            if (kpList != null)
            {
                newQuestion.Knowledges = kpList.ToList();
            }

            if (addQuestion.OptionNum > 0)//有选项
            {
                newQuestion.IsObjective = true;

                if (addQuestion.SmallQuNum > 0)//有小问
                {
                    var details = new List<SmallQuestionDto>();

                    for (int i = 0; i < addQuestion.SmallQuNum; i++)
                    {
                        var newDetail = new SmallQuestionDto
                        {
                            Id = IdHelper.Instance.Guid32,
                            Body = string.Empty,
                            IsObjective = true,
                            Images = null,
                            Sort = i,
                            OptionStyle = (byte)OptionStyle.AddFromPaper
                        };

                        var answers = new List<AnswerDto>();
                        for (int j = 0; j < addQuestion.OptionNum; j++)
                        {
                            var newAnswer = new AnswerDto
                            {
                                Id = IdHelper.Instance.Guid32,
                                Body = string.Empty,
                                Images = null,
                                IsCorrect = false,
                                Sort = j
                            };

                            answers.Add(newAnswer);
                        }
                        newDetail.Answers = answers;

                        details.Add(newDetail);
                    }

                    newQuestion.HasSmall = true;
                    newQuestion.Details = details;
                }
                else//无小问
                {
                    newQuestion.Details = null;

                    var answers = new List<AnswerDto>();
                    for (int i = 0; i < addQuestion.OptionNum; i++)
                    {
                        var newAnswer = new AnswerDto
                        {
                            Id = IdHelper.Instance.Guid32,
                            Body = string.Empty,
                            Images = null,
                            IsCorrect = false,
                            Sort = i
                        };

                        answers.Add(newAnswer);
                    }

                    newQuestion.Answers = answers;
                }
            }
            else//无选项
            {
                if (addQuestion.SmallQuNum > 0) //有小问
                {
                    var details = new List<SmallQuestionDto>();

                    for (int i = 0; i < addQuestion.SmallQuNum; i++)
                    {
                        var newDetail = new SmallQuestionDto
                        {
                            Id = IdHelper.Instance.Guid32,
                            Body = string.Empty,
                            IsObjective = false,
                            Images = null,
                            Sort = i,
                            OptionStyle = (byte)OptionStyle.AddFromPaper,
                            Answers = null
                        };
                        details.Add(newDetail);
                    }

                    newQuestion.HasSmall = true;
                    newQuestion.Details = details;
                }
                else //无小问
                {
                    newQuestion.Details = null;
                }

                var newAnswer = new AnswerDto
                {
                    Id = IdHelper.Instance.Guid32,
                    Body = "略",
                    Images = null,
                    IsCorrect = true,
                    Sort = 0
                };

                newQuestion.Answers = new List<AnswerDto>() { newAnswer };
            }

            return newQuestion;
        }
        #endregion

        #region 自动出卷选取题目
        /// <summary>
        /// 自动出卷选取题目
        /// </summary>
        /// <param name="qTypeDic"></param>
        /// <param name="kpsDic"></param>
        /// <param name="difficulty"></param>
        /// <param name="qidList"></param>
        /// <param name="stage"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        private List<QuestionDto> GetAutoQuestions(Dictionary<int, int> qTypeDic, Dictionary<string, decimal> kpsDic, byte difficulty, List<string> qidList, int stage, int subjectId)
        {
            if (qTypeDic == null || qTypeDic.Count < 1 || kpsDic == null || kpsDic.Count < 1)
            {
                return null;
            }

            //根据用户的设置，获取试卷的构成属性（查询条件组合）
            var paperProperties = new PaperFactory().GetPaperProperties(qTypeDic, kpsDic, difficulty);
            if (paperProperties != null)
            {
                var questionIds = new Dictionary<int, List<string>>();

                foreach (var property in paperProperties)
                {
                    var quIdList = questionIds.SelectMany(u => u.Value).ToList();
                    if (qidList != null && qidList.Any())
                    {
                        quIdList.AddRange(qidList);
                    }

                    var query = new QuestionQuery
                    {
                        QuestionType = property.QType,
                        Stages = new List<int> { stage },
                        SubjectId = subjectId,
                        IsHighLight = false,
                        Page = 1,
                        Size = property.Count,
                        Points = property.Points,
                        ShareRange = -1,
                        Difficulties = property.Difficulties.ToArray(),
                        Order = QuestionOrderType.Random,
                        NotInIds = quIdList
                    };

                    var tempResult = QuestionManager.Instance.Query(query);
                    if (tempResult.Any())
                    {
                        var qids = tempResult.Select(t => t.QuestionId).ToList();
                        if (questionIds.Keys.Contains(property.QType))
                        {
                            questionIds[property.QType].AddRange(qids);
                        }
                        else
                        {
                            questionIds.Add(property.QType, qids);
                        }
                    }

                    if (!tempResult.Any() || tempResult.Count < property.Count)//如果数量不够，降级难度
                    {
                        quIdList = questionIds.SelectMany(u => u.Value).ToList();
                        if (qidList != null && qidList.Any())
                        {
                            quIdList.AddRange(qidList);
                        }

                        var queryNoDiff = new QuestionQuery
                        {
                            QuestionType = property.QType,
                            Stages = new List<int> { stage },
                            SubjectId = subjectId,
                            IsHighLight = false,
                            Page = 1,
                            Size = property.Count - tempResult.Count,
                            Points = property.Points,
                            ShareRange = -1,
                            Order = QuestionOrderType.Random,
                            NotInIds = quIdList
                        };

                        var searchResult = QuestionManager.Instance.Query(queryNoDiff);
                        if (searchResult.Any())
                        {
                            var qids = searchResult.Select(t => t.QuestionId).ToList();
                            if (questionIds.Keys.Contains(property.QType))
                            {
                                questionIds[property.QType].AddRange(qids);
                            }
                            else
                            {
                                questionIds.Add(property.QType, qids);
                            }
                        }
                    }
                }

                #region 处理问题不够的情况（随机知识点）

                foreach (var qtype in qTypeDic)
                {
                    if (!questionIds.Keys.Contains(qtype.Key) || qtype.Value != questionIds[qtype.Key].Count)
                    {
                        //当前已经有的数量
                        var currentCount = questionIds.Keys.Contains(qtype.Key) ? questionIds[qtype.Key].Count : 0;

                        var searchCount = qtype.Value - currentCount;//还差的数量
                        if (searchCount > 0)
                        {
                            var quIdList = questionIds.SelectMany(u => u.Value).ToList();
                            if (qidList != null && qidList.Any())
                            {
                                quIdList.AddRange(qidList);
                            }

                            for (var i = 0; i < searchCount; i++)
                            {
                                var r = new Random(Guid.NewGuid().GetHashCode());

                                var num = r.Next(searchCount * 10, searchCount * 100);
                                var kpsArr = kpsDic.Where(u => u.Value > 0).Select(u => u.Key).ToArray();
                                var index = num % kpsArr.Length;

                                var query = new QuestionQuery
                                {
                                    QuestionType = qtype.Key,
                                    Stages = new List<int> { stage },
                                    SubjectId = subjectId,
                                    IsHighLight = false,
                                    Page = 1,
                                    Size = 1,
                                    ShareRange = -1,
                                    Points = new[] { kpsArr[index] },//当前题型随机知识点
                                    Order = QuestionOrderType.Random,
                                    NotInIds = quIdList
                                };

                                var tempResult = QuestionManager.Instance.Query(query);

                                if (tempResult.Any())
                                {
                                    var qids = tempResult.Select(t => t.QuestionId).ToList();
                                    if (questionIds.Keys.Contains(qtype.Key))
                                    {
                                        questionIds[qtype.Key].AddRange(qids);
                                    }
                                    else
                                    {
                                        questionIds.Add(qtype.Key, qids);
                                    }
                                }
                                //else//如果随机知识点没有找到
                                //{
                                //    var query1 = new QuestionQuery
                                //    {
                                //        QuestionType = qtype.Key,
                                //        Stages = StagesList.Select(t => t.Id).Distinct().ToArray(),
                                //        SubjectId = CurrentUser.SubjectId,
                                //        IsHighLight = false,
                                //        Page = 1,
                                //        Size = 1,
                                //        ShareRange = -1,
                                //        Points = kpsArr.ToList(),//当前题型随机知识点
                                //        Order = QuestionOrderType.Random,
                                //        NotInIds = quIdList
                                //    };

                                //    var tempResult1 = QuestionManager.Instance.Query(query1);

                                //    if (tempResult1.Any())
                                //    {
                                //        var qids = tempResult1.Select(t => t.QuestionId).ToList();
                                //        if (questionIds.Keys.Contains(qtype.Key))
                                //        {
                                //            questionIds[qtype.Key].AddRange(qids);
                                //        }
                                //        else
                                //        {
                                //            questionIds.Add(qtype.Key, qids);
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }
                #endregion

                return LoadQuestions(questionIds.SelectMany(d => d.Value).Distinct().ToArray());
            }

            return null;
        }
        #endregion

        #endregion

        #region 出卷基础信息处理
        /// <summary>
        /// 出卷基础信息处理
        /// </summary>
        /// <param name="paperBaseStr"></param>
        /// <param name="paperType"></param>
        /// <param name="subjectId"></param>
        /// <param name="stage"></param>
        public DResult<ChooseQuestionDataDto> PaperBaseAction(string paperBaseStr, string paperType, int subjectId, int stage)
        {
            var paperBase = new PaperBaseDto
            {
                Type = paperType,
                SubjectId = subjectId,
                Stage = stage
            };

            if (!string.IsNullOrEmpty(paperBaseStr))
            {
                var temp = JsonHelper.Json<PaperBaseDto>(paperBaseStr);
                if (temp != null)
                {
                    paperBase = temp;
                }
            }

            if (string.IsNullOrEmpty(paperBase.Type))
            {
                paperBase.Type = "a";
            }

            var currentChooseQus = new List<QuItemDto>();
            if (paperBase.ChooseQus != null)
            {
                currentChooseQus.AddRange(paperBase.ChooseQus.Where(u => u != null && u.Any()).SelectMany(u => u.ToList()).ToList());
            }

            var result = new ChooseQuestionDataDto();

            if (currentChooseQus.Any())
            {
                var newChooseQuType = "B";
                if (string.IsNullOrEmpty(paperBase.AddType) || string.Equals(paperBase.AddType.Trim(), "A", StringComparison.CurrentCultureIgnoreCase))
                {
                    newChooseQuType = "A";
                }

                var i = currentChooseQus.Max(u => u.Sort);
                currentChooseQus.ForEach(u =>
                {
                    if (string.IsNullOrEmpty(u.PaperType))
                    {
                        u.PaperType = newChooseQuType;
                    }
                    u.Sort = i++;
                });

                if (currentChooseQus.Any())
                {
                    var questions = LoadQuestions(currentChooseQus.Select(u => u.QId).Distinct().ToArray());
                    if (questions.Count > 0)
                    {
                        result.CurrentChooseQus = currentChooseQus.DistinctBy(u => u.QId).ToList();
                        result.Questions = questions;
                        result.AutoData = paperBase.AutoData;
                    }
                }
            }
            result.QuestionTypes = SystemContract.GetQuTypeBySubjectId(subjectId);
            result.PaperBaseDto = paperBase;

            return DResult.Succ(result);
        }
        #endregion

        #region 自动出卷基础信息处理
        /// <summary>
        /// 自动出卷基础信息处理
        /// </summary>
        public DResult<ChooseQuestionDataDto> AutoPaperBaseAction(string autoData, string paperType, int subjectId, int stage)
        {
            var resultData = new ChooseQuestionDataDto()
            {
                HasAll = true,
                PaperBaseDto = new PaperBaseDto()
                {
                    Type = paperType,
                    SubjectId = subjectId,
                    Stage = stage
                }
            };

            if (!string.IsNullOrEmpty(autoData))
            {
                var data = JsonHelper.Json<AutoCondition>(autoData);

                if (data != null)
                {
                    Dictionary<string, decimal> kpsDic = null;
                    //知识点
                    var totalCount = data.Qtypes.Sum(q => q.Count);
                    if (totalCount > 0 && totalCount < 101)
                    {
                        var kpsTotalCount = data.Kps.Sum(u => u.Count);
                        kpsDic = data.Kps.ToDictionary(k => k.Name, k => (k.Count / (decimal)kpsTotalCount));
                    }
                    var groupList = data.Qtypes.GroupBy(d => d.PaperSectionType).ToList();

                    var currentChooseQus = new List<QuItemDto>();
                    var questions = new List<QuestionDto>();
                    foreach (var typeGroup in groupList)
                    {
                        //类型
                        var qTypeDic = typeGroup.Where(u => u.Count > 0).ToDictionary(q => q.Type, q => q.Count);

                        var qids = currentChooseQus.Select(u => u.QId).ToList();
                        var result = GetAutoQuestions(qTypeDic, kpsDic, (byte)data.Diffic, qids, stage, subjectId);

                        var sort = 1;
                        if (result != null && result.Any())
                        {
                            questions.AddRange(result);
                            currentChooseQus.AddRange(result.Select(question => new QuItemDto
                            {
                                PaperType = typeGroup.Key,
                                QId = question.Id,
                                Score = 0,
                                Sort = sort++,
                                Type = question.Type
                            }));

                            if (result.Count < qTypeDic.Sum(u => u.Value))
                            {
                                resultData.HasAll = false;
                            }
                        }
                        else
                        {
                            resultData.HasAll = false;
                        }
                    }

                    resultData.CurrentChooseQus = currentChooseQus;
                    resultData.Questions = questions;
                    resultData.AutoData = JsonHelper.ToJson(data);
                }
                else
                {
                    resultData.HasAll = false;
                }
            }
            else
            {
                resultData.HasAll = false;
            }
            resultData.QuestionTypes = SystemContract.GetQuTypeBySubjectId(subjectId);

            return DResult.Succ(resultData);
        }
        #endregion

        #region 试卷添加问题--保存
        /// <summary>
        /// 试卷添加问题--保存
        /// </summary>
        /// <returns></returns>
        public DResult<string> AddPaperQuestion(AddQuestionDto addQuestion)
        {
            var question = ConvertToQuestion(addQuestion);

            return SaveQuestion(question, false, true);
        }
        #endregion

        #region 构造预览数据
        /// <summary>
        /// 构造预览数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>[NonAction]
        public PaperDetailDto MakePreviewData(MakePaperDto data)
        {
            if (data != null)
            {
                var paperInfo = new PaperDetailDto();

                var paper = new PaperDto { PaperTitle = data.PaperTitle, PaperType = (byte)PaperType.Normal };
                if (data.PaperType.Trim() == "AB")
                {
                    paper.PaperType = (byte)PaperType.AB;
                }

                paperInfo.PaperBaseInfo = paper;
                paperInfo.PaperBaseInfo.PaperScores = new PaperScoresDto()
                {
                    TScore = data.PScores.TScore,
                    TScoreA = data.PScores.TScoreA,
                    TScoreB = data.PScores.TScoreB
                };

                if (data.PSection == null) return paperInfo;

                var paperSections = new List<PaperSectionDto>();
                var sortSections = data.PSection.GroupBy(u => u.PaperSectionType).OrderBy(u => u.Key).ToList();

                var qSort = 1;

                foreach (var gsections in sortSections)
                {
                    var sections = gsections.OrderBy(u => u.Sort).ToList();

                    foreach (var section in sections)
                    {
                        var paperSection = new PaperSectionDto
                        {
                            SectionID = "",
                            Description = section.Description,
                            Sort = section.Sort,
                            SectionQuType = section.SectionQuType
                        };

                        if (section.PaperSectionType.Trim() == "A")
                        {
                            paperSection.PaperSectionType = (byte)PaperSectionType.PaperA;
                        }
                        else
                        {
                            paperSection.PaperSectionType = (byte)PaperSectionType.PaperB;
                        }
                        paperSection.SectionScore = section.SectionScore;
                        paperSection.Questions = null;

                        if (section.Questions != null)
                        {
                            var paperQuestion = new List<PaperQuestionDto>();

                            var questions = section.Questions.OrderBy(u => u.Sort).ToList();
                            var qids = questions.Select(u => u.QuestionID).ToList();
                            var vqList = LoadQuestions(qids.ToArray());

                            foreach (var qItem in questions)
                            {
                                var pQuestion = new PaperQuestionDto
                                {
                                    Score = qItem.Score,
                                    Question = vqList.FirstOrDefault(u => u.Id == qItem.QuestionID),
                                    Sort = qSort
                                };

                                paperQuestion.Add(pQuestion);

                                qSort++;
                            }

                            paperSection.Questions = paperQuestion;
                        }

                        paperSections.Add(paperSection);
                    }
                }
                paperInfo.PaperSections = paperSections;
                return paperInfo;
            }
            return null;
        }
        #endregion
    }
}
