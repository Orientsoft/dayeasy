using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 消息/动态类契约 </summary>
    public partial interface IMessageContract : IDependency
    {
        /// <summary> 用户消息列表 </summary>
        /// <param name="userId"></param>
        /// <param name="messageType"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<SystemMessageDto> UserMessages(long userId, int messageType = -1, DPage page = null);

        /// <summary> 用户消息数量 </summary>
        /// <param name="userId"></param>
        /// <param name="unRead"></param>
        /// <returns></returns>
        int UserMessageCount(long userId, bool unRead = true);

        /// <summary> 修改消息状态 </summary>
        /// <param name="userId"></param>
        /// <param name="messageIds"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        bool UpdateMessageStatus(long userId, ICollection<string> messageIds, MessageStatus status);

        /// <summary> 发送系统消息 </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="senderId"></param>
        /// <param name="type"></param>
        /// <param name="receivers"></param>
        /// <returns></returns>
        DResult SendMessage(string title, string content, long senderId, MessageType type = MessageType.System,
            params long[] receivers);

        /// <summary> 根据模版发送系统消息 </summary>
        /// <param name="sendDto"></param>
        /// <returns></returns>
        DResult SendMessage(SystemMessageSendDto sendDto);
    }
}
