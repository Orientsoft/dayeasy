
using System.Collections.Generic;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Utility.Extend;

namespace DayEasy.Message.Services.Helper
{
    /// <summary> 消息适配器 </summary>
    internal abstract class MessageAdapter
    {
        protected MessageAdapterParam AdapterParam { get; private set; }
        protected IUserContract UserContract { get; private set; }

        protected MessageAdapter(MessageAdapterParam adapterParam)
        {
            AdapterParam = adapterParam;
            UserContract = CurrentIocManager.Resolve<IUserContract>();
        }

        public static MessageAdapter Instance(byte type, MessageAdapterParam adapterParam)
        {
            return MessageInstance(type, adapterParam);
        }

        /// <summary> 适配器控制 </summary>
        /// <param name="type"></param>
        /// <param name="adapterParam"></param>
        /// <returns></returns>
        private static MessageAdapter MessageInstance(byte type, MessageAdapterParam adapterParam)
        {
            switch (type)
            {
                case (byte)GroupDynamicType.Homework:
                case (byte)GroupDynamicType.Exam:
                    return new PaperMessageAdapter(adapterParam);
                case (byte)GroupDynamicType.Joint:
                    return new JointMarkingMsgAdapter(adapterParam);
                case (byte)GroupDynamicType.ExamNotice:
                    return new ExaminationMsgAdapter(adapterParam);
                default:
                    return new DefaultMessageAdapter(adapterParam);
            }
        }

        protected DDynamicMessageDto LoadBaseMessage()
        {
            var message = new DDynamicMessageDto
            {
                Id = AdapterParam.Dynamic.Id,
                DynamicType = AdapterParam.Dynamic.DynamicType,
                Title = AdapterParam.Dynamic.Title,
                Message = AdapterParam.Dynamic.Message,
                SendTime = AdapterParam.Dynamic.AddedAt,
                GoodCount = AdapterParam.Dynamic.GoodCount,
                CommentCount = AdapterParam.Dynamic.CommentCount,
                Status = AdapterParam.Dynamic.Status,
                UserId = AdapterParam.Dynamic.AddedBy,
            };
            if (!string.IsNullOrWhiteSpace(message.Message))
            {
                message.Message = message.Message.As<IRegex>().Replace("\n", "<br/>");
            }
            if (!string.IsNullOrWhiteSpace(AdapterParam.Dynamic.Goods))
            {
                var ids = AdapterParam.Dynamic.Goods.JsonToObject<List<long>>();
                message.Goods = ids;
                message.Liked = message.Goods.Contains(AdapterParam.UserId);
            }
            else
            {
                message.Goods = new List<long>();
            }
            var user = UserContract.Load(message.UserId);
            if (user == null)
                return message;
            message.SubjectId = user.SubjectId;
            message.SubjectName = user.SubjectName;
            return message;
        }

        public abstract DDynamicMessageDto LoadMessage();
    }
}
