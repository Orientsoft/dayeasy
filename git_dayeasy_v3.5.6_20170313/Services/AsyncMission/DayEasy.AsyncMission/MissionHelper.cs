using DayEasy.AsyncMission.Models;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.MongoDb;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DayEasy.AsyncMission
{
    public static class MissionHelper
    {
        public static void PushMission(MAsyncMission mission)
        {
            MongoHelper.Insert(mission);
        }

        /// <summary> 推送任务 </summary>
        /// <param name="type">任务类型</param>
        /// <param name="param">任务参数</param>
        /// <param name="creator">推送人Id</param>
        /// <param name="name">任务名称</param>
        /// <param name="priority">优先级</param>
        public static DResult PushMission(MissionType type, MissionParam param, long? creator = null, string name = null,
            int priority = 0)
        {
            name = name ?? type.ToString();
            var collection = new MongoManager().Collection<MAsyncMission>();
            var query = Query.And(Query.EQ(nameof(MAsyncMission.Type), type),
                Query.EQ(nameof(MAsyncMission.Params), JsonHelper.ToJson(param)),
                Query.In(nameof(MAsyncMission.Status), new BsonValue[] { MissionStatus.Pendding, MissionStatus.Running }));
            if (collection.Count(query) > 0)
                return DResult.Error("任务已在队列中...");
            var mission = new MAsyncMission(name, type, param, creator);
            if (priority > 0)
                mission.Priority = priority;
            MongoHelper.Insert(mission);
            return DResult.Success;
        }

        /// <summary> 是否存在异步任务 </summary>
        /// <param name="type">任务类型</param>
        /// <returns></returns>
        public static bool ExistsMission(MissionType type)
        {
            var query = Query.And(Query.EQ(nameof(MAsyncMission.Type), type),
                Query.EQ(nameof(MAsyncMission.Status), MissionStatus.Pendding));
            var collection = new MongoManager().Collection<MAsyncMission>();
            return collection.Count(query) > 0;
        }

        /// <summary> 获取下一个任务，并设置开始时间 </summary>
        /// <param name="type">任务类型</param>
        /// <returns></returns>
        public static MAsyncMission Get(MissionType type)
        {
            var query = Query.And(Query.EQ(nameof(MAsyncMission.Status), MissionStatus.Pendding),
                Query.EQ(nameof(MAsyncMission.Type), type));

            var collection = new MongoManager().Collection<MAsyncMission>();
            var sortBy = SortBy.Descending(nameof(MAsyncMission.Priority)).Ascending(nameof(MAsyncMission.CreationTime));
            var mission = collection.Find(query)
                .SetSortOrder(sortBy)
                .SetLimit(1)
                .FirstOrDefault();
            if (mission == null)
                return null;
            mission.StartTime = Clock.Now;
            mission.Status = MissionStatus.Running;
            collection.Save(mission);
            return mission;
        }

        /// <summary> 任务列表 </summary>
        /// <param name="type">任务类型</param>
        /// <param name="status">任务状态</param>
        /// <param name="keyword"></param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public static DResults<MAsyncMission> Missions(int type = -1, int status = -1, string keyword = null, DPage page = null)
        {
            if (type >= 0 && !Enum.IsDefined(typeof(MissionType), type))
                return DResult.Errors<MAsyncMission>("任务类型异常！");
            if (status >= 0 && !Enum.IsDefined(typeof(MissionStatus), (byte)status))
                return DResult.Errors<MAsyncMission>("任务状态异常！");
            page = page ?? DPage.NewPage();
            var query = Query.Empty;
            if (type >= 0)
                query = Query.EQ(nameof(MAsyncMission.Type), type);
            if (status >= 0)
                query = Query.And(query, Query.EQ(nameof(MAsyncMission.Status), status));
            if (!string.IsNullOrWhiteSpace(keyword))
                query = Query.And(query,
                    Query.Matches(nameof(MAsyncMission.Params), BsonRegularExpression.Create(keyword)));
            var collection = new MongoManager().Collection<MAsyncMission>();
            var count = (int)collection.Count(query);
            var sortBy = SortBy.Ascending(nameof(MAsyncMission.Status));
            if (status == (byte)MissionStatus.Pendding)
                sortBy = sortBy.Descending(nameof(MAsyncMission.Priority));
            sortBy = sortBy.Descending(nameof(MAsyncMission.CreationTime));
            var missions = collection.Find(query)
                .SetSortOrder(sortBy)
                .SetSkip(page.Page * page.Size)
                .SetLimit(page.Size)
                .ToList();
            missions.ForEach(m =>
            {
                m.CreationTime = Clock.Normalize(m.CreationTime);
                if (m.StartTime.HasValue)
                    m.StartTime = Clock.Normalize(m.StartTime.Value);
                if (m.FinishedTime.HasValue)
                    m.FinishedTime = Clock.Normalize(m.FinishedTime.Value);
            });
            return DResult.Succ(missions, count);
        }

        /// <summary> 任务失败 </summary>
        /// <param name="mission">任务</param>
        /// <param name="ex">异常信息</param>
        /// <param name="message">异常消息</param>
        /// <param name="retry">是否重试</param>
        public static void FailMission(MAsyncMission mission, string message = null, Exception ex = null, bool retry = true)
        {
            if (mission == null) return;
            var collection = new MongoManager().Collection<MAsyncMission>();
            mission = collection.FindOneById(mission.Id);
            mission.FailCount = (mission.FailCount ?? 0) + 1;
            var sb = new StringBuilder();
            if (ex != null || !string.IsNullOrWhiteSpace(message))
            {
                if (!string.IsNullOrWhiteSpace(message))
                    sb.AppendLine(message);
                if (ex != null)
                {
                    sb.AppendLine(ex.Message);
                }
            }
            if (mission.FailCount >= 5 || !retry)
            {
                //失败5次，发送Email
                mission.Status = MissionStatus.Invalid;
                mission.Message = sb.ToString();

                var receiver = "manager".Config(string.Empty);
                if (!string.IsNullOrWhiteSpace(receiver))
                {
                    sb.AppendLine(JsonHelper.ToJson(mission, NamingType.CamelCase, true));
                    Consts.CreateEmail().SendEmail(receiver, "异步任务执行失败", sb.ToString());
                }
            }
            else
            {
                //重置状态
                mission.Status = MissionStatus.Pendding;
                mission.Message = sb.ToString();
                mission.StartTime = null;
                //增加优先级
                mission.Priority = mission.Priority + 1;
            }
            collection.Save(mission);
        }

        /// <summary> 完成任务 </summary>
        /// <param name="mission"></param>
        public static void FinishMission(MAsyncMission mission)
        {
            if (mission == null) return;
            var collection = new MongoManager().Collection<MAsyncMission>();
            mission.Status = MissionStatus.Finished;
            mission.FinishedTime = Clock.Now;
            collection.Save(mission);
        }

        /// <summary> 重置任务 </summary>
        /// <param name="id"></param>
        public static DResult ResetMission(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return DResult.Error("参数异常");
            var query = Query.EQ("_id", id);
            var collection = new MongoManager().Collection<MAsyncMission>();
            if (collection.Count(query) == 0)
                return DResult.Error("任务不存在");
            collection.Update(query,
                Update.Set(nameof(MAsyncMission.Status), MissionStatus.Pendding)
                    .Set(nameof(MAsyncMission.StartTime), BsonNull.Value));
            return DResult.Success;
        }

        public static DResult UpdatePriority(string id, int priority)
        {
            if (string.IsNullOrWhiteSpace(id))
                return DResult.Error("参数异常");
            var query = Query.EQ("_id", id);
            var collection = new MongoManager().Collection<MAsyncMission>();
            if (collection.Count(query) == 0)
                return DResult.Error("任务不存在");
            collection.Update(query, Update.Set(nameof(MAsyncMission.Priority), priority));
            return DResult.Success;
        }

        public static void Fail(this MAsyncMission mission, string message = null, Exception ex = null, bool retry = true)
        {
            FailMission(mission, message, ex, retry);
        }

        public static void Finish(this MAsyncMission mission)
        {
            FinishMission(mission);
        }

        public static void AppendLog(this MAsyncMission mission, string log)
        {
            if (mission == null || string.IsNullOrWhiteSpace(log)) return;
            var collection = new MongoManager().Collection<MAsyncMission>();
            mission = collection.FindOneById(mission.Id);
            if (mission == null) return;
            mission.Logs = (string.IsNullOrWhiteSpace(mission.Logs) ? log : mission.Logs + log);
            collection.Save(mission);
        }
    }
}
