
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Paper.Services.Helper.Question;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.Paper.Services
{
    public partial class PaperService
    {
        #region 从数据库加载题目

        /// <summary> 从数据库加载题目 </summary>
        /// <param name="questionId"></param>
        private QuestionDto LoadQuestionFromDb(string questionId)
        {
            var item = QuestionRepository.Load(questionId);
            if (item == null)
                return null;
            var dto = item.MapTo<QuestionDto>();
            if (!string.IsNullOrWhiteSpace(dto.Body))
                dto.Body = dto.Body.HtmlDecode();
            if (!string.IsNullOrWhiteSpace(dto.KnowledgeIDs))
            {
                var points = JsonHelper.Json<Dictionary<string, string>>(dto.KnowledgeIDs);
                dto.Knowledges = points.Select(t => new NameDto(t.Key, t.Value)).ToList();
            }
            var qids = new List<string> { item.Id };

            //小问处理
            if (item.HasSmallQuestion)
            {
                var details = LoadSmallQuestions(new[] { item.Id });
                if (details.ContainsKey(item.Id))
                {
                    dto.Details = details[item.Id];
                    qids.AddRange(dto.Details.Select(d => d.Id));
                }
                //修改序号
                if (dto.Details.Any() && dto.Details.Exists(u => u.Sort < 1))
                {
                    dto.Details.Foreach(d => d.Sort = d.Sort + 1);
                }
            }
            //答案/选项
            var answers = LoadAnswerDtos(qids);
            if (answers.ContainsKey(item.Id))
            {
                dto.Answers = answers[item.Id];
            }
            if (!dto.Details.IsNullOrEmpty())
            {
                foreach (var detail in dto.Details)
                {
                    if (answers.ContainsKey(detail.Id))
                        detail.Answers = answers[detail.Id];
                }
            }
            //解析
            if (item.IsObjective)
            {
                var analysis = LoadAnalysisDtos(new[] { item.Id });
                if (analysis.ContainsKey(item.Id))
                    dto.Analysis = analysis[item.Id];
            }
            return dto;
        }

        private List<QuestionDto> LoadQuestionListFromDb(ICollection<string> qids)
        {
            var list = new List<QuestionDto>();
            var qList = QuestionRepository.Where(q => qids.Contains(q.Id)).ToList();
            if (!qList.Any())
                return list;
            list = qList.MapTo<List<QuestionDto>>();
            list.ForEach(q =>
            {
                if (!string.IsNullOrWhiteSpace(q.Body))
                    q.Body = q.Body.HtmlDecode();
                if (!string.IsNullOrWhiteSpace(q.KnowledgeIDs))
                {
                    var points = JsonHelper.Json<Dictionary<string, string>>(q.KnowledgeIDs);
                    q.Knowledges = points.Select(t => new NameDto(t.Key, t.Value)).ToList();
                }
            });
            var answerQids = new List<string>();
            answerQids.AddRange(qids);
            //小问
            if (list.Any(q => q.HasSmall))
            {
                var ids = list.Where(q => q.HasSmall).Select(q => q.Id).ToList();
                var smallList = LoadSmallQuestions(ids);
                foreach (var dto in list.Where(q => q.HasSmall))
                {
                    if (!smallList.ContainsKey(dto.Id))
                        continue;
                    dto.Details = smallList[dto.Id];
                    answerQids.AddRange(dto.Details.Where(d => d.IsObjective).Select(d => d.Id));
                    //修改序号
                    if (dto.Details.Any() && dto.Details.Exists(u => u.Sort < 1))
                    {
                        dto.Details.Foreach(d => d.Sort = d.Sort + 1);
                    }
                }
            }
            //答案/选项
            var answerList = LoadAnswerDtos(answerQids);
            foreach (var dto in list)
            {
                if (answerList.ContainsKey(dto.Id))
                    dto.Answers = answerList[dto.Id];
                if (dto.Details.IsNullOrEmpty())
                    continue;
                foreach (var detail in dto.Details)
                {
                    if (answerList.ContainsKey(detail.Id))
                        detail.Answers = answerList[detail.Id];
                }
            }
            //解析
            var analysisQids = list.Where(q => q.IsObjective).Select(q => q.Id).ToList();
            if (analysisQids.Any())
            {
                var analysisDict = LoadAnalysisDtos(analysisQids);
                foreach (var dto in list.Where(q => q.IsObjective))
                {
                    if (analysisDict.ContainsKey(dto.Id))
                        dto.Analysis = analysisDict[dto.Id];
                }
            }
            return list;
        }

        private Dictionary<string, List<SmallQuestionDto>> LoadSmallQuestions(ICollection<string> qids)
        {
            var detailDict = new Dictionary<string, List<SmallQuestionDto>>();
            if (qids.IsNullOrEmpty())
                return detailDict;
            Expression<Func<TQ_SmallQuestion, bool>> condition;
            if (qids.Count == 1)
            {
                var id = qids.First();
                condition = d => d.QID == id;
            }
            else
            {
                condition = d => qids.Contains(d.QID);
            }
            detailDict = SmallQuestionRepository.Where(condition)
                .ToList()
                .GroupBy(d => d.QID)
                .ToDictionary(k => k.Key, v => v.OrderBy(d => d.Sort).MapTo<List<SmallQuestionDto>>());
            foreach (var details in detailDict.Values)
            {
                var resetSort = details.Any(d => d.Sort < 1);
                details.ForEach(d =>
                {
                    if (!string.IsNullOrWhiteSpace(d.Body))
                        d.Body = d.Body.HtmlDecode();
                    if (resetSort)
                        d.Sort += 1;
                });
            }
            return detailDict;
        }

        private Dictionary<string, List<AnswerDto>> LoadAnswerDtos(ICollection<string> qids)
        {
            var answerDict = new Dictionary<string, List<AnswerDto>>();
            if (qids.IsNullOrEmpty())
                return answerDict;
            Expression<Func<TQ_Answer, bool>> condition;
            if (qids.Count == 1)
            {
                var id = qids.First();
                condition = a => a.QuestionID == id;
            }
            else
                condition = a => qids.Contains(a.QuestionID);
            answerDict = AnswerRepository.Where(condition)
                .ToList()
                .GroupBy(a => a.QuestionID)
                .ToDictionary(k => k.Key, v => v.OrderBy(a => a.Sort).ToList().MapTo<List<AnswerDto>>());
            foreach (var answer in answerDict.Values)
            {
                answer.ForEach(a =>
                {
                    if (!string.IsNullOrWhiteSpace(a.Body))
                        a.Body = a.Body.HtmlDecode();
                });
            }
            return answerDict;
        }

        private Dictionary<string, AnalysisDto> LoadAnalysisDtos(ICollection<string> qids)
        {
            var analysisDict = new Dictionary<string, AnalysisDto>();
            if (qids.IsNullOrEmpty())
                return analysisDict;
            Expression<Func<TQ_Analysis, bool>> condition;
            if (qids.Count == 1)
            {
                var id = qids.First();
                condition = a => a.QID == id;
            }
            else
            {
                condition = a => qids.Contains(a.QID);
            }
            analysisDict = AnalysisRepository.Where(condition)
                .ToList()
                .ToDictionary(k => k.QID, v => v.MapTo<AnalysisDto>());
            foreach (var analysis in analysisDict.Values)
            {
                analysis.Body = analysis.Body.HtmlDecode();
            }
            return analysisDict;
        }

        #endregion

        #region 转换题目
        /// <summary> 转换题目 </summary>
        private ConvertResult Convert(QuestionDto questionDto, string sourceId = null)
        {
            var result = new ConvertResult();
            if (questionDto == null) return result;
            TQ_Question qItem;
            //另存为
            bool saveAs = !string.IsNullOrWhiteSpace(sourceId),
                //编辑
                update = !saveAs && !string.IsNullOrWhiteSpace(questionDto.Id);
            if (string.IsNullOrWhiteSpace(questionDto.Id))
            {
                qItem = new TQ_Question
                {
                    SubjectID = questionDto.SubjectId,
                    Stage = questionDto.Stage,
                    OptionStyle = questionDto.OptionStyle,
                    IsObjective = (questionDto.Answers != null && questionDto.Answers.Count > 1)
                };
                qItem.Reset(questionDto.UserId);
            }
            else
            {
                qItem = QuestionRepository.Load(questionDto.Id);
                if (saveAs)
                {
                    //重置基础属性
                    qItem.Reset(questionDto.UserId);
                }
            }
            //修改属性
            qItem.OptionStyle = questionDto.OptionStyle;
            qItem.ChangeSourceID = sourceId;
            qItem.QContent = questionDto.Body.FormatBody().ConvertImgs().HtmlEncode();
            qItem.QSummary = string.Empty;
            qItem.HasSmallQuestion = questionDto.HasSmall;
            qItem.DifficultyStar = questionDto.Difficulty;
            qItem.KnowledgeIDs = string.Empty;
            qItem.QType = questionDto.Type;
            //知识点
            if (questionDto.Knowledges != null && questionDto.Knowledges.Count > 0)
                qItem.KnowledgeIDs = JsonHelper.ToJson(questionDto.Knowledges.ToDictionary(k => k.Id, v => v.Name));
            //图片
            if (questionDto.Images != null && questionDto.Images.Length > 0)
            {
                qItem.QImages = questionDto.Images.ToJson();
            }
            else
            {
                qItem.QImages = null;
            }
            qItem.TagIDs = (questionDto.Tags ?? new string[] { }).ToJson();
            qItem.LastModifyTime = Clock.Now;
            // 分享范围
            if (!update && questionDto.Range == (byte)ShareRange.Public)
            {
                qItem.ShareRange = questionDto.Range;
            }

            //小问和选项
            var detailList = new List<TQ_SmallQuestion>();
            var answerList = new List<TQ_Answer>();
            //小问处理
            if (questionDto.Details != null && questionDto.Details.Any())
            {
                var details = questionDto.Details.OrderBy(t => t.Sort).ToList();
                //更新不支持修改题目结构
                if (update && details.Any(d => string.IsNullOrWhiteSpace(d.Id)))
                    return null;
                detailList = details.ParseDetails(qItem.Id, saveAs);
                foreach (var d in details)
                {
                    //小问选项处理
                    if (d.Answers == null || !d.Answers.Any())
                        continue;
                    var answers = d.Answers.OrderBy(t => t.Sort).ToList();
                    //更新不支持修改题目结构
                    if (update && answers.Any(a => string.IsNullOrWhiteSpace(a.Id)))
                        return null;
                    answerList.AddRange(answers.ParseAnswers(d.Id, saveAs));
                }
            }
            //大题答案或选项处理
            if (questionDto.Answers != null && questionDto.Answers.Any())
            {
                //更新不支持修改题目结构
                if (update && questionDto.Answers.Any(a => string.IsNullOrWhiteSpace(a.Id)))
                    return null;
                if (questionDto.Answers.Count == 1)
                {
                    questionDto.Answers[0].Sort = -1;
                    questionDto.Answers[0].IsCorrect = true;
                }
                var answers = questionDto.Answers.OrderBy(t => t.Sort).ToList();
                answerList.AddRange(answers.ParseAnswers(qItem.Id, saveAs));
            }
            if (!qItem.IsObjective && detailList.Any() && detailList.All(t => t.IsObjective))
                qItem.IsObjective = true;
            TQ_Analysis analysis = null;
            //解析
            if (questionDto.Analysis != null)
            {
                analysis = new TQ_Analysis
                {
                    QID = questionDto.Id,
                    AnalysisContent = questionDto.Analysis.Body,
                    Id = (string.IsNullOrWhiteSpace(questionDto.Analysis.Id)
                        ? IdHelper.Instance.Guid32
                        : questionDto.Analysis.Id)
                };
                if (questionDto.Analysis.Images != null && questionDto.Analysis.Images.Any())
                    analysis.AnalysisImage = questionDto.Analysis.Images.ToJson();
            }

            //返回赋值
            //            if (update)
            //            {
            //                var detailIds = detailList.Select(d => d.Id);

            //                var answers =
            //                    AnswerRepository.Where(a => a.QuestionID == qItem.Id || detailIds.Contains(a.QuestionID))
            //                        .ToList();
            //                result.MarkingUpdate =
            //                    answers.Any(a => a.IsCorrect != answerList.First(t => t.Id == a.Id).IsCorrect);
            //            }
            result.Question = qItem;
            result.SmallQuestions = detailList;
            result.Answers = answerList;
            result.Analysis = analysis;
            return result;
        }
        #endregion

        #region 保存题目事务处理

        private int SaveQuestion(TQ_Question question,
            List<TQ_SmallQuestion> details,
            List<TQ_Answer> answers,
            TQ_Analysis analysis = null,
            TQ_Question sourceQuestion = null)
        {
            //添加/更新
            var exists = QuestionRepository.Exists(q => q.Id == question.Id);

            return UnitOfWork.Transaction(() =>
            {
                //存在就更新，不存在就新增
                if (exists)
                {
                    //更新问题
                    QuestionRepository.Update(q => new
                    {
                        q.QContent,
                        q.QImages,
                        q.DifficultyStar,
                        q.KnowledgeIDs,
                        q.LastModifyTime,
                        q.QType,
                        q.ShareRange,
                        q.TagIDs
                    }, question);
                    //更新小问
                    if (details != null && details.Any())
                    {
                        SmallQuestionRepository.Update(d => new
                        {
                            d.SmallQContent,
                            d.SmallQImages
                        }, details.ToArray());
                    }
                    //更新答案/选项
                    if (answers != null && answers.Any())
                    {
                        AnswerRepository.Update(a => new
                        {
                            a.QContent,
                            a.QImages,
                            a.IsCorrect,
                            a.Sort
                        }, answers.ToArray());
                    }
                    //更新解析
                    if (analysis != null)
                    {
                        AnalysisRepository.Update(t => new
                        {
                            t.AnalysisContent,
                            t.AnalysisImage
                        }, analysis);
                    }
                }
                else
                {
                    //新增题目
                    QuestionRepository.Insert(question);
                    //更新源题目
                    if (sourceQuestion != null)
                    {
                        sourceQuestion.LastModifyTime = Clock.Now;
                        QuestionRepository.Update(q => new
                        {
                            q.LastModifyTime
                        }, sourceQuestion);
                    }
                    if (details != null && details.Any())
                    {
                        SmallQuestionRepository.Insert(details);
                    }
                    if (answers != null && answers.Any())
                    {
                        AnswerRepository.Insert(answers);
                    }
                    if (analysis != null)
                    {
                        AnalysisRepository.Insert(analysis);
                    }
                }
            });
        }

        #endregion
    }
}
