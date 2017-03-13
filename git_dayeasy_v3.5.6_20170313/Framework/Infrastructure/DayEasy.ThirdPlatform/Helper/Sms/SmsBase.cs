
using System.Reflection;
using DayEasy.ThirdPlatform.Entity.Result;
using DayEasy.Utility;

namespace DayEasy.ThirdPlatform.Helper.Sms
{
    public abstract class SmsBase
    {
        internal static SmsBase GetInstance(string type)
        {
            SmsBase instance;
            if (!string.IsNullOrEmpty(type))
            {
                var ass = Assembly.GetExecutingAssembly();
                instance =
                    (SmsBase)
                        ass.CreateInstance(string.Format("{0}.Helper.Sms.{1}", ass.GetName().Name, type), true);
            }
            else
                instance = null;
            return instance;
        }

        public abstract DResult<YunpianResult> Send(string mobile, string message);
        public abstract DResults<YunpianResult> SendLotSize(string mobiles, string messages);
    }
}
