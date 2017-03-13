using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;

namespace DayEasy.Message.Services.Helper
{
    internal class JointMarkingMsgAdapter : MessageAdapter
    {
        public JointMarkingMsgAdapter(MessageAdapterParam adapterParam)
            : base(adapterParam)
        {
        }

        public override DDynamicMessageDto LoadMessage()
        {
            var message = LoadBaseMessage().MapTo<JointMarkingMessageDto>();

            var jointBatch = AdapterParam.Dynamic.ContentId; //协同批次号
            if (string.IsNullOrEmpty(jointBatch))
                return message;

            //查询协同阅卷
            var jointModel = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>()
                .SingleOrDefault(u => u.Id == jointBatch && u.Status != (byte)JointStatus.Delete);
            if (jointModel == null)
                return message;

            message.JointBatch = jointModel.Id;
            message.JointStatus = (JointStatus)jointModel.Status;
            message.PaperId = jointModel.PaperId;
            message.GroupId = jointModel.GroupId;
            message.AddedBy = jointModel.AddedBy;
            message.PaperACount = jointModel.PaperACount;
            message.PaperBCount = jointModel.PaperBCount;

            //是否有任务
            var distRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointDistribution>>();
            message.Distributed = distRepository.Exists(d => d.JointBatch == jointModel.Id);

            message.HasMission =
                distRepository.Exists(
                    d => d.JointBatch == jointModel.Id && d.TeacherId == AdapterParam.UserId);

            var paperModel = CurrentIocManager.Resolve<IPaperContract>().PaperDetailById(message.PaperId, false);
            if (paperModel == null || !paperModel.Status || paperModel.Data == null)
                return message;
            var model = paperModel.Data;

            message.PaperType = (PaperType)model.PaperBaseInfo.PaperType;

            if (model.AllObjectiveA || (message.PaperType == PaperType.AB && model.AllObjectiveB))
            {
                message.HasMission = true;
            }

            if (string.IsNullOrWhiteSpace(message.Title))
                message.Title = model.PaperBaseInfo.PaperTitle;

            return message;
        }
    }
}
