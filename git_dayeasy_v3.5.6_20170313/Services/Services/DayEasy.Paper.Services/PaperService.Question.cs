using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Paper.Services.Helper.Question;
using DayEasy.Paper.Services.Model;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.Paper.Services
{
    /// <summary> 试卷相关契约 - 题库板块 </summary>
    public partial class PaperService
    {
        #region 仓储声明/私有方法

        public IDayEasyRepository<TQ_Question, string> QuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_SmallQuestion, string> SmallQuestionRepository { private get; set; }
        public IDayEasyRepository<TQ_Answer, string> AnswerRepository { private get; set; }
        public IDayEasyRepository<TQ_Analysis, string> AnalysisRepository { private get; set; }
        public IDayEasyRepository<TQ_AgencyQuestion, string> AgencyQuestionRepository { private get; set; }

        #endregion

        #region 题目加载

        public QuestionDto LoadQuestion(string questionId, bool fromCache = true)
        {
            if (!fromCache)
                return LoadQuestionFromDb(questionId);
            var item = QuestionCache.Instance.Get(questionId);
            if (item != null)
            {
                return item;
            }
            item = LoadQuestionFromDb(questionId);
            if (item != null)
                QuestionCache.Instance.Set(item);
            return item;
        }

        public List<QuestionDto> LoadQuestions(ICollection<string> questionIds)
        {
            var list = new List<QuestionDto>();
            if (questionIds == null || !questionIds.Any())
                return list;
            questionIds = questionIds.Distinct().ToArray();
            var loadIds = new List<string>();
            //缓存优先
            foreach (var id in questionIds)
            {
                var item = QuestionCache.Instance.Get(id);
                if (item == null)
                {
                    loadIds.Add(id);
                }
                else
                {
                    list.Add(item);
                }
            }
            //数据库加载
            if (loadIds.Any())
            {
                var listFromDb = LoadQuestionListFromDb(loadIds);
                foreach (var dto in listFromDb)
                {
                    QuestionCache.Instance.Set(dto);
                }
                list.AddRange(listFromDb);
            }
            var orderList = questionIds.ToList();
            return list.OrderBy(q => orderList.IndexOf(q.Id)).ToList();
        }

        public DResults<QuestionDto> SearchQuestion(SearchQuestionDto search)
        {
            if (search == null) return DResult.Succ(new List<QuestionDto>(), 0);
            //var query = search.MapTo<QuestionQuery>();
            var query = new QuestionQuery();
            query.AddedBy = search.UserId;
            query.Keyword = search.Keyword;
            query.IsHighLight = search.IsHighLight;
            query.LoadCreator = search.LoadCreator;
            query.NotInIds = search.NotInIds;
            query.Page = search.Page;
            query.Points = search.Points;
            query.Size = search.Size;
            query.Stages = search.Stages;
            query.SubjectId = search.SubjectId;
            query.Tags = search.Tags;
            query.ShareRange = search.ShareRange;
            query.QuestionType = search.QuestionType;
            query.Order = (QuestionOrderType)(byte)search.Order;
            query.TagSortFirstStr = "精选";
            if (search.Difficulties!=null)
                query.Difficulties = search.Difficulties.ToArray();
            else
                query.Difficulties = null;
            //Solr搜索
            return QuestionManager.Instance.Search(query);
        }

        #endregion

        #region 删除题目

        public DResult DeleteQuestion(string questionId, long userId)
        {
            if (string.IsNullOrWhiteSpace(questionId) || questionId.Length != 32)
                return DResult.Error("题目ID异常，请重试！");
            if (userId <= 0)
                return DResult.Error("操作人ID异常！");
            var item = QuestionRepository.Load(questionId);
            if (item == null)
                return DResult.Error("未找到相关题目信息！");
            if (item.AddedBy != userId)
                return DResult.Error("您不能删除别人的题目！");
            if (item.Status == (byte)NormalStatus.Delete)
                return DResult.Success;
            item.Status = (byte)NormalStatus.Delete;
            item.LastModifyTime = Clock.Now;
            var result = QuestionRepository.Update(q => new
            {
                q.Status,
                q.LastModifyTime
            }, item);
            //去掉出题统计~ confirm with gan

            //更新缓存
            if (result > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    //删除检索
                    QuestionManager.Instance.DeleteAsync(questionId);
                    //删除缓存
                    QuestionCache.Instance.Remove(questionId);
                });
            }
            return DResult.FromResult(result);
        }

        public Dictionary<string, string[]> QuestionAnswer(string questionId, string smallId = null,
            string paperId = null)
        {
            var answerDict = new Dictionary<string, string[]>();
            var ids = new List<string>();
            if (string.IsNullOrWhiteSpace(smallId))
            {
                ids = SmallQuestionRepository.Where(t => t.QID == questionId).Select(t => t.Id).ToList();
                ids.Add(questionId);
            }
            else
            {
                ids.AddRange(new[] { questionId, smallId });
            }
            //题目答案
            var answers = AnswerRepository.Where(t => ids.Contains(t.QuestionID)).GroupBy(t => t.QuestionID).ToList();
            var paperAnswers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(paperId))
            {
                //试卷答案
                paperAnswers = PaperAnswerRepository.Where(t => t.PaperId == paperId && t.QuestionId == questionId)
                    .ToDictionary(k => (string.IsNullOrWhiteSpace(k.SmallQuId) ? k.QuestionId : k.SmallQuId),
                        v => v.AnswerContent);
            }
            foreach (var answer in answers)
            {
                if (paperAnswers.ContainsKey(answer.Key))
                {
                    var content = paperAnswers[answer.Key];
                    var currentAnswers = content.Select(t => (int)(t) - 65).ToList();
                    answerDict.Add(answer.Key,
                        answer.Where(t => currentAnswers.Contains(t.Sort)).Select(t => t.Id).ToArray());
                }
                else
                {
                    answerDict.Add(answer.Key, answer.Where(t => t.IsCorrect).Select(t => t.Id).ToArray());
                }
            }
            return answerDict;
        }

        public DResult QuestionShare(string questionId, ShareRange share, long userId)
        {
            if (string.IsNullOrWhiteSpace(questionId))
                return DResult.Error("问题ID为空！");
            var item =
                QuestionRepository.SingleOrDefault(q => q.Id == questionId && q.Status == (byte)NormalStatus.Normal);
            if (item == null)
                return DResult.Error("问题不存在！");
            if (item.AddedBy != userId)
                return DResult.Error("您没有权限分享别人的题目！");
            if (item.ShareRange == (byte)share)
                return DResult.Error("分享状态未发生变化！");
            item.ShareRange = (byte)share;
            var result = QuestionRepository.Update(q => new
            {
                q.ShareRange
            }, item);
            if (result > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    //清除缓存
                    QuestionCache.Instance.Remove(item.Id);
                    //更新索引
                    QuestionManager.Instance.UpdateAsync(item.Id);
                });
            }
            return DResult.FromResult(result);
        }

        #endregion

        #region 保存题目

        public DResult<string> SaveQuestion(QuestionDto questionDto, bool saveAs = false, bool isDraft = false)
        {
            var checkResult = questionDto.CheckQuestion();
            if (!checkResult.Status)
                return DResult.Error<string>(checkResult.Message);
            var convertResult = Convert(questionDto, saveAs ? questionDto.Id : null);
            if (convertResult == null)
                return DResult.Error<string>("题目转换失败~！");
            try
            {
                if (isDraft)
                {
                    convertResult.Question.Status = (byte)NormalStatus.Delete;
                }

                var result = SaveQuestion(convertResult.Question,
                    convertResult.SmallQuestions, convertResult.Answers, convertResult.Analysis);
                if (result <= 0)
                {
                    return DResult.Error<string>("保存失败~！");
                }
                Task.Factory.StartNew(() =>
                {
                    //if (convertResult.MarkingUpdate)
                    //{
                    //    //通知更新未完成阅卷的试卷
                    //    new ChangeObserver().NotifyAsync(convertResult.Question.Id);
                    //}
                    //清除缓存
                    QuestionCache.Instance.Remove(convertResult.Question.Id);
                    //更新索引
                    QuestionManager.Instance.UpdateAsync(convertResult.Question.Id);
                });

                return DResult.Succ(convertResult.Question.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return DResult.Error<string>("保存题目数据异常");
            }
        }

        public DResult SaveQuestionKnowledge(string questionId, List<NameDto> knowledges)
        {
            if (string.IsNullOrWhiteSpace(questionId))
                return DResult.Error("问题ID未找到");
            if (knowledges.IsNullOrEmpty())
                return DResult.Error("请选择要保存的知识点！");
            var knowledge = JsonHelper.ToJson(knowledges.ToDictionary(k => k.Id, v => v.Name));
            var result = QuestionRepository.Update(new TQ_Question
            {
                KnowledgeIDs = knowledge
            }, q => q.Id == questionId, "KnowledgeIDs");
            if (result > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    //清除缓存
                    QuestionCache.Instance.Remove(questionId);
                    //更新索引
                    QuestionManager.Instance.UpdateAsync(questionId);
                });
            }
            return DResult.FromResult(result);
        }

        public DResult SaveQuestionBody(string questionId, string body)
        {
            if (string.IsNullOrWhiteSpace(questionId))
                return DResult.Error("试题ID未找到");
            if (string.IsNullOrWhiteSpace(body))
                return DResult.Error("试题题干不能为空！");
            var result = QuestionRepository.Update(new TQ_Question
            {
                QContent = body.FormatBody().ConvertImgs().HtmlEncode()
            }, q => q.Id == questionId, "QContent");
            if (result > 0)
            {
                ClearQuestionCacheAsync(questionId);
            }
            return DResult.FromResult(result);
        }

        public List<QuestionDto> Variant(string questionId, int count = 1, List<string> excepArray = null, bool isNullData = false)
        {
            var item = LoadQuestion(questionId);
            var result = QuestionManager.Instance.Variant(item, count, excepArray,isNullData);
            if (result.Status && result.Data != null)
                return result.Data.ToList();
            return new List<QuestionDto>();
        }

        public List<string> VariantForIds(string questionId, int count = 1, List<string> excepArray = null)
        {
            var item = LoadQuestion(questionId);
            var result = QuestionManager.Instance.VariantIds(item, count, excepArray);
            if (result.Status && result.Data != null)
                return result.Data.ToList();
            return new List<string>();
        }

        public Task ClearQuestionCacheAsync(params string[] qids)
        {
            return Task.Factory.StartNew(() =>
            {
                if (qids == null || qids.Length == 0)
                    return;
                foreach (var qid in qids)
                {
                    //清除缓存
                    QuestionCache.Instance.Remove(qid);
                    //删除检索
                    QuestionManager.Instance.DeleteAsync(qid);
                    //更新索引
                    QuestionManager.Instance.UpdateAsync(qid);
                    QuestionManager.Instance.Update(qid);
                }
            });
        }
        #endregion
    }
}
