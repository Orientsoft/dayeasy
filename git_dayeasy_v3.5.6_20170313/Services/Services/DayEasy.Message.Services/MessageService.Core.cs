
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Message.Services.Helper;

namespace DayEasy.Message.Services
{
    public partial class MessageService
    {
        public IUserContract UserContract { private get; set; }

        #region 组装前台动态消息数据

        /// <summary>
        /// 组装前台动态消息数据
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dynamicNews"></param>
        /// <param name="userRole"></param>
        /// <returns></returns>
        private DDynamicMessageDto GetNewsDetail(long userId, TM_GroupDynamic dynamicNews, UserRole userRole)
        {
            if (dynamicNews == null)
                return null;
            var adapter = MessageAdapter.Instance(dynamicNews.DynamicType, new MessageAdapterParam
            {
                UserId = userId,
                Role = userRole,
                Dynamic = dynamicNews
            });
            return adapter.LoadMessage();
        }

        #endregion
    }
}
