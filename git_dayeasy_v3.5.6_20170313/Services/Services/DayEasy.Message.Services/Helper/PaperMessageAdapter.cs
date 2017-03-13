
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;

namespace DayEasy.Message.Services.Helper
{
    /// <summary> 试卷消息适配器 </summary>
    internal class PaperMessageAdapter : MessageAdapter
    {
        public PaperMessageAdapter(MessageAdapterParam adapterParam)
            : base(adapterParam)
        {
        }

        public override DDynamicMessageDto LoadMessage()
        {
            var message = LoadBaseMessage().MapTo<PaperDynamicMessageDto>();
            message.PaperId = AdapterParam.Dynamic.ContentId;
            if (AdapterParam.Dynamic.ContentType == (byte)ContentType.Publish)
            {
                var usageRepository = CurrentIocManager.Resolve<IDayEasyRepository<TC_Usage>>();
                var pubModel =
                    usageRepository.SingleOrDefault(u => u.Id == message.PaperId);

                if (pubModel == null)
                    return message;
                message.PaperId = pubModel.SourceID;
                message.Batch = AdapterParam.Dynamic.ContentId;
                message.ExpireTime = pubModel.ExpireTime;
            }
            var paperRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>();

            var paperModel = paperRepository.Load(message.PaperId);
            if (paperModel == null)
                return message;
            if (string.IsNullOrWhiteSpace(message.Title))
                message.Title = paperModel.PaperTitle;
            return message;
        }
    }
}
