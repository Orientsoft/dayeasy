using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Core;
using DayEasy.Core.Modules;

namespace DayEasy.Message.Services
{
    [DependsOn(typeof(CoreModule))]
    public class MessageServiceModule : DModule
    {
        public override void Initialize()
        {
            AutoMapperHelper.CreateMapper<DDynamicMessageDto, PaperDynamicMessageDto>();
            AutoMapperHelper.CreateMapper<DDynamicMessageDto, JointMarkingMessageDto>();
            AutoMapperHelper.CreateMapper<DDynamicMessageDto, ExaminationMessageDto>();

            base.Initialize();
        }
    }
}
