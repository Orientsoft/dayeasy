using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Mongo;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.MongoDb;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Management.Services.Helper
{
    public class KnowledgeMover
    {
        private readonly ILogger _logger = LogManager.Logger<KnowledgeMover>();
        private static bool _isRunning;

        private KnowledgeMover()
        {
        }

        public static KnowledgeMover Instance
        {
            get
            {
                return Singleton<KnowledgeMover>.Instance ?? (Singleton<KnowledgeMover>.Instance = new KnowledgeMover());
            }
        }

        private static MongoCollection<MKnowledgeMover> Collection()
        {
            return new MongoManager().Collection<MKnowledgeMover>();
        }

        public DResult AddMove(string code, string name, string targetCode, string targetName)
        {
            var collection = Collection();
            var item = collection.FindOne(Query.EQ("Code", code));
            if (item != null)
                return DResult.Error("该知识点已在转移队列中！");
            item = new MKnowledgeMover
            {
                Id = IdHelper.Instance.Guid32,
                Code = code,
                Name = name,
                TargetCode = targetCode,
                TargetName = targetName,
                Creation = Clock.Now,
                Status = (byte)NormalStatus.Normal
            };
            collection.Insert(item);
            if (!_isRunning)
                MoveMission();
            return DResult.Success;
        }

        public void MoveMission()
        {
            Task.Factory.StartNew(() =>
            {
                _isRunning = true;
                var collection = Collection();
                MKnowledgeMover item;
                var normalQuery = Query.EQ("Status", (byte)NormalStatus.Normal);
                while ((item = collection.FindOne(normalQuery)) != null)
                {
                    string code = item.Code,
                        targetCode = item.TargetCode,
                        targetName = item.TargetName;
                    var query = Query.EQ("_id", item.Id);
                    var logger = new StringBuilder();
                    var watch = new Stopwatch();
                    watch.Start();
                    collection.Update(query, Update.Set("StartTime", Clock.Now));
                    try
                    {
                        MoveQuestion(code, targetCode, targetName, logger);
                        MovePaper(code, targetCode, targetName, logger);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                    finally
                    {
                        watch.Stop();
                        logger.AppendLine("总耗时：{0}ms".FormatWith(watch.ElapsedMilliseconds));
                        collection.Update(query,
                            Update.Set("Status", (byte)NormalStatus.Delete).Set("FinishedTime", Clock.Now));
                        logger.AppendLine(
                            "队列剩余：{0}".FormatWith(collection.Count(normalQuery)));
                        _logger.Info(logger.ToString());
                    }
                }
                _isRunning = false;
            });
        }

        private static void MoveQuestion(string code, string target, string targetName, StringBuilder logger)
        {
            logger.AppendLine(FormatMessage("开始转移试题{0}->{1}".FormatWith(code, target)));
            var questionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();
            //题目
            var qList =
                questionRepository.Where(t => t.KnowledgeIDs != null && t.KnowledgeIDs.Contains("\"" + code))
                    .Select(q => q.Id).ToList();
            logger.AppendLine(FormatMessage("找到{0}道试题".FormatWith(qList.Count)));
            if (!qList.Any())
                return;
            var result = 0;
            foreach (var qid in qList)
            {
                var qItem = questionRepository.Load(qid);
                var knowledges = JsonHelper.Json<Dictionary<string, string>>(qItem.KnowledgeIDs);
                foreach (var key in knowledges.Keys.ToList())
                {
                    if (key.StartsWith(code))
                        knowledges.Remove(key);
                }
                if (!knowledges.ContainsKey(target))
                    knowledges.Add(target, targetName);
                qItem.KnowledgeIDs = JsonHelper.ToJson(knowledges);
                var c = questionRepository.Update(t => new { t.KnowledgeIDs }, qItem);
                if (c <= 0)
                    continue;
                //清空缓存
                result++;
                CurrentIocManager.Resolve<IPaperContract>().ClearQuestionCacheAsync(qItem.Id);
            }
            logger.AppendLine(FormatMessage("成功转移{0}道试题！".FormatWith(result)));
        }

        private static void MovePaper(string code, string target, string targetName, StringBuilder logger)
        {
            var paperRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>();
            var pList =
                paperRepository.Where(p => p.KnowledgeIDs != null && p.KnowledgeIDs.Contains("\"" + code)).ToList();
            logger.AppendLine(FormatMessage("找到{0}套试卷".FormatWith(pList.Count)));
            if (!pList.Any())
                return;
            var result = 0;
            foreach (var pItem in pList)
            {
                var knowledges = JsonHelper.Json<Dictionary<string, string>>(pItem.KnowledgeIDs);
                foreach (var key in knowledges.Keys.ToList())
                {
                    if (key.StartsWith(code))
                        knowledges.Remove(key);
                }
                if (!knowledges.ContainsKey(target))
                    knowledges.Add(target, targetName);
                pItem.KnowledgeIDs = JsonHelper.ToJson(knowledges);
                result += paperRepository.Update(t => new { t.KnowledgeIDs }, pItem);
            }

            logger.AppendLine(FormatMessage("成功转移{0}套试卷！".FormatWith(result)));
        }

        private static string FormatMessage(string msg)
        {
            return string.Format("{0} --- {1}", msg, Utils.GetTimeNow("HH:mm:ss"));
        }

        public DResults<MKnowledgeMover> MoveList(int status, string key, int page = 0, int size = 12)
        {
            var collection = Collection();
            var query = Query.Empty;
            if (status >= 0)
            {
                query = Query.And(query, Query.EQ("Status", status));
            }
            if (!string.IsNullOrWhiteSpace(key))
            {
                query = Query.And(query, Query.Or(Query.EQ("Code", key), Query.EQ("Name", key)));
            }
            var list = collection.Find(query)
                .SetSortOrder(SortBy.Ascending("Status"))
                .SetSkip(page * size)
                .SetLimit(size)
                .ToList();
            return DResult.Succ(list, (int)collection.Count());
        }

        public DResult ResetMover(string code)
        {
            var collection = Collection();
            collection.Update(Query.EQ("Code", code),
                Update.Set("Status", 0).Set("StartTime", BsonNull.Value).Set("FinishedTime", BsonNull.Value));
            if (!_isRunning)
            {
                MoveMission();
            }
            return DResult.Success;
        }

        public DResult CancelMover(string code)
        {
            var collection = Collection();
            collection.Remove(Query.EQ("Code", code));
            return DResult.Success;
        }
    }
}
