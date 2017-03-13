
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Marking.Joint;
using DayEasy.Contracts.Models;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core.Cache;
using DayEasy.Core.Dependency;
using DayEasy.MongoDb;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.Marking.Services.Helper
{
    /// <summary> 协同阅卷 - 辅助类 MongoDB </summary>
    public class JointHelper
    {
        private static readonly object InstanceLock = new object();
        private readonly object _lockObj;
        private const string CacheName = "joint_helper_{0}";
        private const string AreaCacheName = "joint_area_{0}";
        private const string CacheRegion = "marking";
        private const string FieldId = "_id";
        private const string FieldJointBatch = "JointBatch";
        private const string FieldQuestionTask = "QuestionTask";
        private const string FieldTeacherId = "TeacherId";
        private const string FieldPictureId = "PictureId";
        private readonly string _jointBatch;

        private JointHelper(string jointBatch)
        {
            _jointBatch = jointBatch;
            _lockObj = new object();
        }

        /// <summary> 单例实例化 </summary>
        public static JointHelper Instance(string jointBatch)
        {
            lock (InstanceLock)
            {
                var key = CacheName.FormatWith(jointBatch);
                var cache = new RuntimeMemoryCache(CacheRegion);
                var instance = cache.Get<JointHelper>(key);
                if (instance == null)
                {
                    instance = new JointHelper(jointBatch);
                    cache.Set(key, instance, TimeSpan.FromDays(5));
                }
                return instance;
            }
        }

        private static MongoCollection<MPictureTask> Collection()
        {
            return new MongoManager().Collection<MPictureTask>();
        }

        /// <summary> 初始化试卷任务 </summary>
        public Task InitPictureTask(IDictionary<byte, List<string>> pictures, IDictionary<byte, List<string>> qids)
        {
            return Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrWhiteSpace(_jointBatch) || pictures.IsNullOrEmpty() || qids.IsNullOrEmpty())
                    return;
                var collection = Collection();
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
                        if (collection.FindOne(Query.EQ(FieldPictureId, id)) != null)
                            continue;
                        var task = new MPictureTask
                        {
                            Id = helper.Guid32,
                            JointBatch = _jointBatch,
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
            });
        }

        /// <summary> 协同阅卷 - 分配图片 </summary>
        /// <param name="teacherId"></param>
        /// <param name="qids"></param>
        /// <param name="skip">大于等于0:拿新的，负数：历史记录</param>
        /// <returns></returns>
        public JPictureDto MarkingJointPicture(long teacherId, IList<string> qids, int skip = 1)
        {
            var dto = new JPictureDto();
            MPictureTask item;
            var collection = Collection();

            var baseQuery = Query.EQ(FieldJointBatch, _jointBatch);
            var markedQuery = qids.Aggregate(Query.Empty,
                (current, id) =>
                    Query.And(current,
                        Query.ElemMatch(FieldQuestionTask,
                            Query.And(Query.EQ(FieldTeacherId, teacherId), Query.EQ(FieldId, id)))));
            var findQuery = qids.Aggregate(Query.Empty,
                (current, id) =>
                    Query.And(current,
                        Query.ElemMatch(FieldQuestionTask,
                            Query.And(Query.EQ(FieldTeacherId, 0), Query.EQ(FieldId, id)))));
            if (skip < 0)
            {
                //倒退
                var qid = qids.First();
                var list = collection.Find(Query.And(baseQuery, markedQuery));
                if (list.Count() >= 0 - skip)
                {
                    item = list.OrderByDescending(t => t.QuestionTask.First(q => q.Id == qid).MarkingTime)
                        .Skip(-1 - skip)
                        .FirstOrDefault();
                    if (item != null)
                    {
                        dto.Id = item.PictureId;
                    }
                }
                else if (skip == -1)
                {
                    return MarkingJointPicture(teacherId, qids, 1);
                }
            }
            else
            {
                //拿新的
                lock (_lockObj)
                {
                    item = collection.FindOne(Query.And(baseQuery, findQuery));
                    if (item != null)
                    {
                        dto.Id = item.PictureId;
                        //更新
                        foreach (var qid in qids)
                        {
                            collection.Update(
                                Query.And(Query.EQ(FieldJointBatch, _jointBatch), Query.EQ(FieldId, item.Id),
                                    Query.EQ("QuestionTask._id", qid)),
                                Update.Set("QuestionTask.$.TeacherId", teacherId)
                                    .Set("QuestionTask.$.MarkingTime", Clock.Now));
                        }
                    }
                }
            }

            //已批阅数
            dto.Marked = (int)collection.Count(Query.And(baseQuery, markedQuery));
            //待批阅数
            dto.Left = (int)collection.Count(Query.And(baseQuery, findQuery));
            return dto;
        }

        /// <summary> 设置异常 </summary>
        /// <param name="pictureId"></param>
        /// <param name="qids"></param>
        /// <param name="isSolve"></param>
        public void UpdateException(string pictureId, IList<string> qids, bool isSolve = false)
        {
            var collection = Collection();
            var teacherId = (isSolve ? 0 : -1);
            var baseQuery = Query.EQ(FieldJointBatch, _jointBatch);
            lock (_lockObj)
            {
                foreach (var qid in qids)
                {
                    var query = Query.And(baseQuery, Query.EQ(FieldPictureId, pictureId),
                        Query.EQ("QuestionTask._id", qid));
                    collection.Update(query, Update.Set("QuestionTask.$.TeacherId", teacherId));
                }
            }
        }

        /// <summary> 批阅记录 </summary>
        public List<string> PictureHistory(long teacherId, string qid)
        {
            var query = Query.And(Query.EQ(FieldJointBatch, _jointBatch),
                Query.ElemMatch(FieldQuestionTask,
                    Query.And(Query.EQ(FieldId, qid), Query.EQ(FieldTeacherId, teacherId))));
            var collection = Collection();
            var list = collection.Find(query);
            if (!list.Any())
                return new List<string>();
            var pictures =
                list.OrderBy(t => t.QuestionTask.First(q => q.Id == qid).MarkingTime).Select(t => t.PictureId).ToList();
            return pictures;
        }

        /// <summary> 批阅进度 </summary>
        public Dictionary<string, Dictionary<long, int>> MarkingSchedule()
        {
            var query = Query.EQ(FieldJointBatch, _jointBatch);
            var collection = Collection();
            var list = collection.Find(query);
            var dict = new Dictionary<string, Dictionary<long, int>>();
            foreach (var task in list.SelectMany(t => t.QuestionTask))
            {
                var qid = task.Id;
                if (!dict.ContainsKey(qid))
                {
                    dict.Add(qid, new Dictionary<long, int>());
                }
                if (task.TeacherId <= 0)
                    continue;
                var item = dict[qid];
                if (!item.ContainsKey(task.TeacherId))
                    item.Add(task.TeacherId, 1);
                else
                {
                    item[task.TeacherId] += 1;
                }
            }
            return dict;
        }

        /// <summary> 题目区域缓存 </summary>
        public static Dictionary<string, MkQuestionAreaDto> QuestionAreaCache(string jointBatch)
        {
            var key = AreaCacheName.FormatWith(jointBatch);
            var cache = CacheManager.GetCacher(CacheRegion);
            var dict = cache.Get<Dictionary<string, MkQuestionAreaDto>>(key);
            if (dict != null)
                return dict;
            dict = new Dictionary<string, MkQuestionAreaDto>();
            var markRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingMark>>();
            var marks = markRepository.Where(t => t.BatchNo == jointBatch).Select(t => t.Mark).ToList();
            if (!marks.Any())
                return dict;
            foreach (var mark in marks)
            {
                if (string.IsNullOrWhiteSpace(mark))
                    continue;
                var list = JsonHelper.JsonList<MkQuestionAreaDto>(mark);
                foreach (var dto in list)
                {
                    if (!dict.ContainsKey(dto.Id))
                        dict.Add(dto.Id, dto);
                }
            }
            cache.Set(key, dict, TimeSpan.FromDays(3));
            return dict;
        }

        /// <summary> 重置区域缓存 </summary>
        /// <param name="jointBatch"></param>
        public static void ResetAreaCache(string jointBatch)
        {
            var key = AreaCacheName.FormatWith(jointBatch);
            CacheManager.GetCacher(CacheRegion).Remove(key);
        }
    }
}
