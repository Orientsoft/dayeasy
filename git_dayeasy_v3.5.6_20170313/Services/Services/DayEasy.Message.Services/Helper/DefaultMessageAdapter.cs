
using DayEasy.Contracts.Dtos.Message;

namespace DayEasy.Message.Services.Helper
{
    /// <summary> 默认消息适配器 </summary>
    internal class DefaultMessageAdapter : MessageAdapter
    {
        public DefaultMessageAdapter(MessageAdapterParam adapterParam) :
            base(adapterParam)
        {
        }

        public override DDynamicMessageDto LoadMessage()
        {
            return LoadBaseMessage();
        }
    }
}
