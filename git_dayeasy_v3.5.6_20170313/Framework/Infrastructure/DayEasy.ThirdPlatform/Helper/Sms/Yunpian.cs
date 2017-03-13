
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.ThirdPlatform.Entity.Result;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;

namespace DayEasy.ThirdPlatform.Helper.Sms
{
    public class Yunpian : SmsBase
    {
        private readonly SmsPaltform _platform;

        public Yunpian()
        {
            _platform =
                ConfigUtils<PlatformConfig>.Config.SmsPaltforms.FirstOrDefault(t => t.Type == (int)SmsType.YunPian);
        }

        public override DResult<YunpianResult> Send(string mobile, string message)
        {
            var query = new NameValueCollection {{"apikey", _platform.ApiKey}, {"mobile", mobile}, {"text", message}};
            var encoding = Encoding.GetEncoding(_platform.Charset);
            using (var http = new HttpHelper(_platform.SendUrl, "POST", encoding,
                PlatformUtility.QueryString(query, encoding)))
            {
                var html = http.GetHtml();
                if (string.IsNullOrWhiteSpace(html))
                    return DResult.Error<YunpianResult>("接口请求异常！");
                var result = html.JsonToObject<YunpianResult>();
                return DResult.Succ(result);
            }
        }

        public override DResults<YunpianResult> SendLotSize(string mobiles, string messages)
        {
            var query = new NameValueCollection { { "apikey", _platform.ApiKey }, { "mobile", mobiles }, { "text", messages } };
            var encoding = Encoding.GetEncoding(_platform.Charset);
            using (var http = new HttpHelper(_platform.LotSizeUrl, "POST", encoding,
                PlatformUtility.QueryString(query, encoding)))
            {
                var html = http.GetHtml();
                if (string.IsNullOrWhiteSpace(html))
                    return DResult.Errors<YunpianResult>("接口请求异常！");
                var result = html.JsonToObject <List<YunpianResult>>();
                return DResult.Succ(result, (result != null ? result.Count : 0));
            }
        }
    }
}
