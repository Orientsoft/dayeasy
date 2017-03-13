using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Message.Services
{
    public partial class MessageService : Version3Service, IMessageContract
    {
        private readonly ILogger _logger = LogManager.Logger<MessageService>();
        public MessageService(IDbContextProvider<Version3DbContext> context)
            : base(context)
        { }

        public IDayEasyRepository<TS_Message, string> MessageRepository { private get; set; }

        #region 消息

        /// <summary> 用户消息列表 </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public DResults<SystemMessageDto> UserMessages(long userId, int messageType = -1, DPage page = null)
        {
            page = page ?? DPage.NewPage();
            Expression<Func<TS_Message, bool>> condition =
                m =>
                    m.UserId == userId &&
                    (m.MessageStatus == (byte)MessageStatus.Normal || m.MessageStatus == (byte)MessageStatus.Read);
            if (messageType >= 0)
                condition = condition.And(m => m.MessageType == messageType);
            var list = MessageRepository.Where(condition)
                .OrderBy(m => m.MessageStatus)
                .ThenByDescending(m => m.CreateOn)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            var count = MessageRepository.Count(condition);
            if (!list.Any())
                return DResult.Succ(new List<SystemMessageDto>(), count);
            var messages = list.MapTo<List<SystemMessageDto>>();
            var userIds =
                messages.Where(m => m.SenderId > 0 && m.MessageType != (byte)MessageType.System)
                    .Select(m => m.SenderId)
                    .Distinct();
            var users = UserContract.LoadDList(userIds);
            messages.ForEach(m =>
            {
                if (m.MessageType == (byte)MessageType.System)
                    m.MessageTitle = "系统消息：" + m.MessageTitle;
                if (m.SenderId > 0 && m.MessageType != (byte)MessageType.System)
                {
                    var user = users.FirstOrDefault(u => u.Id == m.SenderId);
                    if (user != null)
                    {
                        m.SenderName = user.Name;
                        m.SenderAvatar = user.Avatar;
                    }
                }
                if (string.IsNullOrWhiteSpace(m.MessageContent) || !m.MessageContent.StartsWith("{"))
                    return;
                var kv = JsonHelper.Json<DKeyValue>(m.MessageContent);
                if (kv == null)
                    return;
                m.Link = kv.Key;
                m.MessageContent = kv.Value;
            });
            return DResult.Succ(messages, count);
        }

        /// <summary>
        /// 用户消息数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unRead">是否未阅</param>
        /// <returns></returns>
        public int UserMessageCount(long userId, bool unRead = true)
        {
            Expression<Func<TS_Message, bool>> condition = m => m.UserId == userId;
            condition = (unRead
                ? condition.And(m => m.MessageStatus == (byte)MessageStatus.Normal)
                : condition.And(m => m.MessageStatus == (byte)MessageStatus.Read));
            return MessageRepository.Count(condition);
        }

        /// <summary>
        /// 更改消息状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="messageIds"></param>
        /// <param name="status">MessageStatus</param>
        /// <returns></returns>
        public bool UpdateMessageStatus(long userId, ICollection<string> messageIds, MessageStatus status)
        {
            if (userId < 1 || messageIds == null || !messageIds.Any())
                return false;
            var result = 0;
            Expression<Func<TS_Message, bool>> condition = m => m.UserId == userId && messageIds.Contains(m.Id);

            switch (status)
            {
                case MessageStatus.Read:
                    condition = condition.And(m => m.MessageStatus == (byte)MessageStatus.Normal);
                    result = MessageRepository.Update(new TS_Message
                    {
                        MessageStatus = (byte)status,
                        ReadTime = Clock.Now
                    }, condition, "MessageStatus", "ReadTime");
                    break;
                case MessageStatus.Delete:
                    condition = condition.And(m =>
                        m.MessageStatus == (byte)MessageStatus.Normal ||
                        m.MessageStatus == (byte)MessageStatus.Read);
                    result = MessageRepository.Update(new TS_Message
                    {
                        MessageStatus = (byte)status
                    }, condition, "MessageStatus");
                    break;
            }
            return result > 0;
        }

        /// <summary> 发消息 </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="senderId"></param>
        /// <param name="type"></param>
        /// <param name="receivers"></param>
        /// <returns></returns>
        public DResult SendMessage(string title, string content, long senderId, MessageType type = MessageType.System,
            params long[] receivers)
        {
            if (title.IsNullOrEmpty())
                return DResult.Error("消息标题和内容不能为空！");
            if (receivers == null || !receivers.Any())
                return DResult.Error("没有消息的接收人信息！");
            var dt = Clock.Now;
            var helper = IdHelper.Instance;
            var list = receivers.Select(id => new TS_Message
            {
                Id = helper.Guid32,
                UserId = id,
                CreateIp = Utils.GetRealIp(),
                CreateOn = dt,
                CreatorId = senderId,
                MessageTitle = title,
                MessageContent = content,
                MessageStatus = (byte)MessageStatus.Normal,
                MessageType = (byte)type
            });
            return MessageRepository.Insert(list) > 0 ? DResult.Success : DResult.Error("发送失败！");
        }

        /// <summary> 发送消息 </summary>
        /// <param name="sendDto"></param>
        /// <returns></returns>
        public DResult SendMessage(SystemMessageSendDto sendDto)
        {
            if (sendDto == null)
                return DResult.Error("参数错误，请重试！");
            if (sendDto.Receivers == null || !sendDto.Receivers.Any())
                return DResult.Error("没有接收人信息");
            string content = null;
            if (sendDto.Content != null)
                content = sendDto.Content.ToJson();
            return SendMessage(sendDto.Title, content, sendDto.SenderId, sendDto.MessageType,
                sendDto.Receivers.ToArray());
        }

        #endregion
    }
}
