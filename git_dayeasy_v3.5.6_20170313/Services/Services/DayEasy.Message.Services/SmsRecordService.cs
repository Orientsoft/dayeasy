using System.Collections.Generic;
using System.Linq;
using System.Text;
using DayEasy.Contracts;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.MongoDb;
using DayEasy.ThirdPlatform;
using DayEasy.ThirdPlatform.Entity.Result;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Driver;

namespace DayEasy.Message.Services
{
    public class SmsRecordService : ISmsRecordContract
    {
        private readonly MongoCollection<MongoSmsRecord> _collection;

        public SmsRecordService()
        {
            _collection = new MongoManager().Collection<MongoSmsRecord>();
        }

        internal void Add(MongoSmsRecord record)
        {
            if (record == null) return;
            Add(new List<MongoSmsRecord> {record});
        }

        internal void Add(List<MongoSmsRecord> records)
        {
            if (records == null) return;
            records.ForEach(record =>
            {
                if (record == null) return;
                _collection.Insert(record);
            });
        }

        internal MongoSmsRecord ConvertToRecord(YunpianResult item)
        {
            return new MongoSmsRecord
            {
                Id = IdHelper.Instance.GetGuid32(),
                Type = 0, //平台类型：0-云片
                Time = Clock.Now,
                Status = (byte) (item != null && item.Code == 0 ? 0 : 2),
                Detail = item != null ? item.ToJson() : string.Empty
            };
        }
        
        public DResult SendVcode(string mobile, string vcode)
        {
            return Send(mobile, "【得一教育】您的验证码是{0}，请您在30分钟内完成验证。如非本人操作，请忽略。".FormatWith(vcode));
        }

        public DResult Send(string mobile, string message)
        {
            var result = SmsHelper.SendSms(mobile, message);
            if (!result.Status) return result;
                
            var record = ConvertToRecord(result.Data);
            record.Mobile = mobile;
            record.Message = message;
            Add(record);

            return record.Status == 0 ? DResult.Success : DResult.Error("短信发送失败");
        }

        public DResult SendLotSize(List<string> mobiles, List<string> messages)
        {
            var result = SendLotSizeAndGetResult(mobiles, messages);
            if (!result.Status) 
                return DResult.Error(result.Message);
            return result.Data.Any(r => r.Status == 0) ? DResult.Success : DResult.Error("短信发送失败");
        }

        public DResults<MongoSmsRecord> SendLotSizeAndGetResult(List<string> mobiles, List<string> messages)
        {
            if (mobiles == null || messages == null || mobiles.Count != messages.Count || !mobiles.Any())
                return DResult.Errors<MongoSmsRecord>("手机号码不能为空，且数量必须与短信内容数量相同");

            var mobileStr = mobiles.Join(",");
            var messageStr = messages.Select(m => Utils.UrlEncode(m, Encoding.UTF8)).Join(",");

            var result = SmsHelper.SendLotSize(mobileStr, messageStr);
            if (!result.Status) return DResult.Errors<MongoSmsRecord>(result.Message);

            if (result.Data == null) return DResult.Errors<MongoSmsRecord>("短信发送失败");
            var list = result.Data.ToList();
            var records = new List<MongoSmsRecord>();
            for (var i = 0; i < list.Count; i++)
            {
                var record = ConvertToRecord(list[i]);
                if (mobiles.Count > i) record.Mobile = mobiles[i];
                if (messages.Count > i) record.Message = messages[i];
                records.Add(record);
            }
            Add(records);
            return DResult.Succ(records, records.Count);
        }
    }
}
