using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    //短信发送记录
    public interface ISmsRecordContract : IDependency
    {
        DResult SendVcode(string mobile, string vcode);

        DResult Send(string mobile, string message);

        DResult SendLotSize(List<string> mobiles, List<string> messages);

        DResults<MongoSmsRecord> SendLotSizeAndGetResult(List<string> mobiles, List<string> messages);
    }
}
