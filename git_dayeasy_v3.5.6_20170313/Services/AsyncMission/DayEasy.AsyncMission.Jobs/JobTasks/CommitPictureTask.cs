using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Models;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core.Dependency;
using DayEasy.MongoDb;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DayEasy.AsyncMission.Jobs.JobTasks
{
    /// <summary> 提交图片任务 </summary>
    public class CommitPictureTask : DTask<CommitPictureParam>
    {
        private readonly PaperDetailDto _paper;
        private readonly List<SmallQuScoreDto> _smallScores;
        private readonly Dictionary<string, List<string>> _paperAnswerDict;

        /// <summary> 构造函数 </summary>
        /// <param name="param"></param>
        /// <param name="logAction"></param>
        public CommitPictureTask(CommitPictureParam param, Action<string> logAction = null)
            : base(param, logAction)
        {
            if (param == null || param.PictureIds.IsNullOrEmpty() || string.IsNullOrWhiteSpace(param.PaperId))
                return;
            LogAction("开始异步自动提交...");
            LogAction("共{0}张试卷".FormatWith(param.PictureIds.Count));
            var watch = new Stopwatch();
            watch.Start();
            //试卷信息
            var paperRepository = CurrentIocManager.Resolve<IPaperContract>();
            var result = paperRepository.PaperDetailById(param.PaperId);
            if (result.Status)
                _paper = result.Data;
            //小问分数
            var smallResult = paperRepository.GetSmallQuScore(param.PaperId);
            if (smallResult.Status)
                _smallScores = smallResult.Data.ToList();
            watch.Stop();

            LogAction("加载试卷信息:{0}ms".FormatWith(watch.ElapsedMilliseconds));
            _paperAnswerDict = new Dictionary<string, List<string>>();
        }

        #region 提交数据
        /// <summary> 提交数据 </summary>
        public override DResult Execute()
        {
            if (Param == null || Param.PictureIds.IsNullOrEmpty() || string.IsNullOrWhiteSpace(Param.PaperId))
                return DResult.Error("任务参数异常");
            var watch = new Stopwatch();
            watch.Start();
            //根据AB卷划分题目信息
            var dict = _paper.PaperSections.GroupBy(t => t.PaperSectionType).ToDictionary(k => k.Key,
                v =>
                {
                    var list = new List<PaperQuestionDto>();
                    v.OrderBy(t => t.Sort).Foreach(s =>
                    {
                        list.AddRange(s.Questions.OrderBy(q => q.Sort));
                    });
                    return list;
                });

            //仓储
            var pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
            var resultRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>();
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();

            //提交列表
            var resultList = new List<TP_MarkingResult>();
            var detailList = new List<TP_MarkingDetail>();
            var updateList = new List<TP_MarkingDetail>();

            var idHelper = IdHelper.Instance;
            var paperId = _paper.PaperBaseInfo.Id;

            var pictures = (from p in pictureRepository.Table
                            where Param.PictureIds.Contains(p.Id)
                            join r in resultRepository.Table.Where(r => r.PaperID == paperId)
                                on new { p.StudentID, p.BatchNo } equals new { r.StudentID, BatchNo = r.Batch } into r1
                            from rr in r1.DefaultIfEmpty()
                            select new { picture = p, result = rr })
                .ToList()
                .ToDictionary(k => k.picture.Id, v => v);

            var markingIds = pictures.Values.Where(t => t.result != null).Select(t => t.result.Id).Distinct();
            var detailDict = detailRepository.Where(d => markingIds.Contains(d.MarkingID))
                .Select(d => new { d.MarkingID, d.QuestionID })
                .ToList()
                .GroupBy(d => d.MarkingID)
                .ToDictionary(k => k.Key, v => v.Select(t => t.QuestionID).Distinct().ToList());

            foreach (var pictureId in Param.PictureIds)
            {
                if (!pictures.ContainsKey(pictureId))
                    continue;
                var picture = pictures[pictureId].picture;
                if (picture == null || !picture.IsSuccess)
                    continue;
                var batch = picture.BatchNo;

                var result = pictures[pictureId].result;
                if (result == null)
                {
                    //数据库排重
                    result =
                        resultList.FirstOrDefault(
                            t => t.PaperID == paperId && t.Batch == batch && t.StudentID == picture.StudentID);
                    if (result == null)
                    {
                        //当前排重（同时提交AB卷的情况）
                        result = new TP_MarkingResult
                        {
                            Id = idHelper.Guid32,
                            Batch = batch,
                            PaperID = paperId,
                            StudentID = picture.StudentID,
                            ClassID = picture.ClassID,
                            AddedAt = Clock.Now,
                            AddedIP = Utils.GetRealIp()
                        };
                        resultList.Add(result);
                    }
                }

                var questions = dict[picture.AnswerImgType == 0 ? (byte)1 : picture.AnswerImgType];
                if (questions == null || !questions.Any())
                    continue;

                if (detailDict.ContainsKey(result.Id))
                {
                    var questionIds = detailDict[result.Id];
                    //已提交过了 
                    if (questionIds.Exists(d => d == questions[0].Question.Id))
                    {
                        // 更新客观题
                        // 1. remove detail list first
                        detailRepository.Delete(d => 
                        d.MarkingID == result.Id 
                        && d.PaperID == picture.PaperID 
                        && d.Batch == batch 
                        && d.StudentID == picture.StudentID);

                        // 2. add new detail items to detailList
                        var sts = (picture.SheetAnswers ?? "[]").JsonToObject<List<int[]>>();
                        FillDetails(result.Id, paperId, batch, picture.StudentID, detailList, sts, questions);

                        continue;
                    }
                }

                var sheets = (picture.SheetAnswers ?? "[]").JsonToObject<List<int[]>>();
                FillDetails(result.Id, paperId, batch, picture.StudentID, detailList, sheets, questions);
            }
            watch.Stop();
            LogAction("组装数据：{0}ms".FormatWith(watch.ElapsedMilliseconds));
            watch.Restart();

            var resultcCode = resultRepository.UnitOfWork.Transaction(() =>
            {
                if (resultList.Any())
                    resultRepository.Insert(resultList);
                if (detailList.Any())
                    detailRepository.Insert(detailList);
            });
            if (resultcCode > 0 && !string.IsNullOrWhiteSpace(Param.JointBatch))
            {
                //协同阅卷任务
                //pictureDicts = InitJointPictures(pictures.Values.Select(t => t.picture));
                //主观题
                var questionDict = dict.ToDictionary(k => k.Key,
                    v => v.Value.Where(q => !q.Question.IsObjective).Select(t => t.Question.Id).ToList());
                //图片
                var pictureDict = pictures.Values.Select(t => t.picture)
                    .GroupBy(t => t.AnswerImgType == 0 ? (byte)1 : t.AnswerImgType)
                    .ToDictionary(k => k.Key, v => v.Select(t => t.Id).ToList());
                InitPictureTask(pictureDict, questionDict);
            }
            watch.Stop();
            LogAction("提交数据：{0}ms".FormatWith(watch.ElapsedMilliseconds));
            return DResult.Success;
        }
        #endregion

        #region 填充阅卷详情
        /// <summary> 填充阅卷详情 </summary>
        private void FillDetails(string markingId, string paperId, string batch, long studentId,
            ICollection<TP_MarkingDetail> detailList, IList<int[]> sheets, IEnumerable<PaperQuestionDto> questions)
        {
            var idHelper = IdHelper.Instance;
            foreach (var item in questions)
            {
                var question = new
                {
                    item.Question.Id,
                    item.Question.IsObjective,
                    item.Question.Details,
                    item.Score,
                    item.Question.Type,
                    item.Question.Answers
                };
                var qid = question.Id;
                if (question.IsObjective && question.Details != null && question.Details.Any())
                {
                    //客观题小问
                    foreach (var dto in question.Details)
                    {
                        var smallId = dto.Id;
                        var detail = new TP_MarkingDetail
                        {
                            Id = idHelper.Guid32,
                            MarkingID = markingId,
                            PaperID = paperId,
                            Batch = batch,
                            StudentID = studentId,
                            QuestionID = qid,
                            SmallQID = dto.Id,
                            AnswerTime = Clock.Now,
                            IsCorrect = false,
                            CurrentScore = 0M,
                            IsFinished = true,
                            MarkingAt = Clock.Now,
                            MarkingBy = 0
                        };
                        var score = _smallScores.FirstOrDefault(s => s.SmallQId == smallId);
                        detail.Score = (score != null
                            ? score.Score
                            : Math.Round(question.Score / question.Details.Count, 1));

                        var sheet = sheets.FirstOrDefault();
                        detail.StudentAnswers(sheet, dto.Answers);
                        List<string> rightAnswers;
                        if (_paperAnswerDict.ContainsKey(smallId))
                            rightAnswers = _paperAnswerDict[smallId];
                        else
                        {
                            rightAnswers = dto.Answers.Where(t => t.IsCorrect).Select(t => t.Id).ToList();
                            _paperAnswerDict.Add(smallId, rightAnswers);
                        }
                        detail.AutoMarking(rightAnswers, question.Type == 3);
                        if (sheet != null)
                            sheets.RemoveAt(0);
                        detailList.Add(detail);
                    }
                }
                else
                {
                    var detail = new TP_MarkingDetail
                    {
                        Id = idHelper.Guid32,
                        MarkingID = markingId,
                        PaperID = paperId,
                        Batch = batch,
                        StudentID = studentId,
                        QuestionID = qid,
                        Score = question.Score,
                        AnswerTime = Clock.Now
                    };
                    if (question.IsObjective)
                    {
                        //客观题
                        detail.IsCorrect = false;
                        detail.CurrentScore = 0M;
                        detail.IsFinished = true;
                        detail.MarkingAt = Clock.Now;
                        detail.MarkingBy = 0;
                        var sheet = sheets.FirstOrDefault();
                        detail.StudentAnswers(sheet, question.Answers);
                        List<string> rightAnswers;
                        if (_paperAnswerDict.ContainsKey(qid))
                            rightAnswers = _paperAnswerDict[qid];
                        else
                        {
                            rightAnswers = question.Answers.Where(t => t.IsCorrect).Select(t => t.Id).ToList();
                            _paperAnswerDict.Add(qid, rightAnswers);
                        }
                        detail.AutoMarking(rightAnswers, question.Type == 3);
                        if (sheet != null)
                            sheets.RemoveAt(0);
                    }
                    else
                    {
                        //主观题，默认正确
                        detail.IsCorrect = true;
                        detail.CurrentScore = detail.Score;
                    }
                    detailList.Add(detail);
                }
            }
        }

        /// <summary> 初始化试卷任务 </summary>
        private void InitPictureTask(IDictionary<byte, List<string>> pictures, IDictionary<byte, List<string>> qids)
        {
            if (string.IsNullOrWhiteSpace(Param.JointBatch) || pictures.IsNullOrEmpty() || qids.IsNullOrEmpty())
                return;
            var collection = new MongoManager().Collection<MPictureTask>();
            var helper = IdHelper.Instance;
            var tasks = new List<MPictureTask>();
            foreach (var picture in pictures)
            {
                if (!qids.ContainsKey(picture.Key))
                    continue;
                var list = qids[picture.Key];
                foreach (var id in picture.Value)
                {
                    //判断重复提交
                    if (collection.Count(Query.EQ(nameof(MPictureTask.PictureId), id)) > 0)
                        continue;
                    var task = new MPictureTask
                    {
                        Id = helper.Guid32,
                        JointBatch = Param.JointBatch,
                        PictureId = id,
                        QuestionTask = list.Select(q => new QuestionTask
                        {
                            Id = q,
                            TeacherId = 0
                        }).ToList()
                    };
                    tasks.Add(task);
                }
            }
            collection.InsertBatch(tasks);
        }
        #endregion
    }
}
