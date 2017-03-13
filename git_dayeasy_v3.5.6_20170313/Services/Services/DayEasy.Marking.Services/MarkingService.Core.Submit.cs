
using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Marking.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Marking.Services
{
    /// <summary> 试卷提交-业务处理 </summary>
    public partial class MarkingService
    {
        public IDayEasyRepository< TC_VideoClassContent, string> VideoContentRepository { private get; set; }

        /// <summary> 根据试卷图片自动提交 </summary>
        /// <param name="pictureId">试卷图片ID</param>
        /// <returns></returns>
        private DResult AutoCommit(string pictureId)
        {
            var picture = MarkingPictureRepository.FirstOrDefault(t => t.Id == pictureId);
            if (picture == null)
                return DResult.Error(MarkingConsts.MsgPaperNotFind);
            if (picture.LastMarkingTime.HasValue)
                return DResult.Error(MarkingConsts.MsgSubmitAgain);
            var data = new MkResultDto
            {
                Batch = picture.BatchNo,
                ClassId = picture.ClassID,
                PaperId = picture.PaperID,
                SectionType = picture.AnswerImgType,
                StudentId = picture.StudentID,
                Details = new List<MkDetailDto>(),
                SheetAnswers = picture.SheetAnswers
            };
            var result = Commit(data, true);
            //更新最后批阅时间
            if (result.Status)
            {
                picture.LastMarkingTime = DateTime.Now;
                MarkingPictureRepository.Update(p => new { p.LastMarkingTime }, picture);
            }
            return result;
        }

        #region 首次提交，自动批阅 + 生成记录

        private DResult Commit(MkResultDto resultDto, bool isPrint = false)
        {
            //1、信息检查
            var result = CheckCommit(resultDto);
            if (!result.Status)
                return result;
            //2、填充 + 自动批阅
            result = FillAndAutoMarking(resultDto);
            if (!result.Status)
                return result;
            //3、生成记录
            var val = GenarationResult(resultDto);
            return val <= 0
                ? DResult.Error(MarkingConsts.MsgCommitError)
                : MarkingConsts.Success;
        }

        #endregion

        #region 提交信息检测

        /// <summary> 提交信息检测 </summary>
        /// <returns></returns>
        private DResult CheckCommit(MkResultDto resultDto)
        {
            //基础信息验证
            if (resultDto == null)
                return DResult.Error(MarkingConsts.MsgSubmitNull);

            if (string.IsNullOrEmpty(resultDto.Batch) || string.IsNullOrWhiteSpace(resultDto.PaperId))
                return DResult.Error(MarkingConsts.MsgPaperNotFind);

            var usage = UsageRepository.FirstOrDefault(u => u.Id == resultDto.Batch);
            if (usage == null)
                return DResult.Error(MarkingConsts.MsgBatchNotFind);
            //验证时效性，打印除外
            if (usage.SourceType != (byte)PublishType.Print)
            {
                if (usage.StartTime > DateTime.Now)
                    return DResult.Error(MarkingConsts.MsgNotStart);
                if (usage.ExpireTime < DateTime.Now)
                    return DResult.Error(MarkingConsts.MsgEnded);
            }

            if (usage.SourceType != (byte)PublishType.Video)
            {
                //试卷ID验证
                if (usage.SourceID != resultDto.PaperId)
                    return DResult.Error(MarkingConsts.MsgPaperIdError);
            }
            else
            {
                //课堂验证
                var hasPaper = VideoContentRepository.Exists(c =>
                    c.ClassRID == usage.SourceID &&
                    c.SourceId == resultDto.PaperId &&
                    c.ContentType == (byte)PublishType.Test);
                if (!hasPaper)
                    return DResult.Error(MarkingConsts.MsgPaperIdError);
            }
            //重复性验证
            var exists = MarkingResultRepository.Exists(m =>
                m.Batch == resultDto.Batch &&
                m.PaperID == resultDto.PaperId &&
                m.StudentID == resultDto.StudentId);
            //Result是否存在~！
            resultDto.IsFinished = exists;
            //打印的可以重复提交
            return exists && resultDto.SectionType == (byte)MarkingPaperType.Normal
                ? DResult.Error(MarkingConsts.MsgSubmitAgain)
                : MarkingConsts.Success;
        }

        #endregion

        #region 填充 + 自动阅卷客观题

        /// <summary> 填充 + 自动阅卷客观题 </summary>
        /// <returns></returns>
        private DResult FillAndAutoMarking(MkResultDto resultDto)
        {
            var paperResult = PaperContract.PaperDetailById(resultDto.PaperId);
            if (!paperResult.Status)
                return DResult.Error(MarkingConsts.MsgPaperNotFind);

            var paper = paperResult.Data;
            //区分AB卷分开提交
            Func<PaperSectionDto, bool> condition = ps => ps.Questions != null;
            if (resultDto.SectionType != (byte)MarkingPaperType.Normal)
            {
                condition = ps => ps.Questions != null && ps.PaperSectionType == resultDto.SectionType;
            }
            //问题列表
            var qList = new List<PaperQuestionDto>();
            paper.PaperSections.Where(condition).OrderBy(s => s.Sort).Foreach(t =>
            {
                qList.AddRange(t.Questions.OrderBy(q=>q.Sort));
            });

            var scoreResult = PaperContract.GetSmallQuScore(resultDto.PaperId);
            FillMarking(qList, scoreResult.Data, resultDto);
            //区分AB卷
            if (resultDto.SectionType != (byte)MarkingPaperType.Normal)
            {
                var id = resultDto.Details.First().QuestionId;
                var exists = MarkingDetailRepository.Exists(t =>
                    t.Batch == resultDto.Batch && t.PaperID == resultDto.PaperId &&
                    t.StudentID == resultDto.StudentId && t.QuestionID == id);
                if (exists)
                    return DResult.Error(MarkingConsts.MsgSubmitAgain);
            }
            return AutoMarking(qList, resultDto);
        }

        /// <summary> 填充阅卷结果 </summary>
        /// <param name="qList"></param>
        /// <param name="smallQScores"></param>
        /// <param name="resultDto"></param>
        private void FillMarking(List<PaperQuestionDto> qList, IEnumerable<SmallQuScoreDto> smallQScores, MkResultDto resultDto)
        {
            //填充空白答案
            var qIds = resultDto.Details.Select(u => u.QuestionId).ToList();
            //答题卡
            var sheets = resultDto.SheetAnswers.JsonToObject<List<int[]>>() ?? new List<int[]>();

            //填充Details
            foreach (var qItem in qList)
            {
                if (qIds.Contains(qItem.Question.Id))
                    continue;
                if (qItem.Question.HasSmall && qItem.Question.IsObjective)
                {
                    //客观题有小问
                    var smallQIds = qItem.Question.Details.OrderBy(t => t.Sort).Select(u => u.Id).ToList();
                    foreach (var qId in smallQIds)
                    {
                        var detail = new MkDetailDto
                        {
                            QuestionId = qItem.Question.Id,
                            SmallQuestionId = qId
                        };
                        //答题卡赋值
                        if (sheets.Any())
                        {
                            detail.AnswerIndexs = sheets[0];
                            sheets.RemoveAt(0);
                        }
                        resultDto.Details.Add(detail);
                    }
                }
                else //无小问
                {
                    var detail = new MkDetailDto { QuestionId = qItem.Question.Id };
                    //答题卡赋值
                    if (qItem.Question.IsObjective && sheets.Any())
                    {
                        detail.AnswerIndexs = sheets[0];
                        sheets.RemoveAt(0);
                    }
                    resultDto.Details.Add(detail);
                }
            }

            //获取题目分数
            foreach (var detail in resultDto.Details)
            {
                List<AnswerDto> aList;
                if (!string.IsNullOrWhiteSpace(detail.SmallQuestionId))
                {
                    aList =
                        qList.First(t => t.Question.Id == detail.QuestionId)
                            .Question.Details.First(t => t.Id == detail.SmallQuestionId).Answers;
                    //有小问
                    if (smallQScores != null)
                    {
                        var dItem = smallQScores.FirstOrDefault(s => s.SmallQId == detail.SmallQuestionId);
                        if (dItem != null)
                            detail.Score = dItem.Score;
                    }
                    //小问没有分数 - 使用大题的分数计算平均分
                    if (detail.Score == 0)
                    {
                        var item = qList.FirstOrDefault(q => q.Question.Id == detail.QuestionId);
                        var smallCount = resultDto.Details.Count(d => d.QuestionId == detail.QuestionId);
                        if (item != null && item.Score > 0 && smallCount > 0)
                        {
                            decimal score;
                            decimal.TryParse((item.Score / smallCount).ToString("0.0"), out score);
                            detail.Score = score;
                        }
                    }
                }
                else
                {
                    aList =
                        qList.First(t => t.Question.Id == detail.QuestionId).Question.Answers;
                    var item = qList.FirstOrDefault(q => q.Question.Id == detail.QuestionId);
                    if (item != null)
                        detail.Score = item.Score;
                }

                if (detail.AnswerIndexs != null && detail.AnswerIndexs.Any())
                {
                    detail.AnswerIdList =
                        aList.Where(t => detail.AnswerIndexs.Contains(t.Sort)).Select(t => t.Id).ToArray();
                    //根据选项序号
                    detail.AnswerContent = detail.AnswerIndexs.Where(t => t >= 0).Aggregate(string.Empty,
                        (c, t) => c + Consts.OptionWords[t]);
                }
                //更新客观题选项值
                else if (detail.AnswerIdList != null && detail.AnswerIdList.Any())
                {
                    //根据ID
                    var ids = detail.AnswerIdList;
                    var sorts = aList.Where(a => ids.Contains(a.Id)).Select(a => a.Sort).OrderBy(s => s);
                    detail.AnswerContent = sorts.Aggregate(string.Empty, (c, t) => c + Consts.OptionWords[t]);
                }
            }
        }

        /// <summary> 自动阅卷 </summary>
        /// <param name="qList"></param>
        /// <param name="resultDto"></param>
        private DResult AutoMarking(IEnumerable<PaperQuestionDto> qList, MkResultDto resultDto)
        {
            //客观题阅卷
            var questions = qList.Where(q => q.Question.IsObjective);
            foreach (var question in questions)
            {
                var qid = question.Question.Id;

                var items = resultDto.Details.Where(d => d.QuestionId == qid).ToList();
                if (!items.Any())
                    continue;
                //阅小问选项
                foreach (var item in items)
                {
                    item.IsFinished = true;
                    item.IsCorrect = false;
                    if (item.AnswerIdList == null || !item.AnswerIdList.Any())
                        continue;
                    //根据ID批阅
                    List<string> answers;
                    if (string.IsNullOrWhiteSpace(item.SmallQuestionId))
                        answers = question.Question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
                    else
                    {
                        var detail =
                            question.Question.Details.FirstOrDefault(t => t.Id == item.SmallQuestionId);
                        if (detail == null)
                            continue;
                        answers = detail.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
                    }
                    item.IsCorrect = MarkingConsts.ArrayEquals(item.AnswerIdList, answers.ToArray());
                    if (!item.IsCorrect.Value && question.Question.Type == 3)
                    {
                        //不定项扣一半
                        if (item.AnswerIdList.All(a => answers.Contains(a)))
                            item.CurrentScore = item.Score / 2M;
                    }
                }
            }
            resultDto.Details.ForEach(d =>
            {
                if (d.IsFinished)
                    return;
                d.IsCorrect = true;
            });
            return MarkingConsts.Success;
        }
        #endregion

        #region 生成Result,并提交到数据库
        /// <summary>
        /// 生成Result,并提交到数据库
        /// </summary>
        /// <returns></returns>
        private int GenarationResult(MkResultDto resultDto)
        {
            TP_MarkingResult item;
            var details = new List<TP_MarkingDetail>();
            var helper = IdHelper.Instance;
            //已提交过了
            if (resultDto.IsFinished)
            {
                item = MarkingResultRepository.FirstOrDefault(t =>
                    t.Batch == resultDto.Batch &&
                    t.PaperID == resultDto.PaperId &&
                    t.StudentID == resultDto.StudentId);
            }
            else
            {
                item = new TP_MarkingResult
                {
                    Id = helper.Guid32,
                    PaperID = resultDto.PaperId,
                    Batch = resultDto.Batch,
                    StudentID = resultDto.StudentId,
                    ClassID = resultDto.ClassId,
                    IsFinished = false,
                    AddedAt = DateTime.Now,
                    AddedIP = Utils.GetRealIp(),
                    ErrorQuestionCount = 0,
                    TotalScore = 0
                };
            }

            foreach (var detail in resultDto.Details)
            {
                var dItem = new TP_MarkingDetail
                {
                    Id = helper.Guid32,
                    MarkingID = item.Id,
                    PaperID = item.PaperID,
                    Batch = item.Batch,
                    QuestionID = detail.QuestionId,
                    SmallQID = detail.SmallQuestionId,
                    StudentID = item.StudentID
                };
                if (detail.AnswerIdList != null && detail.AnswerIdList.Length > 0 && detail.AnswerIdList[0] != "null")
                    dItem.AnswerIDs = detail.AnswerIdList.ToJson();
                dItem.AnswerContent = detail.AnswerContent;
                if (detail.AnswerImageList != null && detail.AnswerImageList.Length > 0 && detail.AnswerImageList[0] != "null")
                    dItem.AnswerImages = detail.AnswerImageList.ToJson();
                //设置得分
                dItem.AnswerTime = DateTime.Now;
                dItem.Score = detail.Score;
                dItem.CurrentScore = 0;
                dItem.IsCorrect = detail.IsCorrect;
                if (detail.IsCorrect.HasValue && detail.IsCorrect.Value)
                    dItem.CurrentScore = detail.Score;
                else if (detail.CurrentScore.HasValue && detail.CurrentScore > 0)
                    dItem.CurrentScore = detail.CurrentScore.Value;
                if (detail.IsFinished)
                {
                    dItem.MarkingBy = 0;
                    dItem.IsFinished = true;
                    dItem.MarkingAt = DateTime.Now;
                }
                details.Add(dItem);
            }
            //重复提交判断
            var i = GenerateResult(details, (resultDto.IsFinished ? null : item));
            if (i <= 0) return i;
            //:todo
            ////统计相关
            //int aqCount = _result.Details.Select(q => q.QuestionId).Distinct().Count();
            ////学生完成作业统计
            //new StudentStatisticFacade().UpdateStatistic(item.Batch, item.StudentID, item.PaperID, aqCount,
            //    0, _isPrint);
            //return i;
            return i;
        }
        #endregion
    }
}
