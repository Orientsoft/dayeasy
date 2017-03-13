using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Marking.Services
{
    /// <summary> 错因分析 </summary>
    public partial class ErrorBookService
    {
        public IUserContract UserContract { private get; set; }
        public IMessageContract MessageContract { private get; set; }

        public IDayEasyRepository<TP_ErrorTagStatistic, string> TagStatisticRepository { private get; set; }
        public IDayEasyRepository<TP_ErrorReasonComment, string> ReasonCommentRepository { private get; set; }

        private void LoadReasonExt(List<ReasonExtDto> reasons)
        {
            var userIds = reasons.Select(r => r.StudentId).ToList();
            var users = UserContract.LoadList(userIds);
            if (users == null || !users.Any())
                return;
            reasons.ForEach(r =>
            {
                if (r.Tag.IsNotNullOrEmpty())
                {
                    r.TagList = JsonHelper.JsonList<NameDto>(r.Tag).ToList();
                    r.Tag = string.Join(",", r.TagList.Select(t => t.Name));
                }

                var user = users.FirstOrDefault(u => u.Id == r.StudentId);
                if (user == null)
                    return;
                r.UserName = user.Name; //UserContract.DisplayName(user);
                r.HeadPic = user.Avatar;
            });
        }

        /// <summary>
        /// 查询试卷中的题目错因分析数量
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public IDictionary<string, int> ReasonCountDict(string batch)
        {
            if (batch.IsNullOrEmpty())
                return new Dictionary<string, int>();
            return ErrorReasonRepository.Where(r =>
                r.Batch == batch && r.Tags != null && r.Tags != string.Empty)
                .GroupBy(r => r.QuestionId)
                .ToDictionary(r => r.Key, r => r.Count());
        }

        /// <summary>
        /// 错因标签列表
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="batch"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public DResults<ReasonExtDto> Reasons(string errorId, string batch, string questionId)
        {
            if (questionId.IsNullOrEmpty() || batch.IsNullOrEmpty())
                return DResult.Errors<ReasonExtDto>("参数错误");

            var reasons = ErrorReasonRepository.Where(r =>
                r.Batch == batch &&
                r.QuestionId == questionId &&
                r.Tags != null && r.Tags != string.Empty)
                .OrderBy(r => r.AddedAt).ToList();
            if (errorId.IsNotNullOrEmpty() && reasons.Count > 1)
            {
                var item = reasons.FirstOrDefault(r => r.ErrorId == errorId);
                if (item != null)
                {
                    reasons.Remove(item);
                    reasons.Insert(0, item);
                }
            }
            var list = reasons.MapTo<List<ReasonExtDto>>();
            LoadReasonExt(list);

            return DResult.Succ(list, reasons.Count);
        }

        /// <summary>
        /// 添加错因标签
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="content"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public DResult AddReason(string errorId, string content, List<string> tags)
        {
            if (errorId.IsNullOrEmpty())
                return DResult.Error("参数错误");
            var item = ErrorQuestionRepository.FirstOrDefault(e => e.Id == errorId);
            if (item == null)
                return DResult.Error("没有查询到错题记录");
            var usage = UsageRepository.FirstOrDefault(u => u.Id == item.Batch && u.SourceID == item.PaperID);
            if (usage == null)
                return DResult.Error("没有查询到发布记录");

            var isUpdate = true;
            var reason = ErrorReasonRepository.FirstOrDefault(r => r.ErrorId == errorId);
            if (reason == null)
            {
                isUpdate = false;
                reason = new TP_ErrorReason
                {
                    Id = IdHelper.Instance.GetGuid32(),
                    ErrorId = errorId,
                    Batch = item.Batch,
                    AddedAt = Clock.Now,
                    ClassId = usage.ClassId,
                    CommentCount = 0,
                    PaperId = item.PaperID,
                    QuestionId = item.QuestionID,
                    StudentId = item.StudentID,
                    ShareType = (byte)ErrorReasonShareType.Open
                };
            }

            if (content.IsNotNullOrEmpty())
            {
                if (content.Length > 140)
                    return DResult.Error("错因分析字数超过140个字符");
                reason.Content = content;
            }

            #region 标签

            var insertTags = new List<TP_ErrorTag>();
            var insertStatistics = new List<TP_ErrorTagStatistic>();
            var updateTags = new List<TP_ErrorTag>();
            var updateStatistics = new List<TP_ErrorTagStatistic>();
            if (reason.Tags.IsNullOrEmpty() && tags != null && tags.Any())
            {
                if (tags.Count > 5)
                    return DResult.Error("错因标签数量超过5个标签");
                if (tags.Any(t => t.Length > 30))
                    return DResult.Error("部分标签字数超过30个字符");
                var tagIds = new List<NameDto>();
                tags.ForEach(t =>
                {
                    //标签
                    var tag = ErrorTagRepository.FirstOrDefault(a => a.TagName == t);
                    if (tag == null)
                    {
                        tag = new TP_ErrorTag
                        {
                            Id = IdHelper.Instance.GetGuid32(),
                            AddedAt = DateTime.Now,
                            AddedBy = item.StudentID,
                            SubjectId = item.SubjectID,
                            TagName = t,
                            UseCount = 1
                        };
                        insertTags.Add(tag);
                    }
                    else
                    {
                        tag.UseCount += 1;
                        updateTags.Add(tag);
                    }
                    //班级错题标签统计
                    var statistic = TagStatisticRepository.FirstOrDefault(a =>
                        a.TagId == tag.Id && a.ClassId == usage.ClassId && a.QuestionId == item.QuestionID);
                    if (statistic == null)
                    {
                        statistic = new TP_ErrorTagStatistic
                        {
                            Id = IdHelper.Instance.GetGuid32(),
                            ClassId = usage.ClassId,
                            QuestionId = item.QuestionID,
                            TagId = tag.Id,
                            UsgCount = 1
                        };
                        insertStatistics.Add(statistic);
                    }
                    else
                    {
                        statistic.UsgCount += 1;
                        updateStatistics.Add(statistic);
                    }
                    tagIds.Add(new NameDto(tag.Id, tag.TagName));
                });
                reason.Tags = JsonHelper.ToJson(tagIds);
            }

            #endregion

            #region 保存数据

            var result = UnitOfWork.Transaction(() =>
            {
                if (insertTags.Any())
                    ErrorTagRepository.Insert(insertTags);
                if (updateTags.Any())
                    ErrorTagRepository.Update(t => new { t.UseCount }, updateTags.ToArray());
                if (insertStatistics.Any())
                    TagStatisticRepository.Insert(insertStatistics);
                if (updateStatistics.Any())
                    TagStatisticRepository.Update(t => new { t.UsgCount }, updateStatistics.ToArray());
                if (isUpdate)
                    ErrorReasonRepository.Update(reason, "Content", "Tags");
                else
                {
                    ErrorReasonRepository.Insert(reason);
                    //var resultId = ErrorReasonRepository.Insert(reason);
                    //if (resultId.IsNullOrEmpty()) return;
                    ////发送通知消息
                    //var studentName = "";
                    //var student = UserContract.Load(item.StudentID);
                    //if (student != null) studentName = UserContract.DisplayName(student);
                    //var link = (Consts.Config.MainSite + "/work/teacher/detail/{0}/{1}/{2}")
                    //    .FormatWith(item.Batch, item.PaperID, item.QuestionID);
                    //var msg = "{0}对试卷《{1}》错题进行了错因分析，<a href='{2}'>点击查看</a>"
                    //    .FormatWith(studentName, item.PaperTitle, link);
                    //MessageContract.SendMessage("错因分析", msg, item.StudentID, MessageType.CommentReply, usage.UserId);
                }
                //修改错题状态
                if ((item.Status & (byte)ErrorQuestionStatus.Marked) == 0)
                {
                    item.Status |= (byte)ErrorQuestionStatus.Marked;
                    ErrorQuestionRepository.Update(e => new { e.Status }, item);
                }
            });

            #endregion

            return result > 0 ? DResult.Success : DResult.Error("保存失败");
        }

        /// <summary>
        /// 删除错因及评论
        /// </summary>
        /// <param name="reasonId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public DResult DeleteComment(string reasonId, long studentId)
        {
            if (reasonId.IsNullOrEmpty())
                return DResult.Error("参数错误");
            var reason = ErrorReasonRepository.FirstOrDefault(r => r.Id == reasonId);
            if (reason == null)
                return DResult.Error("没有查询到错因分析记录");
            if (reason.StudentId != studentId)
                return DResult.Error("智能删除自己的错因分析及评论");

            reason.CommentCount = 0;
            reason.Content = "";

            var result = UnitOfWork.Transaction(() =>
            {
                ErrorReasonRepository.Update(r => new { r.CommentCount, r.Content }, reason);
                ReasonCommentRepository.Delete(c => c.ReasonId == reasonId);
            });
            return result > 0 ? DResult.Success : DResult.Error("删除失败");
        }

        /// <summary>
        /// 错因分析评论列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="reasonId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public DResults<ReasonCommentDto> Comments(DPage page, string reasonId, string parentId)
        {
            if (reasonId.IsNullOrEmpty()) return DResult.Errors<ReasonCommentDto>("参数错误");
            Expression<Func<TP_ErrorReasonComment, bool>> condition = c => c.ReasonId == reasonId;
            condition = parentId.IsNullOrEmpty()
                ? condition.And(c => c.ParentId == null || c.ParentId == "")
                : condition.And(c => c.ParentId == parentId);
            var ordered = ReasonCommentRepository.Where(condition).OrderByDescending(c => c.AddedAt);
            var commentResult = ReasonCommentRepository.PageList(ordered, page);
            if (!commentResult.Status)
                return DResult.Errors<ReasonCommentDto>(commentResult.Message);
            if (!commentResult.Data.Any())
                return DResult.Succ(new List<ReasonCommentDto>(), 0);

            var comments = commentResult.Data.MapTo<List<ReasonCommentDto>>();

            var userIds = comments.Select(c => c.UserId).ToList();
            var users = UserContract.LoadList(userIds);
            if (users == null || !users.Any())
                return DResult.Succ(comments, commentResult.TotalCount);

            comments.ForEach(c =>
            {
                var user = users.FirstOrDefault(u => u.Id == c.UserId);
                if (user == null) return;
                c.UserName = user.Name; //UserContract.DisplayName(user);
                c.Head = user.Avatar;
                c.UserRole = user.Role;

                if (!parentId.IsNullOrEmpty()) return;
                var detailResult = Comments(page, reasonId, c.Id);
                if (!detailResult.Status) return;

                c.DetailCount = detailResult.TotalCount;
                c.Details = detailResult.Data.ToList();
            });

            return DResult.Succ(comments, commentResult.TotalCount);
        }

        /// <summary>
        /// 添加错因分析评论
        /// </summary>
        /// <param name="reasonId"></param>
        /// <param name="parentId"></param>
        /// <param name="content"></param>
        /// <param name="userId"></param>
        /// <param name="receiveId"></param>
        /// <param name="receiveName"></param>
        /// <returns></returns>
        public DResult AddComment(string reasonId, string parentId, string content, long userId, long receiveId, string receiveName)
        {
            if (reasonId.IsNullOrEmpty() || content.IsNullOrEmpty() || userId < 1)
                return DResult.Error("参数错误");
            if (content.Length > 140)
                return DResult.Error("评论内容超过140个字符");
            var reason = ErrorReasonRepository.FirstOrDefault(r => r.Id == reasonId);
            if (reason == null)
                return DResult.Error("没有查询到错因分析记录");
            var item = new TP_ErrorReasonComment
            {
                Id = IdHelper.Instance.GetGuid32(),
                AddedAt = DateTime.Now,
                AddedBy = userId,
                Content = content,
                ReasonId = reasonId
            };
            TP_ErrorReasonComment pComment = null;
            if (parentId.IsNotNullOrEmpty() && receiveName.IsNotNullOrEmpty())
            {
                pComment = ReasonCommentRepository.FirstOrDefault(c => c.Id == parentId);
                if (pComment != null)
                {
                    item.ParentId = parentId;
                    item.ParentName = receiveName;
                }
            }
            else
            {
                reason.CommentCount += 1;
                ErrorReasonRepository.Update(r => new { r.CommentCount }, reason);
            }
            var result = ReasonCommentRepository.Insert(item);
            if (result.IsNullOrEmpty()) return DResult.Error("添加评论失败");

            #region 发送通知消息

            TP_ErrorQuestion errItem;
            UserDto addUser = UserContract.Load(userId),
                receiveUser = UserContract.Load(receiveId);

            if (pComment == null || (receiveUser.Role & (byte)UserRole.Teacher) > 0)
                errItem = ErrorQuestionRepository.FirstOrDefault(e => e.Id == reason.ErrorId);
            else
                errItem = ErrorQuestionRepository.FirstOrDefault(e =>
                    e.Batch == reason.Batch && e.QuestionID == reason.QuestionId && e.StudentID == receiveId);

            if (receiveId < 10 || errItem == null)
                return DResult.Success;
            if (addUser == null || receiveUser == null || userId == receiveUser.Id)
                return DResult.Success;

            var msg = pComment != null
                ? "{0}回复了你在试卷《{1}》错题中的评论，<a href='{2}'>点击查看</a>"
                : "{0}评论了你在试卷《{1}》错题中的错因分析，<a href='{2}'>点击查看</a>";
            var link = ((receiveUser.Role & (byte)UserRole.Student) > 0)
                ? (Consts.Config.MainSite + "/errorBook/detail/{0}")
                    .FormatWith(errItem.Id)
                : (Consts.Config.MainSite + "/work/teacher/detail/{0}/{1}/{2}")
                    .FormatWith(errItem.Batch, errItem.PaperID, errItem.QuestionID);
            MessageContract.SendMessage("错因评论",
                msg.FormatWith(addUser.Name, errItem.PaperTitle, link),
                userId, MessageType.CommentReply, receiveUser.Id);

            #endregion

            return DResult.Success;
        }

        /// <summary>
        /// 推荐标签
        /// </summary>
        /// <param name="errorId"></param>
        /// <returns></returns>
        public DResults<NameDto> RecomTags(string errorId)
        {
            if (errorId.IsNullOrEmpty())
                return DResult.Errors<NameDto>("参数错误");
            var item = ErrorQuestionRepository.FirstOrDefault(e => e.Id == errorId);
            if (item == null)
                return DResult.Errors<NameDto>("没有查询到错题资料");

            var tagIds = TagStatisticRepository.Where(t => t.QuestionId == item.QuestionID)
                .Select(t => t.TagId);
            var tags = ErrorTagRepository.Where(t => tagIds.Contains(t.Id))
                .OrderByDescending(t => t.UseCount).Take(6).ToList();
            if (tags.Count < 6)
            {
                var ids = tags.Select(t => t.Id).ToList();
                tags.AddRange(
                    ErrorTagRepository.Where(t => t.SubjectId == item.SubjectID && !ids.Contains(t.Id))
                        .OrderByDescending(t => t.UseCount).Take(6 - tags.Count).ToList()
                    );
            }
            var list = tags.Select(t => new NameDto(t.Id, t.TagName)).ToList();
            return DResult.Succ(list, list.Count);
        }

        /// <summary>
        /// 班级内错因标签统计
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="questionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public DResults<DKeyValue<string, int>> TagStatistics(string batch, string questionId, int count = 6)
        {
            if (batch.IsNullOrEmpty() || questionId.IsNullOrEmpty())
                return DResult.Errors<DKeyValue<string, int>>("参数错误");

            var usage = UsageRepository.FirstOrDefault(u => u.Id == batch);
            if (usage == null)
                return DResult.Errors<DKeyValue<string, int>>("没有查询到发布记录");

            var result = new List<DKeyValue<string, int>>();

            var list = TagStatisticRepository.Where(t =>
                t.UsgCount > 0 &&
                t.QuestionId == questionId &&
                t.ClassId == usage.ClassId)
                .OrderByDescending(t => t.UsgCount).Take(count).ToList();
            if (!list.Any())
                return DResult.Succ(result, 0);

            var tagIds = list.Select(t => t.TagId).ToList();
            var tags = ErrorTagRepository.Where(t => tagIds.Contains(t.Id)).ToList();

            tags.ForEach(t =>
            {
                var a = list.FirstOrDefault(i => i.TagId == t.Id);
                if (a == null) return;
                result.Add(new DKeyValue<string, int>(t.TagName, a.UsgCount));
            });
            return DResult.Succ(result, result.Count);
        }

        /// <summary>
        /// 指定错题的错因分析 - 编辑错因分析使用
        /// </summary>
        /// <param name="errorId"></param>
        /// <returns></returns>
        public DResult<dynamic> Load(string errorId)
        {
            if (errorId.IsNullOrEmpty()) return DResult.Error<dynamic>("参数错误");
            var count = 0;
            var content = "";
            List<string> tags = new List<string>(),
                sysTags = new List<string>();
            //已分析错因人数
            var item = ErrorQuestionRepository.FirstOrDefault(e => e.Id == errorId);
            if (item != null)
            {
                count = ErrorReasonRepository.Count(r =>
                    r.Batch == item.Batch && r.PaperId == item.PaperID && r.QuestionId == item.QuestionID);
            }
            //当前错题的错因分析
            var reason = ErrorReasonRepository.FirstOrDefault(r => r.ErrorId == errorId);
            if (reason != null)
            {
                content = reason.Content;
                if (reason.Tags.IsNotNullOrEmpty())
                    tags = JsonHelper.JsonList<NameDto>(reason.Tags).Select(r => r.Name).ToList();
            }
            //推荐标签
            var sysResult = RecomTags(errorId);
            if (sysResult.Status)
            {
                sysTags = sysResult.Data.Select(r => r.Name).ToList();
            }
            return DResult.Succ<dynamic>(new { id = errorId, count, content, tags, sys_tags = sysTags });
        }

        /// <summary>
        /// 获取错题ID
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="questionId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public string GetErrorId(string batch, string questionId, long studentId)
        {
            var item = ErrorQuestionRepository.FirstOrDefault(
                i => i.Batch == batch && i.QuestionID == questionId && i.StudentID == studentId);
            return item != null ? item.Id : string.Empty;
        }
    }
}
