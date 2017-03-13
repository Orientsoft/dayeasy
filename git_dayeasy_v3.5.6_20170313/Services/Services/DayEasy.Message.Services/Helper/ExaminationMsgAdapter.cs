using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.Message;

namespace DayEasy.Message.Services.Helper
{
    internal class ExaminationMsgAdapter : MessageAdapter
    {
        public ExaminationMsgAdapter(MessageAdapterParam adapterParam)
            : base(adapterParam)
        {
        }

        public override DDynamicMessageDto LoadMessage()
        {
            var message = LoadBaseMessage().MapTo<ExaminationMessageDto>();
            message.ExamId = AdapterParam.Dynamic.ContentId;
            return message;
        }
    }
}
