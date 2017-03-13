
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;

namespace DayEasy.Message.Services.Helper
{
    internal class MessageAdapterParam
    {
        public long UserId { get; set; }
        public UserRole Role { get; set; }
        public TM_GroupDynamic Dynamic { get; set; }
    }
}
