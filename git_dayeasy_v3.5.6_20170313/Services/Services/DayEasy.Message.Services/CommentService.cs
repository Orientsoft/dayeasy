using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.MongoDb;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace DayEasy.Message.Services
{
    /// <summary> 评论服务~ </summary>
    public class CommentService : ICommentContract
    {
        private readonly MongoCollection<MongoComment> _collection;
        public IUserContract UserContract { private get; set; }

        public CommentService()
        {
            _collection = new MongoManager().Collection<MongoComment>();
        }

        public DResult<CommentDto> Comment(string sourceId, long userId, string message, string replyId = null)
        {
            if (string.IsNullOrEmpty(sourceId))
                return DResult.Error<CommentDto>("评论源ID不能为空！");
            if (userId <= 0)
                return DResult.Error<CommentDto>("评论用户不存在！");
            if (string.IsNullOrWhiteSpace(message))
                return DResult.Error<CommentDto>("评论内容不能为空！");
            var comment = new MongoComment
            {
                Id = IdHelper.Instance.Guid32,
                SourceId = sourceId,
                UserId = userId,
                Message = message.FormatMessage(),
                AddedAt = Clock.Now,
                LikeCount = 0,
                HateCount = 0
            };
            var dto = comment.MapTo<CommentDto>();
            if (!string.IsNullOrWhiteSpace(replyId))
            {
                var parent = _collection.FindOneById(replyId);
                if (parent == null)
                    return DResult.Error<CommentDto>("回复内容不存在！");
                if (parent.Parents == null || !parent.Parents.Any())
                {
                    comment.Parents = new List<string> { replyId };
                }
                else
                {
                    var parentIds = parent.Parents;
                    parentIds.Add(replyId);
                    comment.Parents = parentIds;
                    dto.ParentId = replyId;
                }
            }
            else
            {
                var query = Query.And(Query.EQ("SourceId", sourceId), Query.EQ("Parents", BsonNull.Value));
                var floors = _collection.Find(query).Select(t => t.Floor).ToList();
                if (floors.Any())
                {
                    comment.Floor = Math.Max(floors.Max(), floors.Count()) + 1;
                    if (floors.Any(t => t <= 0))
                    {
                        UpdateFloor(sourceId);
                    }
                }
                else
                {
                    comment.Floor = 1;
                }
                dto.Floor = comment.Floor;
            }
            _collection.Insert(comment);
            return DResult.Succ(dto);
        }

        private void UpdateFloor(string sourceId)
        {
            //update
            var list =
                _collection.Find(Query.And(Query.EQ("SourceId", sourceId), Query.EQ("Parents", BsonNull.Value),
                    Query.EQ("Floor", BsonNull.Value))).SetSortOrder(SortBy<MongoComment>.Ascending(t => t.AddedAt));
            var floor = 1;
            foreach (var item in list)
            {
                item.Floor = floor++;
                _collection.Save(item);
            }
        }

        public DResult<int> Delete(string id, long userId)
        {
            var comment = _collection.FindOneById(id);
            if (comment == null)
                return DResult.Error<int>("评论不存在！");
            if (comment.UserId != userId)
                return DResult.Error<int>("不能删除其他人的评论！");
            var count = (int)_collection.Count(Query.EQ("Parents", id));
            _collection.Remove(Query.Or(Query.EQ("_id", id), Query.EQ("Parents", id)));
            return DResult.Succ(count + 1);
        }

        public DResult Delete(string sourceId)
        {
            _collection.Remove(Query.EQ("SourceId", sourceId));
            return DResult.Success;
        }

        public DResult<int> Like(string id, long userId)
        {
            var comment = _collection.FindOneById(id);
            if (comment == null)
                return DResult.Error<int>("评论不存在！");
            var likes = comment.Likes ?? new List<long>();
            if (likes.Contains(userId))
            {
                likes.Remove(userId);
                comment.LikeCount--;
            }
            else
            {

                likes.Add(userId);
                comment.LikeCount++;
            }
            if (comment.LikeCount < 0)
                comment.LikeCount = 0;
            comment.Likes = likes;
            _collection.Save(comment);
            return DResult.Succ(comment.LikeCount);
        }

        public DResult<int> Hate(string id, long userId)
        {
            var comment = _collection.FindOneById(id);
            if (comment == null)
                return DResult.Error<int>("评论不存在！");
            var hates = comment.Hates ?? new List<long>();
            if (hates.Contains(userId))
            {
                hates.Remove(userId);
                comment.HateCount--;
            }
            else
            {

                hates.Add(userId);
                comment.HateCount++;
            }
            if (comment.HateCount < 0)
                comment.HateCount = 0;
            comment.Hates = hates;
            _collection.Save(comment);
            return DResult.Succ(comment.HateCount);
        }

        public long CommentCount(string sourceId)
        {
            return _collection.Count(Query.EQ("SourceId", sourceId));
        }

        public DResult<CommentResultDto> CommentList(CommentSearchDto searchDto)
        {
            var showNick = searchDto.SourceId.StartsWith("topic_");

            var query = Query.And(Query.EQ("SourceId", searchDto.SourceId), Query.EQ("Parents", BsonNull.Value));
            var list = _collection.Find(query)
                .SetSortOrder(SortBy<MongoComment>.Ascending(t => t.AddedAt))
                .SetSkip(searchDto.Page * searchDto.Size)
                .SetLimit(searchDto.Size);
            if (list.Any(t => t.Floor <= 0))
            {
                UpdateFloor(searchDto.SourceId);
            }
            var comments = list.MapTo<List<CommentDto>>();
            var count = (int)_collection.Count(Query.EQ("SourceId", searchDto.SourceId));
            var commentCount = (int)list.Count();
            var userIds = comments.Select(c => c.UserId);

            comments.ForEach(t =>
            {
                t.CreateTime = Clock.Normalize(t.CreateTime);
                var replyList = _collection.Find(Query.EQ("Parents", t.Id))
                    .SetSortOrder(SortBy<MongoComment>.Ascending(c => c.AddedAt));
                if (replyList.Any())
                {
                    var replys = new List<CommentDto>();
                    foreach (var comment in replyList)
                    {
                        var reply = comment.MapTo<CommentDto>();
                        reply.CreateTime = Clock.Normalize(reply.CreateTime);
                        if (comment.Parents != null && comment.Parents.Count > 1)
                            reply.ParentId = comment.Parents.Last();
                        replys.Add(reply);
                    }
                    t.Replys = replys;
                }
            });
            userIds =
                comments.Where(c => c.Replys != null)
                    .SelectMany(c => c.Replys.Select(r => r.UserId))
                    .Union(userIds)
                    .Distinct()
                    .ToList();
            var users = UserContract.LoadDList(userIds, showNick);

            var result = new CommentResultDto
            {
                Users = users.ToDictionary(k => k.Id, v => v),
                Comments = comments,
                Count = count,
                CommentCount = commentCount
            };
            return DResult.Succ(result);
        }

        public DResult<CommentResultDto> CommentDialog(string id)
        {
            var comment = _collection.FindOneById(id);
            if (comment == null)
                return DResult.Error<CommentResultDto>("评论不存在！");
            if (comment.Parents == null || !comment.Parents.Any())
                return DResult.Error<CommentResultDto>("没有对话内容！");
            var parents = _collection.Find(Query.In("_id", BsonArray.Create(comment.Parents)))
                .SetSortOrder(SortBy<MongoComment>.Ascending(c => c.AddedAt)).ToList();
            parents.Add(comment);
            var comments = parents.MapTo<List<CommentDto>>();
            comments.ForEach(t =>
            {
                t.CreateTime = Clock.Normalize(t.CreateTime);
            });
            var userIds = comments.Select(c => c.UserId);
            var users = UserContract.LoadDList(userIds)
                .ToDictionary(k => k.Id, v => v);
            var result = new CommentResultDto
            {
                Users = users,
                Comments = comments,
                Count = comments.Count
            };
            return DResult.Succ(result);
        }
    }
}
