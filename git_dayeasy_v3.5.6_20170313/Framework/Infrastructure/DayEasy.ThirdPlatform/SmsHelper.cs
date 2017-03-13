
using System.Linq;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.ThirdPlatform.Entity.Result;
using DayEasy.ThirdPlatform.Helper.Sms;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;

namespace DayEasy.ThirdPlatform
{
    /// <summary> 短信发送 </summary>
    public class SmsHelper
    {
        /// <summary> 短信发送基础接口 </summary>
        public static DResult<YunpianResult> SendSms(string mobile, string message)
        {
            var config = ConfigUtils<PlatformConfig>.Config;
            if (config == null) return DResult.Error<YunpianResult>("未找到第三方配置文件！");

            var sms = config.SmsPaltforms.FirstOrDefault(t => t.IsActive);
            if (sms == null) return DResult.Error<YunpianResult>("未配置可用短信接口！");
            var type = (SmsType)sms.Type;
            var helper = SmsBase.GetInstance(type.ToString());
            return helper.Send(mobile, message);
        }

        public static DResults<YunpianResult> SendLotSize(string mobiles, string messages)
        {
            var config = ConfigUtils<PlatformConfig>.Config;
            if (config == null) return DResult.Errors<YunpianResult>("未找到第三方配置文件！");

            var sms = config.SmsPaltforms.FirstOrDefault(t => t.IsActive);
            if (sms == null) return DResult.Errors<YunpianResult>("未配置可用短信接口！");
            var type = (SmsType)sms.Type;
            var helper = SmsBase.GetInstance(type.ToString());
            return helper.SendLotSize(mobiles, messages);
        }
        
    }
}
